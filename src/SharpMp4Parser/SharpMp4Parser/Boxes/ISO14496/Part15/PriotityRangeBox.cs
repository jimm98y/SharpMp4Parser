﻿namespace SharpMp4Parser.Boxes.ISO14496.Part15
{
    /**
      * Created by sannies on 08.09.2014.
      */
    public class PriotityRangeBox : AbstractBox
    {
        public const string TYPE = "svpr";

        int reserved1 = 0;
        int min_priorityId;
        int reserved2 = 0;
        int max_priorityId;

        public PriotityRangeBox() : base(TYPE)
        { }

        protected override long getContentSize()
        {
            return 2;
        }

        protected override void getContent(ByteBuffer byteBuffer)
        {
            IsoTypeWriter.writeUInt8(byteBuffer, (reserved1 << 6) + min_priorityId);
            IsoTypeWriter.writeUInt8(byteBuffer, (reserved2 << 6) + max_priorityId);
        }

        protected override void _parseDetails(ByteBuffer content)
        {
            min_priorityId = IsoTypeReader.readUInt8(content);
            reserved1 = (min_priorityId & 0xC0) >> 6;
            min_priorityId &= 0x3F;
            max_priorityId = IsoTypeReader.readUInt8(content);
            reserved2 = (max_priorityId & 0xC0) >> 6;
            max_priorityId &= 0x3F;
        }

        public int getReserved1()
        {
            return reserved1;
        }

        public void setReserved1(int reserved1)
        {
            this.reserved1 = reserved1;
        }

        public int getMin_priorityId()
        {
            return min_priorityId;
        }

        public void setMin_priorityId(int min_priorityId)
        {
            this.min_priorityId = min_priorityId;
        }

        public int getReserved2()
        {
            return reserved2;
        }

        public void setReserved2(int reserved2)
        {
            this.reserved2 = reserved2;
        }

        public int getMax_priorityId()
        {
            return max_priorityId;
        }

        public void setMax_priorityId(int max_priorityId)
        {
            this.max_priorityId = max_priorityId;
        }
    }
}
