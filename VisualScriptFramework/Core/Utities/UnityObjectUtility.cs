using System;
using System.Collections.Generic;
using System.Linq;

namespace VisualScript.Core.Utities
{
    public static class UnityObjectUtility
    {
        public static bool IsDestroyed(this Object target)
        {
            // Checks whether a Unity object is not actually a null reference,
            // but a rather destroyed native instance.

            return !ReferenceEquals(target, null) && target == null;
        }

        public static bool IsUnityNull(this object obj)
        {
            // Checks whether an object is null or Unity pseudo-null
            // without having to cast to UnityEngine.Object manually

            return obj == null || ((obj is Object) && ((Object)obj) == null);
        }

        public static string ToSafeString(this object obj)
        {
            if (obj == null)
            {
                return "(null)";
            }

            if (obj is Object uo)
            {
                return uo.ToSafeString();
            }

            try
            {
                return obj.ToString();
            }
            catch (Exception ex)
            {
                return $"({ex.GetType().Name} in ToString: {ex.Message})";
            }
        }

        public static T AsUnityNull<T>(this T obj) where T: class
        {
            // Converts a Unity pseudo-null to a real null, allowing for coalesce operators.
            // e.g.: destroyedObject.AsUnityNull() ?? otherObject

            if (obj == null)
            {
                return null;
            }

            return obj;
        }

        public static bool TrulyEqual(Object a, Object b)
        {
            // This method is required when checking two references
            // against one another, where one of them might have been destroyed.
            // It is not required when checking against null.

            // This is because Unity does not compare alive state
            // in the CompareBaseObjects method unless one of the two
            // operators is actually the null literal.

            // From the source:
            /*
              bool lhsIsNull = (object) lhs == null;
              bool rhsIsNull = (object) rhs == null;
              if (rhsIsNull && lhsIsNull)
                return true;
              if (rhsIsNull)
                return !Object.IsNativeObjectAlive(lhs);
              if (lhsIsNull)
                return !Object.IsNativeObjectAlive(rhs);
              return lhs.m_InstanceID == rhs.m_InstanceID;
             */

            // As we can see, Object.IsNativeObjectAlive is not compared
            // across the two objects unless one of the operands is actually null.
            // But it can happen, for example when exiting play mode.
            // If we stored a static reference to a scene object that was destroyed,
            // the reference won't get cleared because assembly reloads don't happen
            // when exiting playmode. But the instance ID of the object will stay
            // the same, because it only gets reserialized. So if we compare our
            // stale reference that was destroyed to a new reference to the object,
            // it will return true, even though one reference is alive and the other isn't.

            if (a != b)
            {
                return false;
            }

            if ((a == null) != (b == null))
            {
                return false;
            }

            return true;
        }

        public static IEnumerable<T> NotUnityNull<T>(this IEnumerable<T> enumerable)
        {
            return enumerable.Where(i => i != null);
        }

       
    }
}
