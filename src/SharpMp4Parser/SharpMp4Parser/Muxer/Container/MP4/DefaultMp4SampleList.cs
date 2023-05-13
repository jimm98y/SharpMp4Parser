using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part12;
using SharpMp4Parser.IsoParser.Boxes.SampleEntry;
using SharpMp4Parser.IsoParser.Tools;
using SharpMp4Parser.Java;
using System;
using System.Collections.Generic;
using System.IO;

namespace SharpMp4Parser.Muxer.Container.MP4
{
    public class DefaultMp4SampleList : AbstractList<Sample>
    {
        private readonly object _syncRoot = new object();
        //private static final Logger LOG = LoggerFactory.getLogger(DefaultMp4SampleList.class);

        private TrackBox trackBox = null;
        private WeakReference<ByteBuffer>[] cache = null;
        private int[] chunkNumsStartSampleNum;
        private int[] chunkNumsToSampleDescriptionIndex;
        private long[] chunkOffsets;
        private long[][] sampleOffsetsWithinChunks;
        private SampleSizeBox ssb;
        private int lastChunk = 0;
        private RandomAccessSource randomAccess;
        private List<SampleEntry> sampleEntries;


        public DefaultMp4SampleList(long track, IsoParser.Container topLevel, RandomAccessSource randomAccessFile)
        {
            this.randomAccess = randomAccessFile;
            MovieBox movieBox = topLevel.getBoxes< MovieBox>(typeof(MovieBox))[0];
            List<TrackBox> trackBoxes = movieBox.getBoxes< TrackBox>(typeof(TrackBox));

            foreach (TrackBox tb in trackBoxes)
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
            sampleEntries = new List<SampleEntry>(trackBox.getSampleTableBox().getSampleDescriptionBox().getBoxes< SampleEntry>(typeof(SampleEntry)));

            if (sampleEntries.Count != trackBox.getSampleTableBox().getSampleDescriptionBox().getBoxes().Count)
                throw new Exception("stsd contains not only sample entries. Something's wrong here! Bailing out");

            chunkOffsets = trackBox.getSampleTableBox().getChunkOffsetBox().getChunkOffsets();
            long[] chunkSizes = new long[chunkOffsets.Length];

            cache = new WeakReference<ByteBuffer>[chunkOffsets.Length];
            for (int i = 0; i < cache.Length; i++)
            {
                cache[i] = new WeakReference<ByteBuffer>(null);
            }

            sampleOffsetsWithinChunks = new long[chunkOffsets.Length][];
            chunkNumsToSampleDescriptionIndex = new int[chunkOffsets.Length];
            ssb = trackBox.getSampleTableBox().getSampleSizeBox();
            List<SampleToChunkBox.Entry> s2chunkEntries = trackBox.getSampleTableBox().getSampleToChunkBox().getEntries();
            SampleToChunkBox.Entry[] entries = s2chunkEntries.ToArray();


            int s2cIndex = 0;
            SampleToChunkBox.Entry next = entries[s2cIndex++];
            int currentChunkNo = 0;
            int currentSamplePerChunk = 0;
            int currentSampleDescriptionIndex = 0;


            long nextFirstChunk = next.getFirstChunk();
            int nextSamplePerChunk = CastUtils.l2i(next.getSamplesPerChunk());
            int nextSampleDescriptionIndex = CastUtils.l2i(next.getSampleDescriptionIndex());

            int currentSampleNo = 1;
            int lastSampleNo = size();

            do
            {
                currentChunkNo++;
                if (currentChunkNo == nextFirstChunk)
                {
                    currentSamplePerChunk = nextSamplePerChunk;
                    currentSampleDescriptionIndex = nextSampleDescriptionIndex;
                    if (entries.Length > s2cIndex)
                    {
                        next = entries[s2cIndex++];
                        nextSamplePerChunk = CastUtils.l2i(next.getSamplesPerChunk());
                        nextSampleDescriptionIndex = CastUtils.l2i(next.getSampleDescriptionIndex());
                        nextFirstChunk = next.getFirstChunk();
                    }
                    else
                    {
                        nextSamplePerChunk = -1;
                        nextSampleDescriptionIndex = -1;
                        nextFirstChunk = long.MaxValue;
                    }
                }
                sampleOffsetsWithinChunks[currentChunkNo - 1] = new long[currentSamplePerChunk];
                chunkNumsToSampleDescriptionIndex[currentChunkNo - 1] = currentSampleDescriptionIndex;

            } while ((currentSampleNo += currentSamplePerChunk) <= lastSampleNo);
            chunkNumsStartSampleNum = new int[currentChunkNo + 1];

            // reset of algorithm
            s2cIndex = 0;
            next = entries[s2cIndex++];
            currentChunkNo = 0;
            currentSamplePerChunk = 0;

            nextFirstChunk = next.getFirstChunk();
            nextSamplePerChunk = CastUtils.l2i(next.getSamplesPerChunk());

            currentSampleNo = 1;
            do
            {
                chunkNumsStartSampleNum[currentChunkNo++] = currentSampleNo;
                if (currentChunkNo == nextFirstChunk)
                {
                    currentSamplePerChunk = nextSamplePerChunk;
                    if (entries.Length > s2cIndex)
                    {
                        next = entries[s2cIndex++];
                        nextSamplePerChunk = CastUtils.l2i(next.getSamplesPerChunk());

                        nextFirstChunk = next.getFirstChunk();
                    }
                    else
                    {
                        nextSamplePerChunk = -1;
                        nextFirstChunk = long.MaxValue;
                    }
                }


            } while ((currentSampleNo += currentSamplePerChunk) <= lastSampleNo);
            chunkNumsStartSampleNum[currentChunkNo] = int.MaxValue;

            currentChunkNo = 0;
            long sampleSum = 0;
            for (int i = 1; i <= ssb.getSampleCount(); i++)
            {
                while (i == chunkNumsStartSampleNum[currentChunkNo])
                {
                    // you might think that an if statement is enough but unfortunately you might as well declare chunks without any samples!
                    currentChunkNo++;
                    sampleSum = 0;
                }
                chunkSizes[currentChunkNo - 1] += ssb.getSampleSizeAtIndex(i - 1);
                long[] sampleOffsetsWithinChunkscurrentChunkNo = sampleOffsetsWithinChunks[currentChunkNo - 1];
                int chunkNumsStartSampleNumcurrentChunkNo = chunkNumsStartSampleNum[currentChunkNo - 1];
                sampleOffsetsWithinChunkscurrentChunkNo[i - chunkNumsStartSampleNumcurrentChunkNo] = sampleSum;
                sampleSum += ssb.getSampleSizeAtIndex(i - 1);
            }
        }

        private int getChunkForSample(int index)
        {
            lock (_syncRoot)
            {
                int sampleNum = index + 1;
                // we always look for the next chunk in the last one to make linear access fast
                if (sampleNum >= chunkNumsStartSampleNum[lastChunk] && sampleNum < chunkNumsStartSampleNum[lastChunk + 1])
                {
                    return lastChunk;
                }
                else if (sampleNum < chunkNumsStartSampleNum[lastChunk])
                {
                    // we could search backwards but i don't believe there is much backward linear access
                    // I'd then rather suspect a start from scratch
                    lastChunk = 0;

                    while (chunkNumsStartSampleNum[lastChunk + 1] <= sampleNum)
                    {
                        lastChunk++;
                    }
                    return lastChunk;

                }
                else
                {
                    lastChunk += 1;

                    while (chunkNumsStartSampleNum[lastChunk + 1] <= sampleNum)
                    {
                        lastChunk++;
                    }
                    return lastChunk;
                }
            }
        }

        public override Sample get(int index)
        {
            if (index >= ssb.getSampleCount())
            {
                throw new IndexOutOfRangeException();
            }
            return new SampleImpl(index, this);
        }

        public override int size()
        {
            return CastUtils.l2i(trackBox.getSampleTableBox().getSampleSizeBox().getSampleCount());
        }

        public class SampleImpl : Sample
        {
            private readonly object _syncRoot = new object();

            private int index;
            private DefaultMp4SampleList that;

            public SampleImpl(int index, DefaultMp4SampleList that) 
            {
                this.index = index;
                this.that = that;
            }

            public void writeTo(WritableByteChannel channel)
            {
                channel.write(asByteBuffer());
            }

            public long getSize()
            {
                return that.ssb.getSampleSizeAtIndex(index);
            }

            public ByteBuffer asByteBuffer()
            {
                lock (_syncRoot)
                {
                    ByteBuffer b;

                    int chunkNumber = that.getChunkForSample(index);
                    WeakReference<ByteBuffer> chunkBufferSr = that.cache[chunkNumber];

                    int chunkStartSample = that.chunkNumsStartSampleNum[chunkNumber] - 1;

                    int sampleInChunk = index - chunkStartSample;
                    long[] sampleOffsetsWithinChunk = that.sampleOffsetsWithinChunks[CastUtils.l2i(chunkNumber)];
                    long offsetWithInChunk = sampleOffsetsWithinChunk[sampleInChunk];

                    ByteBuffer chunkBuffer;
                    if (chunkBufferSr == null || !chunkBufferSr.TryGetTarget(out chunkBuffer))
                    {
                        try
                        {
                            chunkBuffer = that.randomAccess.get(
                                    that.chunkOffsets[CastUtils.l2i(chunkNumber)],
                                    sampleOffsetsWithinChunk[sampleOffsetsWithinChunk.Length - 1] + that.ssb.getSampleSizeAtIndex(chunkStartSample + sampleOffsetsWithinChunk.Length - 1));
                            that.cache[chunkNumber] = new WeakReference<ByteBuffer>(chunkBuffer);
                        }
                        catch (IOException e)
                        {
                            //LOG.error("", e);
                            throw new IndexOutOfRangeException(e.Message);
                        }
                    }
                    b = (ByteBuffer)((ByteBuffer)chunkBuffer.duplicate().position(CastUtils.l2i(offsetWithInChunk))).slice().limit(CastUtils.l2i(that.ssb.getSampleSizeAtIndex(index)));
                    return b;
                }
            }

            public override string ToString()
            {
                return "Sample(index: " + index + " size: " + that.ssb.getSampleSizeAtIndex(index) + ")";
            }

            public SampleEntry getSampleEntry()
            {
                return that.sampleEntries[that.chunkNumsToSampleDescriptionIndex[that.getChunkForSample(this.index)] - 1];
            }
        }
    }
}
