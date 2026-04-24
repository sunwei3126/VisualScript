using System;

namespace VisualScript.Flow.Ports
{
    public interface IUnitValuePortDefinition : IUnitPortDefinition
    {
        Type Type { get; }
    }
}
