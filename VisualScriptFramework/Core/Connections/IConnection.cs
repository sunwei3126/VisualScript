using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTLogic.Core.Connections
{
    public interface IConnection<out TSource, out TDestination>
    {
        TSource Source { get; }
        TDestination Destination { get; }
    }
}
