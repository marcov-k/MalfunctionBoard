using MalfunctionBoard.Records.GridData;

namespace MalfunctionBoard.Records.Displays
{
    [Serializable]
    public record DisplayData(string? DisplayType, string Title, string Binding, GridPos Position, GridDims Dimensions);
}
