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

using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part15;
using SharpMp4Parser.IsoParser.Support;
using SharpMp4Parser.IsoParser.Tools;
using SharpMp4Parser.Java;
using System;
using System.Collections.Generic;

namespace SharpMp4Parser.IsoParser.Boxes.SampleGrouping
{
    /**
     * <h1>4cc = "{@value #TYPE}"</h1>
     * <p>This description table gives information about the characteristics of sample groups. The descriptive
     * information is any other information needed to define or characterize the sample group.</p>
     * <p>There may be multiple instances of this box if there is more than one sample grouping for the samples in a
     * track. Each instance of the SampleGroupDescription box has a type code that distinguishes different
     * sample groupings. Within a track, there shall be at most one instance of this box with a particular grouping
     * type. The associated SampleToGroup shall indicate the same value for the grouping type.</p>
     * <p>The information is stored in the sample group description box after the entry-count. An abstract entry type is
     * defined and sample groupings shall define derived types to represent the description of each sample group.
     * For video tracks, an abstract VisualSampleGroupEntry is used with similar types for audio and hint tracks.</p>
     */
    public class SampleGroupDescriptionBox : AbstractFullBox
    {
        public const string TYPE = "sgpd";
        private string groupingType;
        private int defaultLength;
        private List<GroupEntry> groupEntries = new List<GroupEntry>();

        public SampleGroupDescriptionBox() : base(TYPE)
        {
            setVersion(1);
        }

        public string getGroupingType()
        {
            return groupingType;
        }

        public void setGroupingType(string groupingType)
        {
            this.groupingType = groupingType;
        }

        protected override long getContentSize()
        {
            long size = 8;
            if (getVersion() == 1)
            {
                size += 4;
            }
            size += 4; // entryCount
            foreach (GroupEntry groupEntry in groupEntries)
            {
                if (getVersion() == 1 && defaultLength == 0)
                {
                    size += 4;
                }
                size += defaultLength == 0 ? groupEntry.size() : defaultLength;
            }
            return size;
        }

        protected override void getContent(ByteBuffer byteBuffer)
        {
            writeVersionAndFlags(byteBuffer);
            byteBuffer.put(IsoFile.fourCCtoBytes(groupingType));
            if (getVersion() == 1)
            {
                IsoTypeWriter.writeUInt32(byteBuffer, defaultLength);
            }
            IsoTypeWriter.writeUInt32(byteBuffer, groupEntries.Count);
            foreach (GroupEntry entry in groupEntries)
            {
                ByteBuffer data = entry.get();
                if (getVersion() == 1)
                {
                    if (defaultLength == 0)
                    {
                        IsoTypeWriter.writeUInt32(byteBuffer, data.limit());
                    }
                    else
                    {
                        if (data.limit() > defaultLength)
                        {
                            throw new Exception(
                                    string.Format("SampleGroupDescriptionBox entry size {0} more than {1}", data.limit(), defaultLength));
                        }
                    }
                }
                byteBuffer.put(data);

                int deadBytes = defaultLength == 0 ? 0 : defaultLength - data.limit();
                while (deadBytes-- > 0)
                {
                    byteBuffer.put(0);
                }
            }
        }

        protected override void _parseDetails(ByteBuffer content)
        {
            parseVersionAndFlags(content);
            groupingType = IsoTypeReader.read4cc(content);
            if (getVersion() == 1)
            {
                defaultLength = CastUtils.l2i(IsoTypeReader.readUInt32(content));
            }
            long entryCount = IsoTypeReader.readUInt32(content);
            while (entryCount-- > 0)
            {
                int length = defaultLength;
                if (getVersion() == 1)
                {
                    if (defaultLength == 0)
                    {
                        length = CastUtils.l2i(IsoTypeReader.readUInt32(content));
                    }
                }
                else
                {
                    length = content.limit() - content.position();
                }
                ByteBuffer parseMe = content.slice();
                parseMe.limit(length);
                groupEntries.Add(parseGroupEntry(parseMe, groupingType));
                int parsedBytes = getVersion() == 1 ? length : parseMe.position();
                ((Java.Buffer)content).position(content.position() + parsedBytes);
            }

        }

        private GroupEntry parseGroupEntry(ByteBuffer content, string groupingType)
        {
            GroupEntry groupEntry;
            if (RollRecoveryEntry.TYPE.Equals(groupingType))
            {
                groupEntry = new RollRecoveryEntry();
            }
            else if (RateShareEntry.TYPE.Equals(groupingType))
            {
                groupEntry = new RateShareEntry();
            }
            else if (VisualRandomAccessEntry.TYPE.Equals(groupingType))
            {
                groupEntry = new VisualRandomAccessEntry();
            }
            else if (TemporalLevelEntry.TYPE.Equals(groupingType))
            {
                groupEntry = new TemporalLevelEntry();
            }
            else if (SyncSampleEntry.TYPE.Equals(groupingType))
            {
                groupEntry = new SyncSampleEntry();
            }
            else if (TemporalLayerSampleGroup.TYPE.Equals(groupingType))
            {
                groupEntry = new TemporalLayerSampleGroup();
            }
            else if (TemporalSubLayerSampleGroup.TYPE.Equals(groupingType))
            {
                groupEntry = new TemporalSubLayerSampleGroup();
            }
            else if (StepwiseTemporalLayerEntry.TYPE.Equals(groupingType))
            {
                groupEntry = new StepwiseTemporalLayerEntry();
            }
            else
            {
                if (getVersion() == 0)
                {
                    throw new Exception("SampleGroupDescriptionBox with UnknownEntry are only supported in version 1");
                }
                groupEntry = new UnknownEntry(groupingType);
            }
            groupEntry.parse(content);
            return groupEntry;
        }

        public int getDefaultLength()
        {
            return defaultLength;
        }

        public void setDefaultLength(int defaultLength)
        {
            this.defaultLength = defaultLength;
        }

        public List<GroupEntry> getGroupEntries()
        {
            return groupEntries;
        }

        public void setGroupEntries(List<GroupEntry> groupEntries)
        {
            this.groupEntries = groupEntries;
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

            SampleGroupDescriptionBox that = (SampleGroupDescriptionBox)o;

            if (defaultLength != that.defaultLength)
            {
                return false;
            }
            if (groupEntries != null ? !groupEntries.Equals(that.groupEntries) : that.groupEntries != null)
            {
                return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            int result = 0;
            result = 31 * result + defaultLength;
            result = 31 * result + (groupEntries != null ? groupEntries.GetHashCode() : 0);
            return result;
        }

        public override string ToString()
        {
            return "SampleGroupDescriptionBox{" +
                    "groupingType='" + (groupEntries.Count > 0 ? groupEntries[0].getType() : "????") + '\'' +
                    ", defaultLength=" + defaultLength +
                    ", groupEntries=" + groupEntries +
                    '}';
        }
    }
}