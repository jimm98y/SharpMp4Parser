using SharpMp4Parser.Boxes.ISO14496.Part1.ObjectDescriptors;
using SharpMp4Parser.Java;
using SharpMp4Parser.Support;

namespace SharpMp4Parser.Boxes.Dolby
{
    /**
     * <h1>4cc = "{@value #TYPE}"</h1>
     */
    public class MLPSpecificBox : AbstractBox
    {
        public const string TYPE = "dmlp";

        int format_info;
        int peak_data_rate;
        int reserved;
        int reserved2;

        public MLPSpecificBox() : base(TYPE)
        { }

        protected override long getContentSize()
        {
            return 10;
        }

        protected override void _parseDetails(ByteBuffer content)
        {
            BitReaderBuffer brb = new BitReaderBuffer(content);
            format_info = brb.readBits(32);
            peak_data_rate = brb.readBits(15);
            reserved = brb.readBits(1);
            reserved2 = brb.readBits(32);
        }

        protected override void getContent(ByteBuffer byteBuffer)
        {
            BitWriterBuffer bwb = new BitWriterBuffer(byteBuffer);
            bwb.writeBits(format_info, 32);
            bwb.writeBits(peak_data_rate, 15);
            bwb.writeBits(reserved, 1);
            bwb.writeBits(reserved2, 32);
            //To change body of implemented methods use File | Settings | File Templates.
        }

        public int getFormat_info()
        {
            return format_info;
        }

        public void setFormat_info(int format_info)
        {
            this.format_info = format_info;
        }

        public int getPeak_data_rate()
        {
            return peak_data_rate;
        }

        public void setPeak_data_rate(int peak_data_rate)
        {
            this.peak_data_rate = peak_data_rate;
        }

        public int getReserved()
        {
            return reserved;
        }

        public void setReserved(int reserved)
        {
            this.reserved = reserved;
        }

        public int getReserved2()
        {
            return reserved2;
        }

        public void setReserved2(int reserved2)
        {
            this.reserved2 = reserved2;
        }
    }
}