namespace MalfunctionBoard
{
    using Microsoft.Maui.Layouts;

    public partial class MainPage : ContentPage
    {
        readonly Grid MainGrid;
        readonly Dictionary<GridPos, DashboardDisplay> GridDisplays = [];
        readonly Dictionary<string, DashboardDisplay> DisplayBindings = [];
        const int RowCount = 4;
        const int ColumnCount = 6;
        const double RowSpacing = 10;
        const double ColumnSpacing = 10;
        readonly Thickness Margin = new(10);
        readonly Color CellColor = Colors.DarkGray;
        const double TitleSize = 20;
        const double ValueSize = 30;

        public MainPage()
        {
            MainGrid = new Grid()
            {
                RowDefinitions = {},
                RowSpacing = RowSpacing,
                ColumnDefinitions = {},
                ColumnSpacing = ColumnSpacing,
                Margin = Margin
            };

            for (int i = 0; i < RowCount; i++)
            {
                MainGrid.RowDefinitions.Add(new()
                {
                    Height = new GridLength(1, GridUnitType.Star)
                });
            }
            for (int i = 0; i < ColumnCount; i++)
            {
                MainGrid.ColumnDefinitions.Add(new()
                {
                    Width = new GridLength(1, GridUnitType.Star)
                });
            }

            Content = MainGrid;

            if (AddDisplay<DoubleDisplay>("test1", out var display1))
            {
                display1.Title = "Double Display 1";
                display1.TitleSize = TitleSize;
                display1.Value = 20.0;
                display1.ValueSize = ValueSize;
                display1.BackgroundColor = CellColor;
            }

            if (AddDisplay<StringDisplay>("test2", out var display2, new(6, 3)))
            {
                display2.Title = "String Display 1";
                display2.TitleSize = TitleSize;
                display2.Value = "Test String";
                display2.ValueSize = ValueSize;
                display2.BackgroundColor = CellColor;
            }
        }

        public bool AddDisplay<T>(string binding, out T display, GridDims? dimensions = null) where T : DashboardDisplay, IHasGridDims, new()
        {
            display = new();
            dimensions ??= display.Dimensions;
            if (!DisplayBindings.ContainsKey(binding) && FindOpenPosition(dimensions.Width, dimensions.Height, out var positions))
            {
                foreach (var pos in positions)
                {
                    GridDisplays.Add(pos, display);
                }
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

        bool FindOpenPosition(int width, int height, out List<GridPos> positions)
        {
            if (GridDisplays.Count != RowCount * ColumnCount)
            {
                for (int row = 0; row < RowCount - (height - 1); row++)
                {
                    for (int col = 0; col < ColumnCount - (width - 1); col++)
                    {
                        GridPos start = new(row, col);

                        if (ValidPosition(width, height, start, out positions)) return true;
                    }
                }
            }

            positions = [];
            return false;
        }

        bool ValidPosition(int width, int height, GridPos start, out List<GridPos> positions)
        {
            positions = [];
            for (int row = 0; row < height; row++)
            {
                for (int col = 0; col < width; col++)
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

        record GridPos(int Row, int Col)
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

        public partial class DashboardDisplay : ContentView, IHasGridDims
        {
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

        public partial class IntDisplay : ValueDisplay<int> { }

        public partial class DoubleDisplay : ValueDisplay<double> { public override GridDims Dimensions => new(2, 1); }
        public partial class StringDisplay : ValueDisplay<string> { }
    }
}
