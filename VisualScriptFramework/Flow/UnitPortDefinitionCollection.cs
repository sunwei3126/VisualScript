using System.Collections.ObjectModel;
using VisualScript.Flow.Ports;

namespace VisualScript.Flow
{
    public sealed class UnitPortDefinitionCollection<T> : Collection<T> where T : IUnitPortDefinition { }
}
