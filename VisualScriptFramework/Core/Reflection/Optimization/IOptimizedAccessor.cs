namespace VisualScript.Core.Reflection
{
    public interface IOptimizedAccessor
    {
        void Compile();
        object GetValue(object target);
        void SetValue(object target, object value);
    }
}
