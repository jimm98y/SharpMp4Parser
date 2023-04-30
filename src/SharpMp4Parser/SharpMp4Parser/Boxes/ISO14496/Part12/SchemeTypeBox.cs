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

using System.Text;
using System.Diagnostics;

namespace SharpMp4Parser.Boxes.ISO14496.Part12
{
    /**
     * <h1>4cc = "{@value #TYPE}"</h1>
     * The Scheme Type Box identifies the protection scheme. Resides in  a Protection Scheme Information Box or
     * an SRTP Process Box.
     *
     * @see SchemeInformationBox
     */
    public class SchemeTypeBox : AbstractFullBox
    {
        public const string TYPE = "schm";
        string schemeType = "    ";
        long schemeVersion;
        string schemeUri = null;

        public SchemeTypeBox() : base(TYPE)
        { }

        public string getSchemeType()
        {
            return schemeType;
        }

        public void setSchemeType(string schemeType)
        {
            Debug.Assert(schemeType != null && schemeType.length() == 4, "SchemeType may not be null or not 4 bytes long");
            this.schemeType = schemeType;
        }

        public long getSchemeVersion()
        {
            return schemeVersion;
        }

        public void setSchemeVersion(int schemeVersion)
        {
            this.schemeVersion = schemeVersion;
        }

        public string getSchemeUri()
        {
            return schemeUri;
        }

        public void setSchemeUri(string schemeUri)
        {
            this.schemeUri = schemeUri;
        }

        protected long getContentSize()
        {
            return 12 + (((getFlags() & 1) == 1) ? Utf8.utf8StringLengthInBytes(schemeUri) + 1 : 0);
        }

        public override void _parseDetails(ByteBuffer content)
        {
            parseVersionAndFlags(content);
            schemeType = IsoTypeReader.read4cc(content);
            schemeVersion = IsoTypeReader.readUInt32(content);
            if ((getFlags() & 1) == 1)
            {
                schemeUri = IsoTypeReader.readString(content);
            }
        }

        protected override void getContent(ByteBuffer byteBuffer)
        {
            writeVersionAndFlags(byteBuffer);
            byteBuffer.put(IsoFile.fourCCtoBytes(schemeType));
            IsoTypeWriter.writeUInt32(byteBuffer, schemeVersion);
            if ((getFlags() & 1) == 1)
            {
                byteBuffer.put(Utf8.convert(schemeUri));
            }
        }

        public string toString()
        {
            StringBuilder buffer = new StringBuilder();
            buffer.Append("Schema Type Box[");
            buffer.Append("schemeUri=").Append(schemeUri).Append("; ");
            buffer.Append("schemeType=").Append(schemeType).Append("; ");
            buffer.Append("schemeVersion=").Append(schemeVersion).Append("; ");
            buffer.Append("]");
            return buffer.ToString();
        }
    }
}
