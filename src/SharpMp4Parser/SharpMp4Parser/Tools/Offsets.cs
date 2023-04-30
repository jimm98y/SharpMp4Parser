namespace SharpMp4Parser.Tools
{
    public class Offsets
    {
        public static long find(Container container, ParsableBox target, long offset)
        {
            long nuOffset = offset;
            foreach (Box lightBox in container.getBoxes())
            {
                if (lightBox == target)
                {
                    return nuOffset;
                }
                if (lightBox is Container)
                {
                    long r = find((Container)lightBox, target, 0);
                    if (r > 0)
                    {
                        return r + nuOffset;
                    }
                    else
                    {
                        nuOffset += lightBox.getSize();
                    }
                }
                else
                {
                    nuOffset += lightBox.getSize();
                }
            }
            return -1;
        }
    }
}
