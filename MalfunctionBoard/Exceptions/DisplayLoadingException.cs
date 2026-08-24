namespace MalfunctionBoard.Exceptions
{
    internal class DisplayLoadingException : Exception
    {
        public DisplayLoadingException() : base() { }

        public DisplayLoadingException(string message) : base(message) { }

        public DisplayLoadingException(string message, Exception inner) : base(message, inner) { }
    }
}
