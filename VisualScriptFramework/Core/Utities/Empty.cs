using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualScript.Core.Utities
{
    // For some reason, Enumerable.Empty<T>() seems to allocate 240b in Unity,
    // even though Mono source seems to use a shared 0-length array instance.
    // Maybe it's an old version's bug?
    public static class Empty<T>
    {
        public static readonly T[] array = new T[0];
        public static readonly List<T> list = new List<T>(0);
        public static readonly HashSet<T> hashSet = new HashSet<T>();
    }
}
