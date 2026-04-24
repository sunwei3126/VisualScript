using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTLogic.Core.Reflection
{
    public sealed class GenericClosingException : Exception
    {
        public GenericClosingException(string message) : base(message) { }
        public GenericClosingException(Type open, Type closed) : base($"Open-constructed type '{open}' is not assignable from closed-constructed type '{closed}'.") { }
    }
}
