namespace SharpMp4Parser.Streaming.Extensions
{
    /**
     * Specifies the track ID of a track - if not set it's up to the StreamingMp4Writer to assume one.
     */
    public class TrackIdTrackExtension : TrackExtension
    {
        private long trackId = 1;

        public TrackIdTrackExtension(long trackId)
        {
            this.trackId = trackId;
        }

        public long getTrackId()
        {
            return trackId;
        }
    }
}
