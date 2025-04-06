using SharpMp4Parser.IsoParser.Tools;
using SharpMp4Parser.Java;
using System;
using System.Linq;
using System.Numerics;

namespace SharpMp4Parser.IsoParser.Boxes.ISO23001.Part7
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
                size += pairs.Length * 6;
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
            if (o == null || GetType() != o.GetType())
            {
                return false;
            }

            CencSampleAuxiliaryDataFormat entry = (CencSampleAuxiliaryDataFormat)o;

            if (!new BigInteger(iv).Equals(new BigInteger(entry.iv)))
            {
                return false;
            }
            if (pairs != null ? !pairs.SequenceEqual(entry.pairs) : entry.pairs != null)
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
                    ", pairs=" + pairs.toString() +
                    '}';
        }

        public class Pair
        {
            protected ushort clear;
            protected uint encrypted;

            public ushort Clear { get { return clear; } set { clear = value; } }
            public uint Encrypted { get { return encrypted; } set { encrypted = value; } }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }

            public override bool Equals(object o)
            {
                if (this == o)
                {
                    return true;
                }
                if (o == null || GetType() != o.GetType())
                {
                    return false;
                }

                Pair pair = (Pair)o;

                if (Clear != pair.Clear)
                {
                    return false;
                }
                if (Encrypted != pair.Encrypted)
                {
                    return false;
                }

                return true;
            }

            public override string ToString()
            {
                return "P(" + Clear + "|" + Encrypted + ")";
            }
        }

        private class ByteBytePair : AbstractPair
        {
            public new byte Clear { get { return (byte)clear; } set { clear = value; } }
            public new byte Encrypted { get { return (byte)encrypted; } set { encrypted = value; } }

            public ByteBytePair(int clear, long encrypted)
            {
                this.clear = (byte)clear;
                this.encrypted = (byte)encrypted;
            }
        }

        private class ByteShortPair : AbstractPair
        {
            public new byte Clear { get { return (byte)clear; } set { clear = value; } }
            public new short Encrypted { get { return (short)encrypted; } set { encrypted = (uint)value; } }

            public ByteShortPair(int clear, long encrypted)
            {
                this.clear = (ushort)clear;
                this.encrypted = (uint)encrypted;
            }
        }

        private class ByteIntPair : AbstractPair
        {
            public new byte Clear { get { return (byte)clear; } set { clear = value; } }
            public new int Encrypted { get { return (int)encrypted; } set { encrypted = (uint)value; } }

            public ByteIntPair(int clear, long encrypted)
            {
                this.clear = (ushort)clear;
                this.encrypted = (uint)encrypted;
            }
        }

        private class ByteLongPair : AbstractPair
        {
            public new byte Clear { get { return (byte)clear; } set { clear = value; } }
            public new long Encrypted { get { return encrypted; } set { encrypted = (uint)value; } }

            public ByteLongPair(int clear, long encrypted)
            {
                this.clear = (ushort)clear;
                this.encrypted = (uint)encrypted;
            }
        }

        private class ShortBytePair : AbstractPair
        {
            public new short Clear { get { return (short)clear; } set { clear = (ushort)value; } }
            public new byte Encrypted { get { return (byte)encrypted; } set { encrypted = value; } }

            public ShortBytePair(int clear, long encrypted)
            {
                this.clear = (ushort)clear;
                this.encrypted = (uint)encrypted;
            }
        }

        private class ShortShortPair : AbstractPair
        {
            public new short Clear { get { return (short)clear; } set { clear = (ushort)value; } }
            public new short Encrypted { get { return (short)encrypted; } set { encrypted = (uint)value; } }

            public ShortShortPair(int clear, long encrypted)
            {
                this.clear = (ushort)clear;
                this.encrypted = (uint)encrypted;
            }
        }

        private class ShortIntPair : AbstractPair
        {
            public new short Clear { get { return (short)clear; } set { clear = (ushort)value; } }
            public new int Encrypted { get { return (int)encrypted; } set { encrypted = (uint)value; } }

            public ShortIntPair(int clear, long encrypted)
            {
                this.clear = (ushort)clear;
                this.encrypted = (uint)encrypted;
            }
        }

        private class ShortLongPair : AbstractPair
        {
            public new short Clear { get { return (short)clear; } set { clear = (ushort)value; } }
            public new long Encrypted { get { return encrypted; } set { encrypted = (uint)value; } }

            public ShortLongPair(int clear, long encrypted)
            {
                this.clear = (ushort)clear;
                this.encrypted = (uint)encrypted;
            }
        }

        private class IntBytePair : AbstractPair
        {
            public new int Clear { get { return clear; } set { clear = (ushort)value; } }
            public new byte Encrypted { get { return (byte)encrypted; } set { encrypted = value; } }

            public IntBytePair(int clear, long encrypted)
            {
                this.clear = (ushort)clear;
                this.encrypted = (uint)encrypted;
            }
        }

        private class IntShortPair : AbstractPair
        {
            public new int Clear { get { return clear; } set { clear = (ushort)value; } }
            public new short Encrypted { get { return (short)encrypted; } set { encrypted = (uint)value; } }

            public IntShortPair(int clear, long encrypted)
            {
                this.clear = (ushort)clear;
                this.encrypted = (uint)encrypted;
            }
        }

        private class IntIntPair : AbstractPair
        {
            public new int Clear { get { return clear; } set { clear = (ushort)value; } }
            public new int Encrypted { get { return (int)encrypted; } set { encrypted = (uint)value; } }

            public IntIntPair(int clear, long encrypted)
            {
                this.clear = (ushort)clear;
                this.encrypted = (uint)encrypted;
            }
        }

        private class IntLongPair : AbstractPair
        {
            public new int Clear { get { return clear; } set { clear = (ushort)value; } }
            public new long Encrypted { get { return encrypted; } set { encrypted = (uint)value; } }


            public IntLongPair(int clear, long encrypted)
            {
                this.clear = (ushort)clear;
                this.encrypted = (uint)encrypted;
            }
        }

        private abstract class AbstractPair : Pair
        { }
    }
}