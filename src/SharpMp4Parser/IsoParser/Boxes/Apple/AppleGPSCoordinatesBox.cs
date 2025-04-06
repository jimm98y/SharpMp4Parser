using SharpMp4Parser.IsoParser.Support;
using SharpMp4Parser.IsoParser.Tools;
using SharpMp4Parser.Java;

namespace SharpMp4Parser.IsoParser.Boxes.Apple
{
    /**
     * Created by marwatk on 02/27/15
     */
    public class AppleGPSCoordinatesBox : AbstractBox
    {
        public const string TYPE = "©xyz";
        private const int DEFAULT_LANG = 5575; //Empirical

        string coords;
        int lang = DEFAULT_LANG; //? Docs says lang, but it doesn't match anything in the traditional language map

        public AppleGPSCoordinatesBox() : base(TYPE)
        { }

        public string getValue()
        {
            return coords;
        }

        public void setValue(string iso6709String)
        {
            lang = DEFAULT_LANG;
            coords = iso6709String;
        }

        protected override long getContentSize()
        {
            return 4 + Utf8.utf8StringLengthInBytes(coords);
        }

        protected override void getContent(ByteBuffer byteBuffer)
        {
            byteBuffer.putShort((short)coords.Length);
            byteBuffer.putShort((short)lang);
            byteBuffer.put(Utf8.convert(coords));
        }

        protected override void _parseDetails(ByteBuffer content)
        {
            int length = content.getShort();
            lang = content.getShort(); //Not sure if this is accurate. It always seems to be 15 c7
            byte[] bytes = new byte[length];
            content.get(bytes);
            coords = Utf8.convert(bytes);
        }

        public override string ToString()
        {
            return "AppleGPSCoordinatesBox[" + coords + "]";
        }
    }
}
