﻿namespace SharpMp4Parser.Muxer.Tracks.H265
{
    /**
     * Created by sannies on 02.01.2015.
     */
    public class H265NalUnitHeader
    {
        public int forbiddenZeroFlag;
        public int nalUnitType;
        public int nuhLayerId;
        public int nuhTemporalIdPlusOne;
    }
}
