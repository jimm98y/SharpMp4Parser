using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part12;
using SharpMp4Parser.IsoParser;
using SharpMp4Parser.Java;
using SharpMp4Parser.Streaming.Extensions;
using System;
using System.Collections.Generic;
using SharpMp4Parser.IsoParser.Tools;
using System.Diagnostics;
using System.Linq;
using SharpMp4Parser.Streaming.Output;

namespace SharpMp4Parser.Streaming.Input.MP4
{
    /**
     * Creates a List of StreamingTrack from a classic MP4. Fragmented MP4s don't
     * work and the implementation will consume a lot of heap when the MP4
     * is not a 'fast-start' MP4 (order: ftyp, moov, mdat good;
     * order ftyp, mdat, moov bad).
     */
    // @todo implement FragmentedMp4ContainerSource
    // @todo store mdat of non-fast-start MP4 on disk
    public class ClassicMp4ContainerSource /* : Callable<void> */ {
        Dictionary<TrackBox, Mp4StreamingTrack> tracks = new Dictionary<TrackBox, Mp4StreamingTrack>();
        Dictionary<TrackBox, long> currentChunks = new Dictionary<TrackBox, long>();
        readonly Dictionary<TrackBox, long> currentSamples = new Dictionary<TrackBox, long>();
        readonly DiscardingByteArrayOutputStream baos = new DiscardingByteArrayOutputStream();
        readonly ByteStream readableByteChannel;
        private readonly ByteBuffer BUFFER = ByteBuffer.allocate(65535);


        public ClassicMp4ContainerSource(ByteStream input)
        {
            readableByteChannel = Channels.newChannel(new TeeInputStream(input, baos));
            BasicContainer container = new BasicContainer();
            BoxParser boxParser = new PropertyBoxParserImpl();
            Box current = null;

            while (current == null || !"moov".Equals(current.getType())) {
                current = boxParser.parseBox(readableByteChannel, null);
                container.addBox(current);
            }
            // Either mdat was already read (yeahh sucks but what can you do if it's in the beginning)
            // or it's still coming

            foreach (TrackBox trackBox in Path.getPaths<TrackBox>(container, "moov[0]/trak"))
            {
                Mp4StreamingTrack mp4StreamingTrack = new Mp4StreamingTrack(trackBox);
                tracks.Add(trackBox, mp4StreamingTrack);
                if (trackBox.getSampleTableBox().getCompositionTimeToSample() != null)
                {
                    mp4StreamingTrack.addTrackExtension(new CompositionTimeTrackExtension());
                }
                mp4StreamingTrack.addTrackExtension(new TrackIdTrackExtension(trackBox.getTrackHeaderBox().getTrackId()));
                currentChunks.Add(trackBox, 1);
                currentSamples.Add(trackBox, 1);
            }
        }

        //    public static void main(string[] args)
        //{
        //    ClassicMp4ContainerSource classicMp4ContainerSource = null;
        //        try {
        //        classicMp4ContainerSource = new ClassicMp4ContainerSource(new URI("http://org.mp4parser.s3.amazonaws.com/examples/Cosmos%20Laundromat%20small%20faststart.mp4").toURL().openStream());
        //    } catch (Exception e) {
        //        throw;
        //    }
        //    List<StreamingTrack> streamingTracks = classicMp4ContainerSource.getTracks();
        //    File f = new File("output.mp4");
        //FragmentedMp4Writer writer = new FragmentedMp4Writer(streamingTracks, new FileOutputStream(f).getChannel());

        //System.out.println("Reading and writing started.");
        //classicMp4ContainerSource.call();
        //writer.close();
        //System.err.println(f.getAbsolutePath());

        //    }

        public List<StreamingTrack> getTracks()
        {
            return tracks.Values.Select(x => (StreamingTrack)x).ToList();
        }

        public void call()
        {
            while (true) 
            {
                TrackBox firstInLine = null;

                long currentChunk = 0;
                long currentChunkStartSample = 0;
                long offset = long.MaxValue;
                SampleToChunkBox.Entry entry = null;
                foreach (TrackBox trackBox in tracks.Keys)
                {
                    long _currentChunk = currentChunks[trackBox];
                    long _currentSample = currentSamples[trackBox];
                    long[] chunkOffsets = trackBox.getSampleTableBox().getChunkOffsetBox().getChunkOffsets();

                    if ((CastUtils.l2i(_currentChunk) - 1 < chunkOffsets.Length) && chunkOffsets[CastUtils.l2i(_currentChunk) - 1] < offset)
                    {

                        firstInLine = trackBox;
                        currentChunk = _currentChunk;
                        currentChunkStartSample = _currentSample;
                        offset = chunkOffsets[CastUtils.l2i(_currentChunk) - 1];
                    }
                }
                if (firstInLine == null)
                {
                    break;
                }

                SampleToChunkBox stsc = firstInLine.getSampleTableBox().getSampleToChunkBox();
                foreach (SampleToChunkBox.Entry _entry in stsc.getEntries())
                {
                    if (currentChunk >= _entry.getFirstChunk())
                    {
                        entry = _entry;
                    }
                    else
                    {
                        break;
                    }
                }


                Debug.Assert(entry != null);
                SampleTableBox stbl = firstInLine.getSampleTableBox();

                List<TimeToSampleBox.Entry> times = stbl.getTimeToSampleBox().getEntries();
                List<CompositionTimeToSample.Entry> compositionOffsets = stbl.getCompositionTimeToSample() != null ? stbl.getCompositionTimeToSample().getEntries() : null;

                //System.out.println(trackId + ": Pushing chunk with sample " + currentChunkStartSample + "(offset: " + offset + ") to " + (currentChunkStartSample + entry.getSamplesPerChunk()) + " in the chunk");
                SampleSizeBox stsz = stbl.getSampleSizeBox();

                for (long index = currentChunkStartSample; index < currentChunkStartSample + entry.getSamplesPerChunk(); index++)
                {
                    long duration = times[0].getDelta();
                    if (times[0].getCount() == 1)
                    {
                        times.RemoveAt(0);
                    }
                    else
                    {
                        times[0].setCount(times[0].getCount() - 1);
                    }

                    // Sample Flags Start
                    SampleDependencyTypeBox sdtp = Path.getPath<SampleDependencyTypeBox>(stbl, "sdtp");
                    SampleFlagsSampleExtension sfse = new SampleFlagsSampleExtension();
                    if (sdtp != null)
                    {
                        SampleDependencyTypeBox.Entry e = sdtp.getEntries()[CastUtils.l2i(index)];
                        sfse.setIsLeading(e.getIsLeading());
                        sfse.setSampleDependsOn(e.getSampleDependsOn());
                        sfse.setSampleIsDependedOn(e.getSampleIsDependedOn());
                        sfse.setSampleHasRedundancy(e.getSampleHasRedundancy());
                    }
                    if (stbl.getSyncSampleBox() != null)
                    {
                        if (Arrays.binarySearch(stbl.getSyncSampleBox().getSampleNumber(), index) >= 0)
                        {
                            sfse.setSampleIsNonSyncSample(false);
                        }
                        else
                        {
                            sfse.setSampleIsNonSyncSample(true);
                        }
                    }

                    DegradationPriorityBox stdp = Path.getPath<DegradationPriorityBox>(stbl, "stdp");
                    if (stdp != null)
                    {
                        sfse.setSampleDegradationPriority(stdp.getPriorities()[CastUtils.l2i(index)]);
                    }
                    // Sample Flags Done

                    int sampleSize = CastUtils.l2i(stsz.getSampleSizeAtIndex(CastUtils.l2i(index - 1)));
                    long avail = baos.available();

                    // as long as the sample has not yet been fully read
                    // read more bytes from the input channel to fill
                    //
                    while (avail <= offset + sampleSize)
                    {
                        try
                        {
                            int br = readableByteChannel.read(BUFFER);
                            if (br == -1)
                            {
                                break;
                            }
                            avail = baos.available();
                            ((Java.Buffer)BUFFER).rewind();
                        }
                        catch (Exception)
                        {
                            throw;
                        }
                    }
                    //System.err.println("Get sample content @" + offset + " len=" + sampleSize);
                    byte[] sampleContent = baos.get(offset, sampleSize);

                    StreamingSample streamingSample = new StreamingSampleImpl(sampleContent, duration);
                    streamingSample.addSampleExtension(sfse);
                    if (compositionOffsets != null && compositionOffsets.Count != 0)
                    {
                        long compositionOffset = compositionOffsets[0].getOffset();
                        if (compositionOffsets[0].getCount() == 1)
                        {
                            compositionOffsets.RemoveAt(0);
                        }
                        else
                        {
                            compositionOffsets[0].setCount(compositionOffsets[0].getCount() - 1);
                        }
                        streamingSample.addSampleExtension(CompositionTimeSampleExtension.create(compositionOffset));
                    }

                    if (firstInLine.getTrackHeaderBox().getTrackId() == 1)
                    {
                        Java.LOG.debug("Pushing sample @" + offset + " of " + sampleSize + " bytes (i=" + index + ")");
                    }
                    tracks[firstInLine].getSampleSink().acceptSample(streamingSample, tracks[firstInLine]);


                    offset += sampleSize;


                }
                baos.discardTo(offset);
                currentChunks.Add(firstInLine, currentChunk + 1);
                currentSamples.Add(firstInLine, currentChunkStartSample + entry.getSamplesPerChunk());
            }
            foreach (Mp4StreamingTrack mp4StreamingTrack in tracks.Values)
            {
                mp4StreamingTrack.close();
            }
            Java.LOG.debug("All Samples read.");
        }

        public class Mp4StreamingTrack : StreamingTrack
        {

            private readonly TrackBox trackBox;
            protected Dictionary<Type, TrackExtension> trackExtensions = new Dictionary<Type, TrackExtension>();
            bool allSamplesRead = false;
            SampleSink sampleSink;

            public Mp4StreamingTrack(TrackBox trackBox)
            {
                this.trackBox = trackBox;
            }

            public void close()
            {
                allSamplesRead = true;
            }

            public bool isClosed()
            {
                return allSamplesRead;
            }

            public long getTimescale()
            {
                return trackBox.getMediaBox().getMediaHeaderBox().getTimescale();
            }

            public SampleSink getSampleSink()
            {
                return sampleSink;
            }

            public void setSampleSink(SampleSink sampleSink)
            {
                this.sampleSink = sampleSink;
            }

            public string getHandler()
            {
                return trackBox.getMediaBox().getHandlerBox().getHandlerType();
            }

            public string getLanguage()
            {
                return trackBox.getMediaBox().getMediaHeaderBox().getLanguage();
            }

            public SampleDescriptionBox getSampleDescriptionBox()
            {
                return trackBox.getSampleTableBox().getSampleDescriptionBox();
            }

            public T getTrackExtension<T>(Type clazz) where T : TrackExtension
            {
                return (T)trackExtensions[clazz];
            }

            public void addTrackExtension(TrackExtension trackExtension)
            {

                trackExtensions.Add(trackExtension.GetType(), trackExtension);
            }

            public void removeTrackExtension(Type clazz)
            {
                trackExtensions.Remove(clazz);
            }
        }

        public sealed class TeeInputStream : ByteStream
        {

            /**
             * The output stream that will receive a copy of all bytes read from the
             * proxied input stream.
             */
            private readonly ByteStream branch;
            long counter = 0;


            /**
             * Creates a TeeInputStream that proxies the given {@link InputStream}
             * and copies all read bytes to the given {@link OutputStream}. The given
             * output stream will not be closed when this stream gets closed.
             *
             * @param input  input stream to be proxied
             * @param branch output stream that will receive a copy of all bytes read
             */
            public TeeInputStream(ByteStream input, ByteStream branch) : base(input)
            {
                this.branch = branch;
            }

            /**
             * Reads a single byte from the proxied input stream and writes it to
             * the associated output stream.
             *
             * @return next byte from the stream, or -1 if the stream has ended
             * @throws IOException if the stream could not be read (or written)
             */
            public override int read()
            {
                int ch = base.read();
                if (ch != -1) {
                    branch.write(ch);
                    counter++;
                }
                return ch;
            }

            /**
             * Reads bytes from the proxied input stream and writes the read bytes
             * to the associated output stream.
             *
             * @param bts byte buffer
             * @param st  start offset within the buffer
             * @param end maximum number of bytes to read
             * @return number of bytes read, or -1 if the stream has ended
             * @throws IOException if the stream could not be read (or written)
             */
            public override int read(byte[] bts, int st, int end)
            {
                int n = base.read(bts, st, end);
                if (n != -1) {
                    branch.write(bts, st, n);
                    counter += n;
                }
                return n;
            }

            /**
             * Reads bytes from the proxied input stream and writes the read bytes
             * to the associated output stream.
             *
             * @param bts byte buffer
             * @return number of bytes read, or -1 if the stream has ended
             * @throws IOException if the stream could not be read (or written)
             */
            public override int read(byte[] bts)
            {
                int n = base.read(bts);
                if (n != -1) {
                    branch.write(bts, 0, n);
                    counter += n;
                }
                return n;
            }
        }
    }
}
