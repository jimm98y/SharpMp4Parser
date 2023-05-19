namespace SharpMp4Parser.IsoParser.Tools
{
    /**
     * A little helper for working with arrays as some functions now available in Java 7/8 are
     * not available on all Android platforms.
     */
    public sealed class Mp4Arrays
    {
        public static long[] copyOfAndAppend(long[] original, params long[] toAppend)
        {
            if (original == null)
            {
                original = new long[0];
            }
            if (toAppend == null)
            {
                toAppend = new long[0];
            }
            long[] copy = new long[original.Length + toAppend.Length];
            System.Array.Copy(original, 0, copy, 0, original.Length);
            System.Array.Copy(toAppend, 0, copy, original.Length, toAppend.Length);
            return copy;
        }


        public static int[] copyOfAndAppend(int[] original, params int[] toAppend)
        {
            if (original == null)
            {
                original = new int[0];
            }
            if (toAppend == null)
            {
                toAppend = new int[0];
            }
            int[] copy = new int[original.Length + toAppend.Length];
            System.Array.Copy(original, 0, copy, 0, original.Length);
            System.Array.Copy(toAppend, 0, copy, original.Length, toAppend.Length);
            return copy;
        }

        public static byte[] copyOfAndAppend(byte[] original, params byte[] toAppend)
        {
            if (original == null)
            {
                original = new byte[0];
            }
            if (toAppend == null)
            {
                toAppend = new byte[0];
            }
            byte[] copy = new byte[original.Length + toAppend.Length];
            System.Array.Copy(original, 0, copy, 0, original.Length);
            System.Array.Copy(toAppend, 0, copy, original.Length, toAppend.Length);
            return copy;
        }

        public static double[] copyOfAndAppend(double[] original, params double[] toAppend)
        {
            if (original == null)
            {
                original = new double[0];
            }
            if (toAppend == null)
            {
                toAppend = new double[0];
            }
            double[] copy = new double[original.Length + toAppend.Length];
            System.Array.Copy(original, 0, copy, 0, original.Length);
            System.Array.Copy(toAppend, 0, copy, original.Length, toAppend.Length);
            return copy;
        }
    }
}
