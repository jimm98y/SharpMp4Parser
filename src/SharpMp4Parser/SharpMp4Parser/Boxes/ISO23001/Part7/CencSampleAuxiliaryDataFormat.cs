using System.Linq;

namespace SharpMp4Parser.Boxes.ISO23001.Part7
{
    /**
     * Each encrypted sample in a protected track shall have an Initialization Vector associated with it. Further, each
     * encrypted sample in protected AVC video tracks shall conform to ISO/IEC 14496-10 and ISO/IEC 14496-15
     * and shall use the subsample encryption scheme specified in 9.6.2, which requires subsample encryption data.
     * Both initialization vectors and subsample encryption data are provided as Sample Auxiliary Information with
     * aux_info_type equal to ‘cenc’ and aux_info_type_parameter equal to 0. For tracks protected using
     * the 'cenc' scheme, the default value for aux_info_type is equal to 'cenc' and the default value for the
     * aux_info_type_parameter is 0 so content may be created omitting these optional fields. Storage of
     * sample auxiliary information shall conform to ISO/IEC 14496-12.<br>
     * This class can also be used for PIFF as it has been derived from the PIFF spec.
     */
    public class CencSampleAuxiliaryDataFormat
    {
        public byte[] iv = new byte[0];
        public Pair[] pairs = null;

        public int getSize()
        {
            int size = iv.Length;
            if (pairs != null && pairs.Length > 0)
            {
                size += 2;
                size += (pairs.Length * 6);
            }
            return size;
        }

        public Pair createPair(int clear, long encrypted)
        {
            // Memory saving!!!
            if (clear <= byte.MaxValue)
            {
                if (encrypted <= byte.MaxValue)
                {
                    return new ByteBytePair(clear, encrypted);
                }
                else if (encrypted <= short.MaxValue)
                {
                    return new ByteShortPair(clear, encrypted);
                }
                else if (encrypted <= int.MaxValue)
                {
                    return new ByteIntPair(clear, encrypted);
                }
                else
                {
                    return new ByteLongPair(clear, encrypted);
                }
            }
            else if (clear <= short.MaxValue)
            {
                if (encrypted <= byte.MaxValue)
                {
                    return new ShortBytePair(clear, encrypted);
                }
                else if (encrypted <= short.MaxValue)
                {
                    return new ShortShortPair(clear, encrypted);
                }
                else if (encrypted <= int.MaxValue)
                {
                    return new ShortIntPair(clear, encrypted);
                }
                else
                {
                    return new ShortLongPair(clear, encrypted);
                }
            }
            else
            {
                if (encrypted <= byte.MaxValue)
                {
                    return new IntBytePair(clear, encrypted);
                }
                else if (encrypted <= short.MaxValue)
                {
                    return new IntShortPair(clear, encrypted);
                }
                else if (encrypted <= int.MaxValue)
                {
                    return new IntIntPair(clear, encrypted);
                }
                else
                {
                    return new IntLongPair(clear, encrypted);
                }
            }
        }

        public override bool Equals(object o)
        {
            if (this == o)
            {
                return true;
            }
            if (o == null || getClass() != o.getClass())
            {
                return false;
            }

            CencSampleAuxiliaryDataFormat entry = (CencSampleAuxiliaryDataFormat)o;

            if (!new BigInteger(iv).Equals(new BigInteger(entry.iv)))
            {
                return false;
            }
            if (pairs != null ? !Enumerable.SequenceEqual(pairs, entry.pairs) : entry.pairs != null)
            {
                return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            int result = iv != null ? Arrays.hashCode(iv) : 0;
            result = 31 * result + (pairs != null ? Arrays.hashCode(pairs) : 0);
            return result;
        }

        public override string ToString()
        {
            return "Entry{" +
            "iv=" + Hex.encodeHex(iv) +
                    ", pairs=" + Arrays.toString(pairs) +
                    '}';
        }

        public interface Pair
        {
            int clear();

            long encrypted();
        }

        private class ByteBytePair : AbstractPair
        {
            private byte clear;
            private byte encrypted;

            public ByteBytePair(int clear, long encrypted)
            {
                this.clear = (byte)clear;
                this.encrypted = (byte)encrypted;
            }

            public int clear()
            {
                return clear;
            }

            public long encrypted()
            {
                return encrypted;
            }

        }

        private class ByteShortPair : AbstractPair
        {
            private byte clear;
            private short encrypted;

            public ByteShortPair(int clear, long encrypted)
            {
                this.clear = (byte)clear;
                this.encrypted = (short)encrypted;
            }

            public int clear()
            {
                return clear;
            }

            public long encrypted()
            {
                return encrypted;
            }
        }

        private class ByteIntPair : AbstractPair
        {
            private byte clear;
            private int encrypted;

            public ByteIntPair(int clear, long encrypted)
            {
                this.clear = (byte)clear;
                this.encrypted = (int)encrypted;
            }

            public int clear()
            {
                return clear;
            }

            public long encrypted()
            {
                return encrypted;
            }
        }

        private class ByteLongPair : AbstractPair
        {
            private byte clear;
            private long encrypted;

            public ByteLongPair(int clear, long encrypted)
            {
                this.clear = (byte)clear;
                this.encrypted = encrypted;
            }

            public int clear()
            {
                return clear;
            }

            public long encrypted()
            {
                return encrypted;
            }
        }

        private class ShortBytePair : AbstractPair
        {
            private short clear;
            private byte encrypted;

            public ShortBytePair(int clear, long encrypted)
            {
                this.clear = (short)clear;
                this.encrypted = (byte)encrypted;
            }

            public int clear()
            {
                return clear;
            }

            public long encrypted()
            {
                return encrypted;
            }
        }

        private class ShortShortPair : AbstractPair
        {
            private short clear;
            private short encrypted;

            public ShortShortPair(int clear, long encrypted)
            {
                this.clear = (short)clear;
                this.encrypted = (short)encrypted;
            }

            public int clear()
            {
                return clear;
            }

            public long encrypted()
            {
                return encrypted;
            }
        }

        private class ShortIntPair : AbstractPair
        {
            private short clear;
            private int encrypted;

            public ShortIntPair(int clear, long encrypted)
            {
                this.clear = (short)clear;
                this.encrypted = (int)encrypted;
            }

            public int clear()
            {
                return clear;
            }

            public long encrypted()
            {
                return encrypted;
            }
        }

        private class ShortLongPair : AbstractPair
        {
            private short clear;
            private long encrypted;

            public ShortLongPair(int clear, long encrypted)
            {
                this.clear = (short)clear;
                this.encrypted = encrypted;
            }

            public int clear()
            {
                return clear;
            }

            public long encrypted()
            {
                return encrypted;
            }
        }

        private class IntBytePair : AbstractPair
        {
            private int clear;
            private byte encrypted;

            public IntBytePair(int clear, long encrypted)
            {
                this.clear = clear;
                this.encrypted = (byte)encrypted;
            }

            public int clear()
            {
                return clear;
            }

            public long encrypted()
            {
                return encrypted;
            }
        }

        private class IntShortPair : AbstractPair
        {
            private int clear;
            private short encrypted;

            public IntShortPair(int clear, long encrypted)
            {
                this.clear = clear;
                this.encrypted = (short)encrypted;
            }

            public int clear()
            {
                return clear;
            }

            public long encrypted()
            {
                return encrypted;
            }
        }

        private class IntIntPair : AbstractPair
        {
            private int clear;
            private int encrypted;

            public IntIntPair(int clear, long encrypted)
            {
                this.clear = clear;
                this.encrypted = (int)encrypted;
            }

            public int clear()
            {
                return clear;
            }

            public long encrypted()
            {
                return encrypted;
            }
        }

        private class IntLongPair : AbstractPair
        {
            private int clear;
            private long encrypted;

            public IntLongPair(int clear, long encrypted)
            {
                this.clear = clear;
                this.encrypted = encrypted;
            }

            public int clear()
            {
                return clear;
            }

            public long encrypted()
            {
                return encrypted;
            }
        }

        private abstract class AbstractPair : Pair
        {

            public override bool Equals(object o)
            {
                if (this == o)
                {
                    return true;
                }
                if (o == null || getClass() != o.getClass())
                {
                    return false;
                }

                Pair pair = (Pair)o;

                if (clear() != pair.clear())
                {
                    return false;
                }
                if (encrypted() != pair.encrypted())
                {
                    return false;
                }

                return true;
            }

            public override string ToString()
            {
                return "P(" + clear() + "|" + encrypted() + ")";
            }
        }
    }
}