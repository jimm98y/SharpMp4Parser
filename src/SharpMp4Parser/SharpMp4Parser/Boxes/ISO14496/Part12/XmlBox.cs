using SharpMp4Parser.Java;
using SharpMp4Parser.Support;
using SharpMp4Parser.Tools;

namespace SharpMp4Parser.Boxes.ISO14496.Part12
{
    /**
      * <h1>4cc = "{@value #TYPE}"</h1>
      */
    public class XmlBox : AbstractFullBox
    {
        public const string TYPE = "xml ";
        string xml = "";

        public XmlBox() : base(TYPE)
        { }

        public string getXml()
        {
            return xml;
        }

        public void setXml(string xml)
        {
            this.xml = xml;
        }

        protected override long getContentSize()
        {
            return 4 + Utf8.utf8StringLengthInBytes(xml);
        }

        protected override void _parseDetails(ByteBuffer content)
        {
            parseVersionAndFlags(content);
            xml = IsoTypeReader.readString(content, content.remaining());
        }

        protected override void getContent(ByteBuffer byteBuffer)
        {
            writeVersionAndFlags(byteBuffer);
            byteBuffer.put(Utf8.convert(xml));
        }

        public override string ToString()
        {
            return "XmlBox{" +
                    "xml='" + xml + '\'' +
                    '}';
        }
    }
}
