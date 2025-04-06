﻿using SharpMp4Parser.Java;

namespace SharpMp4Parser.Streaming.Output
{
    /**
     * Controls the creation of media files.
     *
     * @see FragmentedMp4Writer
     * @see StreamingTrack#setSampleSink(SampleSink)
     */
    public interface SampleSink : Closeable
    {
        /**
         * Free all resources blocked and interrupts the process of
         * writing the output. An implementation should flush all samples
         * that have not yet been written and write the file footer -
         * if exists - before actually freeing the resources.
         *
         * @throws IOException if closing fails
         */
        //void close();

        /**
         * Adds a samples to the SampleSink. This might or might not cause writing the sample any output stream or channel.
         * Once this method is called the <code>StreamingTrack</code> must be ready and accept calls to any method.
         *
         * @throws IOException if writing (or reading) fails.
         */
        void acceptSample(StreamingSample streamingSample, StreamingTrack streamingTrack);
    }
}
