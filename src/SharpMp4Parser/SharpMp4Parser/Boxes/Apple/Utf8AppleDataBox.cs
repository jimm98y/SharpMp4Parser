namespace SharpMp4Parser.Boxes.Apple
{
    /**
     *
     */
    public abstract class Utf8AppleDataBox : AppleDataBox
    {
        string value;

        protected Utf8AppleDataBox(string type) : base(type, 1)
        {  }

        public string getValue()
        {
            //patched by Toias Bley / UltraMixer
            if (!isParsed())
            {
                parseDetails();
            }
            return value;
        }

        public void setValue(string value)
        {
            this.value = value;
        }

        public byte[] writeData()
        {
            return Utf8.convert(value);
        }

        protected override int getDataLength()
        {
            return value.getBytes(Charset.forName("UTF-8")).length;
        }

        protected override void parseData(ByteBuffer data)
        {
            value = IsoTypeReader.readString(data, data.remaining());
        }
    }
}