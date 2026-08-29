using MalfunctionBoard.Interfaces;
using MalfunctionBoard.TableDatatypes;

namespace MalfunctionBoard.Displays
{
    public partial class DoubleDisplay : ValueLabelDisplay<MBDouble>, ICreatable
    {
        protected override void UpdateValue(object? newValue)
        {
            if (newValue is MBDouble doubleValue) Value = doubleValue;
        }

        protected override void UpdateDisplayedValue(object newValue)
        {
            if (newValue is MBDouble doubleValue) ValueLabel.Text = doubleValue.Value.ToString();
        }
    }
}
