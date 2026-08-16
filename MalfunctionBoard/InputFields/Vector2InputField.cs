using MalfunctionBoard.Interfaces;

namespace MalfunctionBoard.InputFields
{
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
