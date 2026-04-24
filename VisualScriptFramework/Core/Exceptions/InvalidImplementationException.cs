using System;

namespace VisualScript.Core.Exceptions
{
    public class InvalidImplementationException : Exception
    {
        public InvalidImplementationException() : base() { }
        public InvalidImplementationException(string message) : base(message) { }
    }
}
