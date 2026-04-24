using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualScript.Core.Collections
{
    public interface IKeyedCollection<TKey, TItem> : ICollection<TItem>
    {
        TItem this[TKey key] { get; }
        TItem this[int index] { get; } // For allocation free enumerators
        bool TryGetValue(TKey key, out TItem value);
        bool Contains(TKey key);
        bool Remove(TKey key);
    }
}
