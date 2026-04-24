using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using VisualScript.Core.Reflection;
using VisualScript.Core.Utities;

namespace VisualScript.Core.Reflection
{
    public abstract class ReflectedCloner : Cloner<object>
    {
        public override bool Handles(Type type)
        {
            return false; // Should only be used as a fallback cloner
        }

        public override void FillClone(Type type, ref object clone, object original, CloningContext context)
        {
          
                foreach (var accessor in GetOptimizedAccessors(type))
                {
                    if (context.tryPreserveInstances)
                    {
                        var cloneProperty = accessor.GetValue(clone);
                        Cloning.CloneInto(context, ref cloneProperty, accessor.GetValue(original));
                        accessor.SetValue(clone, cloneProperty);
                    }
                    else
                    {
                        accessor.SetValue(clone, Cloning.Clone(context, accessor.GetValue(original)));
                    }
                }
            }
         

        private readonly Dictionary<Type, MemberInfo[]> accessors = new Dictionary<Type, MemberInfo[]>();

        private MemberInfo[] GetAccessors(Type type)
        {
            if (!accessors.ContainsKey(type))
            {
                accessors.Add(type, GetMembers(type).ToArray());
            }

            return accessors[type];
        }

        private readonly Dictionary<Type, IOptimizedAccessor[]> optimizedAccessors = new Dictionary<Type, IOptimizedAccessor[]>();

        private IOptimizedAccessor[] GetOptimizedAccessors(Type type)
        {
            if (!optimizedAccessors.ContainsKey(type))
            {
                var list = new List<IOptimizedAccessor>();

                foreach (var member in GetMembers(type))
                {
                    if (member is FieldInfo)
                    {
                        list.Add(((FieldInfo)member).Prewarm());
                    }
                    else if (member is PropertyInfo)
                    {
                        list.Add(((PropertyInfo)member).Prewarm());
                    }
                }

                optimizedAccessors.Add(type, list.ToArray());
            }

            return optimizedAccessors[type];
        }

        protected virtual IEnumerable<MemberInfo> GetMembers(Type type)
        {
            var bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            return LinqUtility.Concat<MemberInfo>
                (
                    type.GetFields(bindingFlags).Where(IncludeField),
                    type.GetProperties(bindingFlags).Where(IncludeProperty)
                );
        }

        protected virtual bool IncludeField(FieldInfo field)
        {
            return false;
        }

        protected virtual bool IncludeProperty(PropertyInfo property)
        {
            return false;
        }
    }
}
