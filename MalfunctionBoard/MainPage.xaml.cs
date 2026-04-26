namespace MalfunctionBoard
{
    using Microsoft.Maui.Devices;
    using Microsoft.Maui.Layouts;
    using System.Reflection;
    using static MalfunctionBoard.MainPage;

    public partial class MainPage : ContentPage
    {
        readonly Grid MainGrid;
        readonly Dictionary<GridPos, DashboardDisplay> GridDisplays = [];
        readonly Dictionary<string, DashboardDisplay> DisplayBindings = [];
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
        public Window? PropertiesWindow = null;

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
        }

        public bool TryAddDisplay<T>(string binding, out T display, GridDims? dimensions = null) where T : DashboardDisplay, IHasGridDims, new()
        {
            display = new();
            dimensions ??= display.Dimensions;

            if (!DisplayBindings.ContainsKey(binding) && FindOpenPosition(dimensions, out var positions))
            {
                foreach (var pos in positions)
                {
                    GridDisplays.Add(pos, display);
                }

                display.Position = positions[0];
                MainGrid.SetRow(display, positions[0].Row);
                MainGrid.SetRowSpan(display, dimensions.Height);
                MainGrid.SetColumn(display, positions[0].Col);
                MainGrid.SetColumnSpan(display, dimensions.Width);

                DisplayBindings.Add(binding, display);

                MainGrid.Add(display);
                return true;
            }

            return false;
        }

        bool FindOpenPosition(GridDims dimensions, out List<GridPos> positions)
        {
            if (GridDisplays.Count != RowCount * ColumnCount)
            {
                for (int row = 0; row < RowCount - (dimensions.Height - 1); row++)
                {
                    for (int col = 0; col < ColumnCount - (dimensions.Width - 1); col++)
                    {
                        GridPos start = new() { Row = row, Col = col };

                        if (ValidPosition(dimensions, start, out positions)) return true;
                    }
                }
            }

            positions = [];
            return false;
        }

        bool ValidPosition(GridDims dimensions, GridPos start, out List<GridPos> positions)
        {
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
                if (GridDisplays.ContainsKey(pos)) return false;
            }

            return true;
        }

        void InitDisplayTypes()
        {
            var displayTypes = typeof(DashboardDisplay).Assembly.GetTypes()
                .Where(t => t.IsSubclassOf(typeof(DashboardDisplay)) && t.IsAssignableTo(typeof(ICreatable)));

            foreach (var displayType in displayTypes)
            {
                DisplayTypes.Add(displayType);
            }
        }

        public interface IHasVector2
        {
            int X { get; set; }
            int Y { get; set; }
        }

        public record GridPos : IHasVector2
        {
            public int X
            {
                get => Row;
                set => Row = value;
            }
            public int Y
            {
                get => Col;
                set => Col = value;
            }
            public int Row
            {
                get => _row;
                set => _row = Math.Clamp(value, 0, RowCount - 1);
            }
            int _row;
            public int Col
            {
                get => _col;
                set => _col = Math.Clamp(value, 0, ColumnCount - 1);
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
            public int Width
            {
                get => _width;
                set => _width = Math.Clamp(value, 1, ColumnCount);
            }
            int _width;
            public int Y
            {
                get => Height;
                set => Height = value;
            }
            public int Height
            {
                get => _height;
                set => _height = Math.Clamp(value, 1, RowCount);
            }
            int _height;
        }

        public interface IHasGridDims
        {
            GridDims Dimensions { get; }
        }

        public interface ICreatable { }

        public partial class DashboardDisplay : ContentView, IHasGridDims
        {
            public GridPos Position = new() { Row = 0, Col = 0 };
            public virtual GridDims Dimensions => new() { Width = 1, Height = 1 };
            public string Title
            {
                get => (string)GetValue(TitleProperty);
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
        }

        public partial class ValueDisplay<T> : DashboardDisplay
        {
            public T Value
            {
                get => (T)GetValue(ValueProperty);
                set => SetValue(ValueProperty, value);
            }
            public static readonly BindableProperty ValueProperty =
                BindableProperty.Create(nameof(Value), typeof(T), typeof(ValueDisplay<>), null);
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

            void OpenProperties()
            {
                if (MyPage.PropertiesWindow != null) Application.Current?.CloseWindow(MyPage.PropertiesWindow);

                var window = new Window(new PropertiesPage(DisplayType));
                MyPage.PropertiesWindow = window;
                Application.Current?.OpenWindow(window);
            }
        }
    }

    public partial class PropertiesPage : ContentPage
    {
        const double WindowWidth = 500;
        const double WindowHeight = 400;
        const double VerticalSpacing = 10;
        const double TextSize = 20;
        static readonly Thickness Margin = new(10);
        static readonly Color WindowColor = Colors.DarkGray;
        public string DisplayTitle
        {
            get => (string)GetValue(DisplayTitleProperty);
            set => SetValue(DisplayTitleProperty, value);
        }
        public static readonly BindableProperty DisplayTitleProperty =
            BindableProperty.Create(nameof(DisplayTitle), typeof(string), typeof(PropertiesPage), "New Display");
        public string DisplayBinding
        {
            get => (string)GetValue(DisplayBindingProperty);
            set => SetValue(DisplayBindingProperty, value);
        }
        public static readonly BindableProperty DisplayBindingProperty =
            BindableProperty.Create(nameof(DisplayBinding), typeof(string), typeof(PropertiesPage), "New Binding");
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

        public PropertiesPage(Type displayType)
        {
            Title = "Display Properties";
            BackgroundColor = WindowColor;

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
            pageLayout.Add(titleEntry);

            var bindingEntry = new InputField()
            {
                Title = "Binding",
                FirstPlaceholder = "Enter binding..."
            };
            pageLayout.Add(bindingEntry);

            var posEntry = new Vector2InputField<GridPos>()
            {
                Title = "Position",
                FirstPlaceholder = "Enter x...",
                SecondPlaceholder = "Enter y..."
            };
            pageLayout.Add(posEntry);

            var dimsEntry = new Vector2InputField<GridDims>()
            {
                Title = "Size",
                FirstPlaceholder = "Enter width...",
                SecondPlaceholder = "Enter height..."
            };
            pageLayout.Add(dimsEntry);

            Content = pageLayout;

            Loaded += OnPageLoaded;
        }

        void OnPageLoaded(object? sender, EventArgs e)
        {
            Window.Title = Title;
            (Window.Width, Window.Height) = (WindowWidth, WindowHeight);

            #if MACCATALYST
            (Window.MinimumWidth, Window.MaximumHeight) = (WindowWidth, WindowWidth);
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
                if (!string.IsNullOrEmpty(e.NewTextValue) && !int.TryParse(e.NewTextValue, out _) && sender != null)
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
                        if (int.TryParse(FirstInput, out int x) &&
                            x != Vector.X) Vector = new() { X = x, Y = Vector.Y };
                        break;
                    case nameof(SecondInput):
                        if (int.TryParse(SecondInput, out int y) &&
                            y != Vector.Y) Vector = new() { X = Vector.X, Y = y };
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
}
