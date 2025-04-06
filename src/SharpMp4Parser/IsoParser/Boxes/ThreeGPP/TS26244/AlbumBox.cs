﻿/*  
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
using System.Text;

namespace SharpMp4Parser.IsoParser.Boxes.ThreeGPP.TS26244
{
    /**
     * <h1>4cc = "{@value #TYPE}"</h1>
     * Meta information in a 'udta' box about a track.
     * Defined in 3GPP 26.244.
     *
     * @see UserDataBox
     */
    public class AlbumBox : AbstractFullBox
    {
        public const string TYPE = "albm";

        private string language;
        private string albumTitle;
        private int trackNumber;

        public AlbumBox() : base(TYPE)
        { }

        /**
         * Declares the language code for the {@link #getAlbumTitle()} return value. See ISO 639-2/T for the set of three
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

        public string getAlbumTitle()
        {
            return albumTitle;
        }

        public void setAlbumTitle(string albumTitle)
        {
            this.albumTitle = albumTitle;
        }

        public int getTrackNumber()
        {
            return trackNumber;
        }

        public void setTrackNumber(int trackNumber)
        {
            this.trackNumber = trackNumber;
        }

        protected override long getContentSize()
        {
            return 6 + Utf8.utf8StringLengthInBytes(albumTitle) + 1 + (trackNumber == -1 ? 0 : 1);
        }

        protected override void _parseDetails(ByteBuffer content)
        {
            parseVersionAndFlags(content);
            language = IsoTypeReader.readIso639(content);
            albumTitle = IsoTypeReader.readString(content);

            if (content.remaining() > 0)
            {
                trackNumber = IsoTypeReader.readUInt8(content);
            }
            else
            {
                trackNumber = -1;
            }
        }

        protected override void getContent(ByteBuffer byteBuffer)
        {
            writeVersionAndFlags(byteBuffer);
            IsoTypeWriter.writeIso639(byteBuffer, language);
            byteBuffer.put(Utf8.convert(albumTitle));
            byteBuffer.put(0);
            if (trackNumber != -1)
            {
                IsoTypeWriter.writeUInt8(byteBuffer, trackNumber);
            }
        }

        public override string ToString()
        {
            StringBuilder buffer = new StringBuilder();
            buffer.Append("AlbumBox[language=").Append(getLanguage()).Append(";");
            buffer.Append("albumTitle=").Append(getAlbumTitle());
            if (trackNumber >= 0)
            {
                buffer.Append(";trackNumber=").Append(getTrackNumber());
            }
            buffer.Append("]");
            return buffer.ToString();
        }
    }
}
