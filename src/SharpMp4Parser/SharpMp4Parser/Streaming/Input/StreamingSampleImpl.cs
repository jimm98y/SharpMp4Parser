using SharpMp4Parser.Java;
using System;
using System.Collections.Generic;

namespace SharpMp4Parser.Streaming.Input
{
    public class StreamingSampleImpl : StreamingSample
    {

        private ByteBuffer s;
        private long duration;
        private Dictionary<Type, SampleExtension> sampleExtensions = new Dictionary<Type, SampleExtension>();

        public StreamingSampleImpl(ByteBuffer s, long duration)
        {
            this.s = (ByteBuffer)s.duplicate();
            this.duration = duration;
        }

        public StreamingSampleImpl(byte[] sample, long duration)
        {
            this.duration = duration;
            s = ByteBuffer.wrap(sample);
        }

        public StreamingSampleImpl(List<ByteBuffer> nals, long duration)
        {
            this.duration = duration;
            int size = 0;
            foreach (ByteBuffer nal in nals)
            {
                size += 4;
                size += nal.limit();
            }
            s = ByteBuffer.allocate(size);

            foreach (ByteBuffer nal in nals)
            {
                s.put((byte)((nal.limit() & 0xff000000) >> 24));
                s.put((byte)((nal.limit() & 0xff0000) >> 16));
                s.put((byte)((nal.limit() & 0xff00) >> 8));
                s.put((byte)((nal.limit() & 0xff)));
                s.put((ByteBuffer)((Java.Buffer)nal).rewind());
            }
        }

        public ByteBuffer getContent()
        {
            return s;
        }

        public long getDuration()
        {
            return duration;
        }

        public T getSampleExtension<T>(Type clazz) where T : SampleExtension
        {
            SampleExtension ret = null;
            sampleExtensions.TryGetValue(clazz, out ret);
            return (T)ret;
        }

        public void addSampleExtension(SampleExtension sampleExtension)
        {
            sampleExtensions[sampleExtension.GetType()] = sampleExtension;
        }

        public T removeSampleExtension<T>(Type clazz) where T : SampleExtension
        {
            T ret = (T)sampleExtensions[clazz];
            sampleExtensions.Remove(clazz);
            return ret;
        }
    }
}
