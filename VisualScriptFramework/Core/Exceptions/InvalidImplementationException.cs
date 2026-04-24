using System;

namespace IoTLogic.Core.Exceptions
{
    public class InvalidImplementationException : Exception
    {
        public InvalidImplementationException() : base() { }
        public InvalidImplementationException(string message) : base(message) { }
    }
}
