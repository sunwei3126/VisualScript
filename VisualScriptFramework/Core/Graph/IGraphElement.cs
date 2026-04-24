using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IoTLogic.Core.Collections;
using IoTLogic.Core.Reflection;
using IoTLogic.Core.Utities;

namespace IoTLogic.Core.Graph
{
    public interface IGraphElement : IGraphItem, INotifiedCollectionItem, IDisposable, IPrewarmable,IIdentifiable, IAnalyticsIdentifiable
    {
        new IGraph Graph { get; set; }

        bool HandleDependencies();

        int DependencyOrder { get; }

        new Guid Guid { get; set; }

        void Instantiate(GraphReference instance);

        void Uninstantiate(GraphReference instance);
    }
}
