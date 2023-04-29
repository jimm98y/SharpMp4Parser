namespace SharpMp4Parser.Boxes.Apple
{
    /**
     * Created by sannies on 10/22/13.
     */
    public class AppleGaplessPlaybackBox : AppleVariableSignedIntegerBox
    {
        public AppleGaplessPlaybackBox() : base("pgap")
        {  }
    }
}
