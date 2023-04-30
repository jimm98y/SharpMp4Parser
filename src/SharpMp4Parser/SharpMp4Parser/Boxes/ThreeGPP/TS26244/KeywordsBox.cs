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
using System.Text;

namespace SharpMp4Parser.Boxes.ThreeGPP.TS26244
{
    /**
     * <h1>4cc = "{@value #TYPE}"</h1>
     * List of keywords according to 3GPP 26.244.
     */
    public class KeywordsBox : AbstractFullBox
    {
        public const string TYPE = "kywd";

        private string language;
        private string[] keywords;

        public KeywordsBox() : base(TYPE)
        { }

        public string getLanguage()
        {
            return language;
        }

        public void setLanguage(string language)
        {
            this.language = language;
        }

        public string[] getKeywords()
        {
            return keywords;
        }

        public void setKeywords(string[] keywords)
        {
            this.keywords = keywords;
        }

        protected override long getContentSize()
        {
            long contentSize = 7;
            foreach (string keyword in keywords)
            {
                contentSize += 1 + Utf8.utf8StringLengthInBytes(keyword) + 1;
            }
            return contentSize;
        }

        protected override void _parseDetails(ByteBuffer content)
        {
            parseVersionAndFlags(content);
            language = IsoTypeReader.readIso639(content);
            int keywordCount = IsoTypeReader.readUInt8(content);
            keywords = new string[keywordCount];
            for (int i = 0; i < keywordCount; i++)
            {
                IsoTypeReader.readUInt8(content);
                keywords[i] = IsoTypeReader.readString(content);
            }
        }

        protected override void getContent(ByteBuffer byteBuffer)
        {
            writeVersionAndFlags(byteBuffer);
            IsoTypeWriter.writeIso639(byteBuffer, language);
            IsoTypeWriter.writeUInt8(byteBuffer, keywords.Length);
            foreach (string keyword in keywords)
            {
                IsoTypeWriter.writeUInt8(byteBuffer, Utf8.utf8StringLengthInBytes(keyword) + 1);
                byteBuffer.put(Utf8.convert(keyword));
            }
        }

        public override string ToString()
        {
            StringBuilder buffer = new StringBuilder();
            buffer.Append("KeywordsBox[language=").Append(getLanguage());
            for (int i = 0; i < keywords.Length; i++)
            {
                buffer.Append(";keyword").Append(i).Append("=").Append(keywords[i]);
            }
            buffer.Append("]");
            return buffer.ToString();
        }
    }
}
