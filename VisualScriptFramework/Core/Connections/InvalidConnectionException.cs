using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualScript.Core.Connections
{
    public class InvalidConnectionException : Exception
    {
        public InvalidConnectionException() : base("") { }
        public InvalidConnectionException(string message) : base(message) { }
    }
}
