using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisualScript.Core.Collections;

namespace VisualScript.Core.Graph
{
    public sealed class MergedGraphElementCollection : MergedKeyedCollection<Guid, IGraphElement>, INotifyCollectionChanged<IGraphElement>
    {
        public event Action<IGraphElement> ItemAdded;

        public event Action<IGraphElement> ItemRemoved;

        public event Action CollectionChanged;

        public override void Include<TSubItem>(IKeyedCollection<Guid, TSubItem> collection)
        {
            base.Include(collection);

            var graphElementCollection = collection as IGraphElementCollection<TSubItem>;

            if (graphElementCollection != null)
            {
                graphElementCollection.ItemAdded += (element) => ItemAdded?.Invoke(element);
                graphElementCollection.ItemRemoved += (element) => ItemRemoved?.Invoke(element);
                graphElementCollection.CollectionChanged += () => CollectionChanged?.Invoke();
            }
        }
    }
}
