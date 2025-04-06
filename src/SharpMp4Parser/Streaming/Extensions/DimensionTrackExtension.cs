namespace SharpMp4Parser.Streaming.Extensions
{
    /**
     * Created by sannies on 17.08.2015.
     */
    public class DimensionTrackExtension : TrackExtension
    {
        int width;
        int height;

        public DimensionTrackExtension(int width, int height)
        {
            this.width = width;
            this.height = height;
        }

        public int getWidth()
        {
            return width;
        }

        public void setWidth(int width)
        {
            this.width = width;
        }

        public int getHeight()
        {
            return height;
        }

        public void setHeight(int height)
        {
            this.height = height;
        }

        public override string ToString()
        {
            return "width=" + width + ", height=" + height;
        }
    }
}