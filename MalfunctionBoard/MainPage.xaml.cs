namespace MalfunctionBoard
{
    using Microsoft.Maui.Devices;
    using Microsoft.Maui.Layouts;
    using System.Text.Json;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using FRC.NetworkTables;
    using System.Diagnostics;
    using static MalfunctionBoard.MainPage;

    public partial class MainPage : ContentPage
    {
        readonly Grid MainGrid;
        readonly List<GridDisplay> GridDisplays = [];
        public readonly List<DisplayBinding> DisplayBindings = [];
        readonly List<Type> DisplayTypes = [];
        const int RowCount = 4;
        const int ColumnCount = 6;
        const double RowSpacing = 10;
        const double ColumnSpacing = 10;
        static readonly Thickness GridMargin = new(10);
        const double AddButtonSpacing = 0;
        static readonly Thickness AddButtonMargin = new(10);
        static readonly Color CellColor = Colors.Gray;
        static readonly Color GridColor = Colors.LightGray;
        static readonly Color PageColor = Colors.DarkGray;
        const string TableName = "datatable";

        public MainPage()
        {
            InitDisplayTypes();

            BackgroundColor = PageColor;

            Grid pageLayout = new()
            {
                RowDefinitions =
                {
                    new() { Height = new(1, GridUnitType.Star) },
                    new() { Height = new(10, GridUnitType.Star) }
                },
                ColumnDefinitions =
                {
                    new() { Width = new(1, GridUnitType.Star) }
                },
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill
            };

            HorizontalStackLayout topBar = new()
            {
                BackgroundColor = CellColor,
                Spacing = AddButtonSpacing,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill
            };

            foreach (var displayType in DisplayTypes)
            {
                var button = new DisplayAddButton(displayType, this)
                {
                    Margin = AddButtonMargin
                };

                topBar.Add(button);
            }

            MainGrid = new()
            {
                RowDefinitions = {},
                RowSpacing = RowSpacing,
                ColumnDefinitions = {},
                ColumnSpacing = ColumnSpacing,
                Margin = GridMargin,
                BackgroundColor = GridColor,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill
            };

            for (int i = 0; i < RowCount; i++)
            {
                MainGrid.RowDefinitions.Add(new()
                {
                    Height = new(1, GridUnitType.Star)
                });
            }
            for (int i = 0; i < ColumnCount; i++)
            {
                MainGrid.ColumnDefinitions.Add(new()
                {
                    Width = new(1, GridUnitType.Star)
                });
            }

            pageLayout.Add(topBar, 0, 0);
            pageLayout.Add(MainGrid, 0, 1);
            Content = pageLayout;

            Loaded += OnPageLoaded;
        }

        void OnPageLoaded(object? sender, EventArgs e)
        {
            if (Window != null)
            {
                var displayInfo = DeviceDisplay.Current.MainDisplayInfo;

                if (displayInfo.Density > 0)
                {
                    double screenWidth = displayInfo.Width / displayInfo.Density;
                    double screenHeight = displayInfo.Height / displayInfo.Density - 50;

                    (Window.X, Window.Y) = (0, 0);
                    (Window.Width, Window.Height) = (screenWidth, screenHeight);

                    #if MACCATALYST
                    (Window.MinimumWidth, Window.MaximumWidth) = (screenWidth, screenWidth);
                    (Window.MinimumHeight, Window.MaximumHeight) = (screenHeight, screenHeight);
                    #endif
                }
            }

            Saver.LoadLayout(this);

            var instance = NetworkTableInstance.Default;
            instance.StartServer();
            instance.GetTable(TableName).GetEntry("test").SetDouble(5.5);

            NetworkTableReader.StartReader(TableName, this);
            NetworkTableReader.WritingTester(TableName, "test");
        }

        public bool TryAddDisplay<T>(string title, string binding, GridPos position, GridDims dimensions)
            where T : DashboardDisplay, IHasGridDims, IHasGridPos, new()
        {
            T display = new() { MyPage = this, Title = title, Binding = binding };

            if (ValidBinding(binding) && ValidPosition(dimensions, position, out var positions))
            {
                GridDisplays.AddRange(positions.Select(p => new GridDisplay(p, display)));

                display.Position = positions[0];
                display.Dimensions = dimensions;
                MainGrid.SetRow(display, positions[0].Row);
                MainGrid.SetRowSpan(display, dimensions.Height);
                MainGrid.SetColumn(display, positions[0].Col);
                MainGrid.SetColumnSpan(display, dimensions.Width);

                DisplayBindings.Add(new(binding, display));

                MainGrid.Add(display);
                return true;
            }

            return false;
        }

        public bool TryChangeDisplay(DashboardDisplay display, string newTitle, string newBinding, GridPos newPos, GridDims newDims)
        {
            if (ValidBinding(newBinding, display) && ValidPosition(newDims, newPos, out var positions, display))
            {
                DisplayBindings.RemoveAll(b => b.Display == display);
                DisplayBindings.Add(new(newBinding, display));

                GridDisplays.RemoveAll(d => d.Display == display);
                GridDisplays.AddRange(positions.Select(p => new GridDisplay(p, display)));

                MainGrid.Remove(display);

                display.Title = newTitle;
                display.Binding = newBinding;
                display.Position = positions[0];
                display.Dimensions = newDims;
                MainGrid.SetRow(display, positions[0].Row);
                MainGrid.SetRowSpan(display, newDims.Height);
                MainGrid.SetColumn(display, positions[0].Col);
                MainGrid.SetColumnSpan(display, newDims.Width);

                MainGrid.Add(display);
                return true;
            }

            return false;
        }

        public void UpdateDisplay(string binding, object? data)
        {
            if (data is null) return;

            var display = DisplayBindings.Find(b => b.Binding == binding)?.Display;
            if (display is ITableValue valueDisplay) valueDisplay.TableValue = data;
        }

        public void RemoveDisplay(DashboardDisplay display)
        {
            DisplayBindings.RemoveAll(b => b.Display == display);
            GridDisplays.RemoveAll(d => d.Display == display);
            MainGrid.Remove(display);
        }

        public bool ValidPosition(GridDims dimensions, GridPos start, out List<GridPos> positions, DashboardDisplay? display = null)
        {
            var gridDisplays = GridDisplays.Where(d => d.Display != display);
            positions = [];

            for (int row = 0; row < dimensions.Height; row++)
            {
                for (int col = 0; col < dimensions.Width; col++)
                {
                    positions.Add(new() { Row = start.Row + row, Col = start.Col + col });
                }
            }

            foreach (var pos in positions)
            {
                if (gridDisplays.Any(d => d.Position == pos)) return false;
            }

            return true;
        }

        public bool ValidBinding(string binding, DashboardDisplay? display = null) => !DisplayBindings.Any(b => b.Binding == binding && b.Display != display);

        void InitDisplayTypes()
        {
            var displayTypes = typeof(DashboardDisplay).Assembly.GetTypes()
                .Where(t => t.IsSubclassOf(typeof(DashboardDisplay)) && t.IsAssignableTo(typeof(ICreatable)));

            foreach (var displayType in displayTypes)
            {
                DisplayTypes.Add(displayType);
            }
        }

        public record DisplayBinding(string Binding, DashboardDisplay Display);

        [Serializable]
        public record GridDisplay(GridPos Position, DashboardDisplay Display);

        public interface IHasVector2
        {
            int X { get; set; }
            int MinX { get; }
            int MaxX { get; }
            int Y { get; set; }
            int MinY { get; }
            int MaxY { get; }
        }

        [Serializable]
        public record GridPos : IHasVector2
        {
            public int X
            {
                get => Row;
                set => Row = value;
            }
            public int MinX { get => 0; }
            public int MaxX { get => RowCount - 1; }
            public int Y
            {
                get => Col;
                set => Col = value;
            }
            public int MinY { get => 0; }
            public int MaxY { get => ColumnCount - 1; }
            public int Row
            {
                get => _row;
                set => _row = Math.Clamp(value, MinX, MaxX);
            }
            int _row;
            public int Col
            {
                get => _col;
                set => _col = Math.Clamp(value, MinY, MaxY);
            }
            int _col;
        }

        public record GridDims() : IHasVector2
        {
            public int X
            {
                get => Width;
                set => Width = value;
            }
            public int MinX { get => 1; }
            public int MaxX { get => ColumnCount; }
            public int Width
            {
                get => _width;
                set => _width = Math.Clamp(value, MinX, MaxX);
            }
            int _width;
            public int Y
            {
                get => Height;
                set => Height = value;
            }
            public int MinY { get => 1; }
            public int MaxY { get => RowCount; }
            public int Height
            {
                get => _height;
                set => _height = Math.Clamp(value, MinY, MaxY);
            }
            int _height;
        }

        public interface IHasGridDims
        {
            GridDims Dimensions { get; set; }
        }

        public interface IHasGridPos
        {
            GridPos Position { get; set; }
        }

        public interface ICreatable { }

        public interface ITableValue
        {
            public object? TableValue { get; set; }
        }

        public partial class DashboardDisplay : ContentView, IHasGridDims, IHasGridPos
        {
            public required MainPage? MyPage { get; set; }
            public GridPos Position { get; set; } = new() { Row = 0, Col = 0 };
            public virtual GridDims Dimensions { get; set; } = new() { Width = 1, Height = 1 };
            public string Binding { get; set; } = string.Empty;
            public string Title
            {
                get => ((string)GetValue(TitleProperty))[..^1];
                set => SetValue(TitleProperty, value + ":");
            }
            public static readonly BindableProperty TitleProperty =
                BindableProperty.Create(nameof(Title), typeof(string), typeof(DashboardDisplay), string.Empty);
            public double TitleSize
            {
                get => (double)GetValue(TitleSizeProperty);
                set => SetValue(TitleSizeProperty, value);
            }
            public static readonly BindableProperty TitleSizeProperty =
                BindableProperty.Create(nameof(TitleSize), typeof(double), typeof(DashboardDisplay), 20.0);
            public Color TextColor
            {
                get => (Color)GetValue(TextColorProperty);
                set => SetValue(TextColorProperty, value);
            }
            public static readonly BindableProperty TextColorProperty =
                BindableProperty.Create(nameof(TextColor), typeof(Color), typeof(DashboardDisplay), Colors.Black);
            protected AbsoluteLayout MyLayout;

            public DashboardDisplay()
            {
                BackgroundColor = CellColor;
                Title = "New Display";

                PointerGestureRecognizer clickGesture = new();
                clickGesture.PointerPressed += (_, _) => ChangeDisplay();
                GestureRecognizers.Add(clickGesture);

                HorizontalOptions = LayoutOptions.Fill;
                VerticalOptions = LayoutOptions.Fill;

                var title = new Label();
                title.SetBinding(Label.TextProperty, new Binding(nameof(Title), source: this));
                title.SetBinding(Label.FontSizeProperty, new Binding(nameof(TitleSize), source: this));
                title.SetBinding(Label.TextColorProperty, new Binding(nameof(TextColor), source: this));

                MyLayout = [];

                MyLayout.SetLayoutBounds(title, new(0.5, 0, -1, -1));
                MyLayout.SetLayoutFlags(title, AbsoluteLayoutFlags.PositionProportional);

                MyLayout.Children.Add(title);

                Content = MyLayout;
            }

            void ChangeDisplay()
            {
                if (MyPage is not null) PropertiesPage.OpenPropertiesPage(GetType(), MyPage, this);
            }
        }

        public partial class ValueDisplay<T> : DashboardDisplay, ITableValue
        {
            public object? TableValue
            {
                get => Value;
                set
                {
                    if (value is not null)
                    {
                        Value = (T)Convert.ChangeType(value, typeof(T));
                    }
                }
            }
            public T Value
            {
                get => (T)GetValue(ValueProperty);
                set => SetValue(ValueProperty, value);
            }
            public static readonly BindableProperty ValueProperty =
                BindableProperty.Create(nameof(Value), typeof(T), typeof(ValueDisplay<>), default(T));
            public double ValueSize
            {
                get => (double)GetValue(ValueSizeProperty);
                set => SetValue(ValueSizeProperty, value);
            }
            public static readonly BindableProperty ValueSizeProperty =
                BindableProperty.Create(nameof(ValueSize), typeof(double), typeof(ValueDisplay<>), 30.0);

            public ValueDisplay()
            {
                var valueLabel = new Label();
                valueLabel.SetBinding(Label.TextProperty, new Binding(nameof(Value), source: this));
                valueLabel.SetBinding(Label.FontSizeProperty, new Binding(nameof(ValueSize), source: this));
                valueLabel.SetBinding(Label.TextColorProperty, new Binding(nameof(TextColor), source: this));
                (valueLabel.AnchorX, valueLabel.AnchorY) = (0.5, 0.5);

                MyLayout.SetLayoutBounds(valueLabel, new(0.5, 0.6, -1, -1));
                MyLayout.SetLayoutFlags(valueLabel, AbsoluteLayoutFlags.PositionProportional);

                MyLayout.Children.Add(valueLabel);
            }
        }

        public partial class IntDisplay : ValueDisplay<int>, ICreatable { }

        public partial class DoubleDisplay : ValueDisplay<double>, ICreatable { }

        public partial class StringDisplay : ValueDisplay<string>, ICreatable { }

        public partial class DisplayAddButton : Button
        {
            readonly Type DisplayType;
            readonly MainPage MyPage;

            public DisplayAddButton(Type displayType, MainPage page)
            {
                Text = $"Add {displayType.Name}";
                DisplayType = displayType;
                MyPage = page;
                Clicked += (_, _) => OpenProperties();
            }

            void OpenProperties() => PropertiesPage.OpenPropertiesPage(DisplayType, MyPage);
        }
    }

    public partial class PropertiesPage : ContentPage
    {
        const double WindowWidth = 500;
        const double WindowHeight = 400;
        const double VerticalSpacing = 10;
        const double ButtonSpacing = 10;
        const double TextSize = 20;
        static readonly Thickness Margin = new(10);
        static readonly Color WindowColor = Colors.DarkGray;
        static readonly Color ConfirmColor = Colors.Green;
        static readonly Color ConfirmHoverColor = Colors.DarkGreen;
        static readonly Color CancelColor = Colors.SlateGray;
        static readonly Color CancelHoverColor = Colors.DarkSlateGray;
        static readonly Color DeleteColor = Colors.Red;
        static readonly Color DeleteHoverColor = Colors.DarkRed;
        public string DisplayTitle
        {
            get => (string)GetValue(DisplayTitleProperty);
            set => SetValue(DisplayTitleProperty, value);
        }
        public static readonly BindableProperty DisplayTitleProperty =
            BindableProperty.Create(nameof(DisplayTitle), typeof(string), typeof(PropertiesPage), string.Empty);
        public string DisplayBinding
        {
            get => (string)GetValue(DisplayBindingProperty);
            set => SetValue(DisplayBindingProperty, value);
        }
        public static readonly BindableProperty DisplayBindingProperty =
            BindableProperty.Create(nameof(DisplayBinding), typeof(string), typeof(PropertiesPage), string.Empty);
        public GridPos DisplayPosition
        {
            get => (GridPos)GetValue(DisplayPositionProperty);
            set => SetValue(DisplayPositionProperty, value);
        }
        public static readonly BindableProperty DisplayPositionProperty =
            BindableProperty.Create(nameof(DisplayPosition), typeof(GridPos), typeof(PropertiesPage), new GridPos());
        public GridDims DisplayDimensions
        {
            get => (GridDims)GetValue(DisplayDimensionsProperty);
            set => SetValue(DisplayDimensionsProperty, value);
        }
        public static readonly BindableProperty DisplayDimensionsProperty =
            BindableProperty.Create(nameof(DisplayDimensions), typeof(GridDims), typeof(PropertiesPage), new GridDims());
        readonly MainPage MainPage;
        readonly DashboardDisplay? ShownDisplay;
        readonly MethodInfo? AddDisplayMethod;

        public static void OpenPropertiesPage(Type displayType, MainPage mainPage, DashboardDisplay? display = null)
        {
            var propertiesWindow = new Window(new PropertiesPage(displayType, mainPage, display));
            Application.Current?.OpenWindow(propertiesWindow);

            WindowUtils.MakeModalWindow(propertiesWindow, mainPage.Window);
        }

        public PropertiesPage(Type displayType, MainPage mainPage, DashboardDisplay? display = null)
        {
            Title = "Display Properties";
            BackgroundColor = WindowColor;
            MainPage = mainPage;
            ShownDisplay = display;

            VerticalStackLayout pageLayout = new()
            {
                Spacing = VerticalSpacing,
                Margin = Margin,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill
            };

            var typeLabel = new Label()
            {
                Text = $"Type: {displayType.Name}",
                FontSize = TextSize,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill
            };
            pageLayout.Add(typeLabel);

            var titleEntry = new InputField()
            {
                Title = "Name",
                FirstPlaceholder = "Enter name..."
            };
            titleEntry.SetBinding(InputField.FirstInputProperty, new Binding(nameof(DisplayTitle),
                mode: BindingMode.TwoWay, source: this));
            if (ShownDisplay is not null) titleEntry.FirstInput = ShownDisplay.Title;
            pageLayout.Add(titleEntry);

            var bindingEntry = new InputField()
            {
                Title = "Binding",
                FirstPlaceholder = "Enter binding..."
            };
            bindingEntry.SetBinding(InputField.FirstInputProperty, new Binding(nameof(DisplayBinding),
                mode: BindingMode.TwoWay, source: this));
            if (ShownDisplay is not null) bindingEntry.FirstInput = ShownDisplay.Binding;
            pageLayout.Add(bindingEntry);

            var posEntry = new Vector2InputField<GridPos>()
            {
                Title = "Position",
                FirstPlaceholder = "Enter row...",
                SecondPlaceholder = "Enter column..."
            };
            posEntry.SetBinding(Vector2InputField<GridPos>.VectorProperty, new Binding(nameof(DisplayPosition),
                mode: BindingMode.TwoWay, source: this));
            (posEntry.FirstInput, posEntry.SecondInput) = ShownDisplay is null ? ("0", "0") : (ShownDisplay.Position.X.ToString(), ShownDisplay.Position.Y.ToString());
            pageLayout.Add(posEntry);

            var dimsEntry = new Vector2InputField<GridDims>()
            {
                Title = "Size",
                FirstPlaceholder = "Enter width...",
                SecondPlaceholder = "Enter height..."
            };
            dimsEntry.SetBinding(Vector2InputField<GridDims>.VectorProperty, new Binding(nameof(DisplayDimensions),
                mode: BindingMode.TwoWay, source: this));
            (dimsEntry.FirstInput, dimsEntry.SecondInput) = ShownDisplay is null ? ("1", "1") : (ShownDisplay.Dimensions.Width.ToString(), ShownDisplay.Dimensions.Height.ToString());
            pageLayout.Add(dimsEntry);

            HorizontalStackLayout buttonLayout = new()
            {
                Spacing = ButtonSpacing,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Fill
            };

            if (ShownDisplay is not null)
            {
                Button deleteButton = new()
                {
                    Text = "Delete",
                    FontSize = TextSize,
                    BackgroundColor = DeleteColor,
                    HorizontalOptions = LayoutOptions.Fill,
                    VerticalOptions = LayoutOptions.Fill
                };

                PointerGestureRecognizer deleteGesture = new();
                deleteGesture.PointerEntered += (_, _) => deleteButton.BackgroundColor = DeleteHoverColor;
                deleteGesture.PointerExited += (_, _) => deleteButton.BackgroundColor = DeleteColor;

                deleteButton.GestureRecognizers.Add(deleteGesture);
                deleteButton.Clicked += (_, _) => Delete();

                buttonLayout.Add(deleteButton);
            }

            Button cancelButton = new()
            {
                Text = "Cancel",
                FontSize = TextSize,
                BackgroundColor = CancelColor,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill
            };

            PointerGestureRecognizer cancelGesture = new();
            cancelGesture.PointerEntered += (_, _) => cancelButton.BackgroundColor = CancelHoverColor;
            cancelGesture.PointerExited += (_, _) => cancelButton.BackgroundColor = CancelColor;

            cancelButton.GestureRecognizers.Add(cancelGesture);
            cancelButton.Clicked += (_, _) => Close();

            buttonLayout.Add(cancelButton);

            Button confirmButton = new()
            {
                Text = "Confirm",
                FontSize = TextSize,
                BackgroundColor = ConfirmColor,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill
            };

            PointerGestureRecognizer confirmGesture = new();
            confirmGesture.PointerEntered += (_, _) => confirmButton.BackgroundColor = ConfirmHoverColor;
            confirmGesture.PointerExited += (_, _) => confirmButton.BackgroundColor = ConfirmColor;

            confirmButton.GestureRecognizers.Add(confirmGesture);
            confirmButton.Clicked += (_, _) => Confirm();

            buttonLayout.Add(confirmButton);

            pageLayout.Add(buttonLayout);

            Content = pageLayout;

            var tryAddInfo = typeof(MainPage).GetMethod("TryAddDisplay");
            AddDisplayMethod = tryAddInfo?.MakeGenericMethod(displayType);

            Loaded += OnPageLoaded;
        }

        void OnPageLoaded(object? sender, EventArgs e)
        {
            Window.Title = Title;
            (Window.Width, Window.Height) = (WindowWidth, WindowHeight);

            #if MACCATALYST
            (Window.MinimumWidth, Window.MaximumWidth) = (WindowWidth, WindowWidth);
            (Window.MinimumHeight, Window.MaximumHeight) = (WindowHeight, WindowHeight);
            #endif

            var displayInfo = DeviceDisplay.Current.MainDisplayInfo;

            if (displayInfo.Density > 0)
            {
                double screenWidth = displayInfo.Width / displayInfo.Density;
                double screenHeight = displayInfo.Height / displayInfo.Density;

                Window.X = (screenWidth / 2) - (WindowWidth / 2);
                Window.Y = (screenHeight / 2) - (WindowHeight / 2);
            }
        }

        void Confirm()
        {
            dynamic? success = false;
            if (!string.IsNullOrEmpty(DisplayTitle) && !string.IsNullOrEmpty(DisplayBinding) && DisplayPosition is not null && DisplayDimensions is not null)
            {
                if (ShownDisplay is null) success = AddDisplayMethod?.Invoke(MainPage, [DisplayTitle, DisplayBinding, DisplayPosition, DisplayDimensions]);
                else success = MainPage.TryChangeDisplay(ShownDisplay, DisplayTitle, DisplayBinding, DisplayPosition, DisplayDimensions);
            }

            if (success is not null && success)
            {
                Saver.SaveLayout(MainPage);
                Close();
            }
            else
            {
                string warning = string.Empty;

                if (string.IsNullOrEmpty(DisplayTitle)) warning = "Missing Display Title";
                else if (string.IsNullOrEmpty(DisplayBinding)) warning = "Missing Display Binding";
                else if (!MainPage.ValidBinding(DisplayBinding, ShownDisplay)) warning = "Invalid Binding - binding already exists";
                else if (DisplayPosition is null) warning = "Missing Display Position";
                else if (DisplayDimensions is null) warning = "Missing Display Dimensions";
                else if (!MainPage.ValidPosition(DisplayDimensions, DisplayPosition, out _, ShownDisplay)) warning = "Invalid Position - position already occupied";

                WarningPage.ShowWarning(warning, Window);
            }
        }

        void Delete()
        {
            if (ShownDisplay is not null)
            {
                MainPage.RemoveDisplay(ShownDisplay);
                Saver.SaveLayout(MainPage);
            }
            Close();
        }

        void Close() => Application.Current?.CloseWindow(Window);

        public partial class WarningPage : ContentPage
        {
            const double WindowWidth = 400;
            const double WindowHeight = 200;
            const double WarningTextSize = 30;
            const double ButtonWidth = 100;

            public static void ShowWarning(string warning, Window propertiesWindow)
            {
                var warningWindow = new Window(new WarningPage(warning));
                Application.Current?.OpenWindow(warningWindow);

                WindowUtils.MakeModalWindow(warningWindow, propertiesWindow);
            }

            public WarningPage(string warning)
            {
                Title = "Warning";
                BackgroundColor = WindowColor;

                VerticalStackLayout pageLayout = new()
                {
                    Spacing = VerticalSpacing,
                    Margin = Margin,
                    HorizontalOptions = LayoutOptions.Fill,
                    VerticalOptions = LayoutOptions.Center
                };

                var warningLabel = new Label()
                {
                    Text = warning,
                    FontSize = WarningTextSize,
                    HorizontalTextAlignment = TextAlignment.Center,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Fill
                };
                pageLayout.Add(warningLabel);

                Button closeButton = new()
                {
                    Text = "Ok",
                    FontSize = TextSize,
                    BackgroundColor = ConfirmColor,
                    HorizontalOptions = LayoutOptions.Center,
                    WidthRequest = ButtonWidth,
                    VerticalOptions = LayoutOptions.Fill
                };

                PointerGestureRecognizer closeGesture = new();
                closeGesture.PointerEntered += (_, _) => closeButton.BackgroundColor = ConfirmHoverColor;
                closeGesture.PointerExited += (_, _) => closeButton.BackgroundColor = ConfirmColor;

                closeButton.GestureRecognizers.Add(closeGesture);
                closeButton.Clicked += (_, _) => Close();

                pageLayout.Add(closeButton);

                Content = pageLayout;

                Loaded += OnPageLoaded;
            }

            void OnPageLoaded(object? sender, EventArgs e)
            {
                Window.Title = Title;
                (Window.Width, Window.Height) = (WindowWidth, WindowHeight);

                #if MACCATALYST
                (Window.MinimumWidth, Window.MaximumWidth) = (WindowWidth, WindowWidth);
                (Window.MinimumHeight, Window.MaximumHeight) = (WindowHeight, WindowHeight);
                #endif

                var displayInfo = DeviceDisplay.Current.MainDisplayInfo;

                if (displayInfo.Density > 0)
                {
                    double screenWidth = displayInfo.Width / displayInfo.Density;
                    double screenHeight = displayInfo.Height / displayInfo.Density;

                    Window.X = (screenWidth / 2) - (WindowWidth / 2);
                    Window.Y = (screenHeight / 2) - (WindowHeight / 2);
                }
            }

            void Close() => Application.Current?.CloseWindow(Window);
        }

        public partial class InputField : ContentView
        {
            protected const double Spacing = 0;
            protected const double TextSize = 20;
            public string Title
            {
                get => (string)GetValue(TitleProperty);
                set => SetValue(TitleProperty, value + ":");
            }
            public static readonly BindableProperty TitleProperty =
                BindableProperty.Create(nameof(Title), typeof(string), typeof(InputField), "New Input Field");
            public string FirstPlaceholder
            {
                get => (string)GetValue(FirstPlaceholderProperty);
                set => SetValue(FirstPlaceholderProperty, value);
            }
            public static readonly BindableProperty FirstPlaceholderProperty =
                BindableProperty.Create(nameof(FirstPlaceholder), typeof(string), typeof(InputField), "Enter Input...");
            public virtual string FirstInput
            {
                get => (string)GetValue(FirstInputProperty);
                set => SetValue(FirstInputProperty, value);
            }
            public static readonly BindableProperty FirstInputProperty =
                BindableProperty.Create(nameof(FirstInput), typeof(string), typeof(InputField), string.Empty);

            public InputField()
            {
                var fieldLayout = InitGrid();

                var titleLabel = new Label()
                {
                    FontSize = TextSize
                };
                titleLabel.SetBinding(Label.TextProperty, new Binding(nameof(Title), source: this));
                fieldLayout.Add(titleLabel, 0, 0);

                InitEntries(ref fieldLayout);

                Content = fieldLayout;
            }

            protected virtual Grid InitGrid()
            {
                return new()
                {
                    RowDefinitions =
                    {
                        new(new(1, GridUnitType.Star))
                    },
                    ColumnDefinitions =
                    {
                        new(new(1, GridUnitType.Star)),
                        new(new(3, GridUnitType.Star))
                    },
                    ColumnSpacing = Spacing,
                    HorizontalOptions = LayoutOptions.Fill,
                    VerticalOptions = LayoutOptions.Fill
                };
            }

            protected virtual void InitEntries(ref Grid layout)
            {
                var entry = new Entry()
                {
                    FontSize = TextSize,
                };
                entry.TextChanged += OnInputChanged;
                entry.SetBinding(Entry.TextProperty, new Binding(nameof(FirstInput), source: this));
                entry.SetBinding(Entry.PlaceholderProperty, new Binding(nameof(FirstPlaceholder), source: this));
                layout.Add(entry, 1, 0);
            }

            protected virtual void OnInputChanged(object? sender, TextChangedEventArgs e) { }
        }

        public partial class Vector2InputField<T> : InputField where T : IHasVector2, new()
        {
            public string SecondInput
            {
                get => (string)GetValue(SecondInputProperty);
                set => SetValue(SecondInputProperty, value);
            }
            public static readonly BindableProperty SecondInputProperty =
                BindableProperty.Create(nameof(SecondInput), typeof(string), typeof(Vector2InputField<>), string.Empty);
            public string SecondPlaceholder
            {
                get => (string)GetValue(SecondPlaceholderProperty);
                set => SetValue(SecondPlaceholderProperty, value);
            }
            public static readonly BindableProperty SecondPlaceholderProperty =
                BindableProperty.Create(nameof(SecondPlaceholder), typeof(string), typeof(Vector2InputField<>), "Enter input...");
            public T Vector
            {
                get => (T)GetValue(VectorProperty);
                set => SetValue(VectorProperty, value);
            }
            public static readonly BindableProperty VectorProperty =
                BindableProperty.Create(nameof(Vector), typeof(T), typeof(Vector2InputField<>), new T(),
                    propertyChanged: OnVectorChanged);

            protected override Grid InitGrid()
            {
                return new()
                {
                    RowDefinitions =
                    {
                        new(new(1, GridUnitType.Star))
                    },
                    ColumnDefinitions =
                    {
                        new(new(1, GridUnitType.Star)),
                        new(new(1.5, GridUnitType.Star)),
                        new(new(1.5, GridUnitType.Star))
                    },
                    ColumnSpacing = Spacing,
                    HorizontalOptions = LayoutOptions.Fill,
                    VerticalOptions = LayoutOptions.Fill
                };
            }

            protected override void InitEntries(ref Grid layout)
            {
                base.InitEntries(ref layout);

                var entry = new Entry()
                {
                    FontSize = TextSize
                };
                entry.TextChanged += OnInputChanged;
                entry.SetBinding(Entry.TextProperty, new Binding(nameof(SecondInput), source: this));
                entry.SetBinding(Entry.PlaceholderProperty, new Binding(nameof(SecondPlaceholder), source: this));
                layout.Add(entry, 2, 0);
            }

            protected override void OnInputChanged(object? sender, TextChangedEventArgs e)
            {
                if (!string.IsNullOrEmpty(e.NewTextValue) && !int.TryParse(e.NewTextValue, out int newVal) && sender != null)
                {
                    ((Entry)sender).Text = e.OldTextValue;
                }
            }

            protected override void OnPropertyChanged(string? propertyName = null)
            {
                base.OnPropertyChanged(propertyName);

                switch (propertyName)
                {
                    case nameof(FirstInput):
                        if (int.TryParse(FirstInput, out int x) && x != Vector.X)
                        {
                            FirstInput = Math.Clamp(x, Vector.MinX, Vector.MaxX).ToString();
                            Vector = new() { X = x, Y = Vector.Y };
                        }
                        break;
                    case nameof(SecondInput):
                        if (int.TryParse(SecondInput, out int y) && y != Vector.Y)
                        {
                            SecondInput = Math.Clamp(y, Vector.MinY, Vector.MaxY).ToString();
                            Vector = new() { X = Vector.X, Y = y };
                        }
                        break;
                }
            }

            static void OnVectorChanged(BindableObject bindable, object oldValue, object newValue)
            {
                if (bindable is Vector2InputField<T> control)
                {
                    var oldVector = (T)oldValue;
                    var newVector = (T)newValue;

                    if (newVector.X != oldVector.X) control.FirstInput = newVector.X.ToString();
                    if (newVector.Y != oldVector.Y) control.SecondInput = newVector.Y.ToString();
                }
            }
        }
    }

    public static class WindowUtils
    {
        public static void MakeModalWindow(Window subWindow, Window mainWindow)
        {
            #if WINDOWS
            var main = mainWindow.Handler.PlatformView as Microsoft.UI.Xaml.Window;
            var sub = subWindow.Handler.PlatformView as Microsoft.UI.Xaml.Window;

            if (main != null && sub != null)
            {
                IntPtr mainHwnd = WinRT.Interop.WindowNative.GetWindowHandle(main);
                IntPtr subHwnd = WinRT.Interop.WindowNative.GetWindowHandle(sub);

                const int GWL_HWNDPARENT = -8;
                if (IntPtr.Size == 8)
                {
                    SetWindowLongPtr64(subHwnd, GWL_HWNDPARENT, mainHwnd);
                }
                else
                {
                    SetWindowLong32(subHwnd, GWL_HWNDPARENT, mainHwnd.ToInt32());
                }

                var subAppWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(Microsoft.UI.Win32Interop.GetWindowIdFromWindow(subHwnd));
                var presenter = Microsoft.UI.Windowing.OverlappedPresenter.CreateForDialog();
                presenter.IsModal = true;
                subAppWindow.SetPresenter(presenter);

                sub.Closed += (_, _) => SetForegroundWindow(mainHwnd);
            }
            #endif
        }

        #if WINDOWS
        [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
        static extern int SetWindowLong32(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
        static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetForegroundWindow(IntPtr hWnd);
        #endif
    }

    public static class Saver
    {
        const string FileName = "Layout.mb";
        static readonly JsonSerializerOptions SerializerOptions = new() { WriteIndented = true };

        public static void SaveLayout(MainPage mainPage)
        {
            var displays = mainPage.DisplayBindings.Select(b => b.Display).ToList();

            List<DisplayData> displayData = [];
            foreach (var display in displays)
            {
                displayData.Add(new(display.GetType().AssemblyQualifiedName, display.Title, display.Binding, display.Position, display.Dimensions));
            }

            LayoutData layoutData = new(displayData);
            string jsonData = JsonSerializer.Serialize(layoutData, SerializerOptions);
            File.WriteAllText(FileName, jsonData);
        }

        public static void LoadLayout(MainPage mainPage)
        {
            if (!File.Exists(FileName)) return;

            string jsonData = File.ReadAllText(FileName);
            var layoutData = JsonSerializer.Deserialize<LayoutData>(jsonData);

            if (layoutData is not null)
            {
                foreach (var display in layoutData.Displays)
                {
                    if (display.DisplayType is null) continue;

                    var displayType = Type.GetType(display.DisplayType);
                    if (displayType is null) continue;

                    var tryAddInfo = typeof(MainPage).GetMethod("TryAddDisplay");
                    var addDisplayMethod = tryAddInfo?.MakeGenericMethod(displayType);
                    if (addDisplayMethod is null) continue;

                    addDisplayMethod.Invoke(mainPage, [display.Title, display.Binding, display.Position, display.Dimensions]);
                }
            }
        }

        [Serializable]
        public record LayoutData(List<DisplayData> Displays);

        [Serializable]
        public record DisplayData(string? DisplayType, string Title, string Binding, GridPos Position, GridDims Dimensions);
    }

    public static class NetworkTableReader
    {
        static readonly Random random = new();

        public static void StartReader(string tableName, MainPage mainPage)
        {
            var table = NetworkTableInstance.Default.GetTable(tableName);

            table.AddEntryListener((tbl, key, in entry, in value, flags) =>
            {
                var binding = key.ToString();
                var data = entry.GetObjectValue();

                MainThread.BeginInvokeOnMainThread(() =>
                    mainPage.UpdateDisplay(binding, data));
            },
            NotifyFlags.Immediate | NotifyFlags.New | NotifyFlags.Update | NotifyFlags.Local);
        }

        public static async void WritingTester(string tableName, string binding)
        {
            var table = NetworkTableInstance.Default.GetTable(tableName);
            while (true)
            {
                double testValue = random.NextDouble();
                table.GetEntry(binding).SetDouble(testValue);
                await Task.Delay(500);
            }
        }
    }
}
