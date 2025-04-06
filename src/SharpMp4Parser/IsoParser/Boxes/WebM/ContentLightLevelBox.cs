using SharpMp4Parser.IsoParser.Support;
using SharpMp4Parser.IsoParser.Tools;
using SharpMp4Parser.Java;

namespace SharpMp4Parser.IsoParser.Boxes.WebM
{
    public class ContentLightLevelBox : AbstractFullBox
    {
        public const string TYPE = "CoLL";

        private int maxCLL;
        private int maxFALL;

        protected ContentLightLevelBox() : base(TYPE)
        { }

        protected override long getContentSize()
        {
            return 8;
        }

        protected override void getContent(ByteBuffer byteBuffer)
        {
            writeVersionAndFlags(byteBuffer);
            IsoTypeWriter.writeUInt16(byteBuffer, maxCLL);
            IsoTypeWriter.writeUInt16(byteBuffer, maxFALL);
        }

        protected override void _parseDetails(ByteBuffer content)
        {
            parseVersionAndFlags(content);
            maxCLL = IsoTypeReader.readUInt16(content);
            maxFALL = IsoTypeReader.readUInt16(content);
        }

        public int getMaxCLL()
        {
            return maxCLL;
        }

        public void setMaxCLL(int maxCLL)
        {
            this.maxCLL = maxCLL;
        }

        public int getMaxFALL()
        {
            return maxFALL;
        }

        public void setMaxFALL(int maxFALL)
        {
            this.maxFALL = maxFALL;
        }
    }
}
