using SharpMp4Parser.IsoParser.Support;
using SharpMp4Parser.IsoParser.Tools;
using SharpMp4Parser.Java;
using System;
using System.Linq;

namespace SharpMp4Parser.IsoParser.Boxes.ISO23001.Part7
{
    /**
     *
     */
    public abstract class AbstractTrackEncryptionBox : AbstractFullBox
    {
        int defaultAlgorithmId;
        int defaultIvSize;
        byte[] default_KID;

        protected AbstractTrackEncryptionBox(string type) : base(type)
        { }

        public int getDefaultAlgorithmId()
        {
            return defaultAlgorithmId;
        }

        public void setDefaultAlgorithmId(int defaultAlgorithmId)
        {
            this.defaultAlgorithmId = defaultAlgorithmId;
        }

        public int getDefaultIvSize()
        {
            return defaultIvSize;
        }

        public void setDefaultIvSize(int defaultIvSize)
        {
            this.defaultIvSize = defaultIvSize;
        }

        public Uuid getDefault_KID()
        {
            ByteBuffer b = ByteBuffer.wrap(default_KID);
            b.order(ByteOrder.BIG_ENDIAN);
            return new Uuid(b.getLong(), b.getLong());
        }

        public void setDefault_KID(Uuid uuid)
        {
            ByteBuffer bb = ByteBuffer.wrap(new byte[16]);
            bb.putLong(uuid.MostSignificantBits);
            bb.putLong(uuid.LeastSignificantBits);
            default_KID = bb.array();
        }

        protected override void _parseDetails(ByteBuffer content)
        {
            parseVersionAndFlags(content);
            defaultAlgorithmId = IsoTypeReader.readUInt24(content);
            defaultIvSize = IsoTypeReader.readUInt8(content);
            default_KID = new byte[16];
            content.get(default_KID);
        }

        protected override void getContent(ByteBuffer byteBuffer)
        {
            writeVersionAndFlags(byteBuffer);
            IsoTypeWriter.writeUInt24(byteBuffer, defaultAlgorithmId);
            IsoTypeWriter.writeUInt8(byteBuffer, defaultIvSize);
            byteBuffer.put(default_KID);
        }

        protected override long getContentSize()
        {
            return 24;
        }

        public override bool Equals(object o)
        {
            if (this == o) return true;
            if (o == null || GetType() != o.GetType()) return false;

            AbstractTrackEncryptionBox that = (AbstractTrackEncryptionBox)o;

            if (defaultAlgorithmId != that.defaultAlgorithmId) return false;
            if (defaultIvSize != that.defaultIvSize) return false;
            if (!default_KID.SequenceEqual(that.default_KID)) return false;

            return true;
        }

        public override int GetHashCode()
        {
            int result = defaultAlgorithmId;
            result = 31 * result + defaultIvSize;
            result = 31 * result + (default_KID != null ? Arrays.hashCode(default_KID) : 0);
            return result;
        }
    }
}