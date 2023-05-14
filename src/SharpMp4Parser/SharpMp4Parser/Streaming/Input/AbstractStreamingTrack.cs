using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part12;
using SharpMp4Parser.Streaming.Output;
using System;
using System.Collections.Generic;

namespace SharpMp4Parser.Streaming.Input
{
    public abstract class AbstractStreamingTrack : StreamingTrack
    {
        protected TrackHeaderBox tkhd;
        protected Dictionary<Type, TrackExtension> trackExtensions = new Dictionary<Type, TrackExtension>();

        protected SampleSink sampleSink;

        public AbstractStreamingTrack()
        {
            tkhd = new TrackHeaderBox();
            tkhd.setTrackId(1);
        }

        public void setSampleSink(SampleSink sampleSink)
        {
            this.sampleSink = sampleSink;
        }


        public T getTrackExtension<T>(Type clazz) where T : TrackExtension
        {
            TrackExtension ret = null;
            trackExtensions.TryGetValue(clazz, out ret);
            return (T)ret;
        }

        public void addTrackExtension(TrackExtension trackExtension)
        {

            trackExtensions.Add(trackExtension.GetType(), trackExtension);
        }

        public void removeTrackExtension(Type clazz)
        {
            trackExtensions.Remove(clazz);
        }

        public abstract long getTimescale();
        public abstract string getHandler();
        public abstract string getLanguage();
        public abstract SampleDescriptionBox getSampleDescriptionBox();
        public abstract void close();
    }
}
