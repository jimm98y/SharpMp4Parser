using SharpMp4Parser.IsoParser.Support;
using SharpMp4Parser.Java;
using System.Collections.Generic;

namespace SharpMp4Parser.IsoParser.Boxes.ISO14496.Part15
{
    /**
     * Created by sannies on 08.09.2014.
     */
    public class HevcConfigurationBox : AbstractBox
    {
        public const string TYPE = "hvcC";


        private HevcDecoderConfigurationRecord hevcDecoderConfigurationRecord;

        public HevcConfigurationBox() : base(TYPE)
        {
            hevcDecoderConfigurationRecord = new HevcDecoderConfigurationRecord();
        }

        protected override long getContentSize()
        {
            return hevcDecoderConfigurationRecord.getSize();
        }

        protected override void getContent(ByteBuffer byteBuffer)
        {
            hevcDecoderConfigurationRecord.write(byteBuffer);
        }

        protected override void _parseDetails(ByteBuffer content)
        {
            hevcDecoderConfigurationRecord.parse(content);
        }

        public HevcDecoderConfigurationRecord getHevcDecoderConfigurationRecord()
        {
            return hevcDecoderConfigurationRecord;
        }

        public void setHevcDecoderConfigurationRecord(HevcDecoderConfigurationRecord hevcDecoderConfigurationRecord)
        {
            this.hevcDecoderConfigurationRecord = hevcDecoderConfigurationRecord;
        }

        public override bool Equals(object o)
        {
            if (this == o) return true;
            if (o == null || GetType() != o.GetType()) return false;

            HevcConfigurationBox that = (HevcConfigurationBox)o;

            if (hevcDecoderConfigurationRecord != null ? !hevcDecoderConfigurationRecord.Equals(that.hevcDecoderConfigurationRecord) : that.hevcDecoderConfigurationRecord != null)
                return false;

            return true;
        }

        public override int GetHashCode()
        {
            return hevcDecoderConfigurationRecord != null ? hevcDecoderConfigurationRecord.GetHashCode() : 0;
        }


        public int getConfigurationVersion()
        {
            return hevcDecoderConfigurationRecord.configurationVersion;
        }

        public int getGeneral_profile_space()
        {
            return hevcDecoderConfigurationRecord.general_profile_space;
        }

        public bool isGeneral_tier_flag()
        {
            return hevcDecoderConfigurationRecord.general_tier_flag;
        }


        public int getGeneral_profile_idc()
        {
            return hevcDecoderConfigurationRecord.general_profile_idc;
        }

        public long getGeneral_profile_compatibility_flags()
        {
            return hevcDecoderConfigurationRecord.general_profile_compatibility_flags;
        }

        public long getGeneral_constraint_indicator_flags()
        {
            return hevcDecoderConfigurationRecord.general_constraint_indicator_flags;
        }

        public int getGeneral_level_idc()
        {
            return hevcDecoderConfigurationRecord.general_level_idc;
        }

        public int getMin_spatial_segmentation_idc()
        {
            return hevcDecoderConfigurationRecord.min_spatial_segmentation_idc;
        }

        public int getParallelismType()
        {
            return hevcDecoderConfigurationRecord.parallelismType;
        }

        public int getChromaFormat()
        {
            return hevcDecoderConfigurationRecord.chromaFormat;
        }

        public int getBitDepthLumaMinus8()
        {
            return hevcDecoderConfigurationRecord.bitDepthLumaMinus8;
        }

        public int getBitDepthChromaMinus8()
        {
            return hevcDecoderConfigurationRecord.bitDepthChromaMinus8;
        }

        public int getAvgFrameRate()
        {
            return hevcDecoderConfigurationRecord.avgFrameRate;
        }

        public int getNumTemporalLayers()
        {
            return hevcDecoderConfigurationRecord.numTemporalLayers;
        }

        public int getLengthSizeMinusOne()
        {
            return hevcDecoderConfigurationRecord.lengthSizeMinusOne;
        }

        public bool isTemporalIdNested()
        {
            return hevcDecoderConfigurationRecord.temporalIdNested;
        }

        public int getConstantFrameRate()
        {
            return hevcDecoderConfigurationRecord.constantFrameRate;
        }

        public List<HevcDecoderConfigurationRecord.Array> getArrays()
        {
            return hevcDecoderConfigurationRecord.arrays;
        }
    }
}
