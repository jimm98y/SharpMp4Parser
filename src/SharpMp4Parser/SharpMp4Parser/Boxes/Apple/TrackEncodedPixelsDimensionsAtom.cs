namespace SharpMp4Parser.Boxes.Apple
{
    /**
     * <h1>4cc = "{@value #TYPE}"</h1>
     * This atom carries the pixel dimensions of the track’s encoded pixels.
     * The type of the track encoded pixels dimensions atom is 'enof'.
     */
    public class TrackEncodedPixelsDimensionsAtom : AbstractFullBox
    {
        public const string TYPE = "enof";

        double width;
        double height;

        public TrackEncodedPixelsDimensionsAtom() : base(TYPE)
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
