using System.Reflection;

namespace VisualScript.Core.Reflection
{
    public abstract class InstanceActionInvokerBase<TTarget> : InstanceInvokerBase<TTarget>
    {
        protected InstanceActionInvokerBase(MethodInfo methodInfo) : base(methodInfo) { }
    }
}
