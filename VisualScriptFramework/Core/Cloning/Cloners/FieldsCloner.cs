using System.Reflection;

namespace VisualScript.Core.Reflection
{
    public sealed class FieldsCloner : ReflectedCloner
    {
        protected override bool IncludeField(FieldInfo field)
        {
            return true;
        }

        protected override bool IncludeProperty(PropertyInfo property)
        {
            return false;
        }
    }
}
