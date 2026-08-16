using MalfunctionBoard.Records.Displays;

namespace MalfunctionBoard.Records.GridData
{
    [Serializable]
    public record LayoutData(List<DisplayData> Displays);
}
