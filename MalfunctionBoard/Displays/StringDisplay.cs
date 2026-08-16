using MalfunctionBoard.Interfaces;

namespace MalfunctionBoard.Displays
{
    public partial class StringDisplay : ValueLabelDisplay<string>, ICreatable
    {
        protected override void UpdateValue(object? newValue)
        {
            if (newValue is string stringValue) Value = stringValue;
        }

        protected override void UpdateDisplayedValue(object newValue)
        {
            if (newValue is string stringValue) ValueLabel.Text = stringValue;
        }
    }
}
