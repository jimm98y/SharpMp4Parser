namespace SharpMp4Parser.IsoParser.Boxes.ISO23001.Part7
{
    /**
      * <h1>4cc = "{@value #TYPE}"</h1>
      * The Sample Encryption Box contains the sample specific encryption data, including the initialization
      * vectors needed for decryption and, optionally, alternative decryption parameters. It is used when the
      * sample data in the fragment might be encrypted.
      */
    public class SampleEncryptionBox : AbstractSampleEncryptionBox
    {
        public const string TYPE = "senc";

        /**
         * Creates a SampleEncryptionBox for non-h264 tracks.
         */
        public SampleEncryptionBox() : base(TYPE)
        { }
    }
}
