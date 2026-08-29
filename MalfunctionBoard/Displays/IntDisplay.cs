using MalfunctionBoard.Interfaces;

namespace MalfunctionBoard.Displays
{
    public partial class IntDisplay : ValueLabelDisplay<long>, ICreatable
    {
        protected override void UpdateValue(object? newValue)
        {
            if (newValue is long intValue) Value = intValue;
        }

        protected override void UpdateDisplayedValue(object newValue)
        {
            if (newValue is long intValue) ValueLabel.Text = intValue.ToString();
        }
    }
}
