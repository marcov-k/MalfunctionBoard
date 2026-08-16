using MalfunctionBoard.Interfaces;

namespace MalfunctionBoard.Displays
{
    public partial class IntDisplay : ValueLabelDisplay<int>, ICreatable
    {
        protected override void UpdateValue(object? newValue)
        {
            if (newValue is int intValue) Value = intValue;
        }

        protected override void UpdateDisplayedValue(object newValue)
        {
            if (newValue is int intValue) ValueLabel.Text = intValue.ToString();
        }
    }
}
