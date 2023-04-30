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

using SharpMp4Parser.Java;
using SharpMp4Parser.Support;
using SharpMp4Parser.Tools;
using System.Collections.Generic;
using System.Text;

namespace SharpMp4Parser.Boxes.ISO14496.Part12
{
    /**
     * <h1>4cc = "{@value #TYPE}"</h1>
     * <pre>
     * aligned(8) class ItemLocationBox extends FullBox('iloc', version, 0) {
     *  unsigned int(4) offset_size;
     *  unsigned int(4) length_size;
     *  unsigned int(4) base_offset_size;
     *  if (version == 1)
     *   unsigned int(4) index_size;
     *  else
     *   unsigned int(4) reserved;
     *  unsigned int(16) item_count;
     *  for (i=0; i&lt;item_count; i++) {
     *   unsigned int(16) item_ID;
     *   if (version == 1) {
     *    unsigned int(12) reserved = 0;
     *    unsigned int(4) construction_method;
     *   }
     *   unsigned int(16) data_reference_index;
     *   unsigned int(base_offset_size*8) base_offset;
     *   unsigned int(16) extent_count;
     *   for (j=0; j&lt;extent_count; j++) {
     *    if ((version == 1) &amp;&amp; (index_size &gt; 0)) {
     *     unsigned int(index_size*8) extent_index;
     *    }
     *    unsigned int(offset_size*8) extent_offset;
     *    unsigned int(length_size*8) extent_length;
     *   }
     *  }
     * }
     * </pre>
     */
    public class ItemLocationBox : AbstractFullBox
    {
        public const string TYPE = "iloc";
        public int offsetSize = 8;
        public int lengthSize = 8;
        public int baseOffsetSize = 8;
        public int indexSize = 0;
        public List<Item> items = new List<Item>();

        public ItemLocationBox() : base(TYPE)
        { }

        protected override long getContentSize()
        {
            long size = 8;
            foreach (Item item in items)
            {
                size += item.getSize();
            }
            return size;
        }

        protected override void getContent(ByteBuffer byteBuffer)
        {
            writeVersionAndFlags(byteBuffer);
            IsoTypeWriter.writeUInt8(byteBuffer, ((offsetSize << 4) | lengthSize));
            if (getVersion() == 1)
            {
                IsoTypeWriter.writeUInt8(byteBuffer, (baseOffsetSize << 4 | indexSize));
            }
            else
            {
                IsoTypeWriter.writeUInt8(byteBuffer, (baseOffsetSize << 4));
            }
            IsoTypeWriter.writeUInt16(byteBuffer, items.Count);
            foreach (Item item in items)
            {
                item.getContent(byteBuffer);
            }
        }

        protected override void _parseDetails(ByteBuffer content)
        {
            parseVersionAndFlags(content);
            int tmp = IsoTypeReader.readUInt8(content);
            offsetSize = (int)((uint)tmp >> 4);
            lengthSize = tmp & 0xf;
            tmp = IsoTypeReader.readUInt8(content);
            baseOffsetSize = (int)((uint)tmp >> 4);

            if (getVersion() == 1)
            {
                indexSize = tmp & 0xf;
            }
            int itemCount = IsoTypeReader.readUInt16(content);
            for (int i = 0; i < itemCount; i++)
            {
                items.Add(new Item(this, content));
            }
        }

        public int getOffsetSize()
        {
            return offsetSize;
        }

        public void setOffsetSize(int offsetSize)
        {
            this.offsetSize = offsetSize;
        }

        public int getLengthSize()
        {
            return lengthSize;
        }

        public void setLengthSize(int lengthSize)
        {
            this.lengthSize = lengthSize;
        }

        public int getBaseOffsetSize()
        {
            return baseOffsetSize;
        }

        public void setBaseOffsetSize(int baseOffsetSize)
        {
            this.baseOffsetSize = baseOffsetSize;
        }

        public int getIndexSize()
        {
            return indexSize;
        }

        public void setIndexSize(int indexSize)
        {
            this.indexSize = indexSize;
        }

        public List<Item> getItems()
        {
            return items;
        }

        public void setItems(List<Item> items)
        {
            this.items = items;
        }

        public Item createItem(int itemId, int constructionMethod, int dataReferenceIndex, long baseOffset, List<Extent> extents)
        {
            return new Item(this, itemId, constructionMethod, dataReferenceIndex, baseOffset, extents);
        }

        public Item createItem(ByteBuffer bb)
        {
            return new Item(this, bb);
        }

        public Extent createExtent(long extentOffset, long extentLength, long extentIndex)
        {
            return new Extent(this, extentOffset, extentLength, extentIndex);
        }

        public Extent createExtent(ByteBuffer bb)
        {
            return new Extent(this, bb);
        }

        public class Item
        {
            private ItemLocationBox box;
            public int itemId;
            public int constructionMethod;
            public int dataReferenceIndex;
            public long baseOffset;
            public List<Extent> extents = new List<Extent>();

            public Item(ItemLocationBox box, ByteBuffer input)
            {
                this.box = box;
                itemId = IsoTypeReader.readUInt16(input);

                if (box.getVersion() == 1)
                {
                    int tmp = IsoTypeReader.readUInt16(input);
                    constructionMethod = tmp & 0xf;
                }

                dataReferenceIndex = IsoTypeReader.readUInt16(input);
                if (box.baseOffsetSize > 0)
                {
                    baseOffset = IsoTypeReaderVariable.read(input, box.baseOffsetSize);
                }
                else
                {
                    baseOffset = 0;
                }
                int extentCount = IsoTypeReader.readUInt16(input);


                for (int i = 0; i < extentCount; i++)
                {
                    extents.Add(new Extent(this.box, input));
                }
            }

            public Item(ItemLocationBox box, int itemId, int constructionMethod, int dataReferenceIndex, long baseOffset, List<Extent> extents)
            {
                this.box = box;
                this.itemId = itemId;
                this.constructionMethod = constructionMethod;
                this.dataReferenceIndex = dataReferenceIndex;
                this.baseOffset = baseOffset;
                this.extents = extents;
            }

            public int getSize()
            {
                int size = 2;

                if (box.getVersion() == 1)
                {
                    size += 2;
                }

                size += 2;
                size += box.baseOffsetSize;
                size += 2;


                foreach (Extent extent in extents)
                {
                    size += extent.getSize();
                }
                return size;
            }

            public void setBaseOffset(long baseOffset)
            {
                this.baseOffset = baseOffset;
            }

            public void getContent(ByteBuffer bb)
            {
                IsoTypeWriter.writeUInt16(bb, itemId);

                if (box.getVersion() == 1)
                {
                    IsoTypeWriter.writeUInt16(bb, constructionMethod);
                }


                IsoTypeWriter.writeUInt16(bb, dataReferenceIndex);
                if (box.baseOffsetSize > 0)
                {
                    IsoTypeWriterVariable.write(baseOffset, bb, box.baseOffsetSize);
                }
                IsoTypeWriter.writeUInt16(bb, extents.Count);

                foreach (Extent extent in extents)
                {
                    extent.getContent(bb);
                }
            }

            public override bool Equals(object o)
            {
                if (this == o) return true;
                if (o == null || this.GetType() != o.GetType()) return false;

                Item item = (Item)o;

                if (baseOffset != item.baseOffset) return false;
                if (constructionMethod != item.constructionMethod) return false;
                if (dataReferenceIndex != item.dataReferenceIndex) return false;
                if (itemId != item.itemId) return false;
                if (extents != null ? !extents.Equals(item.extents) : item.extents != null) return false;

                return true;
            }

            public override int GetHashCode()
            {
                int result = itemId;
                result = 31 * result + constructionMethod;
                result = 31 * result + dataReferenceIndex;
                result = 31 * result + (int)(baseOffset ^ (long)((ulong)baseOffset >> 32));
                result = 31 * result + (extents != null ? extents.GetHashCode() : 0);
                return result;
            }

            public override string ToString()
            {
                return "Item{" +
                        "baseOffset=" + baseOffset +
                        ", itemId=" + itemId +
                        ", constructionMethod=" + constructionMethod +
                        ", dataReferenceIndex=" + dataReferenceIndex +
                        ", extents=" + extents +
                        '}';
            }
        }

        public class Extent
        {
            private ItemLocationBox box;
            public long extentOffset;
            public long extentLength;
            public long extentIndex;

            public Extent(ItemLocationBox box, long extentOffset, long extentLength, long extentIndex)
            {
                this.box = box;
                this.extentOffset = extentOffset;
                this.extentLength = extentLength;
                this.extentIndex = extentIndex;
            }

            public Extent(ItemLocationBox box, ByteBuffer input)
            {
                this.box = box;
                if ((box.getVersion() == 1) && box.indexSize > 0)
                {
                    extentIndex = IsoTypeReaderVariable.read(input, box.indexSize);
                }
                extentOffset = IsoTypeReaderVariable.read(input, box.offsetSize);
                extentLength = IsoTypeReaderVariable.read(input, box.lengthSize);
            }

            public void getContent(ByteBuffer os)
            {
                if ((box.getVersion() == 1) && box.indexSize > 0)
                {
                    IsoTypeWriterVariable.write(extentIndex, os, box.indexSize);
                }
                IsoTypeWriterVariable.write(extentOffset, os, box.offsetSize);
                IsoTypeWriterVariable.write(extentLength, os, box.lengthSize);
            }

            public int getSize()
            {
                return (box.indexSize > 0 ? box.indexSize : 0) + box.offsetSize + box.lengthSize;
            }

            public override bool Equals(object o)
            {
                if (this == o) return true;
                if (o == null || GetType() != o.GetType()) return false;

                Extent extent = (Extent)o;

                if (extentIndex != extent.extentIndex) return false;
                if (extentLength != extent.extentLength) return false;
                if (extentOffset != extent.extentOffset) return false;

                return true;
            }

            public override int GetHashCode()
            {
                int result = (int)(extentOffset ^ (long)((ulong)extentOffset >> 32));
                result = 31 * result + (int)(extentLength ^ (long)((ulong)extentLength >> 32));
                result = 31 * result + (int)(extentIndex ^ (long)((ulong)extentIndex >> 32));
                return result;
            }

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("Extent");
                sb.Append("{extentOffset=").Append(extentOffset);
                sb.Append(", extentLength=").Append(extentLength);
                sb.Append(", extentIndex=").Append(extentIndex);
                sb.Append('}');
                return sb.ToString();
            }
        }
    }
}
