namespace MalfunctionBoard.InputFields
{
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
}
