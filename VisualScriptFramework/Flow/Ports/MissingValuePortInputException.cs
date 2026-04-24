using System;

namespace VVisualScript.Flow.Ports
{
    public sealed class MissingValuePortInputException : Exception
    {
        public MissingValuePortInputException(string key) : base($"Missing input value for '{key}'.") { }
    }
}
