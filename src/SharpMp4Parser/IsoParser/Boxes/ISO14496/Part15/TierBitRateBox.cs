﻿using SharpMp4Parser.IsoParser.Support;
using SharpMp4Parser.IsoParser.Tools;
using SharpMp4Parser.Java;

namespace SharpMp4Parser.IsoParser.Boxes.ISO14496.Part15
{
    /**
     * Created by sannies on 08.09.2014.
     */
    public class TierBitRateBox : AbstractBox
    {
        public const string TYPE = "tibr";

        long baseBitRate;
        long maxBitRate;
        long avgBitRate;
        long tierBaseBitRate;
        long tierMaxBitRate;
        long tierAvgBitRate;

        public TierBitRateBox() : base(TYPE)
        { }

        protected override long getContentSize()
        {
            return 24;
        }

        protected override void getContent(ByteBuffer byteBuffer)
        {
            IsoTypeWriter.writeUInt32(byteBuffer, baseBitRate);
            IsoTypeWriter.writeUInt32(byteBuffer, maxBitRate);
            IsoTypeWriter.writeUInt32(byteBuffer, avgBitRate);
            IsoTypeWriter.writeUInt32(byteBuffer, tierBaseBitRate);
            IsoTypeWriter.writeUInt32(byteBuffer, tierMaxBitRate);
            IsoTypeWriter.writeUInt32(byteBuffer, tierAvgBitRate);
        }

        protected override void _parseDetails(ByteBuffer content)
        {
            baseBitRate = IsoTypeReader.readUInt32(content);
            maxBitRate = IsoTypeReader.readUInt32(content);
            avgBitRate = IsoTypeReader.readUInt32(content);
            tierBaseBitRate = IsoTypeReader.readUInt32(content);
            tierMaxBitRate = IsoTypeReader.readUInt32(content);
            tierAvgBitRate = IsoTypeReader.readUInt32(content);
        }

        public long getBaseBitRate()
        {
            return baseBitRate;
        }

        public void setBaseBitRate(long baseBitRate)
        {
            this.baseBitRate = baseBitRate;
        }

        public long getMaxBitRate()
        {
            return maxBitRate;
        }

        public void setMaxBitRate(long maxBitRate)
        {
            this.maxBitRate = maxBitRate;
        }

        public long getAvgBitRate()
        {
            return avgBitRate;
        }

        public void setAvgBitRate(long avgBitRate)
        {
            this.avgBitRate = avgBitRate;
        }

        public long getTierBaseBitRate()
        {
            return tierBaseBitRate;
        }

        public void setTierBaseBitRate(long tierBaseBitRate)
        {
            this.tierBaseBitRate = tierBaseBitRate;
        }

        public long getTierMaxBitRate()
        {
            return tierMaxBitRate;
        }

        public void setTierMaxBitRate(long tierMaxBitRate)
        {
            this.tierMaxBitRate = tierMaxBitRate;
        }

        public long getTierAvgBitRate()
        {
            return tierAvgBitRate;
        }

        public void setTierAvgBitRate(long tierAvgBitRate)
        {
            this.tierAvgBitRate = tierAvgBitRate;
        }
    }
}
