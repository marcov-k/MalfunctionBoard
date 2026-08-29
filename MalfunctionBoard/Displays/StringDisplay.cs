using MalfunctionBoard.Interfaces;
using MalfunctionBoard.TableDatatypes;

namespace MalfunctionBoard.Displays
{
    public partial class StringDisplay : ValueLabelDisplay<MBString>, ICreatable
    {
        protected override void UpdateValue(object? newValue)
        {
            if (newValue is MBString stringValue) Value = stringValue;
        }

        protected override void UpdateDisplayedValue(object newValue)
        {
            if (newValue is MBString stringValue) ValueLabel.Text = stringValue.Data;
        }
    }
}
