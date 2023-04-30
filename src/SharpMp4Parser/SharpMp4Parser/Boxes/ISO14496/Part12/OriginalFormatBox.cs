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

using System;
using System.Diagnostics;

namespace SharpMp4Parser.Boxes.ISO14496.Part12
{
    /**
     * <h1>4cc = "{@value #TYPE}"</h1>
     * The Original Format Box contains the four-character-code of the original untransformed sample description.
     * See ISO/IEC 14496-12 for details.
     *
     * @see ProtectionSchemeInformationBox
     */

    public class OriginalFormatBox : AbstractBox
    {
        public const string TYPE = "frma";

        private string dataFormat = "    ";

        public OriginalFormatBox() : base("frma")
        { }

        public string getDataFormat()
        {
            return dataFormat;
        }


        public void setDataFormat(String dataFormat)
        {
            Debug.Assert(dataFormat.length() == 4);
            this.dataFormat = dataFormat;
        }

        protected long getContentSize()
        {
            return 4;
        }

        public override void _parseDetails(ByteBuffer content)
        {
            dataFormat = IsoTypeReader.read4cc(content);
        }

        protected override void getContent(ByteBuffer byteBuffer)
        {
            byteBuffer.put(IsoFile.fourCCtoBytes(dataFormat));
        }

        public override string ToString()
        {
            return "OriginalFormatBox[dataFormat=" + getDataFormat() + "]";
        }
    }
}
