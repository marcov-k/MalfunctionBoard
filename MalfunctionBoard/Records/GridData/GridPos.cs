using MalfunctionBoard.Interfaces;

namespace MalfunctionBoard.Records.GridData
{
    [Serializable]
    public record GridPos : IHasVector2
    {
        public int X
        {
            get => Row;
            set => Row = value;
        }
        public int MinX { get => 0; }
        public int MaxX { get => MainPage.RowCount - 1; }
        public int Y
        {
            get => Col;
            set => Col = value;
        }
        public int MinY { get => 0; }
        public int MaxY { get => MainPage.ColumnCount - 1; }
        public int Row
        {
            get => _row;
            set => _row = Math.Clamp(value, MinX, MaxX);
        }
        int _row;
        public int Col
        {
            get => _col;
            set => _col = Math.Clamp(value, MinY, MaxY);
        }
        int _col;
    }
}
