﻿using SharpMp4Parser.IsoParser.Boxes.Dolby;
using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part1.ObjectDescriptors;
using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part12;
using SharpMp4Parser.IsoParser.Boxes.SampleEntry;
using SharpMp4Parser.Java;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SharpMp4Parser.Muxer.Tracks
{
    public class DTSTrackImpl : AbstractTrack
    {
        private const int BUFFER = 1024 * 1024 * 64;
        TrackMetaData trackMetaData = new TrackMetaData();
        AudioSampleEntry audioSampleEntry;
        int samplerate;
        int bitrate;
        int frameSize = 0;
        int sampleSize;
        int samplesPerFrame;
        int channelCount;
        DTSSpecificBox ddts = new DTSSpecificBox();
        // Info from the headers
        bool isVBR = false;
#pragma warning disable 0414
        bool coreSubStreamPresent = false;
        bool extensionSubStreamPresent = false;
#pragma warning restore 0414
        int numExtSubStreams = 0;
        int coreMaxSampleRate = 0;
        int coreBitRate = 0;
        int coreChannelMask = 0;
        int coreFramePayloadInBytes = 0;
        int extAvgBitrate = 0;
        int extPeakBitrate = 0;
        int extSmoothBuffSize = 0;
        int extFramePayloadInBytes = 0;
        int maxSampleRate = 0;
        int lbrCodingPresent = 0;
        int numFramesTotal = 0;
        int samplesPerFrameAtMaxFs = 0;
        int numSamplesOrigAudioAtMaxFs = 0;
        int channelMask = 0;
        int codecDelayAtMaxFs = 0;
        int bcCoreMaxSampleRate = 0;
        int bcCoreBitRate = 0;
        int bcCoreChannelMask = 0;
        int lsbTrimPercent = 0;
        string type = "none";
        private long[] sampleDurations;
        private int dataOffset = 0;
        private DataSource dataSource;
        private List<Sample> samples;
        private string lang = "eng";

        public DTSTrackImpl(DataSource dataSource, string lang) : base(dataSource.ToString())
        {
            this.lang = lang;
            this.dataSource = dataSource;
            parse();
        }

        public DTSTrackImpl(DataSource dataSource) : base(dataSource.ToString())
        {
            this.dataSource = dataSource;
            parse();
        }

        public override void close()
        {
            dataSource.close();
        }

        private void parse()
        {
            if (!readVariables())
            {
                throw new Exception();
            }

            audioSampleEntry = new AudioSampleEntry(type);
            audioSampleEntry.setChannelCount(channelCount);
            audioSampleEntry.setSampleRate(samplerate);
            audioSampleEntry.setDataReferenceIndex(1);
            audioSampleEntry.setSampleSize(16);
            audioSampleEntry.addBox(ddts);

            trackMetaData.setCreationTime(DateTime.UtcNow);
            trackMetaData.setModificationTime(DateTime.UtcNow);
            trackMetaData.setLanguage(lang);
            trackMetaData.setTimescale(samplerate); // Audio tracks always use samplerate as timescale
        }

        public override IList<Sample> getSamples()
        {

            return samples;
        }

        public override List<SampleEntry> getSampleEntries()
        {
            return new List<SampleEntry>() { audioSampleEntry };
        }

        public override long[] getSampleDurations()
        {
            return sampleDurations;
        }

        public override List<CompositionTimeToSample.Entry> getCompositionTimeEntries()
        {
            return null;
        }

        public override long[] getSyncSamples()
        {
            return null;
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

        private void parseDtshdhdr(int size, ByteBuffer bb)
        {
            int hdrVersion = bb.getInt();
            int timeCodeHigh = bb.get();
            int timeCode = bb.getInt();
            int timeCodeFrameRate = bb.get();
            int bitwStreamMetadata = bb.getShort();
            int numAudioPresentations = bb.get();
            numExtSubStreams = bb.get();

            if ((bitwStreamMetadata & 1) == 1)
            {
                isVBR = true;
            }
            if ((bitwStreamMetadata & 8) == 8)
            {
                coreSubStreamPresent = true;
            }
            if ((bitwStreamMetadata & 0x10) == 0x10)
            {
                extensionSubStreamPresent = true;
                numExtSubStreams++;
            }
            else
            {
                numExtSubStreams = 0;
            }
            int i = 14;
            while (i < size)
            {
                bb.get();
                i++;
            }
        }

        private bool parseCoressmd(int size, ByteBuffer bb)
        {
            int cmsr_1 = bb.get();
            int cmsr_2 = bb.getShort();
            coreMaxSampleRate = (cmsr_1 << 16) | (cmsr_2 & 0xffff);
            coreBitRate = bb.getShort();
            coreChannelMask = bb.getShort();
            coreFramePayloadInBytes = bb.getInt();
            int i = 11;
            while (i < size)
            {
                bb.get();
                i++;
            }
            return true;
        }

        private bool parseAuprhdr(int size, ByteBuffer bb)
        {
            int audioPresIndex = bb.get();  // Audio_Pres_Index
            int bitwAupresData = bb.getShort(); // Bitw_Aupres_Metadata
            int a = bb.get();
            int b = bb.getShort();
            maxSampleRate = (a << 16) | (b & 0xffff);
            numFramesTotal = bb.getInt();
            samplesPerFrameAtMaxFs = bb.getShort();
            a = bb.get();
            b = bb.getInt();
            numSamplesOrigAudioAtMaxFs = (a << 32) | (b & 0xffff);
            channelMask = bb.getShort();
            codecDelayAtMaxFs = bb.getShort();
            int c = 21;
            if ((bitwAupresData & 3) == 3)
            {
                a = bb.get();
                b = bb.getShort();
                bcCoreMaxSampleRate = (a << 16) | (b & 0xffff);
                bcCoreBitRate = bb.getShort();
                bcCoreChannelMask = bb.getShort();
                c += 7;
            }
            if ((bitwAupresData & 0x4) > 0)
            {
                lsbTrimPercent = bb.get();
                c++;
            }
            if ((bitwAupresData & 0x8) > 0)
            {
                lbrCodingPresent = 1;
            }
            while (c < size)
            {
                bb.get();
                c++;
            }

            return true;
        }

        /**
         * EXTSS_MD
         */
        private bool parseExtssmd(int size, ByteBuffer bb)
        {
            int a = bb.get();
            int b = bb.getShort();
            extAvgBitrate = (a << 16) | (b & 0xffff);
            int i = 3;
            if (isVBR)
            {
                a = bb.get();
                b = bb.getShort();
                extPeakBitrate = (a << 16) | (b & 0xffff);
                extSmoothBuffSize = bb.getShort();
                i += 5;
            }
            else
            {
                extFramePayloadInBytes = bb.getInt();
                i += 4;
            }
            while (i < size)
            {
                bb.get();
                i++;
            }
            return true;
        }

        private bool readVariables()
        {
            ByteBuffer bb = dataSource.map(0, 25000);
            int testHeader1 = bb.getInt();
            int testHeader2 = bb.getInt();
            if (testHeader1 != 0x44545348 || (testHeader2 != 0x44484452))
            { // ASCII: DTSHDHDR
                throw new Exception("data does not start with 'DTSHDHDR' as required for a DTS-HD file");
            }

            while ((testHeader1 != 0x5354524d || testHeader2 != 0x44415441) && bb.remaining() > 100)
            { // ASCII: STRMDATA
                int size = (int)bb.getLong();
                if (testHeader1 == 0x44545348 && testHeader2 == 0x44484452)
                { // ASCII: DTSHDHDR
                    parseDtshdhdr(size, bb);
                }
                else if (testHeader1 == 0x434f5245 && testHeader2 == 0x53534d44)
                { // ASCII: CORESSMD
                    if (!parseCoressmd(size, bb))
                    {
                        return false;
                    }
                }
                else if (testHeader1 == 0x41555052 && testHeader2 == 0x2d484452)
                { // ASCII: AUPR-HDR
                    if (!parseAuprhdr(size, bb))
                    {
                        return false;
                    }
                }
                else if (testHeader1 == 0x45585453 && testHeader2 == 0x535f4d44)
                { // ASCII: EXTSS_MD
                    if (!parseExtssmd(size, bb))
                    {
                        return false;
                    }
                }
                else
                {
                    for (int i = 0; i < size; i++)
                    {
                        byte b = bb.get();
                    }
                }
                testHeader1 = bb.getInt();
                testHeader2 = bb.getInt();
            }
            long dataSize = bb.getLong();
            dataOffset = bb.position();

            int amode = -1;
            int extAudioId = 0;
            int extAudio = 0;

            int corePresent = -1;
            int extPresent = -1;
            int extXch = 0;
            int extXXch = 0;
            int extX96k = 0;
            int extXbr = 0;
            int extLbr = 0;
            int extXll = 0;
            int extCore = 0;

            bool done = false;


            while (!done)
            {
                int offset = bb.position();
                int sync = bb.getInt();
                if (sync == 0x7ffe8001)
                { // DTS_SYNCWORD_CORE
                    if (corePresent == 1)
                    {
                        done = true;
                    }
                    else
                    {
                        corePresent = 1;
                        BitReaderBuffer brb = new BitReaderBuffer(bb);

                        int ftype = brb.readBits(1);
                        int shrt = brb.readBits(5);
                        int cpf = brb.readBits(1); // Should always be 0 - CRC is not present
                        if (ftype != 1 || shrt != 31 || cpf != 0)

                        { // Termination frames - first frame should not be the last...
                            return false;
                        }

                        int nblks = brb.readBits(7);
                        samplesPerFrame = 32 * (nblks + 1);
                        int fsize = brb.readBits(14);
                        frameSize += fsize + 1;
                        amode = brb.readBits(6); // Calc channel layout from this
                        int sfreq = brb.readBits(4);

                        samplerate = getSampleRate(sfreq);

                        int rate = brb.readBits(5);

                        bitrate = getBitRate(rate);

                        int fixedBit = brb.readBits(1);
                        if (fixedBit != 0)

                        {
                            return false;
                        }

                        int dynf = brb.readBits(1);
                        int timef = brb.readBits(1);
                        int auxf = brb.readBits(1);
                        int hdcd = brb.readBits(1);
                        extAudioId = brb.readBits(3);
                        extAudio = brb.readBits(1);
                        int aspf = brb.readBits(1);
                        int lff = brb.readBits(2);
                        int hflag = brb.readBits(1);
                        int hcrc = 0;
                        if (cpf == 1)

                        { // Present only if CPF=1.
                            hcrc = brb.readBits(16);
                        }

                        int filts = brb.readBits(1);
                        int vernum = brb.readBits(4);
                        int chist = brb.readBits(2);
                        int pcmr = brb.readBits(3);
                        switch (pcmr)

                        {
                            case 0:
                            case 1:
                                sampleSize = 16;
                                break;

                            case 2:
                            case 3:
                                sampleSize = 20;
                                break;

                            case 5:
                            case 6:
                                sampleSize = 24;
                                break;

                            default:
                                return false;
                        }

                        int sumf = brb.readBits(1);
                        int sums = brb.readBits(1);
                        int dialnorm = 0;
                        int dng = 0;
                        switch (vernum)

                        {
                            case 6:
                                dialnorm = brb.readBits(4);
                                dng = -(16 + dialnorm);
                                break;

                            case 7:
                                dialnorm = brb.readBits(4);
                                dng = -dialnorm;
                                break;

                            default:
                                brb.readBits(4);
                                break;
                        }
                        ((Java.Buffer)bb).position(offset + fsize + 1);
                    }
                }
                else if (sync == 0x64582025)
                { // DTS_SYNCWORD_SUBSTREAM
                    if (corePresent == -1)
                    {
                        corePresent = 0;
                        samplesPerFrame = samplesPerFrameAtMaxFs;
                    }
                    extPresent = 1;
                    BitReaderBuffer brb = new BitReaderBuffer(bb);
                    int userDefinedBits = brb.readBits(8);
                    int nExtSSIndex = brb.readBits(2);
                    int headerSizeType = brb.readBits(1);
                    int nuBits4Header = 12;
                    int nuBits4ExSSFsize = 20;
                    if (headerSizeType == 0)
                    {
                        nuBits4Header = 8;
                        nuBits4ExSSFsize = 16;
                    }
                    int nuExtSSHeaderSize = brb.readBits(nuBits4Header) + 1;
                    int nuExtSSFsize = brb.readBits(nuBits4ExSSFsize) + 1;
                    ((Java.Buffer)bb).position(offset + nuExtSSHeaderSize);
                    int extSync = bb.getInt();
                    if (extSync == 0x5a5a5a5a)
                    {
                        if (extXch == 1)
                        {
                            done = true;
                        }
                        extXch = 1;
                    }
                    else if (extSync == 0x47004a03)
                    {
                        if (extXXch == 1)
                        {
                            done = true;
                        }
                        extXXch = 1;
                    }
                    else if (extSync == 0x1d95f262)
                    {
                        if (extX96k == 1)
                        {
                            done = true;
                        }
                        extX96k = 1;
                    }
                    else if (extSync == 0x655e315e)
                    {
                        if (extXbr == 1)
                        {
                            done = true;
                        }
                        extXbr = 1;
                    }
                    else if (extSync == 0x0a801921)
                    {
                        if (extLbr == 1)
                        {
                            done = true;
                        }
                        extLbr = 1;
                    }
                    else if (extSync == 0x41a29547)
                    {
                        if (extXll == 1)
                        {
                            done = true;
                        }
                        extXll = 1;
                    }
                    else if (extSync == 0x02b09261)
                    {
                        if (extCore == 1)
                        {
                            done = true;
                        }
                        extCore = 1;
                    }
                    if (!done)
                    {
                        frameSize += nuExtSSFsize;
                    }
                    ((Java.Buffer)bb).position(offset + nuExtSSFsize);
                }
                else
                {
                    throw new Exception("No DTS_SYNCWORD_* found at " + bb.position());
                }

            }
            int fd = -1;
            switch (samplesPerFrame)

            {
                case 512:
                    fd = 0;
                    break;

                case 1024:
                    fd = 1;
                    break;

                case 2048:
                    fd = 2;
                    break;

                case 4096:
                    fd = 3;
                    break;
            }

            if (fd == -1)

            {
                return false;
            }

            int coreLayout = 31;
            switch (amode)

            {
                case 0:
                case 2:
                case 4:
                case 5:
                case 6:
                case 7:
                case 8:
                case 9:
                    coreLayout = amode;
                    break;
            }

            int streamContruction = 0;
            if (corePresent == 0)
            {
                if (extXll == 1)
                {
                    if (extCore == 0)
                    {
                        streamContruction = 17;
                        type = "dtsl";
                    }
                    else
                    {
                        streamContruction = 21;
                        type = "dtsh";
                    }
                }
                else if (extLbr == 1)
                {
                    streamContruction = 18;
                    type = "dtse";
                }
                else if (extCore == 1)
                {
                    type = "dtsh";
                    if (extXXch == 0 && extXll == 0)
                    {
                        streamContruction = 19;
                    }
                    else if (extXXch == 1 && extXll == 0)
                    {
                        streamContruction = 20;
                    }
                    else if (extXXch == 0 && extXll == 1)
                    {
                        streamContruction = 21;
                    }
                }
                samplerate = maxSampleRate;
                sampleSize = 24; // Is this always true?
            }
            else
            {
                if (extPresent < 1)
                {
                    if (extAudio > 0)
                    {
                        switch (extAudioId)
                        {
                            case 0:
                                streamContruction = 2;
                                type = "dtsc";
                                break;

                            case 2:
                                streamContruction = 4;
                                type = "dtsc";
                                break;

                            case 6:
                                streamContruction = 3;
                                type = "dtsh";
                                break;

                            default:
                                streamContruction = 0;
                                type = "dtsh";
                                break;
                        }
                    }
                    else
                    {
                        streamContruction = 1;
                        type = "dtsc";
                    }
                }
                else
                {
                    type = "dtsh";
                    if (extAudio == 0)
                    {
                        if ((extCore == 0) && (extXXch == 1) && (extX96k == 0) && (extXbr == 0) && (extXll == 0) && (extLbr == 0))
                        {
                            streamContruction = 5;
                        }
                        else if (extCore == 0 && extXXch == 0 && extX96k == 0 && extXbr == 1 && extXll == 0 && extLbr == 0)
                        {
                            streamContruction = 6;
                        }
                        else if (extCore == 0 && extXXch == 1 && extX96k == 0 && extXbr == 1 && extXll == 0 && extLbr == 0)
                        {
                            streamContruction = 9;
                        }
                        else if (extCore == 0 && extXXch == 0 && extX96k == 1 && extXbr == 0 && extXll == 0 && extLbr == 0)
                        {
                            streamContruction = 10;
                        }
                        else if (extCore == 0 && extXXch == 1 && extX96k == 1 && extXbr == 0 && extXll == 0 && extLbr == 0)
                        {
                            streamContruction = 13;
                        }
                        else if (extCore == 0 && extXXch == 0 && extX96k == 0 && extXbr == 0 && extXll == 1 && extLbr == 0)
                        {
                            streamContruction = 14;
                        }
                    }
                    else
                    {
                        if ((extAudioId == 0) && (extCore == 0) && (extXXch == 0) && (extX96k == 0) && (extXbr == 1) && (extXll == 0) && (extLbr == 0))
                        {
                            streamContruction = 7;
                        }
                        else if ((extAudioId == 6) && (extCore == 0) && (extXXch == 0) && (extX96k == 0) && (extXbr == 1) && (extXll == 0) && (extLbr == 0))
                        {
                            streamContruction = 8;
                        }
                        else if ((extAudioId == 0) && (extCore == 0) && (extXXch == 0) && (extX96k == 1) && (extXbr == 0) && (extXll == 0) && (extLbr == 0))
                        {
                            streamContruction = 11;
                        }
                        else if ((extAudioId == 6) && (extCore == 0) && (extXXch == 0) && (extX96k == 1) && (extXbr == 0) && (extXll == 0) && (extLbr == 0))
                        {
                            streamContruction = 12;
                        }
                        else if ((extAudioId == 0) && (extCore == 0) && (extXXch == 0) && (extX96k == 0) && (extXbr == 0) && (extXll == 1) && (extLbr == 0))
                        {
                            streamContruction = 15;
                        }
                        else if ((extAudioId == 2) && (extCore == 0) && (extXXch == 0) && (extX96k == 0) && (extXbr == 0) && (extXll == 1) && (extLbr == 0))
                        {
                            streamContruction = 16;
                        }
                    }
                }
            }
            ddts.setDTSSamplingFrequency(maxSampleRate);
            if (isVBR)
            {
                ddts.setMaxBitRate(1000 * (coreBitRate + extPeakBitrate));
            }
            else
            {
                ddts.setMaxBitRate(1000 * (coreBitRate + extAvgBitrate));
            }
            ddts.setAvgBitRate(1000 * (coreBitRate + extAvgBitrate));
            ddts.setPcmSampleDepth(sampleSize);
            ddts.setFrameDuration(fd);
            ddts.setStreamConstruction(streamContruction); // We still need to look at this...
            if ((coreChannelMask & 0x8) > 0 || (coreChannelMask & 0x1000) > 0)
            {
                ddts.setCoreLFEPresent(1);
            }
            else
            {
                ddts.setCoreLFEPresent(0);
            }
            ddts.setCoreLayout(coreLayout);
            ddts.setCoreSize(coreFramePayloadInBytes);
            ddts.setStereoDownmix(0);
            ddts.setRepresentationType(4); // This was set in the other files?
            ddts.setChannelLayout(channelMask);
            if (coreMaxSampleRate > 0 && extAvgBitrate > 0)
            {
                ddts.setMultiAssetFlag(1);
            }
            else
            {
                ddts.setMultiAssetFlag(0);
            }
            ddts.setLBRDurationMod(lbrCodingPresent);
            ddts.setReservedBoxPresent(0);

            channelCount = 0;
            for (int bit = 0; bit < 16; bit++)
            {
                if (((channelMask >> bit) & 1) == 1)
                {
                    switch (bit)
                    {
                        case 0:
                        case 3:
                        case 4:
                        case 7:
                        case 8:
                        case 12:
                        case 14:
                            channelCount++;
                            break;

                        default:
                            channelCount += 2;
                            break;
                    }
                }
            }
            samples = generateSamples(dataSource, dataOffset, dataSize, corePresent);
            sampleDurations = new long[samples.Count];
            Arrays.fill(sampleDurations, samplesPerFrame);

            return true;
        }

        public class DTSTrackSample : Sample
        {
            private ByteBuffer finalSample;
            private readonly DTSTrackImpl that;

            public DTSTrackSample(DTSTrackImpl that, ByteBuffer finalSample)
            {
                this.that = that;
                this.finalSample = finalSample;
            }

            public void writeTo(ByteStream channel)
            {
                channel.write((ByteBuffer)((Java.Buffer)finalSample).rewind());
            }

            public long getSize()
            {
                return ((Java.Buffer)finalSample).rewind().remaining();
            }

            public ByteBuffer asByteBuffer()
            {
                return finalSample;
            }

            public SampleEntry getSampleEntry()
            {
                return that.audioSampleEntry;
            }
        }

        private List<Sample> generateSamples(DataSource dataSource, int dataOffset, long dataSize, int corePresent)
        {
            LookAhead la = new LookAhead(dataSource, dataOffset, dataSize, corePresent);
            ByteBuffer sample;
            List<Sample> mySamples = new List<Sample>();

            while ((sample = la.findNextStart()) != null)
            {
                ByteBuffer finalSample = sample;
                mySamples.Add(new DTSTrackSample(this, finalSample));
                //System.err.println(finalSample.remaining());
            }

            return mySamples;
        }

        private static int getBitRate(int rate)
        {
            int bitrate;
            switch (rate)

            {
                case 0:
                    bitrate = 32;
                    break;

                case 1:
                    bitrate = 56;
                    break;

                case 2:
                    bitrate = 64;
                    break;

                case 3:
                    bitrate = 96;
                    break;

                case 4:
                    bitrate = 112;
                    break;

                case 5:
                    bitrate = 128;
                    break;

                case 6:
                    bitrate = 192;
                    break;

                case 7:
                    bitrate = 224;
                    break;

                case 8:
                    bitrate = 256;
                    break;

                case 9:
                    bitrate = 320;
                    break;

                case 10:
                    bitrate = 384;
                    break;

                case 11:
                    bitrate = 448;
                    break;

                case 12:
                    bitrate = 512;
                    break;

                case 13:
                    bitrate = 576;
                    break;

                case 14:
                    bitrate = 640;
                    break;

                case 15:
                    bitrate = 768;
                    break;

                case 16:
                    bitrate = 960;
                    break;

                case 17:
                    bitrate = 1024;
                    break;

                case 18:
                    bitrate = 1152;
                    break;

                case 19:
                    bitrate = 1280;
                    break;

                case 20:
                    bitrate = 1344;
                    break;

                case 21:
                    bitrate = 1408;
                    break;

                case 22:
                    bitrate = 1411;
                    break;

                case 23:
                    bitrate = 1472;
                    break;

                case 24:
                    bitrate = 1536;
                    break;

                case 25:
                    bitrate = -1; // Open, some other bitrate....
                    break;

                default:
                    throw new Exception("Unknown bitrate value");

            }
            return bitrate;
        }

        private static int getSampleRate(int sfreq)
        {
            int samplerate;
            switch (sfreq)

            {
                case 1:
                    samplerate = 8000;
                    break;

                case 2:
                    samplerate = 16000;
                    break;

                case 3:
                    samplerate = 32000;
                    break;

                case 6:
                    samplerate = 11025;
                    break;

                case 7:
                    samplerate = 22050;
                    break;

                case 8:
                    samplerate = 44100;
                    break;

                case 11:
                    samplerate = 12000;
                    break;

                case 12:
                    samplerate = 24000;
                    break;

                case 13:
                    samplerate = 48000;
                    break;

                default: // No other base samplerates allowed
                    throw new Exception("Unknown Sample Rate");

            }
            return samplerate;
        }

        public class LookAhead
        {
            private readonly int corePresent;
            long bufferStartPos;
            int inBufferPos = 0;
            DataSource dataSource;
            long dataEnd;
            ByteBuffer buffer;

            long start;

            public LookAhead(DataSource dataSource, long bufferStartPos, long dataSize, int corePresent)
            {
                this.dataSource = dataSource;
                this.bufferStartPos = bufferStartPos;
                this.dataEnd = dataSize + bufferStartPos;
                this.corePresent = corePresent;
                fillBuffer();
            }

            public ByteBuffer findNextStart()
            {
                try
                {
                    // If core DTS stream is present then sync word is 0x7FFE8001
                    // otherwise 0x64582025
                    while (corePresent == 1 ? !this.nextFourEquals0x7FFE8001() : !nextFourEquals0x64582025())
                    {
                        this.discardByte();
                    }
                    this.discardNext4AndMarkStart();

                    while (corePresent == 1 ? !this.nextFourEquals0x7FFE8001orEof() : !nextFourEquals0x64582025orEof())
                    {
                        this.discardQWord();
                    }
                    return this.getSample();
                }
                catch (Exception)
                {
                    return null;
                }
            }


            private void fillBuffer()
            {
                buffer = dataSource.map(bufferStartPos, Math.Min(dataEnd - bufferStartPos, BUFFER));
            }

            private bool nextFourEquals0x64582025()
            {
                return nextFourEquals((byte)0x64, (byte)0x58, (byte)0x20, (byte)0x25);
            }

            private bool nextFourEquals0x7FFE8001()
            {
                return nextFourEquals((byte)0x7f, (byte)0xFE, (byte)0x80, (byte)0x01);
            }

            private bool nextFourEquals(byte a, byte b, byte c, byte d)
            {
                if (buffer.limit() - inBufferPos >= 4)
                {
                    return ((buffer.get(inBufferPos) == a &&
                            buffer.get(inBufferPos + 1) == b &&
                            buffer.get(inBufferPos + 2) == c &&
                            (buffer.get(inBufferPos + 3) == d)));
                }
                if (bufferStartPos + inBufferPos + 4 >= dataSource.size())
                {
                    throw new Exception();
                }
                return false;
            }

            private bool nextFourEquals0x64582025orEof()
            {
                return nextFourEqualsOrEof((byte)0x64, (byte)0x58, (byte)0x20, (byte)0x25);
            }


            private bool nextFourEquals0x7FFE8001orEof()
            {
                return nextFourEqualsOrEof((byte)0x7F, (byte)0xFE, (byte)0x80, (byte)0x01);
            }

            private bool nextFourEqualsOrEof(byte a, byte b, byte c, byte d)
            {
                if (buffer.limit() - inBufferPos >= 4)
                {
                    if (((bufferStartPos + inBufferPos) % (1024 * 1024)) == 0)
                    {
                        Java.LOG.debug("" + ((bufferStartPos + inBufferPos) / 1024 / 1024));
                    }
                    return ((buffer.get(inBufferPos) == a /*0x7F */ &&
                            buffer.get(inBufferPos + 1) == b/*0xfe*/ &&
                            buffer.get(inBufferPos + 2) == c /*0x80*/ &&
                            (buffer.get(inBufferPos + 3) == d)));
                }
                else
                {
                    if (bufferStartPos + inBufferPos + 4 > dataEnd)
                    {
                        return bufferStartPos + inBufferPos == dataEnd;
                    }
                    else
                    {
                        bufferStartPos = start;
                        inBufferPos = 0;
                        fillBuffer();
                        return nextFourEquals0x7FFE8001();
                    }
                }
            }


            private void discardByte()
            {
                inBufferPos += 1;
            }

            private void discardQWord()
            {
                inBufferPos += 4;
            }


            private void discardNext4AndMarkStart()
            {
                start = bufferStartPos + inBufferPos;
                inBufferPos += 4;
            }

            private ByteBuffer getSample()
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
                }
            }
        }
    }
}