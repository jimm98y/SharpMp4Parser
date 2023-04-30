using SharpMp4Parser.IsoParser.Support;

namespace SharpMp4Parser.IsoParser.Boxes.Apple
{
    /**
     * <h1>4cc = "{@value #TYPE}"</h1>
     */
    public class GenericMediaHeaderAtom : AbstractContainerBox
    {

        public const string TYPE = "gmhd";

        public GenericMediaHeaderAtom() : base(TYPE)
        { }
    }
}
