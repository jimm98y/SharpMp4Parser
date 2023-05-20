using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part12;
using SharpMp4Parser.IsoParser.Tools;
using SharpMp4Parser.IsoParser;
using SharpMp4Parser.Java;
using SharpMp4Parser.Streaming.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;

namespace SharpMp4Parser.Streaming.Output.MP4
{
    /**
     * Creates a fragmented MP4 file consisting of a header [ftyp, moov], any number of fragments
     * [moof, mdat]+ and a footer [mfra].
     * The MultiTrackFragmentedMp4Writer is a passive component. It will only be active if one of the
     * source tracks pushes a sample via {@link #acceptSample(StreamingSample, StreamingTrack)}.
     * It has to be closed ({@link #close()}) actively to trigger the write of remaining buffered
     * samples and the footer.
     */
    public class FragmentedMp4Writer : DefaultBoxes, SampleSink
    {
        //private static Logger LOG = LoggerFactory.getLogger(FragmentedMp4Writer.class.getName());
        protected readonly ByteStream sink;
        protected List<StreamingTrack> source;
        protected DateTime creationTime;
        protected long sequenceNumber = 1;
        /**
         * Contains the start time of the next segment in line that will be created.
         */
        protected Dictionary<StreamingTrack, long> nextFragmentCreateStartTime = new Dictionary<StreamingTrack, long>();
        /**
         * Contains the start time of the next segment in line that will be written.
         */
        protected Dictionary<StreamingTrack, long> nextFragmentWriteStartTime = new Dictionary<StreamingTrack, long>();
        /**
         * Contains the next sample's start time.
         */
        protected Dictionary<StreamingTrack, long> nextSampleStartTime = new Dictionary<StreamingTrack, long>();
        /**
         * Buffers the samples per track until there are enough samples to form a Segment.
         */
        protected Dictionary<StreamingTrack, List<StreamingSample>> sampleBuffers = new Dictionary<StreamingTrack, List<StreamingSample>>();
        /**
         * Buffers segements until it's time for a segment to be written.
         */
        protected Dictionary<StreamingTrack, Queue<FragmentContainer>> fragmentBuffers = new Dictionary<StreamingTrack, Queue<FragmentContainer>>();
        protected Dictionary<StreamingTrack, long[]> tfraOffsets = new Dictionary<StreamingTrack, long[]>();
        protected Dictionary<StreamingTrack, long[]> tfraTimes = new Dictionary<StreamingTrack, long[]>();
        long bytesWritten = 0;
        volatile bool headerWritten = false;

        public FragmentedMp4Writer(List<StreamingTrack> source, ByteStream sink)
        {
            this.source = new List<StreamingTrack>(source);
            this.sink = sink;
            this.creationTime = DateTime.UtcNow;
            List<long> trackIds = new List<long>();

            foreach (StreamingTrack streamingTrack in source)
            {
                // this connects sample source with sample sink
                streamingTrack.setSampleSink(this);
                sampleBuffers[streamingTrack] = new List<StreamingSample>();
                fragmentBuffers[streamingTrack] = new Queue<FragmentContainer>();
                nextFragmentCreateStartTime[streamingTrack] = 0L;
                nextFragmentWriteStartTime[streamingTrack] = 0L;
                nextSampleStartTime[streamingTrack] = 0L;

                if (streamingTrack.getTrackExtension<TrackIdTrackExtension>(typeof(TrackIdTrackExtension)) != null)
                {
                    TrackIdTrackExtension trackIdTrackExtension = streamingTrack.getTrackExtension<TrackIdTrackExtension>(typeof(TrackIdTrackExtension));
                    Debug.Assert(trackIdTrackExtension != null);
                    if (trackIds.Contains(trackIdTrackExtension.getTrackId()))
                    {
                        throw new Exception("There may not be two tracks with the same trackID within one file");
                    }
                }
            }

            foreach (StreamingTrack streamingTrack in source)
            {
                if (streamingTrack.getTrackExtension<TrackIdTrackExtension>(typeof(TrackIdTrackExtension)) == null)
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

        /**
         * Writes the remaining samples to file (even though the typical condition for wrapping up
         * a segment have not yet been met) and writes the MovieFragmentRandomAccessBox.
         * It does not close the sink!
         *
         * @throws IOException if writing to the underlying data sink fails
         * @see MovieFragmentRandomAccessBox
         */
        public void close()
        {
            foreach (StreamingTrack streamingTrack in source)
            {
                writeFragment(createFragment(streamingTrack, sampleBuffers[streamingTrack]));
                streamingTrack.close();
            }
            writeFooter(createFooter());
        }

        protected void write(ByteStream output, params Box[] boxes)
        {
            foreach (Box box1 in boxes)
            {
                box1.getBox(output);
                bytesWritten += box1.getSize();
            }
        }

        protected override Box createMdhd(StreamingTrack streamingTrack)
        {
            MediaHeaderBox mdhd = new MediaHeaderBox();
            mdhd.setCreationTime(creationTime);
            mdhd.setModificationTime(creationTime);
            mdhd.setDuration(0);//no duration in moov for fragmented movies
            mdhd.setTimescale(streamingTrack.getTimescale());
            mdhd.setLanguage(streamingTrack.getLanguage());
            return mdhd;
        }


        protected Box createMvex()
        {
            MovieExtendsBox mvex = new MovieExtendsBox();
            MovieExtendsHeaderBox mved = new MovieExtendsHeaderBox();
            mved.setVersion(1);

            mved.setFragmentDuration(0);

            mvex.addBox(mved);
            foreach (StreamingTrack streamingTrack in source)
            {
                mvex.addBox(createTrex(streamingTrack));
            }
            return mvex;
        }

        protected Box createTrex(StreamingTrack streamingTrack)
        {
            TrackExtendsBox trex = new TrackExtendsBox();
            trex.setTrackId(streamingTrack.getTrackExtension< TrackIdTrackExtension>(typeof(TrackIdTrackExtension)).getTrackId());
            trex.setDefaultSampleDescriptionIndex(1);
            trex.setDefaultSampleDuration(0);
            trex.setDefaultSampleSize(0);
            SampleFlags sf = new SampleFlags();

            trex.setDefaultSampleFlags(sf);
            return trex;
        }

        protected override Box createMvhd()
        {
            MovieHeaderBox mvhd = new MovieHeaderBox();
            mvhd.setVersion(1);
            mvhd.setCreationTime(creationTime);
            mvhd.setModificationTime(creationTime);
            mvhd.setDuration(0);//no duration in moov for fragmented movies

            long[] timescales = new long[0];
            long maxTrackId = 0;
            foreach (StreamingTrack streamingTrack in source)
            {
                timescales = Mp4Arrays.copyOfAndAppend(timescales, streamingTrack.getTimescale());
                maxTrackId = Math.Max(streamingTrack.getTrackExtension< TrackIdTrackExtension>(typeof(TrackIdTrackExtension)).getTrackId(), maxTrackId);
            }

            mvhd.setTimescale(Mp4Math.lcm(timescales));
            // find the next available trackId
            mvhd.setNextTrackId(maxTrackId + 1);
            return mvhd;
        }


        protected Box createMoov()
        {
            MovieBox movieBox = new MovieBox();

            movieBox.addBox(createMvhd());

            foreach (StreamingTrack streamingTrack in source)
            {
                movieBox.addBox(createTrak(streamingTrack));
            }
            movieBox.addBox(createMvex());

            // metadata here
            return movieBox;
        }

        protected Box[] createHeader()
        {
            return new Box[] { createFtyp(), createMoov() };
        }

        class TracksComparer : IComparer<StreamingTrack>
        {
            private Dictionary<StreamingTrack, long> nextFragmentWriteStartTime;

            public TracksComparer(Dictionary<StreamingTrack, long> nextFragmentWriteStartTime)
            {
                this.nextFragmentWriteStartTime = nextFragmentWriteStartTime;
            }

            public int Compare(StreamingTrack o1, StreamingTrack o2)
            {
                // compare times and account for timestamps!
                long a = nextFragmentWriteStartTime[o1] * o2.getTimescale();
                long b = nextFragmentWriteStartTime[o2] * o1.getTimescale();
                double d = Math.Sign(a - b);
                return (int)d;
            }
        }

        private void sortTracks()
        {
            source = source.OrderBy(x => x, new TracksComparer(nextFragmentWriteStartTime)).ToList();
        }

        public void acceptSample(StreamingSample streamingSample, StreamingTrack streamingTrack)
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

                    writeHeader(createHeader());
                    headerWritten = true;
                }
            }

            if (isFragmentReady(streamingTrack, streamingSample))
            {

                FragmentContainer fragmentContainer = createFragmentContainer(streamingTrack);
                //System.err.println("Creating fragment for " + streamingTrack);
                sampleBuffers[streamingTrack].Clear();
                nextFragmentCreateStartTime[streamingTrack] = nextFragmentCreateStartTime[streamingTrack] + fragmentContainer.duration;
                Queue<FragmentContainer> fragmentQueue = fragmentBuffers[streamingTrack];
                fragmentQueue.Enqueue(fragmentContainer);

                if (headerWritten && this.source[0] == streamingTrack)
                {

                    Queue<FragmentContainer> tracksFragmentQueue;
                    StreamingTrack currentStreamingTrack;
                    // This will write AT LEAST the currently created fragment and possibly a few more
                    while ((tracksFragmentQueue = fragmentBuffers[(currentStreamingTrack = this.source[0])]).Count > 0)
                    {

                        FragmentContainer currentFragmentContainer = tracksFragmentQueue.Dequeue();

                        writeFragment(currentFragmentContainer.fragmentContent);

                        long ts = nextFragmentWriteStartTime[currentStreamingTrack] + currentFragmentContainer.duration;
                        nextFragmentWriteStartTime[currentStreamingTrack] = ts;
                        //if (LOG.isDebugEnabled())
                        //{
                        //LOG.debug(currentStreamingTrack + " advanced to " + (double)ts / currentStreamingTrack.getTimescale());
                    }
                    //sortTracks();
                }
            }

            sampleBuffers[streamingTrack].Add(streamingSample);
            nextSampleStartTime[streamingTrack] = nextSampleStartTime[streamingTrack] + streamingSample.getDuration();
        }

        /**
         * Tests if the currently received samples for a given track
         * form a valid fragment taking the latest received sample into
         * account. The next sample is not part of the segment and
         * will be added to the fragment buffer later.
         *
         * @param streamingTrack track to test
         * @param next           the lastest samples
         * @return true if a fragment has been created.
         */
        protected bool isFragmentReady(StreamingTrack streamingTrack, StreamingSample next)
        {
            long ts = nextSampleStartTime[streamingTrack];
            long cfst = nextFragmentCreateStartTime[streamingTrack];

            if ((ts > cfst + 3 * streamingTrack.getTimescale()))
            {
                // mininum fragment length == 3 seconds
                SampleFlagsSampleExtension sfExt = next.getSampleExtension< SampleFlagsSampleExtension>(typeof(SampleFlagsSampleExtension));
                if (sfExt == null || sfExt.isSyncSample())
                {
                    //System.err.println(streamingTrack + " ready at " + ts);
                    // the next sample needs to be a sync sample
                    // when there is no SampleFlagsSampleExtension we assume syncSample == true
                    return true;
                }
            }
            return false;
        }

        protected Box[] createFragment(StreamingTrack streamingTrack, List<StreamingSample> samples)
        {
            //nextFragmentCreateStartTime[streamingTrack];
            tfraOffsets.TryGetValue(streamingTrack, out var origTrack);
            tfraTimes.TryGetValue(streamingTrack, out var origTimes);
            nextFragmentCreateStartTime.TryGetValue(streamingTrack, out var nextFragment);
            tfraOffsets[streamingTrack] = Mp4Arrays.copyOfAndAppend(origTrack, bytesWritten);
            tfraTimes[streamingTrack] = Mp4Arrays.copyOfAndAppend(origTimes, nextFragment);

            //LOG.trace("Container created");
            Box moof = createMoof(streamingTrack, samples);
            //LOG.trace("moof created");
            Box mdat = createMdat(samples);
            //LOG.trace("mdat created");

            //if (LOG.isDebugEnabled())
            //{
            //    double duration = nextSampleStartTime.get(streamingTrack) - nextFragmentCreateStartTime.get(streamingTrack);
            //    LOG.debug("created fragment for " + streamingTrack + " of " + (duration / streamingTrack.getTimescale()) + " seconds");
            //}
            return new Box[] { moof, mdat };
        }

        private FragmentContainer createFragmentContainer(StreamingTrack streamingTrack)
        {
            FragmentContainer fragmentContainer = new FragmentContainer();
            List<StreamingSample> samples = new List<StreamingSample>(sampleBuffers[streamingTrack]);
            fragmentContainer.fragmentContent = createFragment(streamingTrack, samples);
            fragmentContainer.duration = nextSampleStartTime[streamingTrack] - nextFragmentCreateStartTime[streamingTrack];
            return fragmentContainer;
        }


        /**
         * Writes the given boxes. It's called as soon as the MultiTrackFragmentedMp4Writer
         * received a sample from each source as this is the first point in time where the
         * MultiTrackFragmentedMp4Writer can be sure that all config data is available from
         * the sources.
         * It typically writes a ftyp/moov pair but will write what ever
         * the boxes argument contains
         *
         * @param boxes any number of boxes that form the header
         * @throws IOException when writing to the sink fails.
         * @see FileTypeBox
         * @see ProgressiveDownloadInformationBox
         * @see MovieBox
         * @see SegmentIndexBox
         */
        protected void writeHeader(params Box[] boxes)
        {
            write(sink, boxes);
        }

        /**
         * Writes the given boxes. It's called as soon as a fragment is created.
         * It typically write a single moof/mdat pair but will write what ever
         * the boxes argument contains
         *
         * @param boxes any number of boxes that form fragment
         * @throws IOException when writing to the sink fails.
         * @see MovieFragmentBox
         * @see MediaDataBox
         * @see SegmentTypeBox
         * @see SegmentIndexBox
         */
        protected void writeFragment(params Box[] boxes)
        {
            write(sink, boxes);
        }

        /**
         * Writes the given boxes. It's called as last write operation. Typically the only
         * box written is the MovieFragmentRandomAccessBox.
         *
         * @param boxes any number of boxes to conclude the file.
         * @throws IOException when writing to the sink fails.
         * @see MovieFragmentRandomAccessBox
         */
        protected void writeFooter(params Box[] boxes)
        {
            write(sink, boxes);
        }

        private Box createMoof(StreamingTrack streamingTrack, List<StreamingSample> samples)
        {
            MovieFragmentBox moof = new MovieFragmentBox();
            createMfhd(sequenceNumber, moof);
            createTraf(streamingTrack, moof, samples);
            TrackRunBox firstTrun = moof.getTrackRunBoxes()[0];
            firstTrun.setDataOffset(1); // dummy to make size correct
            firstTrun.setDataOffset((int)(8 + moof.getSize())); // mdat header + moof size
            return moof;
        }

        protected void createTfhd(StreamingTrack streamingTrack, TrackFragmentBox parent)
        {
            TrackFragmentHeaderBox tfhd = new TrackFragmentHeaderBox();
            SampleFlags sf = new SampleFlags();
            DefaultSampleFlagsTrackExtension defaultSampleFlagsTrackExtension = streamingTrack.getTrackExtension< DefaultSampleFlagsTrackExtension>(typeof(DefaultSampleFlagsTrackExtension));
            // I don't like the idea of using sampleflags in trex as it breaks the "self-contained" property of a fragment
            if (defaultSampleFlagsTrackExtension != null)
            {
                sf.setIsLeading(defaultSampleFlagsTrackExtension.getIsLeading());
                sf.setSampleIsDependedOn(defaultSampleFlagsTrackExtension.getSampleIsDependedOn());
                sf.setSampleDependsOn(defaultSampleFlagsTrackExtension.getSampleDependsOn());
                sf.setSampleHasRedundancy(defaultSampleFlagsTrackExtension.getSampleHasRedundancy());
                sf.setSampleIsDifferenceSample(defaultSampleFlagsTrackExtension.isSampleIsNonSyncSample());
                sf.setSamplePaddingValue(defaultSampleFlagsTrackExtension.getSamplePaddingValue());
                sf.setSampleDegradationPriority(defaultSampleFlagsTrackExtension.getSampleDegradationPriority());

            }
            tfhd.setDefaultSampleFlags(sf);
            tfhd.setBaseDataOffset(-1);
            tfhd.setTrackId(streamingTrack.getTrackExtension< TrackIdTrackExtension>(typeof(TrackIdTrackExtension)).getTrackId());
            tfhd.setDefaultBaseIsMoof(true);
            parent.addBox(tfhd);
        }

        protected void createTfdt(StreamingTrack streamingTrack, TrackFragmentBox parent)
        {
            TrackFragmentBaseMediaDecodeTimeBox tfdt = new TrackFragmentBaseMediaDecodeTimeBox();
            tfdt.setVersion(1);
            tfdt.setBaseMediaDecodeTime(nextFragmentCreateStartTime[streamingTrack]);
            parent.addBox(tfdt);
        }

        protected void createTrun(StreamingTrack streamingTrack, TrackFragmentBox parent, List<StreamingSample> samples)
        {
            TrackRunBox trun = new TrackRunBox();
            trun.setVersion(1);
            trun.setSampleDurationPresent(true);
            trun.setSampleSizePresent(true);
            List<TrackRunBox.Entry> entries = new List<TrackRunBox.Entry>(samples.Count);


            trun.setSampleCompositionTimeOffsetPresent(streamingTrack.getTrackExtension< CompositionTimeTrackExtension>(typeof(CompositionTimeTrackExtension)) != null);

            DefaultSampleFlagsTrackExtension defaultSampleFlagsTrackExtension = streamingTrack.getTrackExtension< DefaultSampleFlagsTrackExtension>(typeof(DefaultSampleFlagsTrackExtension));
            trun.setSampleFlagsPresent(defaultSampleFlagsTrackExtension == null);

            foreach (StreamingSample streamingSample in samples)
            {
                TrackRunBox.Entry entry = new TrackRunBox.Entry();
                entry.setSampleSize(streamingSample.getContent().remaining());
                if (defaultSampleFlagsTrackExtension == null)
                {
                    SampleFlagsSampleExtension sampleFlagsSampleExtension = streamingSample.getSampleExtension< SampleFlagsSampleExtension>(typeof(SampleFlagsSampleExtension));
                    Debug.Assert(sampleFlagsSampleExtension != null, "SampleDependencySampleExtension missing even though SampleDependencyTrackExtension was present");
                    SampleFlags sflags = new SampleFlags();
                    sflags.setIsLeading(sampleFlagsSampleExtension.getIsLeading());
                    sflags.setSampleIsDependedOn(sampleFlagsSampleExtension.getSampleIsDependedOn());
                    sflags.setSampleDependsOn(sampleFlagsSampleExtension.getSampleDependsOn());
                    sflags.setSampleHasRedundancy(sampleFlagsSampleExtension.getSampleHasRedundancy());
                    sflags.setSampleIsDifferenceSample(sampleFlagsSampleExtension.isSampleIsNonSyncSample());
                    sflags.setSamplePaddingValue(sampleFlagsSampleExtension.getSamplePaddingValue());
                    sflags.setSampleDegradationPriority(sampleFlagsSampleExtension.getSampleDegradationPriority());

                    entry.setSampleFlags(sflags);

                }

                entry.setSampleDuration(streamingSample.getDuration());

                if (trun.isSampleCompositionTimeOffsetPresent())
                {
                    CompositionTimeSampleExtension compositionTimeSampleExtension = streamingSample.getSampleExtension< CompositionTimeSampleExtension>(typeof(CompositionTimeSampleExtension));
                    Debug.Assert(compositionTimeSampleExtension != null, "CompositionTimeSampleExtension missing even though CompositionTimeTrackExtension was present");
                    entry.setSampleCompositionTimeOffset(CastUtils.l2i(compositionTimeSampleExtension.getCompositionTimeOffset()));
                }

                entries.Add(entry);
            }

            trun.setEntries(entries);

            parent.addBox(trun);
        }

        private void createTraf(StreamingTrack streamingTrack, MovieFragmentBox moof, List<StreamingSample> samples)
        {
            TrackFragmentBox traf = new TrackFragmentBox();
            moof.addBox(traf);
            createTfhd(streamingTrack, traf);
            createTfdt(streamingTrack, traf);
            createTrun(streamingTrack, traf, samples);

            if (streamingTrack.getTrackExtension< CencEncryptTrackExtension>(typeof(CencEncryptTrackExtension)) != null)
            {
                //     createSaiz(getTrackExtension(source, CencEncryptTrackExtension.class), sequenceNumber, traf);
                //     createSenc(getTrackExtension(source, CencEncryptTrackExtension.class), sequenceNumber, traf);
                //     createSaio(getTrackExtension(source, CencEncryptTrackExtension.class), sequenceNumber, traf);
            }


            /*      Map<String, List<GroupEntry>> groupEntryFamilies = new HashMap<String, List<GroupEntry>>();
                  for (Map.Entry<GroupEntry, long[]> sg : track.getSampleGroups().entrySet()) {
                      String type = sg.getKey().getType();
                      List<GroupEntry> groupEntries = groupEntryFamilies.get(type);
                      if (groupEntries == null) {
                          groupEntries = new ArrayList<GroupEntry>();
                          groupEntryFamilies.put(type, groupEntries);
                      }
                      groupEntries.add(sg.getKey());
                  }


                  for (Map.Entry<String, List<GroupEntry>> sg : groupEntryFamilies.entrySet()) {
                      SampleGroupDescriptionBox sgpd = new SampleGroupDescriptionBox();
                      String type = sg.getKey();
                      sgpd.setGroupEntries(sg.getValue());
                      SampleToGroupBox sbgp = new SampleToGroupBox();
                      sbgp.setGroupingType(type);
                      SampleToGroupBox.Entry last = null;
                      for (int i = l2i(startSample - 1); i < l2i(endSample - 1); i++) {
                          int index = 0;
                          for (int j = 0; j < sg.getValue().size(); j++) {
                              GroupEntry groupEntry = sg.getValue().get(j);
                              long[] sampleNums = track.getSampleGroups().get(groupEntry);
                              if (Arrays.binarySearch(sampleNums, i) >= 0) {
                                  index = j + 1;
                              }
                          }
                          if (last == null || last.getGroupDescriptionIndex() != index) {
                              last = new SampleToGroupBox.Entry(1, index);
                              sbgp.getEntries().add(last);
                          } else {
                              last.setSampleCount(last.getSampleCount() + 1);
                          }
                      }
                      traf.addBox(sgpd);
                      traf.addBox(sbgp);
                  }*/

        }

        protected Box[] createFooter()
        {
            MovieFragmentRandomAccessBox mfra = new MovieFragmentRandomAccessBox();

            foreach (StreamingTrack track in source)
            {
                mfra.addBox(createTfra(track));
            }

            MovieFragmentRandomAccessOffsetBox mfro = new MovieFragmentRandomAccessOffsetBox();
            mfra.addBox(mfro);
            mfro.setMfraSize(mfra.getSize());
            return new Box[] { mfra };
        }

        /**
         * Creates a 'tfra' - track fragment random access box for the given track with the isoFile.
         * The tfra contains a map of random access points with time as key and offset within the isofile
         * as value.
         *
         * @param track the concerned track
         * @return a track fragment random access box.
         */
        protected Box createTfra(StreamingTrack track)
        {
            TrackFragmentRandomAccessBox tfra = new TrackFragmentRandomAccessBox();
            tfra.setVersion(1); // use long offsets and times
            long[] offsets = tfraOffsets[track];
            long[] times = tfraTimes[track];
            List<TrackFragmentRandomAccessBox.Entry> entries = new List<TrackFragmentRandomAccessBox.Entry>(times.Length);
            for (int i = 0; i < times.Length; i++)
            {
                entries.Add(new TrackFragmentRandomAccessBox.Entry(times[i], offsets[i], 1, 1, 1));
            }


            tfra.setEntries(entries);
            tfra.setTrackId(track.getTrackExtension< TrackIdTrackExtension>(typeof(TrackIdTrackExtension)).getTrackId());
            return tfra;
        }


        private void createMfhd(long sequenceNumber, MovieFragmentBox moof)
        {
            MovieFragmentHeaderBox mfhd = new MovieFragmentHeaderBox();
            mfhd.setSequenceNumber(sequenceNumber);
            moof.addBox(mfhd);
        }

        class CustomBox : Box
        {
            private List<StreamingSample> samples;

            public CustomBox(List<StreamingSample> samples)
            {
                this.samples = samples;
            }

            public string getType()
            {
                return "mdat";
            }

            public long getSize()
            {
                long l = 8;
                foreach (StreamingSample streamingSample in samples)
                {
                    l += streamingSample.getContent().limit();
                }
                return l;
            }

            public void getBox(ByteStream writableByteChannel)
            {
                long l = 8;
                foreach (StreamingSample streamingSample in samples)
                {
                    ByteBuffer sampleContent = streamingSample.getContent();
                    l += sampleContent.limit();
                }
                ByteBuffer bb = ByteBuffer.allocate(8);
                IsoTypeWriter.writeUInt32(bb, l);
                bb.put(IsoFile.fourCCtoBytes(getType()));
                writableByteChannel.write((ByteBuffer)((Java.Buffer)bb).rewind());

                foreach (StreamingSample streamingSample in samples)
                {
                    writableByteChannel.write((ByteBuffer)((Java.Buffer)streamingSample.getContent()).rewind());
                }
            }
        }

        public class FragmentContainer
        {
            public Box[] fragmentContent;
            public long duration;
        }

        private Box createMdat(List<StreamingSample> samples)
        {
            return new CustomBox(samples);
        }
    }
}