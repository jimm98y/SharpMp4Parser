using SharpMp4Parser.Java;
using SharpMp4Parser.Support;
using SharpMp4Parser.Tools;
using System;
using System.Text;

namespace SharpMp4Parser.Boxes.Microsoft
{
    /**
     * <h1>4cc = "uuid", d08a4f18-10f3-4a82-b6c8-32d8aba183d3</h1>
     * aligned(8) class UuidBasedProtectionSystemSpecificHeaderBox extends FullBox(‘uuid’,
     * extended_type=0xd08a4f18-10f3-4a82-b6c8-32d8aba183d3,
     * version=0, flags=0)
     * {
     * unsigned int(8)[16] SystemID;
     * unsigned int(32) DataSize;
     * unsigned int(8)[DataSize] Data;
     * }
     */
    public class UuidBasedProtectionSystemSpecificHeaderBox : AbstractFullBox
    {
        public static byte[] USER_TYPE = new byte[]{(byte) 0xd0, (byte) 0x8a, 0x4f, 0x18, 0x10, (byte) 0xf3, 0x4a, (byte) 0x82,
            (byte) 0xb6, (byte) 0xc8, 0x32, (byte) 0xd8, (byte) 0xab, (byte) 0xa1, (byte) 0x83, (byte) 0xd3};

        Uuid systemId;

        ProtectionSpecificHeader protectionSpecificHeader;

        public UuidBasedProtectionSystemSpecificHeaderBox() : base("uuid", USER_TYPE)
        { }

        protected override long getContentSize()
        {
            return 24 + protectionSpecificHeader.getData().limit();
        }

        public override byte[] getUserType()
        {
            return USER_TYPE;
        }

        protected override void getContent(ByteBuffer byteBuffer)
        {
            writeVersionAndFlags(byteBuffer);
            IsoTypeWriter.writeUInt64(byteBuffer, systemId.MostSignificantBits);
            IsoTypeWriter.writeUInt64(byteBuffer, systemId.LeastSignificantBits);
            ByteBuffer data = protectionSpecificHeader.getData();
            ((Java.Buffer)data).rewind();
            IsoTypeWriter.writeUInt32(byteBuffer, data.limit());
            byteBuffer.put(data);
        }

        protected override void _parseDetails(ByteBuffer content)
        {
            parseVersionAndFlags(content);
            byte[] systemIdBytes = new byte[16];
            content.get(systemIdBytes);
            systemId = UUIDConverter.convert(systemIdBytes);
            int dataSize = CastUtils.l2i(IsoTypeReader.readUInt32(content));
            protectionSpecificHeader = ProtectionSpecificHeader.createFor(systemId, content);
        }

        public Uuid getSystemId()
        {
            return systemId;
        }

        public void setSystemId(Uuid systemId)
        {
            this.systemId = systemId;
        }

        public string getSystemIdString()
        {
            return systemId.ToString();
        }

        public ProtectionSpecificHeader getProtectionSpecificHeader()
        {
            return protectionSpecificHeader;
        }

        public void setProtectionSpecificHeader(ProtectionSpecificHeader protectionSpecificHeader)
        {
            this.protectionSpecificHeader = protectionSpecificHeader;
        }

        public string getProtectionSpecificHeaderString()
        {
            return protectionSpecificHeader.ToString();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("UuidBasedProtectionSystemSpecificHeaderBox");
            sb.Append("{systemId=").Append(systemId.ToString());
            sb.Append(", dataSize=").Append(protectionSpecificHeader.getData().limit());
            sb.Append('}');
            return sb.ToString();
        }
    }
}
