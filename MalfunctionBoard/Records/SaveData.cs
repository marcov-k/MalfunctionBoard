using MalfunctionBoard.Records.GridData;

namespace MalfunctionBoard.Records
{
    [Serializable]
    public record SaveData(LayoutData Layout, string TableName);
}
