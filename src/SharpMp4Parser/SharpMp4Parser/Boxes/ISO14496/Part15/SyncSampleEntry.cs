using SharpMp4Parser.Boxes.SampleGrouping;
using SharpMp4Parser.Java;
using SharpMp4Parser.Tools;

namespace SharpMp4Parser.Boxes.ISO14496.Part15
{
    /**
     * A sync sample sample group entry identifies samples containing a sync sample of a specific type.
     */
    public class SyncSampleEntry : GroupEntry
    {
        public const string TYPE = "sync";

        int reserved;
        int nalUnitType;

        public override void parse(ByteBuffer byteBuffer)
        {
            int a = IsoTypeReader.readUInt8(byteBuffer);
            reserved = (a & 0xC0) >> 6;
            nalUnitType = a & 0x3F;
        }

        public override ByteBuffer get()
        {
            ByteBuffer b = ByteBuffer.allocate(1);
            IsoTypeWriter.writeUInt8(b, (nalUnitType + (reserved << 6)));
            return (ByteBuffer)((Buffer)b).rewind();
        }

        public override bool Equals(object o)
        {
            if (this == o) return true;
            if (o == null || GetType() != o.GetType()) return false;

            SyncSampleEntry that = (SyncSampleEntry)o;

            if (nalUnitType != that.nalUnitType) return false;
            if (reserved != that.reserved) return false;

            return true;
        }

        public override int GetHashCode()
        {
            int result = reserved;
            result = 31 * result + nalUnitType;
            return result;
        }

        public int getReserved()
        {
            return reserved;
        }

        public void setReserved(int reserved)
        {
            this.reserved = reserved;
        }

        public int getNalUnitType()
        {
            return nalUnitType;
        }

        public void setNalUnitType(int nalUnitType)
        {
            this.nalUnitType = nalUnitType;
        }

        public override string getType()
        {
            return TYPE;
        }

        public override string ToString()
        {
            return "SyncSampleEntry{" +
                    "reserved=" + reserved +
                    ", nalUnitType=" + nalUnitType +
                    '}';
        }
    }
}
