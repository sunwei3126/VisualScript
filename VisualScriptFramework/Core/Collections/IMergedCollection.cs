using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTLogic.Core.Collections
{
    public interface IMergedCollection<T> : ICollection<T>
    {
        bool Includes<TI>() where TI : T;
        bool Includes(Type elementType);
    }
}
