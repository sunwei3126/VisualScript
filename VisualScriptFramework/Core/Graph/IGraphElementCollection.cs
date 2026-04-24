using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisualScript.Core.Collections;

namespace VisualScript.Core.Graph
{
    public interface IGraphElementCollection<T> : IKeyedCollection<Guid, T>, INotifyCollectionChanged<T> where T : IGraphElement
    {

    }
}
