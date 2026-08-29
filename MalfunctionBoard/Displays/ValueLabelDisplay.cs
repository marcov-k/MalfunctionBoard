using Microsoft.Maui.Layouts;

namespace MalfunctionBoard.Displays
{
    public abstract partial class ValueLabelDisplay<T> : ValueDisplay<T>
    {
        protected readonly Label ValueLabel;

        public ValueLabelDisplay()
        {
            ValueLabel = new Label();
            ValueLabel.SetBinding(Label.FontSizeProperty, new Binding(nameof(ValueSize), source: this));
            ValueLabel.SetBinding(Label.TextColorProperty, new Binding(nameof(TextColor), source: this));
            (ValueLabel.AnchorX, ValueLabel.AnchorY) = (0.5, 0.5);

            MyLayout.SetLayoutBounds(ValueLabel, new(0.5, 0.6, -1, -1));
            MyLayout.SetLayoutFlags(ValueLabel, AbsoluteLayoutFlags.PositionProportional);

            MyLayout.Children.Add(ValueLabel);
        }
    }
}
