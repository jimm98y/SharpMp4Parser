using SharpMp4Parser.Support;

namespace SharpMp4Parser.Boxes.ISO14496.Part12
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
            return this.GetType().Name + "[entryCount=" + getChunkOffsets().Length + "]";
        }
    }
}