using SharpMp4Parser.IsoParser.Support;
using SharpMp4Parser.IsoParser.Tools;
using SharpMp4Parser.Java;

namespace SharpMp4Parser.IsoParser.Boxes.ISO14496.Part30
{
    /**
     * Created by sannies on 04.12.2014.
     */
    public class WebVTTSourceLabelBox : AbstractBox
    {
        public const string TYPE = "vlab";


        string sourceLabel = "";

        public WebVTTSourceLabelBox() : base(TYPE)
        { }

        protected override long getContentSize()
        {
            return Utf8.utf8StringLengthInBytes(sourceLabel);
        }

        protected override void getContent(ByteBuffer byteBuffer)
        {
            byteBuffer.put(Utf8.convert(sourceLabel));
        }

        protected override void _parseDetails(ByteBuffer content)
        {
            sourceLabel = IsoTypeReader.readString(content, content.remaining());
        }

        public string getSourceLabel()
        {
            return sourceLabel;
        }

        public void setSourceLabel(string sourceLabel)
        {
            this.sourceLabel = sourceLabel;
        }
    }
}
