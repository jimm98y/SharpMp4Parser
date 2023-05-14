using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part12;
using SharpMp4Parser.IsoParser.Boxes.SampleEntry;
using SharpMp4Parser.Java;
using System;
using System.Collections.Generic;
using System.IO;
using static SharpMp4Parser.Muxer.Container.MP4.DefaultMp4SampleList;

namespace SharpMp4Parser.Muxer.Tracks
{
    /**
     * Bundles common functionality and parsing patterns of NAL based formats such as H264(AVC) and H265 (HEVC).
     */
    public abstract class AbstractH26XTrack : AbstractTrack
    {

        public static int BUFFER = 65535 << 10;
        protected long[] decodingTimes;
        protected List<CompositionTimeToSample.Entry> ctts = new List<CompositionTimeToSample.Entry>();
        protected List<SampleDependencyTypeBox.Entry> sdtp = new List<SampleDependencyTypeBox.Entry>();
        protected List<int> stss = new List<int>();
        protected TrackMetaData trackMetaData = new TrackMetaData();
        bool tripleZeroIsEndOfSequence = true;
        private DataSource dataSource;

        public AbstractH26XTrack(DataSource dataSource, bool tripleZeroIsEndOfSequence) : base(dataSource.ToString())
        {
            this.dataSource = dataSource;
            this.tripleZeroIsEndOfSequence = tripleZeroIsEndOfSequence;
        }

        public AbstractH26XTrack(DataSource dataSource) : this(dataSource, true)
        { }

        protected static ByteStream cleanBuffer(ByteStream input)
        {
            return new CleanByteStreamBase(input);
        }

        protected static byte[] toArray(ByteBuffer buf)
        {
            buf = (ByteBuffer)buf.duplicate();
            byte[] b = new byte[buf.remaining()];
            buf.get(b, 0, b.Length);
            return b;
        }

        public override TrackMetaData getTrackMetaData()
        {
            return trackMetaData;
        }

        protected ByteBuffer findNextNal(LookAhead la)
        {
            try
            {
                while (!la.nextThreeEquals001())
                {
                    la.discardByte();
                }
                la.discardNext3AndMarkStart();

                while (!la.nextThreeEquals000or001orEof(tripleZeroIsEndOfSequence))
                {
                    la.discardByte();
                }
                return la.getNal();
            }
            catch (Exception)
            {
                return null;
            }
        }

        abstract protected SampleEntry getCurrentSampleEntry();

        /**
         * Builds an MP4 sample from a list of NALs. Each NAL will be preceded by its
         * 4 byte (unit32) length.
         *
         * @param nals a list of NALs that form the sample
         * @return sample as it appears in the MP4 file
         */
        protected virtual Sample createSampleObject(List<ByteBuffer> nals)
        {
            byte[] sizeInfo = new byte[nals.Count * 4];
            ByteBuffer sizeBuf = ByteBuffer.wrap(sizeInfo);
            foreach (ByteBuffer b in nals)
            {
                sizeBuf.putInt(b.remaining());
            }

            ByteBuffer[] data = new ByteBuffer[nals.Count * 2];

            for (int i = 0; i < nals.Count; i++)
            {
                data[2 * i] = ByteBuffer.wrap(sizeInfo, i * 4, 4);
                data[2 * i + 1] = nals[i];
            }

            return new SampleImpl(data, getCurrentSampleEntry());
        }

        public override long[] getSampleDurations()
        {
            return decodingTimes;
        }

        public override List<CompositionTimeToSample.Entry> getCompositionTimeEntries()
        {
            return ctts;
        }

        public override long[] getSyncSamples()
        {
            long[] returns = new long[stss.Count];
            for (int i = 0; i < stss.Count; i++)
            {
                returns[i] = stss[i];
            }
            return returns;
        }

        public override List<SampleDependencyTypeBox.Entry> getSampleDependencies()
        {
            return sdtp;
        }

        public override void close()
        {
            dataSource.close();
        }

        public sealed class LookAhead
        {
            long bufferStartPos = 0;
            int inBufferPos = 0;
            DataSource dataSource;
            ByteBuffer buffer;

            long start;

            public LookAhead(DataSource dataSource)
            {
                this.dataSource = dataSource;
                fillBuffer();
            }

            public void fillBuffer()
            {
                buffer = dataSource.map(bufferStartPos, Math.Min(dataSource.size() - bufferStartPos, BUFFER));
            }

            public bool nextThreeEquals001()
            {
                if (buffer.limit() - inBufferPos >= 3)
                {
                    return (buffer.get(inBufferPos) == 0 &&
                            buffer.get(inBufferPos + 1) == 0 &&
                            buffer.get(inBufferPos + 2) == 1);
                }
                if (bufferStartPos + inBufferPos + 3 >= dataSource.size())
                {
                    throw new Exception();
                }
                return false;
            }

            public bool nextThreeEquals000or001orEof(bool tripleZeroIsEndOfSequence)
            {
                if (buffer.limit() - inBufferPos >= 3)
                {
                    return ((buffer.get(inBufferPos) == 0 &&
                            buffer.get(inBufferPos + 1) == 0 &&
                            ((buffer.get(inBufferPos + 2) == 0 && tripleZeroIsEndOfSequence) || buffer.get(inBufferPos + 2) == 1)));
                }
                else
                {
                    if (bufferStartPos + inBufferPos + 3 > dataSource.size())
                    {
                        return bufferStartPos + inBufferPos == dataSource.size();
                    }
                    else
                    {
                        bufferStartPos = start;
                        inBufferPos = 0;
                        fillBuffer();
                        return nextThreeEquals000or001orEof(tripleZeroIsEndOfSequence);
                    }
                }
            }
            public void discardByte()
            {
                inBufferPos++;
            }

            public void discardNext3AndMarkStart()
            {
                inBufferPos += 3;
                start = bufferStartPos + inBufferPos;
            }

            public ByteBuffer getNal()
            {
                if (start >= bufferStartPos)
                {

                    ((Java.Buffer)buffer).position((int)(start - bufferStartPos));
                    Java.Buffer sample = buffer.slice();
                    ((Java.Buffer)sample).limit((int)(inBufferPos - (start - bufferStartPos)));
                    return (ByteBuffer)sample;
                }
                else
                {
                    throw new Exception("damn! NAL exceeds buffer");
                    // this can only happen if NAL is bigger than the buffer
                    // and that most likely cannot happen with correct inputs
                }
            }
        }
    }
}
