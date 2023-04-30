using SharpMp4Parser.Support;

namespace SharpMp4Parser.Boxes.Apple
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
