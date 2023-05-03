using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part12;
using SharpMp4Parser.Java;
using SharpMp4Parser.Streaming.Output;
using System;

namespace SharpMp4Parser.Streaming
{
    public interface StreamingTrack : Closeable
    {
        /**
         * Gets the time scale of the track. Typically called by the SampleSink.
         * Might throw IllegalStateException if called before the first sample has been pushed into the SampleSink.
         *
         * @return the track's time scale
         */
        long getTimescale();

        string getHandler();


        string getLanguage();

        /**
         * All implementing classes must make sure the all generated samples are pushed to the sampleSink.
         * When a sample is pushed all methods must yield valid results.
         *
         * @param sampleSink the sink for all generated samples.
         */
        void setSampleSink(SampleSink sampleSink);

        SampleDescriptionBox getSampleDescriptionBox();

        T getTrackExtension<T>(Type clazz) where T : TrackExtension;

        void addTrackExtension(TrackExtension trackExtension);

        void removeTrackExtension(Type clazz);
    }
}