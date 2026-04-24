using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualScript.Core.Pooling
{
    public interface IPoolable
    {
        void New();
        void Free();
    }
}
