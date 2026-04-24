using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTLogic.Core.Graph
{
    public sealed class GraphPointerException : Exception
    {
        public GraphPointer pointer { get; }

        public GraphPointerException(string message, GraphPointer pointer) : base(message + "\n" + pointer)
        {
            this.pointer = pointer;
        }
    }
}
