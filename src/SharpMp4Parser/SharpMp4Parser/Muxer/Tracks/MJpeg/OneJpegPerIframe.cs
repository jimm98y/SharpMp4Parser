using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part1.ObjectDescriptors;
using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part12;
using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part14;
using SharpMp4Parser.IsoParser.Boxes.SampleEntry;
using SharpMp4Parser.IsoParser.Tools;
using SharpMp4Parser.Java;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpMp4Parser.Muxer.Tracks.MJpeg
{
    /**
     * Created by sannies on 13.02.2015.
     */
    public class OneJpegPerIframe : AbstractTrack
    {
        private File[] jpegs;
        private TrackMetaData trackMetaData = new TrackMetaData();
        private long[] sampleDurations;
        private long[] syncSamples;
        private VisualSampleEntry mp4v;

        public OneJpegPerIframe(string name, File[] jpegs, Track alignTo) : base(name)
        {
            this.jpegs = jpegs;
            if (alignTo.getSyncSamples().Length != jpegs.Length)
            {
                throw new Exception("Number of sync samples doesn't match the number of stills (" + alignTo.getSyncSamples().Length + " vs. " + jpegs.Length + ")");
            }
            BufferedImage a = ImageIO.read(jpegs[0]);
            trackMetaData.setWidth(a.getWidth());
            trackMetaData.setHeight(a.getHeight());
            trackMetaData.setTimescale(alignTo.getTrackMetaData().getTimescale());


            long[]
            sampleDurationsToiAlignTo = alignTo.getSampleDurations();
            long[]
            syncSamples = alignTo.getSyncSamples();
            int currentSyncSample = 1;
            long duration = 0;
            sampleDurations = new long[syncSamples.Length];

            for (int i = 1; i < sampleDurationsToiAlignTo.Length; i++)
            {
                if (currentSyncSample < syncSamples.Length && i == syncSamples[currentSyncSample])
                {
                    sampleDurations[currentSyncSample - 1] = duration;
                    duration = 0;
                    currentSyncSample++;
                }
                duration += sampleDurationsToiAlignTo[i];
            }
            sampleDurations[sampleDurations.Length - 1] = duration;

            mp4v = new VisualSampleEntry("mp4v");
            ESDescriptorBox esds = new ESDescriptorBox();
            esds.setData(ByteBuffer.wrap(Hex.decodeHex("038080801B000100048080800D6C11000000000A1CB4000A1CB4068080800102")));
            esds.setEsDescriptor((ESDescriptor)ObjectDescriptorFactory.createFrom(-1, ByteBuffer.wrap(Hex.decodeHex("038080801B000100048080800D6C11000000000A1CB4000A1CB4068080800102"))));
            mp4v.addBox(esds);
            this.syncSamples = new long[jpegs.Length];
            for (int i = 0; i < this.syncSamples.Length; i++)
            {
                this.syncSamples[i] = i + 1;

            }

            double earliestTrackPresentationTime = 0;
            bool acceptDwell = true;
            bool acceptEdit = true;
            foreach (Edit edit in alignTo.getEdits())
            {
                if (edit.getMediaTime() == -1 && !acceptDwell)
                {
                    throw new Exception("Cannot accept edit list for processing (1)");
                }
                if (edit.getMediaTime() >= 0 && !acceptEdit)
                {
                    throw new Exception("Cannot accept edit list for processing (2)");
                }
                if (edit.getMediaTime() == -1)
                {
                    earliestTrackPresentationTime += edit.getSegmentDuration();
                }
                else /* if edit.getMediaTime() >= 0 */
                {
                    earliestTrackPresentationTime -= (double)edit.getMediaTime() / edit.getTimeScale();
                    acceptEdit = false;
                    acceptDwell = false;
                }
            }
            if (alignTo.getCompositionTimeEntries() != null && alignTo.getCompositionTimeEntries().Count > 0)
            {
                long currentTime = 0;
                int[] ptss = CompositionTimeToSample.blowupCompositionTimes(alignTo.getCompositionTimeEntries());
                for (int j = 0; j < ptss.Length && j < 50; j++)
                {
                    ptss[j] += (int)currentTime;
                    currentTime += alignTo.getSampleDurations()[j];
                }
                ptss = ptss.OrderBy(x => x).ToArray();
                earliestTrackPresentationTime += (double)ptss[0] / alignTo.getTrackMetaData().getTimescale();
            }

            if (earliestTrackPresentationTime < 0)
            {
                getEdits().Add(new Edit((long)(-earliestTrackPresentationTime * getTrackMetaData().getTimescale()), getTrackMetaData().getTimescale(), 1.0, (double)getDuration() / getTrackMetaData().getTimescale()));
            }
            else if (earliestTrackPresentationTime > 0)
            {
                getEdits().Add(new Edit(-1, getTrackMetaData().getTimescale(), 1.0, earliestTrackPresentationTime));
                getEdits().Add(new Edit(0, getTrackMetaData().getTimescale(), 1.0, (double)getDuration() / getTrackMetaData().getTimescale()));
            }

        }

        public override List<SampleEntry> getSampleEntries()
        {
            return new List<SampleEntry>() { mp4v };
        }

        public override long[] getSampleDurations()
        {
            return sampleDurations;
        }

        public override TrackMetaData getTrackMetaData()
        {
            return trackMetaData;
        }

        public override string getHandler()
        {
            return "vide";
        }

        public override long[] getSyncSamples()
        {
            return syncSamples;
        }

        public class JpegSample : Sample
        {
            ByteBuffer sample = null;
            File[] jpegs;
            int index;

            public JpegSample(File[] jpegs, int index)
            {
                this.jpegs = jpegs;
                this.index = index;
            }

            public void writeTo(WritableByteChannel channel)
            {
                RandomAccessFile raf = new RandomAccessFile(jpegs[index], "r");
                raf.getChannel().transferTo(0, raf.length(), channel);
                raf.close();
            }

            public long getSize()
            {
                return jpegs[index].length();
            }

            public ByteBuffer asByteBuffer()
            {
                if (sample == null)
                {
                    try
                    {
                        RandomAccessFile raf = new RandomAccessFile(jpegs[index], "r");
                        sample = raf.getChannel().map(FileChannel.MapMode.READ_ONLY, 0, raf.length());
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
                return sample;
            }

            public SampleEntry getSampleEntry()
            {
                return mp4v;
            }
        }

        public class JpegSampleList : List<Sample>
        {
            File[] jpegs;
            VisualSampleEntry mp4v;

            public JpegSampleList(File[] jpegs, VisualSampleEntry mp4v)
            {
                this.mp4v = mp4v;
                this.jpegs = jpegs;
            }

            public int size()
            {
                return jpegs.Length;
            }

            public Sample get(int index)
            {
                return new JpegSample(jpegs, index);
            }
        }

        public override List<Sample> getSamples()
        {
            return new JpegSampleList(jpegs, mp4v);
        }

        public override void close()
        {

        }
    }
}
