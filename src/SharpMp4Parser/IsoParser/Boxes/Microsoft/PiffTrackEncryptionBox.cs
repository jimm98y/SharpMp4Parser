using SharpMp4Parser.IsoParser.Boxes.ISO23001.Part7;

namespace SharpMp4Parser.IsoParser.Boxes.Microsoft
{
    /**
     * aligned(8) class TrackEncryptionBox extends FullBox(‘uuid’,
     * extended_type=0x8974dbce-7be7-4c51-84f9-7148f9882554, version=0,
     * flags=0)
     * {
     * unsigned int(24) default_AlgorithmID;
     * unsigned int(8) default_IV_size;
     * unsigned int(8)[16] default_KID;
     * }
     */
    public class PiffTrackEncryptionBox : AbstractTrackEncryptionBox
    {
        public PiffTrackEncryptionBox() : base("uuid")
        { }

        public override byte[] getUserType()
        {
            return new byte[]{ 0x89, 0x74,  0xdb,  0xce, 0x7b,  0xe7, 0x4c, 0x51,
                 0x84,  0xf9, 0x71, 0x48,  0xf9,  0x88, 0x25, 0x54};
        }

        public override int getFlags()
        {
            return 0;
        }
    }
}
