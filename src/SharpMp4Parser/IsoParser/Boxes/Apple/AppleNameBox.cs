namespace SharpMp4Parser.IsoParser.Boxes.Apple
{
    /**
     * Created by sannies on 10/15/13.
     */
    public class AppleNameBox : Utf8AppleDataBox
    {
        public const string TYPE = "©nam";

        public AppleNameBox() : base(TYPE)
        { }
    }
}
