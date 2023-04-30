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
using SharpMp4Parser.Java;
using System.Collections.Generic;
using System.Linq;

namespace SharpMp4Parser.IsoParser.Boxes.ISO14496.Part15
{
    /**
     * <h1>4cc = "{@value #TYPE}"</h1>
     * Defined in ISO/IEC 14496-15:2004.
     * <p>Possible paths</p>
     * <ul>
     * <li>/moov/trak/mdia/minf/stbl/stsd/avc1/avcC</li>
     * <li>/moov/trak/mdia/minf/stbl/stsd/drmi/avcC</li>
     * </ul>
     */
    public sealed class AvcConfigurationBox : AbstractBox
    {
        public const string TYPE = "avcC";

        public AvcDecoderConfigurationRecord avcDecoderConfigurationRecord = new AvcDecoderConfigurationRecord();


        public AvcConfigurationBox() : base(TYPE)
        { }

        public int getConfigurationVersion()
        {
            return avcDecoderConfigurationRecord.configurationVersion;
        }

        public void setConfigurationVersion(int configurationVersion)
        {
            avcDecoderConfigurationRecord.configurationVersion = configurationVersion;
        }

        public int getAvcProfileIndication()
        {
            return avcDecoderConfigurationRecord.avcProfileIndication;
        }

        public void setAvcProfileIndication(int avcProfileIndication)
        {
            avcDecoderConfigurationRecord.avcProfileIndication = avcProfileIndication;
        }

        public int getProfileCompatibility()
        {
            return avcDecoderConfigurationRecord.profileCompatibility;
        }

        public void setProfileCompatibility(int profileCompatibility)
        {
            avcDecoderConfigurationRecord.profileCompatibility = profileCompatibility;
        }

        public int getAvcLevelIndication()
        {
            return avcDecoderConfigurationRecord.avcLevelIndication;
        }

        public void setAvcLevelIndication(int avcLevelIndication)
        {
            avcDecoderConfigurationRecord.avcLevelIndication = avcLevelIndication;
        }

        public int getLengthSizeMinusOne()
        {
            return avcDecoderConfigurationRecord.lengthSizeMinusOne;
        }

        public void setLengthSizeMinusOne(int lengthSizeMinusOne)
        {
            avcDecoderConfigurationRecord.lengthSizeMinusOne = lengthSizeMinusOne;
        }

        public List<ByteBuffer> getSequenceParameterSets()
        {
            return avcDecoderConfigurationRecord.sequenceParameterSets.ToList();
        }

        public void setSequenceParameterSets(List<ByteBuffer> sequenceParameterSets)
        {
            avcDecoderConfigurationRecord.sequenceParameterSets = sequenceParameterSets;
        }

        public List<ByteBuffer> getPictureParameterSets()
        {
            return avcDecoderConfigurationRecord.pictureParameterSets.ToList();
        }

        public void setPictureParameterSets(List<ByteBuffer> pictureParameterSets)
        {
            avcDecoderConfigurationRecord.pictureParameterSets = pictureParameterSets;
        }

        public int getChromaFormat()
        {
            return avcDecoderConfigurationRecord.chromaFormat;
        }

        public void setChromaFormat(int chromaFormat)
        {
            avcDecoderConfigurationRecord.chromaFormat = chromaFormat;
        }

        public int getBitDepthLumaMinus8()
        {
            return avcDecoderConfigurationRecord.bitDepthLumaMinus8;
        }

        public void setBitDepthLumaMinus8(int bitDepthLumaMinus8)
        {
            avcDecoderConfigurationRecord.bitDepthLumaMinus8 = bitDepthLumaMinus8;
        }

        public int getBitDepthChromaMinus8()
        {
            return avcDecoderConfigurationRecord.bitDepthChromaMinus8;
        }

        public void setBitDepthChromaMinus8(int bitDepthChromaMinus8)
        {
            avcDecoderConfigurationRecord.bitDepthChromaMinus8 = bitDepthChromaMinus8;
        }

        public List<ByteBuffer> getSequenceParameterSetExts()
        {
            return avcDecoderConfigurationRecord.sequenceParameterSetExts;
        }

        public void setSequenceParameterSetExts(List<ByteBuffer> sequenceParameterSetExts)
        {
            avcDecoderConfigurationRecord.sequenceParameterSetExts = sequenceParameterSetExts;
        }

        public bool hasExts()
        {
            return avcDecoderConfigurationRecord.hasExts;
        }

        public void setHasExts(bool hasExts)
        {
            avcDecoderConfigurationRecord.hasExts = hasExts;
        }

        protected override void _parseDetails(ByteBuffer content)
        {
            avcDecoderConfigurationRecord = new AvcDecoderConfigurationRecord(content);
        }


        protected override long getContentSize()
        {
            return avcDecoderConfigurationRecord.getContentSize();
        }


        protected override void getContent(ByteBuffer byteBuffer)
        {
            avcDecoderConfigurationRecord.getContent(byteBuffer);
        }


        public AvcDecoderConfigurationRecord getavcDecoderConfigurationRecord()
        {
            return avcDecoderConfigurationRecord;
        }

        public override string ToString()
        {
            return "AvcConfigurationBox{" +
                    "avcDecoderConfigurationRecord=" + avcDecoderConfigurationRecord +
                    '}';
        }
    }
}
