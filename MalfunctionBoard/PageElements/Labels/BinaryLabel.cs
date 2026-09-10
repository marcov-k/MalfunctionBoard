namespace MalfunctionBoard.PageElements.Labels
{
    public partial class BinaryLabel : ContentView
    {
        public bool Value
        {
            get => (bool)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }
        public static readonly BindableProperty ValueProperty =
            BindableProperty.Create(nameof(Value), typeof(bool), typeof(BinaryLabel), false, propertyChanged: OnValueChanged);

        public string TrueLabel { get; set { field = value; UpdateDisplay(); } } = string.Empty;
        public Color TrueColor { get; set { field = value; UpdateDisplay(); } } = Colors.Red;
        public string FalseLabel { get; set { field = value; UpdateDisplay(); } } = string.Empty;
        public Color FalseColor { get; set { field = value; UpdateDisplay(); } } = Colors.Green;
        public double FontSize
        {
            get => (double)GetValue(FontSizeProperty);
            set => SetValue(FontSizeProperty, value);
        }
        public static readonly BindableProperty FontSizeProperty =
            BindableProperty.Create(nameof(FontSize), typeof(double), typeof(BinaryLabel), 20.0);

        readonly Label DisplayLabel;

        public BinaryLabel()
        {
            DisplayLabel = new Label()
            {
                FontSize = FontSize,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };
            DisplayLabel.SetBinding(Label.FontSizeProperty, new Binding(nameof(FontSize), source: this));

            Content = DisplayLabel;

            UpdateDisplay();
        }

        static void OnValueChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is BinaryLabel binaryLabel)
            {
                binaryLabel.UpdateDisplay();
            }
        }

        void UpdateDisplay() => (DisplayLabel.Text, DisplayLabel.TextColor) = Value ? (TrueLabel, TrueColor) : (FalseLabel, FalseColor);
    }
}
