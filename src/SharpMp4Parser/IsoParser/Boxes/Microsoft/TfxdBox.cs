using SharpMp4Parser.IsoParser.Support;
using SharpMp4Parser.IsoParser.Tools;
using SharpMp4Parser.Java;

namespace SharpMp4Parser.IsoParser.Boxes.Microsoft
{
    /**
     * <h1>4cc = "uuid", 6d1d9b05-42d5-44e6-80e2-141daff757b2</h1>
     * The syntax of the fields defined in this section, specified in ABNF [RFC5234], is as follows:
     * TfxdBox = TfxdBoxLength TfxdBoxType [TfxdBoxLongLength] TfxdBoxUUID TfxdBoxFields
     * TfxdBoxChildren
     * TfxdBoxType = "u" "u" "i" "d"
     * TfxdBoxLength = BoxLength
     * TfxdBoxLongLength = LongBoxLength
     * TfxdBoxUUID = %x6D %x1D %x9B %x05 %x42 %xD5 %x44 %xE6
     * %x80 %xE2 %x14 %x1D %xAF %xF7 %x57 %xB2
     * TfxdBoxFields = TfxdBoxVersion
     * TfxdBoxFlags
     * TfxdBoxDataFields32 / TfxdBoxDataFields64
     * TfxdBoxVersion = %x00 / %x01
     * TfxdBoxFlags = 24*24 RESERVED_BIT
     * TfxdBoxDataFields32 = FragmentAbsoluteTime32
     * FragmentDuration32
     * TfxdBoxDataFields64 = FragmentAbsoluteTime64
     * FragmentDuration64
     * FragmentAbsoluteTime64 = UNSIGNED_INT32
     * FragmentDuration64 = UNSIGNED_INT32
     * FragmentAbsoluteTime64 = UNSIGNED_INT64
     * FragmentDuration64 = UNSIGNED_INT64
     * TfxdBoxChildren = *( VendorExtensionUUIDBox )
     */
    //@ExtendedUserType(uuid = "6d1d9b05-42d5-44e6-80e2-141daff757b2")
    public class TfxdBox : AbstractFullBox
    {
        public long fragmentAbsoluteTime;
        public long fragmentAbsoluteDuration;

        public TfxdBox() : base("uuid")
        { }

        public override byte[] getUserType()
        {
            return new byte[]{ 0x6d,  0x1d,  0x9b,  0x05,  0x42,  0xd5,  0x44,
                 0xe6,  0x80,  0xe2, 0x14,  0x1d,  0xaf,  0xf7,  0x57,  0xb2};
        }

        protected override long getContentSize()
        {
            return getVersion() == 0x01 ? 20 : 12;
        }

        protected override void _parseDetails(ByteBuffer content)
        {
            parseVersionAndFlags(content);

            if (getVersion() == 0x01)
            {
                fragmentAbsoluteTime = IsoTypeReader.readUInt64(content);
                fragmentAbsoluteDuration = IsoTypeReader.readUInt64(content);
            }
            else
            {
                fragmentAbsoluteTime = IsoTypeReader.readUInt32(content);
                fragmentAbsoluteDuration = IsoTypeReader.readUInt32(content);
            }
        }

        protected override void getContent(ByteBuffer byteBuffer)
        {
            writeVersionAndFlags(byteBuffer);
            if (getVersion() == 0x01)
            {
                IsoTypeWriter.writeUInt64(byteBuffer, fragmentAbsoluteTime);
                IsoTypeWriter.writeUInt64(byteBuffer, fragmentAbsoluteDuration);
            }
            else
            {
                IsoTypeWriter.writeUInt32(byteBuffer, fragmentAbsoluteTime);
                IsoTypeWriter.writeUInt32(byteBuffer, fragmentAbsoluteDuration);
            }
        }

        public long getFragmentAbsoluteTime()
        {
            return fragmentAbsoluteTime;
        }

        public long getFragmentAbsoluteDuration()
        {
            return fragmentAbsoluteDuration;
        }
    }
}
