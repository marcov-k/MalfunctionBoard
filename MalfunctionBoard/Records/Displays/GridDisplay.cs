using MalfunctionBoard.Displays;
using MalfunctionBoard.Records.GridData;

namespace MalfunctionBoard.Records.Displays
{
    [Serializable]
    public record GridDisplay(GridPos Position, DashboardDisplay Display);
}
