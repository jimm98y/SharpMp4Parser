/*  
 * Copyright 2008 CoreMedia AG, Hamburg
 *
 * Licensed under the Apache License, Version 2.0 (the License); 
 * you may not use this file except in compliance with the License. 
 * You may obtain a copy of the License at 
 * 
 *     http://www.apache.org/licenses/LICENSE-2.0 
 * 
 * Unless required by applicable law or agreed to in writing, software 
 * distributed under the License is distributed on an AS IS BASIS, 
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
 * See the License for the specific language governing permissions and 
 * limitations under the License. 
 */

using SharpMp4Parser.IsoParser.Tools;
using SharpMp4Parser.Java;

namespace SharpMp4Parser.IsoParser.Boxes.ISO14496.Part12
{
    /**
      * <h1>4cc = "{@value #TYPE}"</h1>
      * The chunk offset table gives the index of each chunk into the containing file. Defined in ISO/IEC 14496-12.
      */
    public class StaticChunkOffsetBox : ChunkOffsetBox
    {
        public const string TYPE = "stco";

        private long[] chunkOffsets = new long[0];

        public StaticChunkOffsetBox() : base(TYPE)
        { }

        public override long[] getChunkOffsets()
        {
            return chunkOffsets;
        }

        public override void setChunkOffsets(long[] chunkOffsets)
        {
            this.chunkOffsets = chunkOffsets;
        }

        protected override long getContentSize()
        {
            return 8 + chunkOffsets.Length * 4;
        }

        protected override void _parseDetails(ByteBuffer content)
        {
            parseVersionAndFlags(content);
            int entryCount = CastUtils.l2i(IsoTypeReader.readUInt32(content));
            chunkOffsets = new long[entryCount];
            for (int i = 0; i < entryCount; i++)
            {
                chunkOffsets[i] = IsoTypeReader.readUInt32(content);
            }
        }

        protected override void getContent(ByteBuffer byteBuffer)
        {
            writeVersionAndFlags(byteBuffer);
            IsoTypeWriter.writeUInt32(byteBuffer, chunkOffsets.Length);
            foreach (long chunkOffset in chunkOffsets)
            {
                IsoTypeWriter.writeUInt32(byteBuffer, chunkOffset);
            }
        }
    }
}