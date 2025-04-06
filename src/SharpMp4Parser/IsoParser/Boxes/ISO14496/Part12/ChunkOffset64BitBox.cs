﻿using SharpMp4Parser.IsoParser.Tools;
using SharpMp4Parser.Java;

namespace SharpMp4Parser.IsoParser.Boxes.ISO14496.Part12
{
    /**
     * <h1>4cc = "{@value #TYPE}"</h1>
     * Abstract Chunk Offset Box
     */
    public class ChunkOffset64BitBox : ChunkOffsetBox
    {
        public const string TYPE = "co64";
        private long[] chunkOffsets;

        public ChunkOffset64BitBox() : base(TYPE)
        { }

        public override long[] getChunkOffsets()
        {
            if (!isParsed)
            {
                parseDetails();
            }

            return chunkOffsets;
        }

        public override void setChunkOffsets(long[] chunkOffsets)
        {
            this.chunkOffsets = chunkOffsets;
        }

        protected override long getContentSize()
        {
            return 8 + 8 * chunkOffsets.Length;
        }

        protected override void _parseDetails(ByteBuffer content)
        {
            parseVersionAndFlags(content);
            int entryCount = CastUtils.l2i(IsoTypeReader.readUInt32(content));
            chunkOffsets = new long[entryCount];
            for (int i = 0; i < entryCount; i++)
            {
                chunkOffsets[i] = IsoTypeReader.readUInt64(content);
            }
        }

        protected override void getContent(ByteBuffer byteBuffer)
        {
            writeVersionAndFlags(byteBuffer);
            IsoTypeWriter.writeUInt32(byteBuffer, chunkOffsets.Length);
            foreach (long chunkOffset in chunkOffsets)
            {
                IsoTypeWriter.writeUInt64(byteBuffer, chunkOffset);
            }
        }
    }
}
