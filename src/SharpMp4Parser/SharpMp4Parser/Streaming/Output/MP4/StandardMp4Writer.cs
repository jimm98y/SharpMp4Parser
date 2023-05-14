using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part12;
using SharpMp4Parser.IsoParser.Tools;
using SharpMp4Parser.IsoParser;
using SharpMp4Parser.Java;
using SharpMp4Parser.Streaming.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using System.Diagnostics;
using System.Linq;

namespace SharpMp4Parser.Streaming.Output.MP4
{
    /**
     * Creates an MP4 file with ftyp, mdat+, moov order.
     * A very special property of this variant is that it written sequentially. You can start transferring the
     * data while the <code>sink</code> receives it. (in contrast to typical implementations which need random
     * access to write length fields at the beginning of the file)
     */
    public class StandardMp4Writer : DefaultBoxes, SampleSink
    {

        public static readonly object OBJ = new object();
        //private static Logger LOG = LoggerFactory.getLogger(FragmentedMp4Writer.class.getName());
        protected readonly ByteStream sink;
        protected List<StreamingTrack> source;
        protected DateTime creationTime = DateTime.UtcNow;


        protected ConcurrentDictionary<StreamingTrack, CountDownLatch> congestionControl = new ConcurrentDictionary<StreamingTrack, CountDownLatch>();
        /**
         * Contains the start time of the next segment in line that will be created.
         */
        protected ConcurrentDictionary<StreamingTrack, long> nextChunkCreateStartTime = new ConcurrentDictionary<StreamingTrack, long>();
        /**
         * Contains the start time of the next segment in line that will be written.
         */
        protected ConcurrentDictionary<StreamingTrack, long> nextChunkWriteStartTime = new ConcurrentDictionary<StreamingTrack, long>();
        /**
         * Contains the next sample's start time.
         */
        protected ConcurrentDictionary<StreamingTrack, long> nextSampleStartTime = new ConcurrentDictionary<StreamingTrack, long>();
        /**
         * Buffers the samples per track until there are enough samples to form a Segment.
         */
        protected Dictionary<StreamingTrack, List<StreamingSample>> sampleBuffers = new Dictionary<StreamingTrack, List<StreamingSample>>();
        protected Dictionary<StreamingTrack, TrackBox> trackBoxes = new Dictionary<StreamingTrack, TrackBox>();
        /**
         * Buffers segements until it's time for a segment to be written.
         */
        protected ConcurrentDictionary<StreamingTrack, Queue<ChunkContainer>> chunkBuffers = new ConcurrentDictionary<StreamingTrack, Queue<ChunkContainer>>();
        protected Dictionary<StreamingTrack, long> chunkNumbers = new Dictionary<StreamingTrack, long>();
        protected Dictionary<StreamingTrack, long> sampleNumbers = new Dictionary<StreamingTrack, long>();
        long bytesWritten = 0;
        volatile bool headerWritten = false;


        public StandardMp4Writer(List<StreamingTrack> source, ByteStream sink)
        {
            this.source = new List<StreamingTrack>(source);
            this.sink = sink;

            HashSet<long> trackIds = new HashSet<long>();
            foreach (StreamingTrack streamingTrack in source)
            {
                streamingTrack.setSampleSink(this);
                chunkNumbers.Add(streamingTrack, 1L);
                sampleNumbers.Add(streamingTrack, 1L);
                nextSampleStartTime.TryAdd(streamingTrack, 0L);
                nextChunkCreateStartTime.TryAdd(streamingTrack, 0L);
                nextChunkWriteStartTime.TryAdd(streamingTrack, 0L);
                congestionControl.TryAdd(streamingTrack, new CountDownLatch(0));
                sampleBuffers.Add(streamingTrack, new List<StreamingSample>());
                chunkBuffers.TryAdd(streamingTrack, new Queue<ChunkContainer>());
                if (streamingTrack.getTrackExtension< TrackIdTrackExtension>(typeof(TrackIdTrackExtension)) != null)
                {
                    TrackIdTrackExtension trackIdTrackExtension = streamingTrack.getTrackExtension< TrackIdTrackExtension>(typeof(TrackIdTrackExtension));
                    Debug.Assert(trackIdTrackExtension != null);
                    if (trackIds.Contains(trackIdTrackExtension.getTrackId()))
                    {
                        throw new Exception("There may not be two tracks with the same trackID within one file");
                    }
                }

            }
            foreach (StreamingTrack streamingTrack in source)
            {
                if (streamingTrack.getTrackExtension< TrackIdTrackExtension>(typeof(TrackIdTrackExtension)) == null)
                {
                    long maxTrackId = 0;
                    foreach (long trackId in trackIds)
                    {
                        maxTrackId = Math.Max(trackId, maxTrackId);
                    }
                    TrackIdTrackExtension tiExt = new TrackIdTrackExtension(maxTrackId + 1);
                    trackIds.Add(tiExt.getTrackId());
                    streamingTrack.addTrackExtension(tiExt);
                }
            }
        }

        public void close()
        {
            foreach (StreamingTrack streamingTrack in source)
            {
                writeChunkContainer(createChunkContainer(streamingTrack));
                streamingTrack.close();
            }
            write(sink, createMoov());
        }

        protected Box createMoov()
        {
            MovieBox movieBox = new MovieBox();

            movieBox.addBox(createMvhd());

            foreach (StreamingTrack streamingTrack in source)
            {
                movieBox.addBox(trackBoxes[streamingTrack]);
            }

            // metadata here
            return movieBox;
        }

        class TrackComparator : IComparer<StreamingTrack>
        {
            private ConcurrentDictionary<StreamingTrack, long> nextChunkWriteStartTime;

            public TrackComparator(ConcurrentDictionary<StreamingTrack, long> nextChunkWriteStartTime)
            {
                this.nextChunkWriteStartTime = nextChunkWriteStartTime;
            }

            public int Compare(StreamingTrack o1, StreamingTrack o2)
            {
                // compare times and account for timestamps!
                long a = nextChunkWriteStartTime[o1] * o2.getTimescale();
                long b = nextChunkWriteStartTime[o2] * o1.getTimescale();
                double d = Math.Sign(a - b);
                return (int)d;
            }
        }

        private void sortTracks()
        {
            source = source.OrderBy(x => x, new TrackComparator(nextChunkWriteStartTime)).ToList();
        }

        protected override Box createMvhd()
        {
            MovieHeaderBox mvhd = new MovieHeaderBox();
            mvhd.setVersion(1);
            mvhd.setCreationTime(creationTime);
            mvhd.setModificationTime(creationTime);


            long[] timescales = new long[0];
            long maxTrackId = 0;
            double duration = 0;
            foreach (StreamingTrack streamingTrack in source)
            {
                duration = Math.Max((double)nextSampleStartTime[streamingTrack] / streamingTrack.getTimescale(), duration);
                timescales = Mp4Arrays.copyOfAndAppend(timescales, streamingTrack.getTimescale());
                maxTrackId = Math.Max(streamingTrack.getTrackExtension<TrackIdTrackExtension>(typeof(TrackIdTrackExtension)).getTrackId(), maxTrackId);
            }


            mvhd.setTimescale(Mp4Math.lcm(timescales));
            mvhd.setDuration((long)(Mp4Math.lcm(timescales) * duration));
            // find the next available trackId
            mvhd.setNextTrackId(maxTrackId + 1);
            return mvhd;
        }

        protected void write(ByteStream output, params Box[] boxes)
        {
            foreach (Box box1 in boxes)
            {
                box1.getBox(output);
                bytesWritten += box1.getSize();
            }
        }

        /**
         * Tests if the currently received samples for a given track
         * are already a 'chunk' as we want to have it. The next
         * sample will not be part of the chunk
         * will be added to the fragment buffer later.
         *
         * @param streamingTrack track to test
         * @param next           the lastest samples
         * @return true if a chunk is to b e created.
         */
        protected bool isChunkReady(StreamingTrack streamingTrack, StreamingSample next)
        {
            long ts = nextSampleStartTime[streamingTrack];
            long cfst = nextChunkCreateStartTime[streamingTrack];

            return (ts >= cfst + 2 * streamingTrack.getTimescale());
            // chunk interleave of 2 seconds
        }

        protected void writeChunkContainer(ChunkContainer chunkContainer)
        {
            TrackBox tb = trackBoxes[chunkContainer.streamingTrack];
            ChunkOffsetBox stco = Path.getPath<ChunkOffsetBox>(tb, "mdia[0]/minf[0]/stbl[0]/stco[0]");
            Debug.Assert(stco != null);
            stco.setChunkOffsets(Mp4Arrays.copyOfAndAppend(stco.getChunkOffsets(), bytesWritten + 8));
            write(sink, chunkContainer.mdat);
        }

        public void acceptSample(StreamingSample streamingSample, StreamingTrack streamingTrack)
        {

            TrackBox tb = trackBoxes[streamingTrack];
            if (tb == null)
            {
                tb = new TrackBox();
                tb.addBox(createTkhd(streamingTrack));
                tb.addBox(createMdia(streamingTrack));
                trackBoxes.Add(streamingTrack, tb);
            }

            // We might want to do that when the chunk is created to save memory copy


            lock (OBJ)
            {
                // need to synchronized here - I don't want two headers written under any circumstances
                if (!headerWritten)
                {
                    bool allTracksAtLeastOneSample = true;
                    foreach (StreamingTrack track in source)
                    {
                        allTracksAtLeastOneSample &= (nextSampleStartTime[track] > 0 || track == streamingTrack);
                    }
                    if (allTracksAtLeastOneSample)
                    {
                        write(sink, createFtyp());
                        headerWritten = true;
                    }
                }
            }

            try
            {
                CountDownLatch cdl = congestionControl[streamingTrack];
                if (cdl.getCount() > 0)
                {
                    cdl.await();
                }
            }
            catch (Exception)
            {
                // don't care just move on
            }

            if (isChunkReady(streamingTrack, streamingSample))
            {

                ChunkContainer chunkContainer = createChunkContainer(streamingTrack);
                //System.err.println("Creating fragment for " + streamingTrack);
                sampleBuffers[streamingTrack].Clear();
                nextChunkCreateStartTime.TryAdd(streamingTrack, nextChunkCreateStartTime[streamingTrack] + chunkContainer.duration);
                Queue<ChunkContainer> chunkQueue = chunkBuffers[streamingTrack];
                chunkQueue.Enqueue(chunkContainer);
                lock (OBJ)
                {
                    if (headerWritten && this.source[0] == streamingTrack)
                    {

                        Queue<ChunkContainer> tracksFragmentQueue;
                        StreamingTrack currentStreamingTrack;
                        // This will write AT LEAST the currently created fragment and possibly a few more
                        while ((tracksFragmentQueue = chunkBuffers[(currentStreamingTrack = this.source[0])]).Count != 0)
                        {
                            ChunkContainer currentFragmentContainer = tracksFragmentQueue.Dequeue();
                            writeChunkContainer(currentFragmentContainer);
                            congestionControl[currentStreamingTrack].countDown();
                            long ts = nextChunkWriteStartTime[currentStreamingTrack] + currentFragmentContainer.duration;
                            nextChunkWriteStartTime.TryAdd(currentStreamingTrack, ts);
                            //if (LOG.isTraceEnabled())
                            //{
                            //    LOG.trace(currentStreamingTrack + " advanced to " + (double)ts / currentStreamingTrack.getTimescale());
                            //}
                            sortTracks();
                        }
                    }
                    else
                    {
                        if (chunkQueue.Count > 10)
                        {
                            // if there are more than 10 fragments in the queue we don't want more samples of this track
                            // System.err.println("Stopping " + streamingTrack);
                            congestionControl.TryAdd(streamingTrack, new CountDownLatch(chunkQueue.Count));
                        }
                    }
                }


            }


            sampleBuffers[streamingTrack].Add(streamingSample);
            nextSampleStartTime.TryAdd(streamingTrack, nextSampleStartTime[streamingTrack] + streamingSample.getDuration());

        }

        private ChunkContainer createChunkContainer(StreamingTrack streamingTrack)
        {
            List<StreamingSample> samples = sampleBuffers[streamingTrack];
            long chunkNumber = chunkNumbers[streamingTrack];
            chunkNumbers.Add(streamingTrack, chunkNumber + 1);
            ChunkContainer cc = new ChunkContainer();
            cc.streamingTrack = streamingTrack;
            cc.mdat = new Mdat(samples);
            cc.duration = nextSampleStartTime[streamingTrack] - nextChunkCreateStartTime[streamingTrack];
            TrackBox tb = trackBoxes[streamingTrack];
            SampleTableBox stbl = Path.getPath<SampleTableBox>(tb, "mdia[0]/minf[0]/stbl[0]");
            Debug.Assert(stbl != null);
            SampleToChunkBox stsc = Path.getPath<SampleToChunkBox>(stbl, "stsc[0]");
            Debug.Assert(stsc != null);
            if (stsc.getEntries().Count == 0)
            {
                List<SampleToChunkBox.Entry> entries = new List<SampleToChunkBox.Entry>();
                stsc.setEntries(entries);
                entries.Add(new SampleToChunkBox.Entry(chunkNumber, samples.Count, 1));
            }
            else
            {
                SampleToChunkBox.Entry e = stsc.getEntries()[stsc.getEntries().Count - 1];
                if (e.getSamplesPerChunk() != samples.Count)
                {
                    stsc.getEntries().Add(new SampleToChunkBox.Entry(chunkNumber, samples.Count, 1));
                }
            }
            long sampleNumber = sampleNumbers[streamingTrack];

            SampleSizeBox stsz = Path.getPath<SampleSizeBox>(stbl, "stsz[0]");
            TimeToSampleBox stts = Path.getPath<TimeToSampleBox>(stbl, "stts[0]");
            SyncSampleBox stss = Path.getPath<SyncSampleBox>(stbl, "stss[0]");
            CompositionTimeToSample ctts = Path.getPath<CompositionTimeToSample>(stbl, "ctts[0]");
            if (streamingTrack.getTrackExtension<CompositionTimeTrackExtension>(typeof(CompositionTimeTrackExtension)) != null)
            {
                if (ctts == null)
                {
                    ctts = new CompositionTimeToSample();
                    ctts.setEntries(new List<CompositionTimeToSample.Entry>());

                    List<Box> bs = new List<Box>(stbl.getBoxes());
                    bs.Insert(bs.IndexOf(stts), ctts);
                }
            }

            long[] sampleSizes = new long[samples.Count];
            int i = 0;
            foreach (StreamingSample sample in samples)
            {
                sampleSizes[i++] = sample.getContent().limit();

                if (ctts != null)
                {
                    ctts.getEntries().Add(
                            new CompositionTimeToSample.Entry(1, CastUtils.l2i(sample.getSampleExtension< CompositionTimeSampleExtension>(typeof(CompositionTimeSampleExtension)).getCompositionTimeOffset())));
                }

                Debug.Assert(stts != null);
                if (stts.getEntries().Count == 0)
                {
                    List<TimeToSampleBox.Entry> entries = new List<TimeToSampleBox.Entry>(stts.getEntries());
                    entries.Add(new TimeToSampleBox.Entry(1, sample.getDuration()));
                    stts.setEntries(entries);
                }
                else
                {
                    TimeToSampleBox.Entry sttsEntry = stts.getEntries()[stts.getEntries().Count - 1];
                    if (sttsEntry.getDelta() == sample.getDuration())
                    {
                        sttsEntry.setCount(sttsEntry.getCount() + 1);
                    }
                    else
                    {
                        stts.getEntries().Add(new TimeToSampleBox.Entry(1, sample.getDuration()));
                    }
                }
                SampleFlagsSampleExtension sampleFlagsSampleExtension = sample.getSampleExtension<SampleFlagsSampleExtension>(typeof(SampleFlagsSampleExtension));
                if (sampleFlagsSampleExtension != null && sampleFlagsSampleExtension.isSyncSample())
                {
                    if (stss == null)
                    {
                        stss = new SyncSampleBox();
                        stbl.addBox(stss);
                    }
                    stss.setSampleNumber(Mp4Arrays.copyOfAndAppend(stss.getSampleNumber(), sampleNumber));
                }
                sampleNumber++;

            }
            Debug.Assert(stsz != null);
            stsz.setSampleSizes(Mp4Arrays.copyOfAndAppend(stsz.getSampleSizes(), sampleSizes));

            sampleNumbers.Add(streamingTrack, sampleNumber);
            samples.Clear();
            //LOG.debug("CC created. mdat size: " + cc.mdat.size);
            return cc;
        }

        protected override Box createMdhd(StreamingTrack streamingTrack)
        {
            MediaHeaderBox mdhd = new MediaHeaderBox();
            mdhd.setCreationTime(creationTime);
            mdhd.setModificationTime(creationTime);
            mdhd.setDuration(nextSampleStartTime[streamingTrack]);
            mdhd.setTimescale(streamingTrack.getTimescale());
            mdhd.setLanguage(streamingTrack.getLanguage());
            return mdhd;
        }

        public class Mdat : Box
        {
            List<StreamingSample> samples;
            long size;

            public Mdat(List<StreamingSample> samples)
            {
                this.samples = new List<StreamingSample>(samples);
                size = 8;
                foreach (StreamingSample sample in samples)
                {
                    size += sample.getContent().limit();
                }
            }

            public string getType()
            {
                return "mdat";
            }

            public long getSize()
            {
                return size;
            }

            public void getBox(ByteStream writableByteChannel)
            {
                writableByteChannel.write(ByteBuffer.wrap(new byte[]{
                    (byte) ((size & 0xff000000) >> 24),
                    (byte) ((size & 0xff0000) >> 16),
                    (byte) ((size & 0xff00) >> 8),
                    (byte) ((size & 0xff)),
                    109, 100, 97, 116, // mdat

                }));
                foreach (StreamingSample sample in samples)
                {
                    writableByteChannel.write((ByteBuffer)sample.getContent().rewind());
                }

            }
        }

        public class ChunkContainer
        {
            public Mdat mdat;
            public StreamingTrack streamingTrack;
            public long duration;
        }
    }
}
