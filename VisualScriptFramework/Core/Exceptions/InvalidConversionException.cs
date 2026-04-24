using System;

namespace VisualScript.Core.Exceptions
{
    public class InvalidConversionException : InvalidCastException
    {
        public InvalidConversionException() : base() { }
        public InvalidConversionException(string message) : base(message) { }
        public InvalidConversionException(string message, Exception innerException) : base(message, innerException) { }
    }
}
