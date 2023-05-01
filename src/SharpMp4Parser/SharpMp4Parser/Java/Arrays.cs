using System;
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

        internal static int binarySearch(long[] syncSamples, long v)
        {
            throw new NotImplementedException();
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
    }
}
