namespace SharpMp4Parser.Streaming.Extensions
{
    /**
     * Gives a track a name.
     */
    public class NameTrackExtension : TrackExtension
    {
        private string name;

        public static NameTrackExtension create(string name)
        {
            NameTrackExtension nameTrackExtension = new NameTrackExtension();
            nameTrackExtension.name = name;
            return nameTrackExtension;
        }

        public string getName()
        {
            return name;
        }
    }
}