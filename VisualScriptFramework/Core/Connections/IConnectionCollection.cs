using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTLogic.Core.Connections
{
    public interface IConnectionCollection<TConnection, TSource, TDestination> : ICollection<TConnection>
     where TConnection : IConnection<TSource, TDestination>
    {
        IEnumerable<TConnection> this[TSource source] { get; }
        IEnumerable<TConnection> this[TDestination destination] { get; }
        IEnumerable<TConnection> WithSource(TSource source);
        IEnumerable<TConnection> WithDestination(TDestination destination);
    }
}
