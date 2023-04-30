using SharpMp4Parser.Java;
using SharpMp4Parser.Support;
using SharpMp4Parser.Tools;
using System.Collections.Generic;

namespace SharpMp4Parser.Boxes.ISO14496.Part12
{
    /**
     * <h1>4cc = "{@value #TYPE}"</h1>
     * <pre>
     * aligned(8) class SubSampleInformationBox extends FullBox('subs', version, 0) {
     *  unsigned int(32) entry_count;
     *  int i,j;
     *  for (i=0; i &lt; entry_count; i++) {
     *   unsigned int(32) sample_delta;
     *   unsigned int(16) subsample_count;
     *   if (subsample_count &gt; 0) {
     *    for (j=0; j &lt; subsample_count; j++) {
     *     if(version == 1)
     *     {
     *      unsigned int(32) subsample_size;
     *     }
     *     else
     *     {
     *      unsigned int(16) subsample_size;
     *     }
     *     unsigned int(8) subsample_priority;
     *     unsigned int(8) discardable;
     *     unsigned int(32) reserved = 0;
     *    }
     *   }
     *  }
     * }
     * </pre>
     */
    public class SubSampleInformationBox : AbstractFullBox
    {
        public const string TYPE = "subs";

        private List<SubSampleEntry> entries = new List<SubSampleEntry>();

        public SubSampleInformationBox() : base(TYPE)
        { }

        public List<SubSampleEntry> getEntries()
        {
            return entries;
        }

        public void setEntries(List<SubSampleEntry> entries)
        {
            this.entries = entries;
        }

        protected override long getContentSize()
        {
            long size = 8;

            foreach (SubSampleEntry entry in entries)
            {
                size += 4;
                size += 2;
                for (int j = 0; j < entry.getSubsampleEntries().Count; j++)
                {

                    if (getVersion() == 1)
                    {
                        size += 4;
                    }
                    else
                    {
                        size += 2;
                    }
                    size += 2;
                    size += 4;
                }
            }
            return size;
        }

        protected override void _parseDetails(ByteBuffer content)
        {
            parseVersionAndFlags(content);

            long entryCount = IsoTypeReader.readUInt32(content);

            for (int i = 0; i < entryCount; i++)
            {
                SubSampleEntry SubSampleEntry = new SubSampleEntry();
                SubSampleEntry.setSampleDelta(IsoTypeReader.readUInt32(content));
                int subsampleCount = IsoTypeReader.readUInt16(content);
                for (int j = 0; j < subsampleCount; j++)
                {
                    SubSampleEntry.SubsampleEntry subsampleEntry = new SubSampleEntry.SubsampleEntry();
                    subsampleEntry.setSubsampleSize(getVersion() == 1 ? IsoTypeReader.readUInt32(content) : IsoTypeReader.readUInt16(content));
                    subsampleEntry.setSubsamplePriority(IsoTypeReader.readUInt8(content));
                    subsampleEntry.setDiscardable(IsoTypeReader.readUInt8(content));
                    subsampleEntry.setReserved(IsoTypeReader.readUInt32(content));
                    SubSampleEntry.getSubsampleEntries().Add(subsampleEntry);
                }
                entries.Add(SubSampleEntry);
            }
        }

        protected override void getContent(ByteBuffer byteBuffer)
        {
            writeVersionAndFlags(byteBuffer);
            IsoTypeWriter.writeUInt32(byteBuffer, entries.Count);
            foreach (SubSampleEntry subSampleEntry in entries)
            {
                IsoTypeWriter.writeUInt32(byteBuffer, subSampleEntry.getSampleDelta());
                IsoTypeWriter.writeUInt16(byteBuffer, subSampleEntry.getSubsampleCount());
                List<SubSampleEntry.SubsampleEntry> subsampleEntries = subSampleEntry.getSubsampleEntries();
                foreach (SubSampleEntry.SubsampleEntry subsampleEntry in subsampleEntries)
                {
                    if (getVersion() == 1)
                    {
                        IsoTypeWriter.writeUInt32(byteBuffer, subsampleEntry.getSubsampleSize());
                    }
                    else
                    {
                        IsoTypeWriter.writeUInt16(byteBuffer, CastUtils.l2i(subsampleEntry.getSubsampleSize()));
                    }
                    IsoTypeWriter.writeUInt8(byteBuffer, subsampleEntry.getSubsamplePriority());
                    IsoTypeWriter.writeUInt8(byteBuffer, subsampleEntry.getDiscardable());
                    IsoTypeWriter.writeUInt32(byteBuffer, subsampleEntry.getReserved());
                }
            }
        }

        public override string ToString()
        {
            return "SubSampleInformationBox{" +
                    "entryCount=" + entries.Count +
                    ", entries=" + entries +
                    '}';
        }

        public sealed class SubSampleEntry
        {
            private long sampleDelta;
            private List<SubsampleEntry> subsampleEntries = new List<SubsampleEntry>();

            public long getSampleDelta()
            {
                return sampleDelta;
            }

            public void setSampleDelta(long sampleDelta)
            {
                this.sampleDelta = sampleDelta;
            }

            public int getSubsampleCount()
            {
                return subsampleEntries.Count;
            }

            public List<SubsampleEntry> getSubsampleEntries()
            {
                return subsampleEntries;
            }

            public override string ToString()
            {
                return "SampleEntry{" +
                        "sampleDelta=" + sampleDelta +
                        ", subsampleCount=" + subsampleEntries.Count +
                        ", subsampleEntries=" + subsampleEntries +
                        '}';
            }

            public sealed class SubsampleEntry
            {
                private long subsampleSize;
                private int subsamplePriority;
                private int discardable;
                private long reserved;

                public long getSubsampleSize()
                {
                    return subsampleSize;
                }

                public void setSubsampleSize(long subsampleSize)
                {
                    this.subsampleSize = subsampleSize;
                }

                public int getSubsamplePriority()
                {
                    return subsamplePriority;
                }

                public void setSubsamplePriority(int subsamplePriority)
                {
                    this.subsamplePriority = subsamplePriority;
                }

                public int getDiscardable()
                {
                    return discardable;
                }

                public void setDiscardable(int discardable)
                {
                    this.discardable = discardable;
                }

                public long getReserved()
                {
                    return reserved;
                }

                public void setReserved(long reserved)
                {
                    this.reserved = reserved;
                }

                public override string ToString()
                {
                    return "SubsampleEntry{" +
                            "subsampleSize=" + subsampleSize +
                            ", subsamplePriority=" + subsamplePriority +
                            ", discardable=" + discardable +
                            ", reserved=" + reserved +
                            '}';
                }
            }
        }
    }
}