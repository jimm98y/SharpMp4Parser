﻿/*
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

using SharpMp4Parser.Java;
using System.Text;

namespace SharpMp4Parser.IsoParser.Boxes.SampleGrouping
{
    /**
     * <h1>4cc = "{@value #TYPE}"</h1>
     * <p>
     * For some coding systems a sync sample is specified to be a random access point after which all samples in decoding
     * order can be correctly decoded. However, it may be possible to encode an “open” random access point, after which all
     * samples in output order can be correctly decoded, but some samples following the random access point in decoding
     * order and preceding the random access point in output order need not be correctly decodable. For example, an intra
     * picture starting an open group of pictures can be followed in decoding order by (bi-)predicted pictures that however
     * precede the intra picture in output order; though they possibly cannot be correctly decoded if the decoding starts
     * from the intra picture, they are not needed.
     * </p>
     * <p>
     * Such "open" random-access samples can be marked by being a member of this group. Samples marked by this group must
     * be random access points, and may also be sync points (i.e. it is not required that samples marked by the sync sample
     * table be excluded).
     * </p>
     */
    public class VisualRandomAccessEntry : GroupEntry
    {
        public const string TYPE = "rap ";
        private bool numLeadingSamplesKnown;
        private short numLeadingSamples;

        public override string getType()
        {
            return TYPE;
        }

        public bool isNumLeadingSamplesKnown()
        {
            return numLeadingSamplesKnown;
        }

        public void setNumLeadingSamplesKnown(bool numLeadingSamplesKnown)
        {
            this.numLeadingSamplesKnown = numLeadingSamplesKnown;
        }

        public short getNumLeadingSamples()
        {
            return numLeadingSamples;
        }

        public void setNumLeadingSamples(short numLeadingSamples)
        {
            this.numLeadingSamples = numLeadingSamples;
        }

        public override void parse(ByteBuffer byteBuffer)
        {
            byte b = byteBuffer.get();
            numLeadingSamplesKnown = (b & 0x80) == 0x80;
            numLeadingSamples = (short)(b & 0x7f);
        }

        public override ByteBuffer get()
        {
            ByteBuffer content = ByteBuffer.allocate(1);
            content.put((byte)((numLeadingSamplesKnown ? 0x80 : 0x00) | numLeadingSamples & 0x7f));
            content.rewind();
            return content;
        }

        public override bool Equals(object o)
        {
            if (this == o) return true;
            if (o == null || GetType() != o.GetType()) return false;

            VisualRandomAccessEntry that = (VisualRandomAccessEntry)o;

            if (numLeadingSamples != that.numLeadingSamples) return false;
            if (numLeadingSamplesKnown != that.numLeadingSamplesKnown) return false;

            return true;
        }

        public override int GetHashCode()
        {
            int result = numLeadingSamplesKnown ? 1 : 0;
            result = 31 * result + numLeadingSamples;
            return result;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("VisualRandomAccessEntry");
            sb.Append("{numLeadingSamplesKnown=").Append(numLeadingSamplesKnown);
            sb.Append(", numLeadingSamples=").Append(numLeadingSamples);
            sb.Append('}');
            return sb.ToString();
        }
    }
}
