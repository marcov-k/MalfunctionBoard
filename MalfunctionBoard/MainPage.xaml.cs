namespace MalfunctionBoard
{
    using Microsoft.Maui.Layouts;

    public partial class MainPage : ContentPage
    {
        Grid MainGrid;

        public MainPage()
        {
            int rowCount = 4;
            int columnCount = 6;
            double rowSpacing = 10;
            double columnSpacing = 10;
            var margin = new Thickness(10);
            var cellColor = Colors.DarkGray;

            double titleSize = 20;
            double valueSize = 30;

            var rowDef = new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) };
            var colDef = new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) };

            MainGrid = new Grid()
            {
                RowDefinitions = {},
                RowSpacing = rowSpacing,
                ColumnDefinitions = {},
                ColumnSpacing = columnSpacing,
                Margin = margin
            };

            for (int i = 0; i < rowCount; i++)
            {
                MainGrid.RowDefinitions.Add(rowDef);
            }
            for (int i = 0; i < columnCount; i++)
            {
                MainGrid.ColumnDefinitions.Add(colDef);
            }

            for (int row = 0; row < MainGrid.RowDefinitions.Count; row++)
            {
                for (int col = 0; col < MainGrid.ColumnDefinitions.Count; col++)
                {
                    MainGrid.Add(new BoxView()
                    {
                        Color = cellColor
                    }, row, col);
                }
            }

            MainGrid.Add(new DoubleDisplay()
            {
                Title = "Double Display 1",
                TitleSize = titleSize,
                Value = 20.0,
                ValueSize = valueSize
            }, 0, 0);
            MainGrid.Add(new DoubleDisplay()
            {
                Title = "Double Display 2",
                TitleSize = titleSize,
                Value = 10.0,
                ValueSize = valueSize
            }, 1, 2);

            Content = MainGrid;
        }

        abstract partial class DashboardDisplay : ContentView
        {
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

        abstract partial class ValueDisplay<T> : DashboardDisplay
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

        partial class IntDisplay : ValueDisplay<int> { }

        partial class DoubleDisplay : ValueDisplay<double> { }

        partial class StringDisplay : ValueDisplay<string> { }
    }
}
