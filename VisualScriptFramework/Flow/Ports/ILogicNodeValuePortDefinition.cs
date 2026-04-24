using System;

namespace IoTLogic.Flow.Ports
{
    public interface IUnitValuePortDefinition : IUnitPortDefinition
    {
        Type Type { get; }
    }
}
