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
     */
    public class RecordingYearBox : AbstractFullBox
    {
        public const string TYPE = "yrrc";

        int recordingYear;

        public RecordingYearBox() : base(TYPE)
        { }

        protected override long getContentSize()
        {
            return 6;
        }

        public int getRecordingYear()
        {
            return recordingYear;
        }

        public void setRecordingYear(int recordingYear)
        {
            this.recordingYear = recordingYear;
        }

        protected override void _parseDetails(ByteBuffer content)
        {
            parseVersionAndFlags(content);
            recordingYear = IsoTypeReader.readUInt16(content);
        }

        protected override void getContent(ByteBuffer byteBuffer)
        {
            writeVersionAndFlags(byteBuffer);
            IsoTypeWriter.writeUInt16(byteBuffer, recordingYear);
        }
    }
}
