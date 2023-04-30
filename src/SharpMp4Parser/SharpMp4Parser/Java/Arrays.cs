using System.Collections;
using System.Collections.Generic;

namespace SharpMp4Parser.Java
{
    public static class Arrays
    {
        public static int hashCode<T>(T[] array)
        {
            return ((IStructuralEquatable)array).GetHashCode(EqualityComparer<int>.Default);
        }

        public static string toString<T>(this IEnumerable<T> list)
        {
            return "[" + string.Join(",", list) + "]";
        }
    }
}
