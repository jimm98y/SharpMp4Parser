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

using System.Collections.Generic;

namespace SharpMp4Parser.Boxes.ISO14496.Part15
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
            this.avcDecoderConfigurationRecord.configurationVersion = configurationVersion;
        }

        public int getAvcProfileIndication()
        {
            return avcDecoderConfigurationRecord.avcProfileIndication;
        }

        public void setAvcProfileIndication(int avcProfileIndication)
        {
            this.avcDecoderConfigurationRecord.avcProfileIndication = avcProfileIndication;
        }

        public int getProfileCompatibility()
        {
            return avcDecoderConfigurationRecord.profileCompatibility;
        }

        public void setProfileCompatibility(int profileCompatibility)
        {
            this.avcDecoderConfigurationRecord.profileCompatibility = profileCompatibility;
        }

        public int getAvcLevelIndication()
        {
            return avcDecoderConfigurationRecord.avcLevelIndication;
        }

        public void setAvcLevelIndication(int avcLevelIndication)
        {
            this.avcDecoderConfigurationRecord.avcLevelIndication = avcLevelIndication;
        }

        public int getLengthSizeMinusOne()
        {
            return avcDecoderConfigurationRecord.lengthSizeMinusOne;
        }

        public void setLengthSizeMinusOne(int lengthSizeMinusOne)
        {
            this.avcDecoderConfigurationRecord.lengthSizeMinusOne = lengthSizeMinusOne;
        }

        public List<ByteBuffer> getSequenceParameterSets()
        {
            return avcDecoderConfigurationRecord.sequenceParameterSets.ToList();
        }

        public void setSequenceParameterSets(List<ByteBuffer> sequenceParameterSets)
        {
            this.avcDecoderConfigurationRecord.sequenceParameterSets = sequenceParameterSets;
        }

        public List<ByteBuffer> getPictureParameterSets()
        {
            return avcDecoderConfigurationRecord.pictureParameterSets.ToList();
        }

        public void setPictureParameterSets(List<ByteBuffer> pictureParameterSets)
        {
            this.avcDecoderConfigurationRecord.pictureParameterSets = pictureParameterSets;
        }

        public int getChromaFormat()
        {
            return avcDecoderConfigurationRecord.chromaFormat;
        }

        public void setChromaFormat(int chromaFormat)
        {
            this.avcDecoderConfigurationRecord.chromaFormat = chromaFormat;
        }

        public int getBitDepthLumaMinus8()
        {
            return avcDecoderConfigurationRecord.bitDepthLumaMinus8;
        }

        public void setBitDepthLumaMinus8(int bitDepthLumaMinus8)
        {
            this.avcDecoderConfigurationRecord.bitDepthLumaMinus8 = bitDepthLumaMinus8;
        }

        public int getBitDepthChromaMinus8()
        {
            return avcDecoderConfigurationRecord.bitDepthChromaMinus8;
        }

        public void setBitDepthChromaMinus8(int bitDepthChromaMinus8)
        {
            this.avcDecoderConfigurationRecord.bitDepthChromaMinus8 = bitDepthChromaMinus8;
        }

        public List<ByteBuffer> getSequenceParameterSetExts()
        {
            return avcDecoderConfigurationRecord.sequenceParameterSetExts;
        }

        public void setSequenceParameterSetExts(List<ByteBuffer> sequenceParameterSetExts)
        {
            this.avcDecoderConfigurationRecord.sequenceParameterSetExts = sequenceParameterSetExts;
        }

        public bool hasExts()
        {
            return avcDecoderConfigurationRecord.hasExts;
        }

        public void setHasExts(bool hasExts)
        {
            this.avcDecoderConfigurationRecord.hasExts = hasExts;
        }

        public override void _parseDetails(ByteBuffer content)
        {
            avcDecoderConfigurationRecord = new AvcDecoderConfigurationRecord(content);
        }


        public override long getContentSize()
        {
            return avcDecoderConfigurationRecord.getContentSize();
        }


        public override void getContent(ByteBuffer byteBuffer)
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
