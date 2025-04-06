using SharpMp4Parser.Java;
using System;

namespace SharpMp4Parser.Streaming
{
    /**
     * The most simple sample has a presentation time and content.
     */
    public interface StreamingSample
    {
        ByteBuffer getContent();

        long getDuration();

        T getSampleExtension<T>(Type clazz) where T : SampleExtension;

        void addSampleExtension(SampleExtension sampleExtension);

        T removeSampleExtension<T>(Type clazz) where T : SampleExtension;
    }
}
