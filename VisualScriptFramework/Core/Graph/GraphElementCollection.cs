using System;
using IoTLogic.Core.Pooling;
using IoTLogic.Core.Collections;

namespace IoTLogic.Core.Graph
{
    public sealed class GraphElementCollection<TElement> : GuidCollection<TElement>, IGraphElementCollection<TElement>, IProxyableNotifyCollectionChanged<TElement>
         where TElement : IGraphElement
    {
        public GraphElementCollection(IGraph graph) : base()
        {
            Ensure.Ensure.That(nameof(graph)).IsNotNull(graph);

            this.Graph = graph;
        }

        public IGraph Graph { get; }

        public event Action<TElement> ItemAdded;

        public event Action<TElement> ItemRemoved;

        public event Action CollectionChanged;

        public bool ProxyCollectionChange { get; set; }

        public void BeforeAdd(TElement element)
        {
            if (element.Graph != null)
            {
                if (element.Graph == Graph)
                {
                    throw new InvalidOperationException("Graph elements cannot be added multiple time into the same graph.");
                }
                else
                {
                    throw new InvalidOperationException("Graph elements cannot be shared across graphs.");
                }
            }

            element.Graph = Graph;
            element.BeforeAdd();
        }

        public void AfterAdd(TElement element)
        {
            element.AfterAdd();
            ItemAdded?.Invoke(element);
            CollectionChanged?.Invoke();
        }

        public void BeforeRemove(TElement element)
        {
            element.BeforeRemove();
        }

        public void AfterRemove(TElement element)
        {
            element.Graph = null;
            element.AfterRemove();
            ItemRemoved?.Invoke(element);
            CollectionChanged?.Invoke();
        }

        protected override void InsertItem(int index, TElement element)
        {
            Ensure.Ensure.That(nameof(element)).IsNotNull(element);

            if (!ProxyCollectionChange)
            {
                BeforeAdd(element);
            }

            base.InsertItem(index, element);

            if (!ProxyCollectionChange)
            {
                AfterAdd(element);
            }
        }

        protected override void RemoveItem(int index)
        {
            var element = this[index];

            if (!Contains(element))
            {
                throw new ArgumentOutOfRangeException(nameof(element));
            }

            if (!ProxyCollectionChange)
            {
                BeforeRemove(element);
            }

            base.RemoveItem(index);

            if (!ProxyCollectionChange)
            {
                AfterRemove(element);
            }
        }

        protected override void ClearItems()
        {
            // this.OrderByDescending(e => e.dependencyOrder).ToList()
            var toRemove = ListPool<TElement>.New();
            foreach (var element in this)
            {
                toRemove.Add(element);
            }

            toRemove.Sort((a, b) => b.DependencyOrder.CompareTo(a.DependencyOrder));

            foreach (var element in toRemove)
            {
                Remove(element);
            }

            ListPool<TElement>.Free(toRemove);
        }

        protected override void SetItem(int index, TElement item)
        {
            throw new NotSupportedException();
        }

        public new NoAllocEnumerator<TElement> GetEnumerator()
        {
            return new NoAllocEnumerator<TElement>(this);
        }
    }
}
