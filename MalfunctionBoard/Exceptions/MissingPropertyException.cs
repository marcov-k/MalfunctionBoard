namespace MalfunctionBoard.Exceptions
{
    internal class MissingPropertyException : Exception
    {
        public string Property { get; } = string.Empty;

        public MissingPropertyException() : base() { }

        public MissingPropertyException(string property)
            : base(FormatMessage(property))
        {
            Property = property;
        }

        public MissingPropertyException(string property, Exception inner)
            : base(FormatMessage(property), inner)
        {
            Property = property;
        }

        static string FormatMessage(string property) => $"Missing Display Property: {property}";
    }
}
