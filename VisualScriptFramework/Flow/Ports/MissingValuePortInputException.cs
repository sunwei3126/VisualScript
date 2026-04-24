using System;

namespace IoTLogic.Flow.Ports
{
    public sealed class MissingValuePortInputException : Exception
    {
        public MissingValuePortInputException(string key) : base($"Missing input value for '{key}'.") { }
    }
}
