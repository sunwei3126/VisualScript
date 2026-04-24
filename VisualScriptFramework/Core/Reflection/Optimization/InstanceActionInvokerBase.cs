using System.Reflection;

namespace IoTLogic.Core.Reflection
{
    public abstract class InstanceActionInvokerBase<TTarget> : InstanceInvokerBase<TTarget>
    {
        protected InstanceActionInvokerBase(MethodInfo methodInfo) : base(methodInfo) { }
    }
}
