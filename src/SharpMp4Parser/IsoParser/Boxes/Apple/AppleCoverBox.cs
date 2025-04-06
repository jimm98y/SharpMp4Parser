using SharpMp4Parser.Java;

namespace SharpMp4Parser.IsoParser.Boxes.Apple
{
    /**
     * Created by Tobias Bley / UltraMixer on 04/25/2014.
     * 2014-07-22 @aldenml Added minimal support for image data manipulation (read and write).
     */
    public class AppleCoverBox : AppleDataBox
    {
        private const int IMAGE_TYPE_JPG = 13;
        private const int IMAGE_TYPE_PNG = 14;

        private byte[] data;

        public AppleCoverBox() : base("covr", 1)
        { }

        public byte[] getCoverData()
        {
            return data;
        }

        public void setJpg(byte[] data)
        {
            setImageData(data, IMAGE_TYPE_JPG);
        }

        public void setPng(byte[] data)
        {
            setImageData(data, IMAGE_TYPE_PNG);
        }

        protected override byte[] writeData()
        {
            return data;
        }

        protected override void parseData(ByteBuffer data)
        {
            this.data = new byte[data.limit()];
            data.get(this.data);
        }

        protected override int getDataLength()
        {
            return data.Length;
        }

        private void setImageData(byte[] data, int dataType)
        {
            this.data = data;
            this.dataType = dataType;
        }
    }
}
