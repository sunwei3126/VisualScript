using VisualScript.Core.Collections;

namespace VisualScript.Flow.Ports
{
    public interface IUnitPortCollection<TPort> : IKeyedCollection<string, TPort> where TPort : IUnitPort
    {
        TPort Single();
    }
}
