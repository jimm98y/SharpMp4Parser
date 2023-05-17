using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part12;
using SharpMp4Parser.IsoParser.Boxes.SampleEntry;
using SharpMp4Parser.IsoParser.Tools;
using SharpMp4Parser.Java;
using System;
using System.IO;
using System.Collections.Generic;
using SharpMp4Parser.IsoParser;

namespace SharpMp4Parser.Muxer.Container.MP4
{
    public class FragmentedMp4SampleList : AbstractList<Sample>
    {
        private IsoParser.Container isofile;
        private TrackBox trackBox = null;
        private TrackExtendsBox trex = null;
        private readonly Dictionary<TrackFragmentBox, MovieFragmentBox> traf2moof = new Dictionary<TrackFragmentBox, MovieFragmentBox>();
        private readonly WeakReference<Sample>[] sampleCache;
        private List<TrackFragmentBox> allTrafs;
        private readonly Dictionary<TrackRunBox, WeakReference<ByteBuffer>>
                trunDataCache = new Dictionary<TrackRunBox, WeakReference<ByteBuffer>>();
        private int[] firstSamples;
        private int size_ = -1;
        private readonly RandomAccessSource randomAccess;
        private readonly List<SampleEntry> sampleEntries;

        public FragmentedMp4SampleList(long track, IsoParser.Container isofile, RandomAccessSource randomAccess)
        {
            this.isofile = isofile;
            this.randomAccess = randomAccess;
            List<TrackBox> tbs = IsoParser.Tools.Path.getPaths<TrackBox>(isofile, "moov[0]/trak");
            foreach (TrackBox tb in tbs)
            {
                if (tb.getTrackHeaderBox().getTrackId() == track)
                {
                    trackBox = tb;
                }
            }
            if (trackBox == null)
            {
                throw new Exception("This MP4 does not contain track " + track);
            }

            List<TrackExtendsBox> trexs = IsoParser.Tools.Path.getPaths<TrackExtendsBox>(isofile, "moov[0]/mvex[0]/trex");
            foreach (TrackExtendsBox box in trexs)
            {
                if (box.getTrackId() == trackBox.getTrackHeaderBox().getTrackId())
                {
                    trex = box;
                }
            }
            sampleEntries = new List<SampleEntry>(trackBox.getSampleTableBox().getSampleDescriptionBox().getBoxes<SampleEntry>(typeof(SampleEntry)));

            if (sampleEntries.Count != trackBox.getSampleTableBox().getSampleDescriptionBox().getBoxes().Count)
                throw new Exception("stsd contains not only sample entries. Something's wrong here! Bailing out");
            sampleCache = new WeakReference<Sample>[size()];
            initAllFragments();
        }

        private void initAllFragments()
        {
            List<TrackFragmentBox> trafs = new List<TrackFragmentBox>();
            foreach (MovieFragmentBox moof in isofile.getBoxes< MovieFragmentBox>(typeof(MovieFragmentBox)))
            {
                foreach (TrackFragmentBox trackFragmentBox in moof.getBoxes<TrackFragmentBox>(typeof(TrackFragmentBox)))
                {
                    if (trackFragmentBox.getTrackFragmentHeaderBox().getTrackId() == trackBox.getTrackHeaderBox().getTrackId())
                    {
                        trafs.Add(trackFragmentBox);
                        traf2moof.Add(trackFragmentBox, moof);
                    }
                }
            }

            allTrafs = trafs;
            int firstSample = 1;
            firstSamples = new int[allTrafs.Count];
            for (int i = 0; i < allTrafs.Count; i++)
            {
                firstSamples[i] = firstSample;
                firstSample += getTrafSize(allTrafs[i]);
            }
        }

        private int getTrafSize(TrackFragmentBox traf)
        {
            List<Box> boxes = traf.getBoxes();
            int size = 0;
            foreach (Box b in boxes)
            {
                if (b is TrackRunBox)
                {
                    size += CastUtils.l2i(((TrackRunBox)b).getSampleCount());
                }
            }
            return size;
        }

        public class CustomSample : Sample
        {
            private long sampleSize;
            private Java.Buffer finalTrunData;
            private long finalOffset;
            private readonly TrackFragmentHeaderBox tfhd;
            private readonly List<SampleEntry> sampleEntries;

            public CustomSample(List<SampleEntry> sampleEntries, long sampleSize, Java.Buffer finalTrunData, long finalOffset, TrackFragmentHeaderBox tfhd)
            {
                this.sampleEntries = sampleEntries;
                this.sampleSize = sampleSize;
                this.finalTrunData = finalTrunData;
                this.finalOffset = finalOffset;
                this.tfhd = tfhd;
            }

            public void writeTo(ByteStream channel)
            {
                ByteBuffer bb = asByteBuffer();
                /*int a =*/
                channel.write(bb);
            }

            public long getSize()
            {
                return sampleSize;
            }

            public ByteBuffer asByteBuffer()
            {
                return (ByteBuffer)((Java.Buffer)((ByteBuffer)((Java.Buffer)finalTrunData).position((int)finalOffset)).slice()).limit(CastUtils.l2i(sampleSize));
            }

            public SampleEntry getSampleEntry()
            {
                return sampleEntries.Count == 1 ? sampleEntries[0] : sampleEntries[CastUtils.l2i(Math.Max(0, tfhd.getSampleDescriptionIndex() - 1))];
            }
        }

        public override Sample get(int index)
        {

            Sample cachedSample;
            if (sampleCache[index] != null && (sampleCache[index].TryGetTarget(out cachedSample)))
            {
                return cachedSample;
            }


            int targetIndex = index + 1;
            int j = firstSamples.Length - 1;
            while (targetIndex - firstSamples[j] < 0)
            {
                j--;
            }

            TrackFragmentBox trackFragmentBox = allTrafs[j];
            // we got the correct traf.
            int sampleIndexWithInTraf = targetIndex - firstSamples[j];
            int previousTrunsSize = 0;
            MovieFragmentBox moof = traf2moof[trackFragmentBox];

            foreach (Box box in trackFragmentBox.getBoxes())
            {
                if (box is TrackRunBox) {
                    TrackRunBox trun = (TrackRunBox)box;


                    if (trun.getEntries().Count <= (sampleIndexWithInTraf - previousTrunsSize))
                    {
                        previousTrunsSize += trun.getEntries().Count;
                    }
                    else
                    {
                        // we are in correct trun box
                        List<TrackRunBox.Entry> trackRunEntries = trun.getEntries();
                        TrackFragmentHeaderBox tfhd = trackFragmentBox.getTrackFragmentHeaderBox();
                        bool sampleSizePresent = trun.isSampleSizePresent();
                        bool hasDefaultSampleSize = tfhd.hasDefaultSampleSize();
                        long defaultSampleSize = 0;
                        if (!sampleSizePresent)
                        {
                            if (hasDefaultSampleSize)
                            {
                                defaultSampleSize = tfhd.getDefaultSampleSize();
                            }
                            else
                            {
                                if (trex == null)
                                {
                                    throw new Exception("File doesn't contain trex box but track fragments aren't fully self contained. Cannot determine sample size.");
                                }
                                defaultSampleSize = trex.getDefaultSampleSize();
                            }
                        }

                        WeakReference<ByteBuffer> trunDataRef;
                        trunDataCache.TryGetValue(trun, out trunDataRef);

                        ByteBuffer trunData = null;
                        if(trunDataRef != null)
                        {
                            trunDataRef.TryGetTarget(out trunData);
                        }

                        if (trunData == null)
                        {
                            long offs = 0;

                            if (tfhd.hasBaseDataOffset())
                            {
                                offs += tfhd.getBaseDataOffset();
                            }
                            else
                            {
                                offs += Offsets.find(isofile, moof, 0);
                            }

                            if (trun.isDataOffsetPresent())
                            {
                                offs += trun.getDataOffset();
                            }
                            long size = 0;
                            foreach (TrackRunBox.Entry e in trackRunEntries)
                            {
                                if (sampleSizePresent)
                                {
                                    size += e.getSampleSize();
                                }
                                else
                                {
                                    size += defaultSampleSize;
                                }
                            }
                            try
                            {
                                trunData = randomAccess.get(offs, size);
                                trunDataCache[trun] = new WeakReference<ByteBuffer>(trunData);
                            }
                            catch (IOException)
                            {
                                throw;
                            }
                        }

                        long off = 0;
                        for (int i = 0; i < (sampleIndexWithInTraf - previousTrunsSize); i++)
                        {
                            if (sampleSizePresent)
                            {
                                off += trackRunEntries[i].getSampleSize();
                            }
                            else
                            {
                                off += defaultSampleSize;
                            }
                        }
                        long sampleSize;
                        if (sampleSizePresent)
                        {
                            sampleSize = trackRunEntries[sampleIndexWithInTraf - previousTrunsSize].getSampleSize();
                        }
                        else
                        {
                            sampleSize = defaultSampleSize;
                        }

                        ByteBuffer finalTrunData = trunData;
                        long finalOffset = off;
                        // System.err.println("sNo. " + index + " offset: " + finalOffset + " size: " + sampleSize);
                        Sample sample = new CustomSample(sampleEntries, sampleSize, finalTrunData, finalOffset, tfhd);

                        sampleCache[index] = new WeakReference<Sample>(sample);
                        return sample;
                    }
                }
            }

            throw new Exception("Couldn't find sample in the traf I was looking");
        }

        public override int size()
        {
            if (size_ != -1)
            {
                return size_;
            }
            long i = 0;
            foreach (MovieFragmentBox moof in isofile.getBoxes< MovieFragmentBox>(typeof(MovieFragmentBox)))
            {
                foreach (TrackFragmentBox trackFragmentBox in moof.getBoxes<TrackFragmentBox>(typeof(TrackFragmentBox))) 
                {
                    if (trackFragmentBox.getTrackFragmentHeaderBox().getTrackId() == trackBox.getTrackHeaderBox().getTrackId())
                    {
                        foreach (TrackRunBox trackRunBox in trackFragmentBox.getBoxes<TrackRunBox>(typeof(TrackRunBox))) 
                        {
                            i += trackRunBox.getSampleCount();
                        }
                    }
                }
            }

            size_ = (int)i;
            return (int)i;
        }
    }
}