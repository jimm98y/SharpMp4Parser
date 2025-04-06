using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SharpMp4Parser.Java
{
    public static class Arrays
    {
        public static int hashCode<T>(T[] array)
        {
            return ((IStructuralEquatable)array).GetHashCode(EqualityComparer<T>.Default);
        }

        public static string toString<T>(this IEnumerable<T> list)
        {
            return "[" + string.Join(",", list) + "]";
        }

        internal static int binarySearch(long[] syncSamples, long item)
        {
            return syncSamples.ToList().BinarySearch(item);
        }

        public static void fill<T>(T[] array, T value)
        {
            fill(array, 0, array.Length, value);
        }

        public static void fill<T>(T[] array, int fromIndex, int toIndex, T value)
        {
            for (int i = fromIndex; i < toIndex; i++)
            {
                array[i] = value;
            }
        }

        internal static IList<T> asList<T>(params T[] items)
        {
            return items.ToList();
        }
    }
}
