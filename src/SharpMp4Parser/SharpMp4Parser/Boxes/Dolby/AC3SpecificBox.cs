using SharpMp4Parser.Boxes.ISO14496.Part1.ObjectDescriptors;
using SharpMp4Parser.Java;
using SharpMp4Parser.Support;

namespace SharpMp4Parser.Boxes.Dolby
{
    /**
     * <h1>4cc = "{@value #TYPE}"</h1>
     */
    public class AC3SpecificBox : AbstractBox
    {
        public const string TYPE = "dac3";
        int fscod;
        int bsid;
        int bsmod;
        int acmod;
        int lfeon;
        int bitRateCode;
        int reserved;

        public AC3SpecificBox() : base(TYPE)
        { }

        protected override long getContentSize()
        {
            return 3;
        }

        protected override void _parseDetails(ByteBuffer content)
        {
            BitReaderBuffer brb = new BitReaderBuffer(content);
            fscod = brb.readBits(2);
            bsid = brb.readBits(5);
            bsmod = brb.readBits(3);
            acmod = brb.readBits(3);
            lfeon = brb.readBits(1);
            bitRateCode = brb.readBits(5);
            reserved = brb.readBits(5);
        }

        protected override void getContent(ByteBuffer byteBuffer)
        {
            BitWriterBuffer bwb = new BitWriterBuffer(byteBuffer);
            bwb.writeBits(fscod, 2);
            bwb.writeBits(bsid, 5);
            bwb.writeBits(bsmod, 3);
            bwb.writeBits(acmod, 3);
            bwb.writeBits(lfeon, 1);
            bwb.writeBits(bitRateCode, 5);
            bwb.writeBits(reserved, 5);
        }

        public int getFscod()
        {
            return fscod;
        }

        public void setFscod(int fscod)
        {
            this.fscod = fscod;
        }

        public int getBsid()
        {
            return bsid;
        }

        public void setBsid(int bsid)
        {
            this.bsid = bsid;
        }

        public int getBsmod()
        {
            return bsmod;
        }

        public void setBsmod(int bsmod)
        {
            this.bsmod = bsmod;
        }

        public int getAcmod()
        {
            return acmod;
        }

        public void setAcmod(int acmod)
        {
            this.acmod = acmod;
        }

        public int getLfeon()
        {
            return lfeon;
        }

        public void setLfeon(int lfeon)
        {
            this.lfeon = lfeon;
        }

        public int getBitRateCode()
        {
            return bitRateCode;
        }

        public void setBitRateCode(int bitRateCode)
        {
            this.bitRateCode = bitRateCode;
        }

        public int getReserved()
        {
            return reserved;
        }

        public void setReserved(int reserved)
        {
            this.reserved = reserved;
        }

        public override string ToString()
        {
            return "AC3SpecificBox{" +
                    "fscod=" + fscod +
                    ", bsid=" + bsid +
                    ", bsmod=" + bsmod +
                    ", acmod=" + acmod +
                    ", lfeon=" + lfeon +
                    ", bitRateCode=" + bitRateCode +
                    ", reserved=" + reserved +
                    '}';
        }
    }
}
