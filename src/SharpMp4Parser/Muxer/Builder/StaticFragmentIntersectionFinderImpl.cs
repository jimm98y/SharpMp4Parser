using System.Collections.Generic;

namespace SharpMp4Parser.Muxer.Builder
{
    /**
     * Uses a predefined list of sample numbers to divide up a track.
     */
    public class StaticFragmentIntersectionFinderImpl : Fragmenter
    {
        Dictionary<Track, long[]> _sampleNumbers;

        public StaticFragmentIntersectionFinderImpl(Dictionary<Track, long[]> sampleNumbers)
        {
            this._sampleNumbers = sampleNumbers;
        }

        public long[] sampleNumbers(Track track)
        {
            return _sampleNumbers[track];
        }
    }
}
