namespace MalfunctionBoard.Exceptions
{
    internal class InvalidBindingException : Exception
    {
        public string Binding { get; } = string.Empty;

        public InvalidBindingException() : base() { }

        public InvalidBindingException(string binding)
            : base(FormatMessage(binding))
        {
            Binding = binding;
        }

        public InvalidBindingException(string binding, Exception inner)
            : base(FormatMessage(binding), inner)
        {
            Binding = binding;
        }

        static string FormatMessage(string binding) => $"Invalid Binding - Binding Already Exists: \"{binding}\"";
    }
}
