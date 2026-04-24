using System;

namespace IoTLogic.Core.Exceptions
{
    public class InvalidConversionException : InvalidCastException
    {
        public InvalidConversionException() : base() { }
        public InvalidConversionException(string message) : base(message) { }
        public InvalidConversionException(string message, Exception innerException) : base(message, innerException) { }
    }
}
