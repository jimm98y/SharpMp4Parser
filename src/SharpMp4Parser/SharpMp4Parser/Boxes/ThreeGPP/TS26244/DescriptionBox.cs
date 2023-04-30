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

namespace SharpMp4Parser.Boxes.ThreeGPP.TS26244
{
    /**
     * <h1>4cc = "{@value #TYPE}"</h1>
     * Gives a language dependent description of the media contained in the ISO file.
     */
    public class DescriptionBox : AbstractFullBox
    {
        public const string TYPE = "dscp";

        private string language;
        private string description;

        public DescriptionBox() : base(TYPE)
        { }

        public string getLanguage()
        {
            return language;
        }

        public void setLanguage(string language)
        {
            this.language = language;
        }

        public string getDescription()
        {
            return description;
        }

        public void setDescription(string description)
        {
            this.description = description;
        }

        protected long getContentSize()
        {
            return 7 + Utf8.utf8StringLengthInBytes(description);
        }

        public override void _parseDetails(ByteBuffer content)
        {
            parseVersionAndFlags(content);
            language = IsoTypeReader.readIso639(content);
            description = IsoTypeReader.readString(content);
        }

        protected override void getContent(ByteBuffer byteBuffer)
        {
            writeVersionAndFlags(byteBuffer);
            IsoTypeWriter.writeIso639(byteBuffer, language);
            byteBuffer.put(Utf8.convert(description));
            byteBuffer.put((byte)0);
        }

        public override string ToString()
        {
            return "DescriptionBox[language=" + getLanguage() + ";description=" + getDescription() + "]";
        }
    }
}
