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

using SharpMp4Parser.Java;
using System.Text;

namespace SharpMp4Parser.IsoParser.Boxes.SampleGrouping
{
    /**
     * <h1>4cc = "{@value #TYPE}"</h1>
     * The Temporal Level sample grouping ('tele') provides a codec-independent sample grouping that can be used to group samples (access units) in a track (and potential track fragments) according to temporal level, where samples of one temporal level have no coding dependencies on samples of higher temporal levels. The temporal level equals the sample group description index (taking values 1, 2, 3, etc). The bitstream containing only the access units from the first temporal level to a higher temporal level remains conforming to the coding standard.
     * <p>
     * A grouping according to temporal level facilitates easy extraction of temporal subsequences, for instance using the Subsegment Indexing box in 0.</p>
     */
    public class TemporalLevelEntry : GroupEntry
    {
        public const string TYPE = "tele";
        private bool levelIndependentlyDecodable;
        private short reserved = 0;

        public override string getType()
        {
            return TYPE;
        }

        public bool isLevelIndependentlyDecodable()
        {
            return levelIndependentlyDecodable;
        }

        public void setLevelIndependentlyDecodable(bool levelIndependentlyDecodable)
        {
            this.levelIndependentlyDecodable = levelIndependentlyDecodable;
        }

        public override void parse(ByteBuffer byteBuffer)
        {
            byte b = byteBuffer.get();
            levelIndependentlyDecodable = (b & 0x80) == 0x80;
        }

        public override ByteBuffer get()
        {
            ByteBuffer content = ByteBuffer.allocate(1);
            content.put((byte)(levelIndependentlyDecodable ? 0x80 : 0x00));
            content.rewind();
            return content;
        }

        public override bool Equals(object o)
        {
            if (this == o) return true;
            if (o == null || GetType() != o.GetType()) return false;

            TemporalLevelEntry that = (TemporalLevelEntry)o;

            if (levelIndependentlyDecodable != that.levelIndependentlyDecodable) return false;
            if (reserved != that.reserved) return false;

            return true;
        }

        public override int GetHashCode()
        {
            int result = levelIndependentlyDecodable ? 1 : 0;
            result = 31 * result + reserved;
            return result;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("TemporalLevelEntry");
            sb.Append("{levelIndependentlyDecodable=").Append(levelIndependentlyDecodable);
            sb.Append('}');
            return sb.ToString();
        }
    }
}