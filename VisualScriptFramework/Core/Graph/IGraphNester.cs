using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTLogic.Core.Graph
{
    public interface IGraphNester : IGraphParent
    {
        IGraphNest Nest { get; }

        void InstantiateNest();
        void UninstantiateNest();
    }
}
