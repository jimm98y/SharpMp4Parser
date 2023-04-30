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

using System.Collections.Generic;
using System.Text;

namespace SharpMp4Parser.Boxes.ISO14496.Part12
{
    /**
     * <h1>4cc = "{@value #TYPE}"</h1>
     * aligned(8) class TrackRunBox
     * extends FullBox('trun', version, tr_flags) {
     * unsigned int(32)	sample_count;
     * // the following are optional fields
     * signed int(32)	data_offset;
     * unsigned int(32)	first_sample_flags;
     * // all fields in the following array are optional
     * {
     * unsigned int(32)	sample_duration;
     * unsigned int(32)	sample_size;
     * unsigned int(32)	sample_flags
     * if (version == 0)
     * { unsigned int(32)	sample_composition_time_offset; }
     * else
     * { signed int(32)		sample_composition_time_offset; }
     * }[ sample_count ]
     * }
     */

    public class TrackRunBox : AbstractFullBox
    {
        public const string TYPE = "trun";
        private int dataOffset;
        private SampleFlags firstSampleFlags;
        private List<Entry> entries = new List<Entry>();


        public TrackRunBox() : base(TYPE)
        { }

        public List<Entry> getEntries()
        {
            return entries;
        }

        public void setEntries(List<Entry> entries)
        {
            this.entries = entries;
        }

        public long[] getSampleCompositionTimeOffsets()
        {
            if (isSampleCompositionTimeOffsetPresent())
            {
                long[] result = new long[entries.size()];

                for (int i = 0; i < result.Length; i++)
                {
                    result[i] = entries[i].getSampleCompositionTimeOffset();
                }
                return result;
            }
            return null;
        }

        protected long getContentSize()
        {
            long size = 8;
            int flags = getFlags();

            if ((flags & 0x1) == 0x1)
            { //dataOffsetPresent
                size += 4;
            }
            if ((flags & 0x4) == 0x4)
            { //firstSampleFlagsPresent
                size += 4;
            }

            long entrySize = 0;
            if ((flags & 0x100) == 0x100)
            { //sampleDurationPresent
                entrySize += 4;
            }
            if ((flags & 0x200) == 0x200)
            { //sampleSizePresent
                entrySize += 4;
            }
            if ((flags & 0x400) == 0x400)
            { //sampleFlagsPresent
                entrySize += 4;
            }
            if ((flags & 0x800) == 0x800)
            { //sampleCompositionTimeOffsetPresent
                entrySize += 4;
            }
            size += entrySize * entries.size();
            return size;
        }

        protected void getContent(ByteBuffer byteBuffer)
        {
            writeVersionAndFlags(byteBuffer);
            IsoTypeWriter.writeUInt32(byteBuffer, entries.size());
            int flags = getFlags();

            if ((flags & 0x1) == 1)
            { //dataOffsetPresent
                IsoTypeWriter.writeUInt32(byteBuffer, dataOffset);
            }
            if ((flags & 0x4) == 0x4)
            { //firstSampleFlagsPresent
                firstSampleFlags.getContent(byteBuffer);
            }

            foreach (Entry entry in entries)
            {
                if ((flags & 0x100) == 0x100)
                { //sampleDurationPresent
                    IsoTypeWriter.writeUInt32(byteBuffer, entry.sampleDuration);
                }
                if ((flags & 0x200) == 0x200)
                { //sampleSizePresent
                    IsoTypeWriter.writeUInt32(byteBuffer, entry.sampleSize);
                }
                if ((flags & 0x400) == 0x400)
                { //sampleFlagsPresent
                    entry.sampleFlags.getContent(byteBuffer);
                }
                if ((flags & 0x800) == 0x800)
                { //sampleCompositionTimeOffsetPresent
                    if (getVersion() == 0)
                    {
                        IsoTypeWriter.writeUInt32(byteBuffer, entry.sampleCompositionTimeOffset);
                    }
                    else
                    {
                        byteBuffer.putInt((int)entry.sampleCompositionTimeOffset);
                    }
                }
            }
        }

        public override void _parseDetails(ByteBuffer content)
        {
            parseVersionAndFlags(content);
            long sampleCount = IsoTypeReader.readUInt32(content);

            if ((getFlags() & 0x1) == 1)
            { //dataOffsetPresent
                dataOffset = CastUtils.l2i(IsoTypeReader.readUInt32(content));
            }
            else
            {
                dataOffset = -1;
            }
            if ((getFlags() & 0x4) == 0x4)
            { //firstSampleFlagsPresent
                firstSampleFlags = new SampleFlags(content);
            }

            for (int i = 0; i < sampleCount; i++)
            {
                Entry entry = new Entry();
                if ((getFlags() & 0x100) == 0x100)
                { //sampleDurationPresent
                    entry.sampleDuration = IsoTypeReader.readUInt32(content);
                }
                if ((getFlags() & 0x200) == 0x200)
                { //sampleSizePresent
                    entry.sampleSize = IsoTypeReader.readUInt32(content);
                }
                if ((getFlags() & 0x400) == 0x400)
                { //sampleFlagsPresent
                    entry.sampleFlags = new SampleFlags(content);
                }
                if ((getFlags() & 0x800) == 0x800)
                { //sampleCompositionTimeOffsetPresent
                    entry.sampleCompositionTimeOffset = content.getInt();
                }
                entries.Add(entry);
            }
        }

        public long getSampleCount()
        {
            return entries.size();
        }

        public bool isDataOffsetPresent()
        {
            return (getFlags() & 0x1) == 1;
        }

        public void setDataOffsetPresent(bool v)
        {
            if (v)
            {
                setFlags(getFlags() | 0x01);
            }
            else
            {
                setFlags(getFlags() & (0xFFFFFF ^ 0x1));
            }
        }

        public bool isFirstSampleFlagsPresent()
        {
            return (getFlags() & 0x4) == 0x4;
        }


        public bool isSampleSizePresent()
        {
            return (getFlags() & 0x200) == 0x200;
        }

        public void setSampleSizePresent(bool v)
        {
            if (v)
            {
                setFlags(getFlags() | 0x200);
            }
            else
            {
                setFlags(getFlags() & (0xFFFFFF ^ 0x200));
            }
        }

        public bool isSampleDurationPresent()
        {
            return (getFlags() & 0x100) == 0x100;
        }

        public void setSampleDurationPresent(bool v)
        {

            if (v)
            {
                setFlags(getFlags() | 0x100);
            }
            else
            {
                setFlags(getFlags() & (0xFFFFFF ^ 0x100));
            }
        }

        public bool isSampleFlagsPresent()
        {
            return (getFlags() & 0x400) == 0x400;
        }

        public void setSampleFlagsPresent(bool v)
        {
            if (v)
            {
                setFlags(getFlags() | 0x400);
            }
            else
            {
                setFlags(getFlags() & (0xFFFFFF ^ 0x400));
            }
        }

        public bool isSampleCompositionTimeOffsetPresent()
        {
            return (getFlags() & 0x800) == 0x800;
        }

        public void setSampleCompositionTimeOffsetPresent(bool v)
        {
            if (v)
            {
                setFlags(getFlags() | 0x800);
            }
            else
            {
                setFlags(getFlags() & (0xFFFFFF ^ 0x800));
            }

        }

        public int getDataOffset()
        {
            return dataOffset;
        }

        public void setDataOffset(int dataOffset)
        {
            if (dataOffset == -1)
            {
                setFlags(getFlags() & (0xFFFFFF ^ 1));
            }
            else
            {
                setFlags(getFlags() | 0x1); // turn on dataoffset
            }
            this.dataOffset = dataOffset;
        }

        public SampleFlags getFirstSampleFlags()
        {
            return firstSampleFlags;
        }

        public void setFirstSampleFlags(SampleFlags firstSampleFlags)
        {
            if (firstSampleFlags == null)
            {
                setFlags(getFlags() & (0xFFFFFF ^ 0x4));
            }
            else
            {
                setFlags(getFlags() | 0x4);
            }
            this.firstSampleFlags = firstSampleFlags;
        }

        public override string toString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("TrackRunBox");
            sb.Append("{sampleCount=").Append(entries.size());
            sb.Append(", dataOffset=").Append(dataOffset);
            sb.Append(", dataOffsetPresent=").Append(isDataOffsetPresent());
            sb.Append(", sampleSizePresent=").Append(isSampleSizePresent());
            sb.Append(", sampleDurationPresent=").Append(isSampleDurationPresent());
            sb.Append(", sampleFlagsPresentPresent=").Append(isSampleFlagsPresent());
            sb.Append(", sampleCompositionTimeOffsetPresent=").Append(isSampleCompositionTimeOffsetPresent());
            sb.Append(", firstSampleFlags=").Append(firstSampleFlags);
            sb.Append('}');
            return sb.ToString();
        }

        public sealed class Entry
        {
            private long sampleDuration;
            private long sampleSize;
            private SampleFlags sampleFlags;
            private long sampleCompositionTimeOffset;

            public Entry()
            {
            }

            public Entry(long sampleDuration, long sampleSize, SampleFlags sampleFlags, int sampleCompositionTimeOffset)
            {
                this.sampleDuration = sampleDuration;
                this.sampleSize = sampleSize;
                this.sampleFlags = sampleFlags;
                this.sampleCompositionTimeOffset = sampleCompositionTimeOffset;
            }

            public long getSampleDuration()
            {
                return sampleDuration;
            }

            public void setSampleDuration(long sampleDuration)
            {
                this.sampleDuration = sampleDuration;
            }

            public long getSampleSize()
            {
                return sampleSize;
            }

            public void setSampleSize(long sampleSize)
            {
                this.sampleSize = sampleSize;
            }

            public SampleFlags getSampleFlags()
            {
                return sampleFlags;
            }

            public void setSampleFlags(SampleFlags sampleFlags)
            {
                this.sampleFlags = sampleFlags;
            }

            public long getSampleCompositionTimeOffset()
            {
                return sampleCompositionTimeOffset;
            }

            public void setSampleCompositionTimeOffset(int sampleCompositionTimeOffset)
            {
                this.sampleCompositionTimeOffset = sampleCompositionTimeOffset;
            }

            public override string ToString()
            {
                return "Entry{" +
                        "duration=" + sampleDuration +
                        ", size=" + sampleSize +
                        ", dlags=" + sampleFlags +
                        ", compTimeOffset=" + sampleCompositionTimeOffset +
                        '}';
            }
        }
    }
}
