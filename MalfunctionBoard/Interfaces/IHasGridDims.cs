using MalfunctionBoard.Records.GridData;

namespace MalfunctionBoard.Interfaces
{
    public interface IHasGridDims
    {
        GridDims Dimensions { get; set; }
    }
}
