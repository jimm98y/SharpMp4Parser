using SharpMp4Parser.IsoParser.Boxes.ISO23001.Part7;

namespace SharpMp4Parser.IsoParser.Boxes.Microsoft
{
    /**
     * <pre>
     * aligned(8) class SampleEncryptionBox extends FullBox(‘uuid’, extended_type= 0xA2394F52-5A9B-4f14-A244-6C427C648DF4, version=0, flags=0)
     * {
     *  if (flags &amp; 0x000001)
     *  {
     *   unsigned int(24) AlgorithmID;
     *   unsigned int(8) IV_size;
     *   unsigned int(8)[16] KID;
     *  }
     *  unsigned int (32) sample_count;
     *  {
     *   unsigned int(IV_size) InitializationVector;
     *   if (flags &amp; 0x000002)
     *   {
     *    unsigned int(16) NumberOfEntries;
     *    {
     *     unsigned int(16) BytesOfClearData;
     *     unsigned int(32) BytesOfEncryptedData;
     *    } [ NumberOfEntries]
     *   }
     *  }[ sample_count ]
     * }
     * </pre>
     */
    public class PiffSampleEncryptionBox : AbstractSampleEncryptionBox
    {

        /**
         * Creates a AbstractSampleEncryptionBox for non-h264 tracks.
         */
        public PiffSampleEncryptionBox() : base("uuid")
        { }

        public override byte[] getUserType()
        {
            return new byte[] { 0xA2, 0x39, 0x4F, 0x52, 0x5A, 0x9B, 0x4f, 0x14, 0xA2, 0x44, 0x6C, 0x42, 0x7C, 0x64, 0x8D, 0xF4 };
        }

        public int getAlgorithmId()
        {
            return algorithmId;
        }

        public void setAlgorithmId(int algorithmId)
        {
            this.algorithmId = algorithmId;
        }

        public int getIvSize()
        {
            return ivSize;
        }

        public void setIvSize(int ivSize)
        {
            this.ivSize = ivSize;
        }

        public byte[] getKid()
        {
            return kid;
        }

        public void setKid(byte[] kid)
        {
            this.kid = kid;
        }

        protected override bool isOverrideTrackEncryptionBoxParameters()
        {
            return (getFlags() & 0x1) > 0;
        }


        public void setOverrideTrackEncryptionBoxParameters(bool b)
        {
            if (b)
            {
                setFlags(getFlags() | 0x1);
            }
            else
            {
                setFlags(getFlags() & (0xffffff ^ 0x1));
            }
        }
    }
}
