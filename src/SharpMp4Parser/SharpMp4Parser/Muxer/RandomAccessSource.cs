using SharpMp4Parser.Java;

namespace SharpMp4Parser.Muxer
{
    /**
     * Allows random access to some data source as some data structure such as chunks in an mdat
     * require random access with absolute offsets.
     */
    public interface RandomAccessSource : Closeable
    {
        ByteBuffer get(long offset, long size);
    }
}
