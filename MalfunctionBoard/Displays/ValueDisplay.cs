using MalfunctionBoard.Interfaces;

namespace MalfunctionBoard.Displays
{
    public partial class ValueDisplay<T> : DashboardDisplay, ITableValue
    {
        public object? TableValue
        {
            get => Value;
            set => UpdateValue(value);
        }
        public T Value
        {
            get => (T)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }
        public static readonly BindableProperty ValueProperty =
            BindableProperty.Create(nameof(Value), typeof(T), typeof(ValueDisplay<>), default(T), propertyChanged: OnValueChanged);
        public double ValueSize
        {
            get => (double)GetValue(ValueSizeProperty);
            set => SetValue(ValueSizeProperty, value);
        }
        public static readonly BindableProperty ValueSizeProperty =
            BindableProperty.Create(nameof(ValueSize), typeof(double), typeof(ValueDisplay<>), 20.0);

        protected virtual void UpdateValue(object? newValue)
        {
            throw new NotImplementedException();
        }

        protected static void OnValueChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is ValueDisplay<T> control) control.UpdateDisplayedValue(newValue);
        }

        protected virtual void UpdateDisplayedValue(object newValue)
        {
            throw new NotImplementedException();
        }
    }
}
