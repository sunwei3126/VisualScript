using System;
using System.Collections.ObjectModel;

namespace VisualScript.Flow.Ports
{
    public sealed class UnitPortCollection<TPort> : KeyedCollection<string, TPort>, IUnitPortCollection<TPort>
         where TPort : IUnitPort
    {
        public IUnit Unit { get; }

        public UnitPortCollection(IUnit unit)
        {
            this.Unit = unit;
        }

        private void BeforeAdd(TPort port)
        {
            if (port.Unit != null)
            {
                if (port.Unit == Unit)
                {
                    throw new InvalidOperationException("Node ports cannot be added multiple time to the same unit.");
                }
                else
                {
                    throw new InvalidOperationException("Node ports cannot be shared across nodes.");
                }
            }

            port.Unit = Unit;
        }

        private void AfterAdd(TPort port)
        {
            Unit.PortsChanged();
        }

        private void BeforeRemove(TPort port)
        {
        }

        private void AfterRemove(TPort port)
        {
            port.Unit = null;
            Unit.PortsChanged();
        }

        public TPort Single()
        {
            if (Count != 0)
            {
                throw new InvalidOperationException("Port collection does not have a single port.");
            }

            return this[0];
        }

        protected override string GetKeyForItem(TPort item)
        {
            return item.Key;
        }

        public bool TryGetValue(string key, out TPort value)
        {
            if (Dictionary == null)
            {
                value = default(TPort);
                return false;
            }

            return Dictionary.TryGetValue(key, out value);
        }

        protected override void InsertItem(int index, TPort item)
        {
            BeforeAdd(item);
            base.InsertItem(index, item);
            AfterAdd(item);
        }

        protected override void RemoveItem(int index)
        {
            var item = this[index];
            BeforeRemove(item);
            base.RemoveItem(index);
            AfterRemove(item);
        }

        protected override void SetItem(int index, TPort item)
        {
            throw new NotSupportedException();
        }

        protected override void ClearItems()
        {
            while (Count > 0)
            {
                RemoveItem(0);
            }
        }
    }
}
