﻿using SharpMp4Parser.IsoParser.Support;

namespace SharpMp4Parser.IsoParser.Boxes.Apple
{
    /**
     * <h1>4cc = "{@value #TYPE}"</h1>
     * Don't know what it is but it is obviously a container box.
     */
    public class TrackApertureModeDimensionAtom : AbstractContainerBox
    {
        public const string TYPE = "tapt";

        public TrackApertureModeDimensionAtom() : base(TYPE)
        { }
    }
}
