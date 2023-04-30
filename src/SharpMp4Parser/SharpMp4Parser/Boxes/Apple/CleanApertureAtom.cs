using SharpMp4Parser.Java;
using SharpMp4Parser.Support;
using SharpMp4Parser.Tools;

namespace SharpMp4Parser.Boxes.Apple
{
    /**
     * <h1>4cc = "{@value #TYPE}"</h1>
     * A container atom that stores information for video correction in the form of three required atoms.
     * This atom is optionally included in the track atom. The type of the track aperture mode dimensions
     * atom is 'tapt'.
     */
    public class CleanApertureAtom : AbstractFullBox
    {
        public const string TYPE = "clef";

        double width;
        double height;

        public CleanApertureAtom() : base(TYPE)
        { }


        protected override long getContentSize()
        {
            return 12;
        }

        protected override void getContent(ByteBuffer byteBuffer)
        {
            writeVersionAndFlags(byteBuffer);
            IsoTypeWriter.writeFixedPoint1616(byteBuffer, width);
            IsoTypeWriter.writeFixedPoint1616(byteBuffer, height);
        }

        protected override void _parseDetails(ByteBuffer content)
        {
            parseVersionAndFlags(content);
            width = IsoTypeReader.readFixedPoint1616(content);
            height = IsoTypeReader.readFixedPoint1616(content);
        }

        public double getWidth()
        {
            return width;
        }

        public void setWidth(double width)
        {
            this.width = width;
        }

        public double getHeight()
        {
            return height;
        }

        public void setHeight(double height)
        {
            this.height = height;
        }
    }
}
