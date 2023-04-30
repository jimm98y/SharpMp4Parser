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
     * Meta information in a 'udta' box about a track.
     * Defined in 3GPP 26.244.
     *
     * @see UserDataBox
     */
    public class AuthorBox : AbstractFullBox
    {
        public const string TYPE = "auth";

        private string language;
        private string author;

        public AuthorBox() : base(TYPE)
        { }

        /**
         * Declares the language code for the {@link #getAuthor()} return value. See ISO 639-2/T for the set of three
         * character codes.Each character is packed as the difference between its ASCII value and 0x60. The code is
         * confined to being three lower-case letters, so these values are strictly positive.
         *
         * @return the language code
         */
        public string getLanguage()
        {
            return language;
        }

        public void setLanguage(string language)
        {
            this.language = language;
        }

        /**
         * Author information.
         *
         * @return the author
         */
        public string getAuthor()
        {
            return author;
        }

        public void setAuthor(string author)
        {
            this.author = author;
        }

        protected long getContentSize()
        {
            return 7 + Utf8.utf8StringLengthInBytes(author);
        }

        public override void _parseDetails(ByteBuffer content)
        {
            parseVersionAndFlags(content);
            language = IsoTypeReader.readIso639(content);
            author = IsoTypeReader.readString(content);
        }

        protected override void getContent(ByteBuffer byteBuffer)
        {
            writeVersionAndFlags(byteBuffer);
            IsoTypeWriter.writeIso639(byteBuffer, language);
            byteBuffer.put(Utf8.convert(author));
            byteBuffer.put((byte)0);
        }

        public override string ToString()
        {
            return "AuthorBox[language=" + getLanguage() + ";author=" + getAuthor() + "]";
        }
    }
}