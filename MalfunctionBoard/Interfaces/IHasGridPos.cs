using MalfunctionBoard.Records.GridData;

namespace MalfunctionBoard.Interfaces
{
    public interface IHasGridPos
    {
        GridPos Position { get; set; }
    }
}
