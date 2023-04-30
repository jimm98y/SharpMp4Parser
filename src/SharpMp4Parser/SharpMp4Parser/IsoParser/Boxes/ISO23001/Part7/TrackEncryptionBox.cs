namespace SharpMp4Parser.IsoParser.Boxes.ISO23001.Part7
{
    /**
     * <h1>4cc = "{@value #TYPE}"</h1>
     */
    public class TrackEncryptionBox : AbstractTrackEncryptionBox
    {
        public const string TYPE = "tenc";

        public TrackEncryptionBox() : base(TYPE)
        { }
    }
}