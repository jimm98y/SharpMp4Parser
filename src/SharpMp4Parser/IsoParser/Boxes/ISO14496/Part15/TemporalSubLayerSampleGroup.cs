﻿using SharpMp4Parser.IsoParser.Boxes.SampleGrouping;
using SharpMp4Parser.Java;

namespace SharpMp4Parser.IsoParser.Boxes.ISO14496.Part15
{
    /**
     * This sample group is used to mark temporal layer access (TSA) samples.
     */
    public class TemporalSubLayerSampleGroup : GroupEntry
    {
        public const string TYPE = "tsas";
        //int i;

        public override void parse(ByteBuffer byteBuffer)
        {
        }

        public override string getType()
        {
            return TYPE;
        }

        public override ByteBuffer get()
        {
            return ByteBuffer.allocate(0);
        }

        public override bool Equals(object o)
        {
            if (this == o) return true;
            if (o == null || GetType() != o.GetType()) return false;


            return true;
        }

        public override int GetHashCode()
        {
            return 41;
        }
    }
}
