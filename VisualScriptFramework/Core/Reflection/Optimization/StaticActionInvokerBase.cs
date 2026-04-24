using System.Reflection;

namespace IoTLogic.Core.Reflection
{
    public abstract class StaticActionInvokerBase : StaticInvokerBase
    {
        protected StaticActionInvokerBase(MethodInfo methodInfo) : base(methodInfo) { }
    }
}
