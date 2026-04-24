using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTLogic.Core.Collections
{
    public interface IProxyableNotifyCollectionChanged<T>
    {
        bool ProxyCollectionChange { get; set; }

        void BeforeAdd(T item);

        void AfterAdd(T item);

        void BeforeRemove(T item);

        void AfterRemove(T item);
    }
}
