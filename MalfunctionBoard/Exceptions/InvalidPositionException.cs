using MalfunctionBoard.Records.GridData;

namespace MalfunctionBoard.Exceptions
{
    internal class InvalidPositionException : Exception
    {
        public GridPos Position { get; } = new();
        public GridDims Dimensions { get; } = new();

        public InvalidPositionException() : base() { }

        public InvalidPositionException(GridPos position, GridDims dimensions)
            : base(FormatMessage(position, dimensions))
        {
            Position = position;
            Dimensions = dimensions;
        }

        public InvalidPositionException(GridPos position, GridDims dimensions, Exception inner)
            : base(FormatMessage(position, dimensions), inner)
        {
            Position = position;
            Dimensions = dimensions;
        }

        static string FormatMessage(GridPos pos, GridDims dims) => $"Invalid Position: Dimensions with Width {dims.Width} and Height {dims.Height} at Row {pos.Row} and Column {pos.Col} Conflict with an Existing Display";
    }
}
