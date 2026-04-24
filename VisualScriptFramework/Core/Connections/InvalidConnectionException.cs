using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTLogic.Core.Connections
{
    public class InvalidConnectionException : Exception
    {
        public InvalidConnectionException() : base("") { }
        public InvalidConnectionException(string message) : base(message) { }
    }
}
