using MalfunctionBoard.Interfaces;

namespace MalfunctionBoard.Displays
{
    public partial class DoubleDisplay : ValueLabelDisplay<double>, ICreatable
    {
        protected override void UpdateValue(object? newValue)
        {
            if (newValue is double doubleValue) Value = doubleValue;
        }

        protected override void UpdateDisplayedValue(object newValue)
        {
            if (newValue is double doubleValue) ValueLabel.Text = doubleValue.ToString();
        }
    }
}
