using System.Collections.ObjectModel;
using IoTLogic.Flow.Ports;

namespace IoTLogic.Flow
{
    public sealed class UnitPortDefinitionCollection<T> : Collection<T> where T : IUnitPortDefinition { }
}
