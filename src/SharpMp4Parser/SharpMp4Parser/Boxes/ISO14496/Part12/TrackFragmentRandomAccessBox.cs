/*
 * Copyright 2009 castLabs GmbH, Berlin
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
using System.Linq;

namespace SharpMp4Parser.Boxes.ISO14496.Part12
{
    /**
     * Each entry contains the location and the presentation time of the random accessible sample.
     * It indicates that the sample in the entry can be randomaccessed. Note that not every random
     * accessible sample in the track needs to be listed in the table.
     *
     * @see MovieFragmentRandomAccessBox
     */
    public class TrackFragmentRandomAccessBox : AbstractFullBox
    {
        public const string TYPE = "tfra";

        private long trackId;
        private int reserved;
        private int lengthSizeOfTrafNum = 2;
        private int lengthSizeOfTrunNum = 2;
        private int lengthSizeOfSampleNum = 2;
        private List<Entry> entries = new List<Entry>();

        public TrackFragmentRandomAccessBox() : base(TYPE)
        { }

        protected override long getContentSize()
        {
            long contentSize = 4;
            contentSize += 4 + 4 /*26 + 2 + 2 + 2 */ + 4;
            if (getVersion() == 1)
            {
                contentSize += (8 + 8) * entries.Count;
            }
            else
            {
                contentSize += (4 + 4) * entries.Count;
            }
            contentSize += lengthSizeOfTrafNum * entries.Count;
            contentSize += lengthSizeOfTrunNum * entries.Count;
            contentSize += lengthSizeOfSampleNum * entries.Count;
            return contentSize;
        }

        protected override void _parseDetails(ByteBuffer content)
        {
            parseVersionAndFlags(content);
            trackId = IsoTypeReader.readUInt32(content);
            long temp = IsoTypeReader.readUInt32(content);
            reserved = (int)(temp >> 6);
            lengthSizeOfTrafNum = ((int)(temp & 0x3F) >> 4) + 1;
            lengthSizeOfTrunNum = ((int)(temp & 0xC) >> 2) + 1;
            lengthSizeOfSampleNum = ((int)(temp & 0x3)) + 1;
            long numberOfEntries = IsoTypeReader.readUInt32(content);

            entries = new List<Entry>();

            for (int i = 0; i < numberOfEntries; i++)
            {
                Entry entry = new Entry();
                if (getVersion() == 1)
                {
                    entry.time = IsoTypeReader.readUInt64(content);
                    entry.moofOffset = IsoTypeReader.readUInt64(content);
                }
                else
                {
                    entry.time = IsoTypeReader.readUInt32(content);
                    entry.moofOffset = IsoTypeReader.readUInt32(content);
                }
                entry.trafNumber = IsoTypeReaderVariable.read(content, lengthSizeOfTrafNum);
                entry.trunNumber = IsoTypeReaderVariable.read(content, lengthSizeOfTrunNum);
                entry.sampleNumber = IsoTypeReaderVariable.read(content, lengthSizeOfSampleNum);

                entries.Add(entry);
            }

        }

        protected override void getContent(ByteBuffer byteBuffer)
        {
            writeVersionAndFlags(byteBuffer);
            IsoTypeWriter.writeUInt32(byteBuffer, trackId);
            long temp;
            temp = reserved << 6;
            temp = temp | (long)(((lengthSizeOfTrafNum - 1) & 0x3) << 4);
            temp = temp | (long)(((lengthSizeOfTrunNum - 1) & 0x3) << 2);
            temp = temp | (long)((lengthSizeOfSampleNum - 1) & 0x3);
            IsoTypeWriter.writeUInt32(byteBuffer, temp);
            IsoTypeWriter.writeUInt32(byteBuffer, entries.Count);

            foreach (Entry entry in entries)
            {
                if (getVersion() == 1)
                {
                    IsoTypeWriter.writeUInt64(byteBuffer, entry.time);
                    IsoTypeWriter.writeUInt64(byteBuffer, entry.moofOffset);
                }
                else
                {
                    IsoTypeWriter.writeUInt32(byteBuffer, entry.time);
                    IsoTypeWriter.writeUInt32(byteBuffer, entry.moofOffset);
                }
                IsoTypeWriterVariable.write(entry.trafNumber, byteBuffer, lengthSizeOfTrafNum);
                IsoTypeWriterVariable.write(entry.trunNumber, byteBuffer, lengthSizeOfTrunNum);
                IsoTypeWriterVariable.write(entry.sampleNumber, byteBuffer, lengthSizeOfSampleNum);

            }
        }

        public long getTrackId()
        {
            return trackId;
        }

        public void setTrackId(long trackId)
        {
            this.trackId = trackId;
        }

        public int getReserved()
        {
            return reserved;
        }

        public int getLengthSizeOfTrafNum()
        {
            return lengthSizeOfTrafNum;
        }

        public void setLengthSizeOfTrafNum(int lengthSizeOfTrafNum)
        {
            this.lengthSizeOfTrafNum = lengthSizeOfTrafNum;
        }

        public int getLengthSizeOfTrunNum()
        {
            return lengthSizeOfTrunNum;
        }

        public void setLengthSizeOfTrunNum(int lengthSizeOfTrunNum)
        {
            this.lengthSizeOfTrunNum = lengthSizeOfTrunNum;
        }

        public int getLengthSizeOfSampleNum()
        {
            return lengthSizeOfSampleNum;
        }

        public void setLengthSizeOfSampleNum(int lengthSizeOfSampleNum)
        {
            this.lengthSizeOfSampleNum = lengthSizeOfSampleNum;
        }

        public long getNumberOfEntries()
        {
            return entries.Count;
        }

        public List<Entry> getEntries()
        {
            return entries.ToList();
        }

        public void setEntries(List<Entry> entries)
        {
            this.entries = entries;
        }

        public override string ToString()
        {
            return "TrackFragmentRandomAccessBox{" +
                    "trackId=" + trackId +
                    ", entries=" + entries +
                    '}';
        }

        public sealed class Entry
        {
            public long time;
            public long moofOffset;
            public long trafNumber;
            public long trunNumber;
            public long sampleNumber;

            public Entry()
            {
            }

            public Entry(long time, long moofOffset, long trafNumber, long trunNumber, long sampleNumber)
            {
                this.moofOffset = moofOffset;
                this.sampleNumber = sampleNumber;
                this.time = time;
                this.trafNumber = trafNumber;
                this.trunNumber = trunNumber;
            }

            public long getTime()
            {
                return time;
            }

            public void setTime(long time)
            {
                this.time = time;
            }

            public long getMoofOffset()
            {
                return moofOffset;
            }

            public void setMoofOffset(long moofOffset)
            {
                this.moofOffset = moofOffset;
            }

            public long getTrafNumber()
            {
                return trafNumber;
            }

            public void setTrafNumber(long trafNumber)
            {
                this.trafNumber = trafNumber;
            }

            public long getTrunNumber()
            {
                return trunNumber;
            }

            public void setTrunNumber(long trunNumber)
            {
                this.trunNumber = trunNumber;
            }

            public long getSampleNumber()
            {
                return sampleNumber;
            }

            public void setSampleNumber(long sampleNumber)
            {
                this.sampleNumber = sampleNumber;
            }

            public override string ToString()
            {
                return "Entry{" +
                        "time=" + time +
                        ", moofOffset=" + moofOffset +
                        ", trafNumber=" + trafNumber +
                        ", trunNumber=" + trunNumber +
                        ", sampleNumber=" + sampleNumber +
                        '}';
            }

            public override bool Equals(object o)
            {
                if (this == o) return true;
                if (o == null || GetType() != o.GetType()) return false;

                Entry entry = (Entry)o;

                if (moofOffset != entry.moofOffset) return false;
                if (sampleNumber != entry.sampleNumber) return false;
                if (time != entry.time) return false;
                if (trafNumber != entry.trafNumber) return false;
                if (trunNumber != entry.trunNumber) return false;

                return true;
            }

            public override int GetHashCode()
            {
                int result = (int)(time ^ (long)((ulong)time >> 32));
                result = 31 * result + (int)(moofOffset ^ (long)((ulong)moofOffset >> 32));
                result = 31 * result + (int)(trafNumber ^ (long)((ulong)trafNumber >> 32));
                result = 31 * result + (int)(trunNumber ^ (long)((ulong)trunNumber >> 32));
                result = 31 * result + (int)(sampleNumber ^ (long)((ulong)sampleNumber >> 32));
                return result;
            }
        }
    }
}
