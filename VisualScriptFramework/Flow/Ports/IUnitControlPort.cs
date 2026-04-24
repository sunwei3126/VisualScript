using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualScript.Flow.Ports
{
    public interface IUnitControlPort : IUnitPort
    {
        bool IsPredictable { get; }
        bool CouldBeEntered { get; }
    }
}
