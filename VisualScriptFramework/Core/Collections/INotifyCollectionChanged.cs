using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTLogic.Core.Collections
{
    public interface INotifyCollectionChanged<T>
    {
        event Action<T> ItemAdded;

        event Action<T> ItemRemoved;

        event Action CollectionChanged;
    }
}
