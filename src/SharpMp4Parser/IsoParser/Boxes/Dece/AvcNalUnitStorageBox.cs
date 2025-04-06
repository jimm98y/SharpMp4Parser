/*
 * Copyright 2011 castLabs, Berlin
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

using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part15;
using SharpMp4Parser.IsoParser.Support;
using SharpMp4Parser.Java;
using System.Collections.Generic;

namespace SharpMp4Parser.IsoParser.Boxes.Dece
{
    /**
     * <h1>4cc = "{@value #TYPE}"</h1>
     * The AVC NAL Unit Storage Box SHALL contain an AVCDecoderConfigurationRecord,
     * as defined in section 5.2.4.1 of the ISO 14496-12.
     */
    public class AvcNalUnitStorageBox : AbstractBox
    {
        public const string TYPE = "avcn";
        AvcDecoderConfigurationRecord avcDecoderConfigurationRecord;

        public AvcNalUnitStorageBox() : base(TYPE)
        { }

        public AvcNalUnitStorageBox(AvcConfigurationBox avcConfigurationBox) : base(TYPE)
        {
            avcDecoderConfigurationRecord = avcConfigurationBox.getavcDecoderConfigurationRecord();
        }

        public AvcDecoderConfigurationRecord getAvcDecoderConfigurationRecord()
        {
            return avcDecoderConfigurationRecord;
        }

        // just to display sps in isoviewer no practical use
        public int getLengthSizeMinusOne()
        {
            return avcDecoderConfigurationRecord.lengthSizeMinusOne;
        }

        public List<string> getSequenceParameterSetsAsStrings()
        {
            return avcDecoderConfigurationRecord.getSequenceParameterSetsAsStrings();
        }

        public List<string> getSequenceParameterSetExtsAsStrings()
        {
            return avcDecoderConfigurationRecord.getSequenceParameterSetExtsAsStrings();
        }

        public List<string> getPictureParameterSetsAsStrings()
        {
            return avcDecoderConfigurationRecord.getPictureParameterSetsAsStrings();
        }

        protected override long getContentSize()
        {
            return avcDecoderConfigurationRecord.getContentSize();
        }

        protected override void _parseDetails(ByteBuffer content)
        {
            avcDecoderConfigurationRecord = new AvcDecoderConfigurationRecord(content);
        }

        protected override void getContent(ByteBuffer byteBuffer)
        {
            avcDecoderConfigurationRecord.getContent(byteBuffer);
        }

        public override string ToString()
        {
            return "AvcNalUnitStorageBox{" +
                    "SPS=" + avcDecoderConfigurationRecord.getSequenceParameterSetsAsStrings() +
                    ",PPS=" + avcDecoderConfigurationRecord.getPictureParameterSetsAsStrings() +
                    ",lengthSize=" + (avcDecoderConfigurationRecord.lengthSizeMinusOne + 1) +
                    '}';
        }
    }
}
