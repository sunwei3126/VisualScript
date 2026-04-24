using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTLogic.Core.Ensure
{
    internal static class XComparable
    {
        internal static bool IsLt<T>(this IComparable<T> x, T y)
        {
            return x.CompareTo(y) < 0;
        }

        internal static bool IsEq<T>(this IComparable<T> x, T y)
        {
            return x.CompareTo(y) == 0;
        }

        internal static bool IsGt<T>(this IComparable<T> x, T y)
        {
            return x.CompareTo(y) > 0;
        }
    }
}
