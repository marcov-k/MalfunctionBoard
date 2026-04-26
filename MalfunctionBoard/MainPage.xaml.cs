namespace MalfunctionBoard
{
    using Microsoft.Maui.Layouts;
    using Microsoft.Maui.Devices;
    using System.Reflection;

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

        private void OnPageLoaded(object? sender, EventArgs e)
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
                        GridPos start = new(row, col);

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
                    positions.Add(new(start.Row + row, start.Col + col));
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

        public record GridPos(int Row, int Col)
        {
            public int Row { get; } = Math.Clamp(Row, 0, RowCount - 1);
            public int Col { get; } = Math.Clamp(Col, 0, ColumnCount - 1);
        }

        public record GridDims(int Width, int Height)
        {
            public int Width { get; } = Math.Clamp(Width, 1, ColumnCount);
            public int Height { get; } = Math.Clamp(Height, 1, RowCount);
        }

        public interface IHasGridDims
        {
            GridDims Dimensions { get; }
        }

        public interface ICreatable { }

        public partial class DashboardDisplay : ContentView, IHasGridDims
        {
            public GridPos Position = new(0, 0);
            public virtual GridDims Dimensions => new(1, 1);
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

        public partial class DoubleDisplay : ValueDisplay<double>, ICreatable { public override GridDims Dimensions => new(2, 1); }

        public partial class StringDisplay : ValueDisplay<string>, ICreatable { }

        public partial class DisplayAddButton : Button
        {
            public DisplayAddButton(Type displayType, MainPage page)
            {
                Text = $"Add {displayType.Name}";
                Clicked += (_, _) => Application.Current?.OpenWindow(new(new PropertiesPage(displayType)));
            }
        }
    }

    public partial class PropertiesPage : ContentPage
    {
        const double WindowWidth = 400;
        const double WindowHeight = 800;

        public PropertiesPage(Type displayType)
        {
            Title = "Display Properties";

            Loaded += OnPageLoaded;
        }

        private void OnPageLoaded(object? sender, EventArgs e)
        {
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
    }
}
