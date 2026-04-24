using System.Reflection;

namespace VisualScript.Core.Reflection
{
    public abstract class StaticActionInvokerBase : StaticInvokerBase
    {
        protected StaticActionInvokerBase(MethodInfo methodInfo) : base(methodInfo) { }
    }
}
