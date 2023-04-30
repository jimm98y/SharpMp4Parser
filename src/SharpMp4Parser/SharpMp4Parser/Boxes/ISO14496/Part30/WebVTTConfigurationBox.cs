namespace SharpMp4Parser.Boxes.ISO14496.Part30
{
    /**
     * Created by sannies on 04.12.2014.
     */
    public class WebVTTConfigurationBox : AbstractBox
    {
        public const string TYPE = "vttC";

        string config = "";

        public WebVTTConfigurationBox() : base(TYPE)
        { }

        protected override long getContentSize()
        {
            return Utf8.utf8StringLengthInBytes(config);
        }

        protected override void getContent(ByteBuffer byteBuffer)
        {
            byteBuffer.put(Utf8.convert(config));
        }

        protected override void _parseDetails(ByteBuffer content)
        {
            config = IsoTypeReader.readString(content, content.remaining());
        }

        public string getConfig()
        {
            return config;
        }

        public void setConfig(string config)
        {
            this.config = config;
        }
    }
}
