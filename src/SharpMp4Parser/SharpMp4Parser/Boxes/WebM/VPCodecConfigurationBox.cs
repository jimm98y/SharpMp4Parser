using SharpMp4Parser.Boxes.ISO14496.Part1.ObjectDescriptors;
using SharpMp4Parser.Java;
using SharpMp4Parser.Support;

namespace SharpMp4Parser.Boxes.WebM
{
    public class VPCodecConfigurationBox : AbstractFullBox
    {
        public const string TYPE = "vpcC";

        private int profile;
        private int level;
        private int bitDepth;
        private int chromaSubsampling;
        private int videoFullRangeFlag;
        private int colourPrimaries;
        private int transferCharacteristics;
        private int matrixCoefficients;

        private byte[] codecIntializationData;


        public VPCodecConfigurationBox() : base(TYPE)
        { }

        protected override long getContentSize()
        {
            return codecIntializationData.Length + 12;
        }

        protected override void getContent(ByteBuffer byteBuffer)
        {
            writeVersionAndFlags(byteBuffer);
            BitWriterBuffer bwb = new BitWriterBuffer(byteBuffer);
            bwb.writeBits(profile, 8);
            bwb.writeBits(level, 8);
            bwb.writeBits(bitDepth, 4);
            bwb.writeBits(chromaSubsampling, 3);
            bwb.writeBits(videoFullRangeFlag, 1);
            bwb.writeBits(colourPrimaries, 8);
            bwb.writeBits(transferCharacteristics, 8);
            bwb.writeBits(matrixCoefficients, 8);
            bwb.writeBits(codecIntializationData.Length, 16);
            byteBuffer.put(codecIntializationData);
        }

        // aligned (8) class VPCodecConfigurationRecord {
        //    unsigned int (8)     profile;
        //    unsigned int (8)     level;
        //    unsigned int (4)     bitDepth;
        //    unsigned int (3)     chromaSubsampling;
        //    unsigned int (1)     videoFullRangeFlag;
        //    unsigned int (8)     colourPrimaries;
        //    unsigned int (8)     transferCharacteristics;
        //    unsigned int (8)     matrixCoefficients;
        //    unsigned int (16)    codecIntializationDataSize;
        //    unsigned int (8)[]   codecIntializationData;
        //}

        protected override void _parseDetails(ByteBuffer content)
        {
            parseVersionAndFlags(content);
            BitReaderBuffer brb = new BitReaderBuffer(content);
            profile = brb.readBits(8);
            level = brb.readBits(8);
            bitDepth = brb.readBits(4);
            chromaSubsampling = brb.readBits(3);
            videoFullRangeFlag = brb.readBits(1);
            colourPrimaries = brb.readBits(8);
            transferCharacteristics = brb.readBits(8);
            matrixCoefficients = brb.readBits(8);
            int len = brb.readBits(16);
            codecIntializationData = new byte[len];
            content.get(codecIntializationData);
        }

        public int getProfile()
        {
            return profile;
        }

        public void setProfile(int profile)
        {
            this.profile = profile;
        }

        public int getLevel()
        {
            return level;
        }

        public void setLevel(int level)
        {
            this.level = level;
        }

        public int getBitDepth()
        {
            return bitDepth;
        }

        public void setBitDepth(int bitDepth)
        {
            this.bitDepth = bitDepth;
        }

        public int getChromaSubsampling()
        {
            return chromaSubsampling;
        }

        public void setChromaSubsampling(int chromaSubsampling)
        {
            this.chromaSubsampling = chromaSubsampling;
        }

        public int getVideoFullRangeFlag()
        {
            return videoFullRangeFlag;
        }

        public void setVideoFullRangeFlag(int videoFullRangeFlag)
        {
            this.videoFullRangeFlag = videoFullRangeFlag;
        }

        public int getColourPrimaries()
        {
            return colourPrimaries;
        }

        public void setColourPrimaries(int colourPrimaries)
        {
            this.colourPrimaries = colourPrimaries;
        }

        public int getTransferCharacteristics()
        {
            return transferCharacteristics;
        }

        public void setTransferCharacteristics(int transferCharacteristics)
        {
            this.transferCharacteristics = transferCharacteristics;
        }

        public int getMatrixCoefficients()
        {
            return matrixCoefficients;
        }

        public void setMatrixCoefficients(int matrixCoefficients)
        {
            this.matrixCoefficients = matrixCoefficients;
        }

        public byte[] getCodecIntializationData()
        {
            return codecIntializationData;
        }

        public void setCodecIntializationData(byte[] codecIntializationData)
        {
            this.codecIntializationData = codecIntializationData;
        }

        public override string ToString()
        {
            return "VPCodecConfigurationBox{" +
                    "profile=" + profile +
                    ", level=" + level +
                    ", bitDepth=" + bitDepth +
                    ", chromaSubsampling=" + chromaSubsampling +
                    ", videoFullRangeFlag=" + videoFullRangeFlag +
            ", colourPrimaries=" + colourPrimaries +
                    ", transferCharacteristics=" + transferCharacteristics +
                    ", matrixCoefficients=" + matrixCoefficients +
                    ", codecIntializationData=" + Arrays.toString(codecIntializationData) +
                    '}';
        }
    }
}