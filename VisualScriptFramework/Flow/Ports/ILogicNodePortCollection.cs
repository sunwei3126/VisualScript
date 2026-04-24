using IoTLogic.Core.Collections;

namespace IoTLogic.Flow.Ports
{
    public interface IUnitPortCollection<TPort> : IKeyedCollection<string, TPort> where TPort : IUnitPort
    {
        TPort Single();
    }
}
