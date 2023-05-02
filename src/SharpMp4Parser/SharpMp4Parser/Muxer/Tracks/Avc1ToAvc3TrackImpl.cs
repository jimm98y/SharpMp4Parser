using SharpMp4Parser.IsoParser.Tools;
using SharpMp4Parser.Java;
using System;
using System.Collections.Generic;
using SharpMp4Parser.IsoParser.Boxes.SampleEntry;
using SharpMp4Parser.IsoParser;
using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part15;

namespace SharpMp4Parser.Muxer.Tracks
{
    /**
     * Converts an avc1 track to an avc3 track. The major difference is the location of SPS/PPS: While the avc1 track
     * has all SPS/PPS in the <code>SampleEntry</code> the avc3 track has all required SPS/PPS include in each sync sample.
     */
    public class Avc1ToAvc3TrackImpl : WrappingTrack
    {

        IList<Sample> samples;
        private Dictionary<SampleEntry, SampleEntry> avc1toavc3 = new Dictionary<SampleEntry, SampleEntry>();

        public Avc1ToAvc3TrackImpl(Track parent) : base(parent)
        {
            foreach (SampleEntry sampleEntry in parent.getSampleEntries())
            {
                if (sampleEntry.getType().Equals("avc1"))
                {
                    ByteArrayOutputStream baos = new ByteArrayOutputStream();
                    try
                    {
                        // This creates a copy cause I can't change the original instance
                        sampleEntry.getBox(Channels.newChannel(baos));
                        VisualSampleEntry avc3SampleEntry = (VisualSampleEntry)new IsoFile(new ByteBufferByteChannel(ByteBuffer.wrap(baos.toByteArray()))).getBoxes()[0];
                        avc3SampleEntry.setType("avc3");
                        avc1toavc3.Add(sampleEntry, avc3SampleEntry);
                    }
                    catch (Exception)
                    {
                        throw new Exception("Dumping sample entry to memory failed");
                    }
                }
                else
                {
                    avc1toavc3.Add(sampleEntry, sampleEntry);
                }
            }

            samples = new ReplaceSyncSamplesList(parent.getSamples(), this);
        }

        public override List<SampleEntry> getSampleEntries()
        {
            return new List<SampleEntry>(avc1toavc3.Values);
        }

        public override IList<Sample> getSamples()
        {
            return samples;
        }

        public class CustomSample : Sample
        {
            private ByteBuffer buf;
            private int len;
            private AvcConfigurationBox avcC;
            private SampleEntry se;
            private Sample orignalSample;

            public CustomSample(ByteBuffer buf, int len, AvcConfigurationBox avcC, SampleEntry se, Sample orignalSample)
            {
                this.buf = buf;
                this.len = len;
                this.avcC = avcC;
                this.se = se;
                this.orignalSample = orignalSample;
            }

            public SampleEntry getSampleEntry()
            {
                return se;
            }

            public void writeTo(WritableByteChannel channel)
            {

                foreach (ByteBuffer bytes in avcC.getSequenceParameterSets())
                {
                    IsoTypeWriterVariable.write(bytes.limit(), (ByteBuffer)((Java.Buffer)buf).rewind(), len);
                    channel.write((ByteBuffer)((Java.Buffer)buf).rewind());
                    channel.write(bytes);
                }
                foreach (ByteBuffer bytes in avcC.getSequenceParameterSetExts())
                {
                    IsoTypeWriterVariable.write(bytes.limit(), (ByteBuffer)((Java.Buffer)buf).rewind(), len);
                    channel.write((ByteBuffer)((Java.Buffer)buf).rewind());
                    channel.write((bytes));
                }
                foreach (ByteBuffer bytes in avcC.getPictureParameterSets())
                {
                    IsoTypeWriterVariable.write(bytes.limit(), (ByteBuffer)((Java.Buffer)buf).rewind(), len);
                    channel.write((ByteBuffer)((Java.Buffer)buf).rewind());
                    channel.write((bytes));
                }
                orignalSample.writeTo(channel);
            }

            public long getSize()
            {

                int spsPpsSize = 0;
                foreach (ByteBuffer bytes in avcC.getSequenceParameterSets())
                {
                    spsPpsSize += len + bytes.limit();
                }
                foreach (ByteBuffer bytes in avcC.getSequenceParameterSetExts())
                {
                    spsPpsSize += len + bytes.limit();
                }
                foreach (ByteBuffer bytes in avcC.getPictureParameterSets())
                {
                    spsPpsSize += len + bytes.limit();
                }
                return orignalSample.getSize() + spsPpsSize;
            }

            public ByteBuffer asByteBuffer()
            {

                int spsPpsSize = 0;
                foreach (ByteBuffer bytes in avcC.getSequenceParameterSets())
                {
                    spsPpsSize += len + bytes.limit();
                }
                foreach (ByteBuffer bytes in avcC.getSequenceParameterSetExts())
                {
                    spsPpsSize += len + bytes.limit();
                }
                foreach (ByteBuffer bytes in avcC.getPictureParameterSets())
                {
                    spsPpsSize += len + bytes.limit();
                }


                ByteBuffer data = ByteBuffer.allocate(CastUtils.l2i(orignalSample.getSize()) + spsPpsSize);
                foreach (ByteBuffer bytes in avcC.getSequenceParameterSets())
                {
                    IsoTypeWriterVariable.write(bytes.limit(), data, len);
                    data.put(bytes);
                }
                foreach (ByteBuffer bytes in avcC.getSequenceParameterSetExts())
                {
                    IsoTypeWriterVariable.write(bytes.limit(), data, len);
                    data.put(bytes);
                }
                foreach (ByteBuffer bytes in avcC.getPictureParameterSets())
                {
                    IsoTypeWriterVariable.write(bytes.limit(), data, len);
                    data.put(bytes);
                }
                data.put(orignalSample.asByteBuffer());
                return (ByteBuffer)((Java.Buffer)data).rewind();
            }
        }

        private class ReplaceSyncSamplesList : AbstractList<Sample>
        {
            IList<Sample> parentSamples;
            Avc1ToAvc3TrackImpl that;

            public ReplaceSyncSamplesList(IList<Sample> parentSamples, Avc1ToAvc3TrackImpl that)
            {
                this.that = that;
                this.parentSamples = parentSamples;
            }

            public override Sample get(int index)
            {
                Sample orignalSample = parentSamples[index];
                if (orignalSample.getSampleEntry().getType().Equals("avc1") && Arrays.binarySearch(that.getSyncSamples(), index + 1) >= 0)
                {

                    AvcConfigurationBox avcC = orignalSample.getSampleEntry().getBoxes<AvcConfigurationBox>(typeof(AvcConfigurationBox))[0];
                    int len = avcC.getLengthSizeMinusOne() + 1;
                    ByteBuffer buf = ByteBuffer.allocate(len);

                    SampleEntry se = that.avc1toavc3[orignalSample.getSampleEntry()];


                    return new CustomSample(buf, len, avcC, se, orignalSample);

                }
                else
                {
                    return orignalSample;
                }
            }

            public override int size()
            {
                return parentSamples.Count;
            }
        }
    }
}