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
      * The copyright box contains a copyright declaration which applies to the entire presentation, when contained
      * within the MovieBox, or, when contained in a track, to that entire track. There may be multple boxes using
      * different language codes.
      *
      * @see MovieBox
      * @see TrackBox
      */
    public class CopyrightBox : AbstractFullBox
    {
        public const string TYPE = "cprt";

        private string language;
        private string copyright;

        public CopyrightBox() : base(TYPE)
        { }

        public string getLanguage()
        {
            return language;
        }

        public void setLanguage(string language)
        {
            this.language = language;
        }

        public string getCopyright()
        {
            return copyright;
        }

        public void setCopyright(string copyright)
        {
            this.copyright = copyright;
        }

        protected override long getContentSize()
        {
            return 7 + Utf8.utf8StringLengthInBytes(copyright);
        }

        protected override void _parseDetails(ByteBuffer content)
        {
            parseVersionAndFlags(content);
            language = IsoTypeReader.readIso639(content);
            copyright = IsoTypeReader.readString(content);
        }

        protected override void getContent(ByteBuffer byteBuffer)
        {
            writeVersionAndFlags(byteBuffer);
            IsoTypeWriter.writeIso639(byteBuffer, language);
            byteBuffer.put(Utf8.convert(copyright));
            byteBuffer.put(0);
        }

        public override string ToString()
        {
            return "CopyrightBox[language=" + getLanguage() + ";copyright=" + getCopyright() + "]";
        }
    }
}
