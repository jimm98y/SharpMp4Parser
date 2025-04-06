﻿using SharpMp4Parser.IsoParser.Boxes.Dolby;
using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part1.ObjectDescriptors;
using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part12;
using SharpMp4Parser.IsoParser.Boxes.SampleEntry;
using SharpMp4Parser.IsoParser.Tools;
using SharpMp4Parser.Java;
using System;
using System.Collections.Generic;

namespace SharpMp4Parser.Muxer.Tracks
{
    public class EC3TrackImpl : AbstractTrack
    {
        private const long MAX_FRAMES_PER_MMAP = 20;
        private readonly DataSource dataSource;
        TrackMetaData trackMetaData = new TrackMetaData();
        AudioSampleEntry audioSampleEntry;

        private int bitrate;
        private int frameSize;

        private List<BitStreamInfo> bitStreamInfos = new List<BitStreamInfo>();
        private List<Sample> samples;
        private long[] decodingTimes;

        public EC3TrackImpl(DataSource dataSource) : base(dataSource.ToString())
        {
            this.dataSource = dataSource;

            bool done = false;

            while (!done)
            {
                BitStreamInfo bsi = readVariables();
                if (bsi == null)
                {
                    throw new Exception();
                }
                foreach (BitStreamInfo entry in bitStreamInfos)
                {
                    if (bsi.strmtyp != 1 && entry.substreamid == bsi.substreamid)
                    {
                        done = true;
                    }
                }
                if (!done)
                {
                    bitStreamInfos.Add(bsi);
                }
            }


            if (bitStreamInfos.Count == 0)
            {
                throw new Exception();
            }
            int samplerate = bitStreamInfos[0].samplerate;

            audioSampleEntry = new AudioSampleEntry("ec-3");
            audioSampleEntry.setChannelCount(2);  // According to  ETSI TS 102 366 Annex F
            audioSampleEntry.setSampleRate(samplerate);
            audioSampleEntry.setDataReferenceIndex(1);
            audioSampleEntry.setSampleSize(16);

            EC3SpecificBox ec3 = new EC3SpecificBox();
            int[] deps = new int[bitStreamInfos.Count];
            int[] chan_locs = new int[bitStreamInfos.Count];
            foreach (BitStreamInfo bsi in bitStreamInfos)
            {
                if (bsi.strmtyp == 1)
                {
                    deps[bsi.substreamid]++;
                    chan_locs[bsi.substreamid] = ((bsi.chanmap >> 6) & 0x100) | ((bsi.chanmap >> 5) & 0xff);
                }
            }
            foreach (BitStreamInfo bsi in bitStreamInfos)
            {
                if (bsi.strmtyp != 1)
                {
                    EC3SpecificBox.Entry e = new EC3SpecificBox.Entry();
                    e.fscod = bsi.fscod;
                    e.bsid = bsi.bsid;
                    e.bsmod = bsi.bsmod;
                    e.acmod = bsi.acmod;
                    e.lfeon = bsi.lfeon;
                    e.reserved = 0;
                    e.num_dep_sub = deps[bsi.substreamid];
                    e.chan_loc = chan_locs[bsi.substreamid];
                    e.reserved2 = 0;
                    ec3.addEntry(e);
                }
                bitrate += bsi.bitrate;
                frameSize += bsi.frameSize;
            }

            ec3.setDataRate(bitrate / 1000);
            audioSampleEntry.addBox(ec3);

            trackMetaData.setCreationTime(DateTime.UtcNow);
            trackMetaData.setModificationTime(DateTime.UtcNow);

            trackMetaData.setTimescale(samplerate); // Audio tracks always use samplerate as timescale
            trackMetaData.setVolume(1);

            ((Java.Buffer)dataSource).position(0);
            samples = readSamples();
            this.decodingTimes = new long[samples.Count];
            Arrays.fill(decodingTimes, 1536);
        }

        public override void close()
        {
            dataSource.close();
        }

        public override IList<Sample> getSamples()
        {
            return samples;
        }

        public override List<SampleEntry> getSampleEntries()
        {
            return new List<SampleEntry>() { audioSampleEntry };
        }

        public override List<CompositionTimeToSample.Entry> getCompositionTimeEntries()
        {
            return null;
        }

        public override long[] getSyncSamples()
        {
            return null;
        }

        public override long[] getSampleDurations()
        {
            return decodingTimes;
        }

        public override List<SampleDependencyTypeBox.Entry> getSampleDependencies()
        {
            return null;
        }

        public override TrackMetaData getTrackMetaData()
        {
            return trackMetaData;
        }

        public override string getHandler()
        {
            return "soun";
        }

        public override SubSampleInformationBox getSubsampleInformationBox()
        {
            return null;
        }

        private BitStreamInfo readVariables()
        {
            long startPosition = dataSource.position();
            ByteBuffer bb = ByteBuffer.allocate(200);
            dataSource.read(bb);
            ((Java.Buffer)bb).rewind();

            BitReaderBuffer brb = new BitReaderBuffer(bb);
            int syncword = brb.readBits(16);
            if (syncword != 0xb77)
            {
                return null;
            }

            BitStreamInfo entry = new BitStreamInfo();

            entry.strmtyp = brb.readBits(2);
            entry.substreamid = brb.readBits(3);
            int frmsiz = brb.readBits(11);
            entry.frameSize = 2 * (frmsiz + 1);

            entry.fscod = brb.readBits(2);
            int fscod2 = -1;
            int numblkscod;
            if (entry.fscod == 3)
            {
                fscod2 = brb.readBits(2);
                numblkscod = 3;
            }
            else
            {
                numblkscod = brb.readBits(2);
            }
            int numberOfBlocksPerSyncFrame = 0;
            switch (numblkscod)
            {
                case 0:
                    numberOfBlocksPerSyncFrame = 1;
                    break;

                case 1:
                    numberOfBlocksPerSyncFrame = 2;
                    break;

                case 2:
                    numberOfBlocksPerSyncFrame = 3;
                    break;

                case 3:
                    numberOfBlocksPerSyncFrame = 6;
                    break;

            }
            entry.frameSize *= (6 / numberOfBlocksPerSyncFrame);

            entry.acmod = brb.readBits(3);
            entry.lfeon = brb.readBits(1);
            entry.bsid = brb.readBits(5);
            brb.readBits(5);
            if (1 == brb.readBits(1))
            {
                brb.readBits(8); // compr
            }
            if (0 == entry.acmod)
            {
                brb.readBits(5);
                if (1 == brb.readBits(1))
                {
                    brb.readBits(8);
                }
            }
            if (1 == entry.strmtyp)
            {
                if (1 == brb.readBits(1))
                {
                    entry.chanmap = brb.readBits(16);
                }
            }
            if (1 == brb.readBits(1))
            {     // mixing metadata
                if (entry.acmod > 2)
                {
                    brb.readBits(2);
                }
                if (1 == (entry.acmod & 1) && entry.acmod > 2)
                {
                    brb.readBits(3);
                    brb.readBits(3);
                }
                if (0 < (entry.acmod & 4))
                {
                    brb.readBits(3);
                    brb.readBits(3);
                }
                if (1 == entry.lfeon)
                {
                    if (1 == brb.readBits(1))
                    {
                        brb.readBits(5);
                    }
                }
                if (0 == entry.strmtyp)
                {
                    if (1 == brb.readBits(1))
                    {
                        brb.readBits(6);
                    }
                    if (0 == entry.acmod)
                    {
                        if (1 == brb.readBits(1))
                        {
                            brb.readBits(6);
                        }
                    }
                    if (1 == brb.readBits(1))
                    {
                        brb.readBits(6);
                    }
                    int mixdef = brb.readBits(2);
                    if (1 == mixdef)
                    {
                        brb.readBits(5);
                    }
                    else if (2 == mixdef)
                    {
                        brb.readBits(12);
                    }
                    else if (3 == mixdef)
                    {
                        int mixdeflen = brb.readBits(5);
                        if (1 == brb.readBits(1))
                        {
                            brb.readBits(5);
                            if (1 == brb.readBits(1))
                            {
                                brb.readBits(4);
                            }
                            if (1 == brb.readBits(1))
                            {
                                brb.readBits(4);
                            }
                            if (1 == brb.readBits(1))
                            {
                                brb.readBits(4);
                            }
                            if (1 == brb.readBits(1))
                            {
                                brb.readBits(4);
                            }
                            if (1 == brb.readBits(1))
                            {
                                brb.readBits(4);
                            }
                            if (1 == brb.readBits(1))
                            {
                                brb.readBits(4);
                            }
                            if (1 == brb.readBits(1))
                            {
                                brb.readBits(4);
                            }
                            if (1 == brb.readBits(1))
                            {
                                if (1 == brb.readBits(1))
                                {
                                    brb.readBits(4);
                                }
                                if (1 == brb.readBits(1))
                                {
                                    brb.readBits(4);
                                }
                            }
                        }
                        if (1 == brb.readBits(1))
                        {
                            brb.readBits(5);
                            if (1 == brb.readBits(1))
                            {
                                brb.readBits(7);
                                if (1 == brb.readBits(1))
                                {
                                    brb.readBits(8);
                                }
                            }
                        }
                        for (int i = 0; i < (mixdeflen + 2); i++)
                        {
                            brb.readBits(8);
                        }
                        brb.byteSync();
                    }
                    if (entry.acmod < 2)
                    {
                        if (1 == brb.readBits(1))
                        {
                            brb.readBits(14);
                        }
                        if (0 == entry.acmod)
                        {
                            if (1 == brb.readBits(1))
                            {
                                brb.readBits(14);
                            }
                        }
                        if (1 == brb.readBits(1))
                        {
                            if (numblkscod == 0)
                            {
                                brb.readBits(5);
                            }
                            else
                            {
                                for (int i = 0; i < numberOfBlocksPerSyncFrame; i++)
                                {
                                    if (1 == brb.readBits(1))
                                    {
                                        brb.readBits(5);
                                    }
                                }
                            }

                        }
                    }
                }
            }
            if (1 == brb.readBits(1))
            { // infomdate
                entry.bsmod = brb.readBits(3);
            }

            switch (entry.fscod)
            {
                case 0:
                    entry.samplerate = 48000;
                    break;

                case 1:
                    entry.samplerate = 44100;
                    break;

                case 2:
                    entry.samplerate = 32000;
                    break;

                case 3:
                    {
                        switch (fscod2)
                        {
                            case 0:
                                entry.samplerate = 24000;
                                break;

                            case 1:
                                entry.samplerate = 22050;
                                break;

                            case 2:
                                entry.samplerate = 16000;
                                break;

                            case 3:
                                entry.samplerate = 0;
                                break;
                        }
                        break;
                    }

            }
            if (entry.samplerate == 0)
            {
                return null;
            }

            entry.bitrate = (int)(((double)entry.samplerate) / 1536.0 * entry.frameSize * 8);

            dataSource.position(startPosition + entry.frameSize);
            return entry;
        }


        public class EC3TrackSample : Sample
        {
            private DataSource dataSource;
            private int start;
            private int frameSize;
            private EC3TrackImpl that;

            public EC3TrackSample(EC3TrackImpl that, DataSource dataSource, int start, int frameSize)
            {
                this.that = that;
                this.dataSource = dataSource;
                this.start = start;
                this.frameSize = frameSize;
            }

            public void writeTo(ByteStream channel)
            {
                dataSource.transferTo(start, frameSize, channel);
            }

            public long getSize()
            {
                return frameSize;
            }

            public ByteBuffer asByteBuffer()
            {
                try
                {
                    return dataSource.map(start, frameSize);
                }
                catch (Exception)
                {
                    throw;
                }
            }

            public SampleEntry getSampleEntry()
            {
                return that.audioSampleEntry;
            }
        }


        private List<Sample> readSamples()
        {
            int framesLeft = CastUtils.l2i((dataSource.size() - dataSource.position()) / frameSize);
            List<Sample> mySamples = new List<Sample>(framesLeft);
            for (int i = 0; i < framesLeft; i++)
            {
                int start = i * frameSize;
                mySamples.Add(new EC3TrackSample(this, dataSource, start, frameSize));
            }

            return mySamples;
        }

        public override string ToString()
        {
            return "EC3TrackImpl{" +
                    "bitrate=" + bitrate +
                    ", bitStreamInfos=" + bitStreamInfos +
                    '}';
        }

        public sealed class BitStreamInfo : EC3SpecificBox.Entry
        {
            public int frameSize;
            public int substreamid;
            public int bitrate;
            public int samplerate;
            public int strmtyp;
            public int chanmap;

            public override string ToString()
            {
                return "BitStreamInfo{" +
                        "frameSize=" + frameSize +
                        ", substreamid=" + substreamid +
                        ", bitrate=" + bitrate +
                        ", samplerate=" + samplerate +
                        ", strmtyp=" + strmtyp +
                        ", chanmap=" + chanmap +
                        '}';
            }
        }
    }
}