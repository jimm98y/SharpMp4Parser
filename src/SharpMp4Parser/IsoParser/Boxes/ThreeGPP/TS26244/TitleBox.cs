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

using SharpMp4Parser.IsoParser.Support;
using SharpMp4Parser.IsoParser.Tools;
using SharpMp4Parser.Java;

namespace SharpMp4Parser.IsoParser.Boxes.ThreeGPP.TS26244
{
    /**
      * <h1>4cc = "{@value #TYPE}"</h1>
      * <pre>
      * Box Type: 'titl'
      * Container: {@link UserDataBox} ('udta')
      * Mandatory: No
      * Quantity: Zero or one
      * </pre>
      * Title for the media.
      */
    public class TitleBox : AbstractFullBox
    {
        public const string TYPE = "titl";

        private string language;
        private string title;

        public TitleBox() : base(TYPE)
        { }

        public string getLanguage()
        {
            return language;
        }

        /**
         * Sets the 3-letter ISO-639 language for this title.
         *
         * @param language 3-letter ISO-639 code
         */
        public void setLanguage(string language)
        {
            this.language = language;
        }

        public string getTitle()
        {
            return title;
        }

        public void setTitle(string title)
        {
            this.title = title;
        }

        protected override long getContentSize()
        {
            return 7 + Utf8.utf8StringLengthInBytes(title);
        }


        protected override void getContent(ByteBuffer byteBuffer)
        {
            writeVersionAndFlags(byteBuffer);
            IsoTypeWriter.writeIso639(byteBuffer, language);
            byteBuffer.put(Utf8.convert(title));
            byteBuffer.put(0);
        }

        protected override void _parseDetails(ByteBuffer content)
        {
            parseVersionAndFlags(content);
            language = IsoTypeReader.readIso639(content);
            title = IsoTypeReader.readString(content);
        }

        public override string ToString()
        {
            return "TitleBox[language=" + getLanguage() + ";title=" + getTitle() + "]";
        }
    }
}
