using System.Collections.Concurrent;

namespace SharpMp4Parser.Streaming.Extensions
{
    public class CompositionTimeSampleExtension : SampleExtension
    {
        public static ConcurrentDictionary<long, CompositionTimeSampleExtension> pool =
                new ConcurrentDictionary<long, CompositionTimeSampleExtension>();
        private long ctts;

        public static CompositionTimeSampleExtension create(long offset)
        {
            CompositionTimeSampleExtension c;

            if (!pool.TryGetValue(offset, out c))
            {
                c = new CompositionTimeSampleExtension();
                c.ctts = offset;
                pool.TryAdd(offset, c);
            }
            return c;
        }

        /**
         * This value provides the offset between decoding time and composition time. The offset is expressed as
         * signed long such that CT(n) = DT(n) + CTTS(n). This method is
         *
         * @return offset between decoding time and composition time.
         */
        public long getCompositionTimeOffset()
        {
            return ctts;
        }

        public override string ToString()
        {
            return "ctts=" + ctts;
        }
    }
}
