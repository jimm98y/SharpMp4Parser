using SharpMp4Parser.IsoParser.Support;
using SharpMp4Parser.Java;

namespace SharpMp4Parser.IsoParser.Boxes.Apple
{
    /**
     * <h1>4cc = "{@value #TYPE}"</h1>
     * This extension specifies the height-to-width ratio of pixels found in
     * the video sample. This is a required extension for MPEG-4 and
     * uncompressed Y ́CbCr video formats when non-square pixels are used. It
     * is optional when square pixels are used.
     */
    public class PixelAspectRationAtom : AbstractBox
    {
        public const string TYPE = "pasp";
        /**
         * An unsigned 32-bit integer specifying the horizontal spacing of pixels,
         * such as luma sampling instants for Y ́CbCr or YUV video.
         */
        private int hSpacing;
        /**
         * An unsigned 32-bit integer specifying the vertical spacing of pixels,
         * such as video picture lines.
         */
        private int vSpacing;

        public PixelAspectRationAtom() : base(TYPE)
        { }

        public int gethSpacing()
        {
            return hSpacing;
        }

        public void sethSpacing(int hSpacing)
        {
            this.hSpacing = hSpacing;
        }

        public int getvSpacing()
        {
            return vSpacing;
        }

        public void setvSpacing(int vSpacing)
        {
            this.vSpacing = vSpacing;
        }

        protected override long getContentSize()
        {
            return 8;
        }

        protected override void getContent(ByteBuffer byteBuffer)
        {
            byteBuffer.putInt(hSpacing);
            byteBuffer.putInt(vSpacing);

        }

        protected override void _parseDetails(ByteBuffer content)
        {
            hSpacing = content.getInt();
            vSpacing = content.getInt();
        }
    }
}