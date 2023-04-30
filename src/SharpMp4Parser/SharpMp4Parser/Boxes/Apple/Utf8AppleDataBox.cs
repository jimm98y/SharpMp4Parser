using SharpMp4Parser.Java;
using SharpMp4Parser.Tools;
using System.Text;

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
            if (!IsParsed())
            {
                parseDetails();
            }
            return value;
        }

        public void setValue(string value)
        {
            this.value = value;
        }

        protected override byte[] writeData()
        {
            return Utf8.convert(value);
        }

        protected override int getDataLength()
        {
            return Encoding.UTF8.GetBytes(value).Length;
        }

        protected override void parseData(ByteBuffer data)
        {
            value = IsoTypeReader.readString(data, data.remaining());
        }
    }
}