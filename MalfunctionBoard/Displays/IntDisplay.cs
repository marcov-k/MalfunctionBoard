using MalfunctionBoard.Interfaces;
using MalfunctionBoard.TableDatatypes;

namespace MalfunctionBoard.Displays
{
    public partial class IntDisplay : ValueLabelDisplay<MBInt>, ICreatable
    {
        protected override void UpdateValue(object? newValue)
        {
            if (newValue is MBInt intValue) Value = intValue;
        }

        protected override void UpdateDisplayedValue(object newValue)
        {
            if (newValue is MBInt intValue) ValueLabel.Text = intValue.Value.ToString();
        }
    }
}
