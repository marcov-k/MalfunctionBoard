using MalfunctionBoard.Interfaces;

namespace MalfunctionBoard.Records.GridData
{
    public record GridDims() : IHasVector2
    {
        public int X
        {
            get => Width;
            set => Width = value;
        }
        public int MinX => 1;
        public int MaxX => MainPage.ColumnCount;
        public int Width
        {
            get => _width;
            set => _width = Math.Clamp(value, MinX, MaxX);
        }
        int _width;
        public int Y
        {
            get => Height;
            set => Height = value;
        }
        public int MinY => 1;
        public int MaxY => MainPage.RowCount;
        public int Height
        {
            get => _height;
            set => _height = Math.Clamp(value, MinY, MaxY);
        }
        int _height;
    }
}
