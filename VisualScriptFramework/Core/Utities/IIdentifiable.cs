using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualScript.Core.Utities
{
    public interface IIdentifiable
    {
        Guid Guid { get; }
    }
}
