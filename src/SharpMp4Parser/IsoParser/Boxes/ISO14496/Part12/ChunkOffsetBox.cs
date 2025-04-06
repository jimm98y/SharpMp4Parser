using SharpMp4Parser.IsoParser.Support;

namespace SharpMp4Parser.IsoParser.Boxes.ISO14496.Part12
{
    /**
     * Abstract Chunk Offset Box
     */
    public abstract class ChunkOffsetBox : AbstractFullBox
    {
        public ChunkOffsetBox(string type) : base(type)
        { }

        public abstract long[] getChunkOffsets();

        public abstract void setChunkOffsets(long[] chunkOffsets);

        public override string ToString()
        {
            return GetType().Name + "[entryCount=" + getChunkOffsets().Length + "]";
        }
    }
}