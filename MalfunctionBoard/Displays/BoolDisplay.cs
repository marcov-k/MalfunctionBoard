using MalfunctionBoard.Interfaces;
using MalfunctionBoard.TableDatatypes;

namespace MalfunctionBoard.Displays
{
    public partial class BoolDisplay : ValueLabelDisplay<MBBool>, ICreatable
    {
        protected override void UpdateValue(object? newValue)
        {
            if (newValue is MBBool boolValue) Value = boolValue;
        }

        protected override void UpdateDisplayedValue(object newValue)
        {
            if (newValue is MBBool boolValue) ValueLabel.Text = boolValue.ToString();
        }
    }
}
