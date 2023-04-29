namespace SharpMp4Parser.Boxes.Apple
{
    /**
     * <h1>4cc = "{@value #TYPE}"</h1>
     * undocumented iTunes MetaData Box.
     */
    public class AppleItemListBox : AbstractContainerBox
    {
        public const string TYPE = "ilst";

        public AppleItemListBox() : base(TYPE)
        {  }
    }
}
