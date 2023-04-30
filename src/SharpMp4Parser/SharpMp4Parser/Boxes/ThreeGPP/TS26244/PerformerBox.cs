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

namespace SharpMp4Parser.Boxes.ThreeGPP.TS26244
{
    /**
     * <h1>4cc = "{@value #TYPE}"</h1>
     * Used to give information about the performer. Mostly used in confunction with music files.
     * See 3GPP 26.234 for details.
     */
    public class PerformerBox : AbstractFullBox
    {
        public const string TYPE = "perf";

        private string language;
        private string performer;

        public PerformerBox() : base(TYPE)
        { }

        public string getLanguage()
        {
            return language;
        }

        public void setLanguage(string language)
        {
            this.language = language;
        }

        public string getPerformer()
        {
            return performer;
        }

        public void setPerformer(string performer)
        {
            this.performer = performer;
        }

        protected override long getContentSize()
        {
            return 6 + Utf8.utf8StringLengthInBytes(performer) + 1;
        }

        protected override void getContent(ByteBuffer byteBuffer)
        {
            writeVersionAndFlags(byteBuffer);
            IsoTypeWriter.writeIso639(byteBuffer, language);
            byteBuffer.put(Utf8.convert(performer));
            byteBuffer.put((byte)0);
        }

        protected override void _parseDetails(ByteBuffer content)
        {
            parseVersionAndFlags(content);
            language = IsoTypeReader.readIso639(content);
            performer = IsoTypeReader.readString(content);
        }

        public override string ToString()
        {
            return "PerformerBox[language=" + getLanguage() + ";performer=" + getPerformer() + "]";
        }
    }
}
