namespace MalfunctionBoard.Interfaces
{
    public interface IHasVector2
    {
        int X { get; set; }
        int MinX { get; }
        int MaxX { get; }
        int Y { get; set; }
        int MinY { get; }
        int MaxY { get; }
    }
}
