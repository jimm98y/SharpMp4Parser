namespace SharpMp4Parser.Boxes.ISO14496.Part12
{
    /**
     * <h1>4cc = "{@value #TYPE}"</h1>
     */
    public class SubtitleMediaHeaderBox : AbstractMediaHeaderBox
    {

        public const string TYPE = "sthd";

        public SubtitleMediaHeaderBox() : base(TYPE)
        { }

        protected long getContentSize()
        {
            return 4;
        }

        public override void _parseDetails(ByteBuffer content)
        {
            parseVersionAndFlags(content);
        }

        protected override void getContent(ByteBuffer byteBuffer)
        {
            writeVersionAndFlags(byteBuffer);
        }

        public string toString()
        {
            return "SubtitleMediaHeaderBox";
        }
    }
}
