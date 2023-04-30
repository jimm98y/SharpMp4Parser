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
     * <pre>
     * aligned(8) class SampleDependencyTypeBox extends FullBox('sdtp', version = 0, 0) {
     *  for (i=0; i &lt; sample_count; i++){
     *   unsigned int(2) isLeading;
     *   unsigned int(2) sample_depends_on;
     *   unsigned int(2) sample_is_depended_on;
     *   unsigned int(2) sample_has_redundancy;
     *  }
     * }
     * </pre>
     */
    public class SampleDependencyTypeBox : AbstractFullBox
    {
        public const string TYPE = "sdtp";

        private List<Entry> entries = new List<Entry>();

        public SampleDependencyTypeBox() : base(TYPE)
        { }

        protected override long getContentSize()
        {
            return 4 + entries.size();
        }

        protected override void getContent(ByteBuffer byteBuffer)
        {
            writeVersionAndFlags(byteBuffer);
            foreach (Entry entry in entries)
            {
                IsoTypeWriter.writeUInt8(byteBuffer, entry.value);
            }
        }

        public override void _parseDetails(ByteBuffer content)
        {
            parseVersionAndFlags(content);
            while (content.remaining() > 0)
            {
                entries.Add(new Entry(IsoTypeReader.readUInt8(content)));
            }
        }

        public List<Entry> getEntries()
        {
            return entries;
        }

        public void setEntries(List<Entry> entries)
        {
            this.entries = entries;
        }

        public override string toString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("SampleDependencyTypeBox");
            sb.Append("{entries=").Append(entries);
            sb.Append('}');
            return sb.ToString();
        }

        public sealed class Entry
        {
            public int value;

            public Entry(int value)
            {
                this.value = value;
            }

            public byte getIsLeading()
            {
                return (byte)((value >> 6) & 0x03);
            }

            public void setIsLeading(int res)
            {
                value = (res & 0x03) << 6 | value & 0x3f;
            }

            public byte getSampleDependsOn()
            {
                return (byte)((value >> 4) & 0x03);
            }

            public void setSampleDependsOn(int sdo)
            {
                value = (sdo & 0x03) << 4 | value & 0xcf;
            }

            public byte getSampleIsDependedOn()
            {
                return (byte)((value >> 2) & 0x03);
            }

            public void setSampleIsDependedOn(int sido)
            {
                value = (sido & 0x03) << 2 | value & 0xf3;
            }

            public byte getSampleHasRedundancy()
            {
                return (byte)(value & 0x03);
            }

            public void setSampleHasRedundancy(int shr)
            {
                value = shr & 0x03 | value & 0xfc;
            }

            public override string ToString()
            {
                return "Entry{" +
                        "isLeading=" + getIsLeading() +
                        ", sampleDependsOn=" + getSampleDependsOn() +
                        ", sampleIsDependentOn=" + getSampleIsDependedOn() +
                        ", sampleHasRedundancy=" + getSampleHasRedundancy() +
                        '}';
            }

            public override bool Equals(object o)
            {
                if (this == o) return true;
                if (o == null || getClass() != o.getClass()) return false;

                Entry entry = (Entry)o;

                if (value != entry.value) return false;

                return true;
            }

            public override int GetHashCode()
            {
                return value;
            }
        }
    }
}