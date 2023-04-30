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

using System.Collections.Generic;

namespace SharpMp4Parser.Boxes.ISO14496.Part12
{
    /**
     * <h1>4cc = "{@value #TYPE}"</h1>
     * Samples within the media data are grouped into chunks. Chunks can be of different sizes, and the
     * samples within a chunk can have different sizes. This table can be used to find the chunk that
     * contains a sample, its position, and the associated sample description. Defined in ISO/IEC 14496-12.
     */
    public class SampleToChunkBox : AbstractFullBox
    {
        public const string TYPE = "stsc";
        List<Entry> entries = new List<Entry>();

        public SampleToChunkBox() : base(TYPE)
        { }

        public List<Entry> getEntries()
        {
            return entries;
        }

        public void setEntries(List<Entry> entries)
        {
            this.entries = entries;
        }

        protected long getContentSize()
        {
            return entries.size() * 12 + 8;
        }

        public override void _parseDetails(ByteBuffer content)
        {
            parseVersionAndFlags(content);

            int entryCount = CastUtils.l2i(IsoTypeReader.readUInt32(content));
            entries = new List<Entry>(entryCount);
            for (int i = 0; i < entryCount; i++)
            {
                entries.Add(new Entry(
                        IsoTypeReader.readUInt32(content),
                        IsoTypeReader.readUInt32(content),
                        IsoTypeReader.readUInt32(content)));
            }
        }

        protected override void getContent(ByteBuffer byteBuffer)
        {
            writeVersionAndFlags(byteBuffer);
            IsoTypeWriter.writeUInt32(byteBuffer, entries.size());
            foreach (Entry entry in entries)
            {
                IsoTypeWriter.writeUInt32(byteBuffer, entry.getFirstChunk());
                IsoTypeWriter.writeUInt32(byteBuffer, entry.getSamplesPerChunk());
                IsoTypeWriter.writeUInt32(byteBuffer, entry.getSampleDescriptionIndex());
            }
        }

        public override string ToString()
        {
            return "SampleToChunkBox[entryCount=" + entries.size() + "]";
        }

        /**
         * Decompresses the list of entries and returns the number of samples per chunk for
         * every single chunk.
         *
         * @param chunkCount overall number of chunks
         * @return number of samples per chunk
         */
        public long[] blowup(int chunkCount)
        {
            long[] numberOfSamples = new long[chunkCount];
            int j = 0;
            List<SampleToChunkBox.Entry> sampleToChunkEntries = new LinkedList<Entry>(entries);
            Collections.reverse(sampleToChunkEntries);
            Iterator<Entry> iterator = sampleToChunkEntries.iterator();
            SampleToChunkBox.Entry currentEntry = iterator.next();

            for (int i = numberOfSamples.Length; i > 1; i--)
            {
                numberOfSamples[i - 1] = currentEntry.getSamplesPerChunk();
                if (i == currentEntry.getFirstChunk())
                {
                    currentEntry = iterator.next();
                }
            }
            numberOfSamples[0] = currentEntry.getSamplesPerChunk();
            return numberOfSamples;
        }

        public sealed class Entry
        {
            long firstChunk;
            long samplesPerChunk;
            long sampleDescriptionIndex;

            public Entry(long firstChunk, long samplesPerChunk, long sampleDescriptionIndex)
            {
                this.firstChunk = firstChunk;
                this.samplesPerChunk = samplesPerChunk;
                this.sampleDescriptionIndex = sampleDescriptionIndex;
            }

            public long getFirstChunk()
            {
                return firstChunk;
            }

            public void setFirstChunk(long firstChunk)
            {
                this.firstChunk = firstChunk;
            }

            public long getSamplesPerChunk()
            {
                return samplesPerChunk;
            }

            public void setSamplesPerChunk(long samplesPerChunk)
            {
                this.samplesPerChunk = samplesPerChunk;
            }

            public long getSampleDescriptionIndex()
            {
                return sampleDescriptionIndex;
            }

            public void setSampleDescriptionIndex(long sampleDescriptionIndex)
            {
                this.sampleDescriptionIndex = sampleDescriptionIndex;
            }

            public override string ToString()
            {
                return "Entry{" +
                        "firstChunk=" + firstChunk +
                        ", samplesPerChunk=" + samplesPerChunk +
                        ", sampleDescriptionIndex=" + sampleDescriptionIndex +
                        '}';
            }

            public override bool Equals(object o)
            {
                if (this == o) return true;
                if (o == null || getClass() != o.getClass()) return false;

                Entry entry = (Entry)o;

                if (firstChunk != entry.firstChunk) return false;
                if (sampleDescriptionIndex != entry.sampleDescriptionIndex) return false;
                if (samplesPerChunk != entry.samplesPerChunk) return false;

                return true;
            }

            public override int GetHashCode()
            {
                int result = (int)(firstChunk ^ (firstChunk >>> 32));
                result = 31 * result + (int)(samplesPerChunk ^ (samplesPerChunk >>> 32));
                result = 31 * result + (int)(sampleDescriptionIndex ^ (sampleDescriptionIndex >>> 32));
                return result;
            }
        }
    }
}
