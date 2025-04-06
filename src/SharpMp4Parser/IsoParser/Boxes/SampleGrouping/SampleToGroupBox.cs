/*
 * Copyright 2012 castLabs, Berlin
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

using SharpMp4Parser.IsoParser.Support;
using SharpMp4Parser.IsoParser.Tools;
using SharpMp4Parser.Java;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpMp4Parser.IsoParser.Boxes.SampleGrouping
{
    /**
     * <h1>4cc = "{@value #TYPE}"</h1>
     * <p>This table can be used to find the group that a sample belongs to and the associated description of that
     * sample group. The table is compactly coded with each entry giving the index of the first sample of a run of
     * samples with the same sample group descriptor. The sample group description ID is an index that refers to a
     * SampleGroupDescription box, which contains entries describing the characteristics of each sample group.</p>
     * <p>There may be multiple instances of this box if there is more than one sample grouping for the samples in a
     * track. Each instance of the SampleToGroup box has a type code that distinguishes different sample
     * groupings. Within a track, there shall be at most one instance of this box with a particular grouping type. The
     * associated SampleGroupDescription shall indicate the same value for the grouping type.</p>
     * <p>Version 1 of this box should only be used if a grouping type parameter is needed.</p>
     */
    public class SampleToGroupBox : AbstractFullBox
    {
        public const string TYPE = "sbgp";
        List<Entry> entries = new List<Entry>();
        private string groupingType;
        private string groupingTypeParameter;

        public SampleToGroupBox() : base(TYPE)
        { }

        protected override long getContentSize()
        {
            return getVersion() == 1 ? entries.Count * 8 + 16 : entries.Count * 8 + 12;
        }

        protected override void getContent(ByteBuffer byteBuffer)
        {
            writeVersionAndFlags(byteBuffer);
            byteBuffer.put(Encoding.UTF8.GetBytes(groupingType));
            if (getVersion() == 1)
            {
                byteBuffer.put(Encoding.UTF8.GetBytes(groupingTypeParameter));
            }
            IsoTypeWriter.writeUInt32(byteBuffer, entries.Count);
            foreach (Entry entry in entries)
            {
                IsoTypeWriter.writeUInt32(byteBuffer, entry.getSampleCount());
                IsoTypeWriter.writeUInt32(byteBuffer, entry.getGroupDescriptionIndex());
            }
        }

        protected override void _parseDetails(ByteBuffer content)
        {
            parseVersionAndFlags(content);
            groupingType = IsoTypeReader.read4cc(content);
            if (getVersion() == 1)
            {
                groupingTypeParameter = IsoTypeReader.read4cc(content);
            }
            long entryCount = IsoTypeReader.readUInt32(content);
            while (entryCount-- > 0)
            {
                entries.Add(new Entry(CastUtils.l2i(IsoTypeReader.readUInt32(content)), CastUtils.l2i(IsoTypeReader.readUInt32(content))));
            }
        }

        public string getGroupingType()
        {
            return groupingType;
        }

        public void setGroupingType(string groupingType)
        {
            this.groupingType = groupingType;
        }

        public string getGroupingTypeParameter()
        {
            return groupingTypeParameter;
        }

        public void setGroupingTypeParameter(string groupingTypeParameter)
        {
            this.groupingTypeParameter = groupingTypeParameter;
        }

        public List<Entry> getEntries()
        {
            return entries;
        }

        public void setEntries(List<Entry> entries)
        {
            this.entries = entries;
        }

        public sealed class Entry
        {
            private long sampleCount;
            private int groupDescriptionIndex;

            public Entry(long sampleCount, int groupDescriptionIndex)
            {
                this.sampleCount = sampleCount;
                this.groupDescriptionIndex = groupDescriptionIndex;
            }

            public long getSampleCount()
            {
                return sampleCount;
            }

            public void setSampleCount(long sampleCount)
            {
                this.sampleCount = sampleCount;
            }

            public int getGroupDescriptionIndex()
            {
                return groupDescriptionIndex;
            }

            public void setGroupDescriptionIndex(int groupDescriptionIndex)
            {
                this.groupDescriptionIndex = groupDescriptionIndex;
            }

            public override string ToString()
            {
                return "Entry{" +
                        "sampleCount=" + sampleCount +
                        ", groupDescriptionIndex=" + groupDescriptionIndex +
                        '}';
            }

            public override bool Equals(object o)
            {
                if (this == o)
                {
                    return true;
                }
                if (o == null || GetType() != o.GetType())
                {
                    return false;
                }

                Entry entry = (Entry)o;

                if (groupDescriptionIndex != entry.groupDescriptionIndex)
                {
                    return false;
                }
                if (sampleCount != entry.sampleCount)
                {
                    return false;
                }

                return true;
            }

            public override int GetHashCode()
            {
                int result = (int)(sampleCount ^ (long)((ulong)sampleCount >> 32));
                result = 31 * result + groupDescriptionIndex;
                return result;
            }
        }
    }
}