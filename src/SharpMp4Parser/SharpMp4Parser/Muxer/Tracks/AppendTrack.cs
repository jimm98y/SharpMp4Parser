using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part1.ObjectDescriptors;
using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part12;
using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part14;
using SharpMp4Parser.IsoParser.Boxes.SampleEntry;
using SharpMp4Parser.IsoParser;
using SharpMp4Parser.Java;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SharpMp4Parser.Muxer.Tracks
{
    /**
     * Appends two or more <code>Tracks</code> of the same type. No only that the type must be equal
     * also the decoder settings must be the same.
     */
    public class AppendTrack : AbstractTrack
    {
        //private static Logger LOG = LoggerFactory.getLogger(AppendTrack.class);
        Track[] tracks;

        private List<Sample> lists = new List<Sample>();
        private List<SampleEntry> sampleEntries = new List<SampleEntry>();
        private long[] decodingTimes;

        public AppendTrack(params Track[] tracks) : base(appendTracknames(tracks))
        {
            this.tracks = tracks;


            foreach (Track track in tracks)
            {
                sampleEntries.AddRange(track.getSampleEntries());
            }

            foreach (Track track in tracks)
            {
                //System.err.println("Track " + track + " is about to be appended");
                lists.AddRange(track.getSamples());
            }

            int numSamples = 0;
            foreach (Track track in tracks)
            {
                numSamples += track.getSampleDurations().Length;
            }
            decodingTimes = new long[numSamples];
            int index = 0;
            // should use system arraycopy but this works too (yes it's slow ...)
            foreach (Track track in tracks)
            {
                long[] durs = track.getSampleDurations();
                System.Array.Copy(durs, 0, decodingTimes, index, durs.Length);
                index += durs.Length;
            }
        }

        public static string appendTracknames(params Track[] tracks)
        {
            string name = "";
            foreach (Track track in tracks)
            {
                name += track.getName() + " + ";
            }
            return name.Substring(0, name.Length - 3);
        }

        public override void close()
        {
            foreach (Track track in tracks)
            {
                track.close();
            }
        }

        private SampleDescriptionBox mergeStsds(SampleDescriptionBox stsd1, SampleDescriptionBox stsd2)
        {
            ByteStream curBaos = new ByteStream();
            ByteStream refBaos = new ByteStream();
            try
            {
                stsd1.getBox(Channels.newChannel(curBaos));
                stsd2.getBox(Channels.newChannel(refBaos));
            }
            catch (IOException)
            {
                //LOG.error(e.getMessage());
                return null;
            }
            byte[] cur = curBaos.toByteArray();
            byte[] refr = refBaos.toByteArray();

            if (!Enumerable.SequenceEqual(refr, cur))
            {
                SampleEntry se = mergeSampleEntry(stsd1.getBoxes<SampleEntry>(typeof(SampleEntry))[0], stsd2.getBoxes<SampleEntry>(typeof(SampleEntry))[0]);
                if (se != null)
                {
                    stsd1.setBoxes(new List<Box>() { se });
                }
                else
                {
                    throw new IOException("Cannot merge " + stsd1.getBoxes<SampleEntry>(typeof(SampleEntry))[0] + " and " + stsd2.getBoxes<SampleEntry>(typeof(SampleEntry))[0]);
                }
            }
            return stsd1;
        }

        private SampleEntry mergeSampleEntry(SampleEntry se1, SampleEntry se2)
        {
            if (!se1.getType().Equals(se2.getType()))
            {
                return null;
            }
            else if (se1 is VisualSampleEntry && se2 is VisualSampleEntry)
            {
                return mergeVisualSampleEntry((VisualSampleEntry)se1, (VisualSampleEntry)se2);
            }
            else if (se1 is AudioSampleEntry && se2 is AudioSampleEntry)
            {
                return mergeAudioSampleEntries((AudioSampleEntry)se1, (AudioSampleEntry)se2);
            }
            else
            {
                return null;
            }
        }

        private VisualSampleEntry mergeVisualSampleEntry(VisualSampleEntry vse1, VisualSampleEntry vse2)
        {
            VisualSampleEntry vse = new VisualSampleEntry();
            if (vse1.getHorizresolution() == vse2.getHorizresolution())
            {
                vse.setHorizresolution(vse1.getHorizresolution());
            }
            else
            {
                //LOG.error("Horizontal Resolution differs");
                return null;
            }
            vse.setCompressorname(vse1.getCompressorname()); // ignore if they differ
            if (vse1.getDepth() == vse2.getDepth())
            {
                vse.setDepth(vse1.getDepth());
            }
            else
            {
                //LOG.error("Depth differs");
                return null;
            }

            if (vse1.getFrameCount() == vse2.getFrameCount())
            {
                vse.setFrameCount(vse1.getFrameCount());
            }
            else
            {
                //LOG.error("frame count differs");
                return null;
            }

            if (vse1.getHeight() == vse2.getHeight())
            {
                vse.setHeight(vse1.getHeight());
            }
            else
            {
                //LOG.error("height differs");
                return null;
            }
            if (vse1.getWidth() == vse2.getWidth())
            {
                vse.setWidth(vse1.getWidth());
            }
            else
            {
                //LOG.error("width differs");
                return null;
            }

            if (vse1.getVertresolution() == vse2.getVertresolution())
            {
                vse.setVertresolution(vse1.getVertresolution());
            }
            else
            {
                //LOG.error("vert resolution differs");
                return null;
            }

            if (vse1.getHorizresolution() == vse2.getHorizresolution())
            {
                vse.setHorizresolution(vse1.getHorizresolution());
            }
            else
            {
                //LOG.error("horizontal resolution differs");
                return null;
            }

            if (vse1.getBoxes().Count == vse2.getBoxes().Count)
            {
                List<Box>.Enumerator bxs1 = vse1.getBoxes().GetEnumerator();
                List<Box>.Enumerator bxs2 = vse2.getBoxes().GetEnumerator();
                while (bxs1.MoveNext() && bxs2.MoveNext())
                {
                    Box cur1 = bxs1.Current;
                    Box cur2 = bxs2.Current;
                    ByteStream baos1 = new ByteStream();
                    ByteStream baos2 = new ByteStream();
                    try
                    {
                        cur1.getBox(Channels.newChannel(baos1));
                        cur2.getBox(Channels.newChannel(baos2));
                    }
                    catch (IOException)
                    {
                        //LOG.warn(e.getMessage());
                        return null;
                    }
                    if (Enumerable.SequenceEqual(baos1.toByteArray(), baos2.toByteArray()))
                    {
                        vse.addBox(cur1);
                    }
                    else
                    {
                        if (cur1 is AbstractDescriptorBox && cur2 is AbstractDescriptorBox)
                        {
                            BaseDescriptor esd = mergeDescriptors(((AbstractDescriptorBox)cur1).getDescriptor(), ((AbstractDescriptorBox)cur2).getDescriptor());
                            ((AbstractDescriptorBox)cur1).setDescriptor(esd);
                            vse.addBox(cur1);
                        }
                    }
                }
            }
            return vse;
        }

        private AudioSampleEntry mergeAudioSampleEntries(AudioSampleEntry ase1, AudioSampleEntry ase2)
        {
            AudioSampleEntry ase = new AudioSampleEntry(ase2.getType());
            if (ase1.getBytesPerFrame() == ase2.getBytesPerFrame())
            {
                ase.setBytesPerFrame(ase1.getBytesPerFrame());
            }
            else
            {
                //LOG.error("BytesPerFrame differ");
                return null;
            }
            if (ase1.getBytesPerPacket() == ase2.getBytesPerPacket())
            {
                ase.setBytesPerPacket(ase1.getBytesPerPacket());
            }
            else
            {
                return null;
            }
            if (ase1.getBytesPerSample() == ase2.getBytesPerSample())
            {
                ase.setBytesPerSample(ase1.getBytesPerSample());
            }
            else
            {
                //LOG.error("BytesPerSample differ");
                return null;
            }
            if (ase1.getChannelCount() == ase2.getChannelCount())
            {
                ase.setChannelCount(ase1.getChannelCount());
            }
            else
            {
                return null;
            }
            if (ase1.getPacketSize() == ase2.getPacketSize())
            {
                ase.setPacketSize(ase1.getPacketSize());
            }
            else
            {
                //LOG.error("ChannelCount differ");
                return null;
            }
            if (ase1.getCompressionId() == ase2.getCompressionId())
            {
                ase.setCompressionId(ase1.getCompressionId());
            }
            else
            {
                return null;
            }
            if (ase1.getSampleRate() == ase2.getSampleRate())
            {
                ase.setSampleRate(ase1.getSampleRate());
            }
            else
            {
                return null;
            }
            if (ase1.getSampleSize() == ase2.getSampleSize())
            {
                ase.setSampleSize(ase1.getSampleSize());
            }
            else
            {
                return null;
            }
            if (ase1.getSamplesPerPacket() == ase2.getSamplesPerPacket())
            {
                ase.setSamplesPerPacket(ase1.getSamplesPerPacket());
            }
            else
            {
                return null;
            }
            if (ase1.getSoundVersion() == ase2.getSoundVersion())
            {
                ase.setSoundVersion(ase1.getSoundVersion());
            }
            else
            {
                return null;
            }
            if (Enumerable.SequenceEqual(ase1.getSoundVersion2Data(), ase2.getSoundVersion2Data()))
            {
                ase.setSoundVersion2Data(ase1.getSoundVersion2Data());
            }
            else
            {
                return null;
            }
            if (ase1.getBoxes().Count == ase2.getBoxes().Count)
            {
                List<Box>.Enumerator bxs1 = ase1.getBoxes().GetEnumerator();
                List<Box>.Enumerator bxs2 = ase2.getBoxes().GetEnumerator();
                while (bxs1.MoveNext() && bxs2.MoveNext())
                {
                    Box cur1 = bxs1.Current;
                    Box cur2 = bxs2.Current;
                    ByteStream baos1 = new ByteStream();
                    ByteStream baos2 = new ByteStream();
                    try
                    {
                        cur1.getBox(Channels.newChannel(baos1));
                        cur2.getBox(Channels.newChannel(baos2));
                    }
                    catch (IOException)
                    {
                        //LOG.warn(e.getMessage());
                        return null;
                    }
                    if (Enumerable.SequenceEqual(baos1.toByteArray(), baos2.toByteArray()))
                    {
                        ase.addBox(cur1);
                    }
                    else
                    {
                        if (ESDescriptorBox.TYPE.Equals(cur1.getType()) && ESDescriptorBox.TYPE.Equals(cur2.getType()))
                        {
                            ESDescriptorBox esdsBox1 = (ESDescriptorBox)cur1;
                            ESDescriptorBox esdsBox2 = (ESDescriptorBox)cur2;
                            ESDescriptor esd = mergeDescriptors(esdsBox1.getEsDescriptor(), esdsBox2.getEsDescriptor());
                            esdsBox1.setDescriptor(esd);
                            ase.addBox(cur1);
                        }
                    }
                }
            }
            return ase;

        }


        private ESDescriptor mergeDescriptors(BaseDescriptor des1, BaseDescriptor des2)
        {
            if (des1 is ESDescriptor && des2 is ESDescriptor)
            {
                ESDescriptor esds1 = (ESDescriptor)des1;
                ESDescriptor esds2 = (ESDescriptor)des2;
                if (esds1.getURLFlag() != esds2.getURLFlag())
                {
                    return null;
                }
                if (esds1.getURLLength() != esds2.getURLLength())
                {
                    // ignore different urls
                }
                if (esds1.getDependsOnEsId() != esds2.getDependsOnEsId())
                {
                    return null;
                }
                if (esds1.getEsId() != esds2.getEsId())
                {
                    return null;
                }
                if (esds1.getoCREsId() != esds2.getoCREsId())
                {
                    return null;
                }
                if (esds1.getoCRstreamFlag() != esds2.getoCRstreamFlag())
                {
                    return null;
                }
                if (esds1.getRemoteODFlag() != esds2.getRemoteODFlag())
                {
                    return null;
                }
                if (esds1.getStreamDependenceFlag() != esds2.getStreamDependenceFlag())
                {
                    return null;
                }
                if (esds1.getStreamPriority() != esds2.getStreamPriority())
                {
                    // use stream prio of first (why not)
                }
                if (esds1.getURLString() != null ? !esds1.getURLString().Equals(esds2.getURLString()) : esds2.getURLString() != null)
                {
                    // ignore different urls
                }
                if (esds1.getDecoderConfigDescriptor() != null ? !esds1.getDecoderConfigDescriptor().Equals(esds2.getDecoderConfigDescriptor()) : esds2.getDecoderConfigDescriptor() != null)
                {
                    DecoderConfigDescriptor dcd1 = esds1.getDecoderConfigDescriptor();
                    DecoderConfigDescriptor dcd2 = esds2.getDecoderConfigDescriptor();

                    if (dcd1.getAudioSpecificInfo() != null && dcd2.getAudioSpecificInfo() != null && !dcd1.getAudioSpecificInfo().Equals(dcd2.getAudioSpecificInfo()))
                    {
                        return null;
                    }
                    if (dcd1.getAvgBitRate() != dcd2.getAvgBitRate())
                    {
                        dcd1.setAvgBitRate((dcd1.getAvgBitRate() + dcd2.getAvgBitRate()) / 2);
                    }
                    if (dcd1.getBufferSizeDB() != dcd2.getBufferSizeDB())
                    {
                        // I don't care
                    }

                    if (dcd1.getDecoderSpecificInfo() != null ? !dcd1.getDecoderSpecificInfo().Equals(dcd2.getDecoderSpecificInfo()) : dcd2.getDecoderSpecificInfo() != null)
                    {
                        return null;
                    }

                    if (dcd1.getMaxBitRate() != dcd2.getMaxBitRate())
                    {
                        dcd1.setMaxBitRate(Math.Max(dcd1.getMaxBitRate(), dcd2.getMaxBitRate()));
                    }
                    if (!dcd1.getProfileLevelIndicationDescriptors().Equals(dcd2.getProfileLevelIndicationDescriptors()))
                    {
                        return null;
                    }

                    if (dcd1.getObjectTypeIndication() != dcd2.getObjectTypeIndication())
                    {
                        return null;
                    }
                    if (dcd1.getStreamType() != dcd2.getStreamType())
                    {
                        return null;
                    }
                    if (dcd1.getUpStream() != dcd2.getUpStream())
                    {
                        return null;
                    }

                }
                if (esds1.getOtherDescriptors() != null ? !esds1.getOtherDescriptors().Equals(esds2.getOtherDescriptors()) : esds2.getOtherDescriptors() != null)
                {
                    return null;
                }
                if (esds1.getSlConfigDescriptor() != null ? !esds1.getSlConfigDescriptor().Equals(esds2.getSlConfigDescriptor()) : esds2.getSlConfigDescriptor() != null)
                {
                    return null;
                }
                return esds1;
            }
            else
            {
                //LOG.error("I can only merge ESDescriptors");
                return null;
            }
        }

        public override IList<Sample> getSamples()
        {
            return lists;
        }
        public override List<SampleEntry> getSampleEntries()
        {
            return sampleEntries;
        }

        private readonly object _syncRoot = new object();
        public override long[] getSampleDurations()
        {
            lock (_syncRoot)
            {
                return decodingTimes;
            }
        }

        public override List<CompositionTimeToSample.Entry> getCompositionTimeEntries()
        {
            if (tracks[0].getCompositionTimeEntries() != null && tracks[0].getCompositionTimeEntries().Count != 0)
            {
                List<int[]> lists = new List<int[]>();
                foreach (Track track in tracks)
                {
                    lists.Add(CompositionTimeToSample.blowupCompositionTimes(track.getCompositionTimeEntries()));
                }
                List<CompositionTimeToSample.Entry> compositionTimeEntries = new List<CompositionTimeToSample.Entry>();
                foreach (int[] list in lists)
                {
                    foreach (int compositionTime in list)
                    {
                        if (compositionTimeEntries.Count == 0 || compositionTimeEntries.Last().getOffset() != compositionTime)
                        {
                            CompositionTimeToSample.Entry e = new CompositionTimeToSample.Entry(1, compositionTime);
                            compositionTimeEntries.Add(e);
                        }
                        else
                        {
                            CompositionTimeToSample.Entry e = compositionTimeEntries.Last();
                            e.setCount(e.getCount() + 1);
                        }
                    }
                }
                return compositionTimeEntries;
            }
            else
            {
                return null;
            }
        }

        public override long[] getSyncSamples()
        {
            if (tracks[0].getSyncSamples() != null && tracks[0].getSyncSamples().Length > 0)
            {
                int numSyncSamples = 0;
                foreach (Track track in tracks)
                {
                    numSyncSamples += track.getSyncSamples() != null ? track.getSyncSamples().Length : 0;
                }
                long[] returnSyncSamples = new long[numSyncSamples];

                int pos = 0;
                long samplesBefore = 0;
                foreach (Track track in tracks)
                {
                    if (track.getSyncSamples() != null)
                    {
                        foreach (long l in track.getSyncSamples())
                        {
                            returnSyncSamples[pos++] = samplesBefore + l;
                        }
                    }
                    samplesBefore += track.getSamples().Count;
                }
                return returnSyncSamples;
            }
            else
            {
                return null;
            }
        }

        public override List<SampleDependencyTypeBox.Entry> getSampleDependencies()
        {
            if (tracks[0].getSampleDependencies() != null && tracks[0].getSampleDependencies().Count != 0)
            {
                List<SampleDependencyTypeBox.Entry> list = new List<SampleDependencyTypeBox.Entry>();
                foreach (Track track in tracks)
                {
                    list.AddRange(track.getSampleDependencies());
                }
                return list;
            }
            else
            {
                return null;
            }
        }

        public override TrackMetaData getTrackMetaData()
        {
            return tracks[0].getTrackMetaData();
        }

        public override string getHandler()
        {
            return tracks[0].getHandler();
        }

        public override SubSampleInformationBox getSubsampleInformationBox()
        {
            return tracks[0].getSubsampleInformationBox();
        }
    }
}
