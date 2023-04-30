using SharpMp4Parser.Java;
using SharpMp4Parser.Support;
using SharpMp4Parser.Tools;
using System.Collections.Generic;
using System.Diagnostics;

namespace SharpMp4Parser.Boxes.ISO14496.Part12
{
    /**
     * <h1>4cc = "{@value #TYPE}"</h1>
     * <pre>
     * aligned(8) class CompositionOffsetBox extends FullBox(‘ctts’, version = 0, 0) {
     *  unsigned int(32) entry_count;
     *  int i;
     *  if (version==0) {
     *   for (i=0; i &lt; entry_count; i++) {
     *    unsigned int(32) sample_count;
     *    unsigned int(32) sample_offset;
     *   }
     *  }
     *  else if (version == 1) {
     *   for (i=0; i &lt; entry_count; i++) {
     *    unsigned int(32) sample_count;
     *    signed int(32) sample_offset;
     *   }
     *  }
     * }
     * </pre>
     * <p>
     * This box provides the offset between decoding time and composition time.
     * In version 0 of this box the decoding time must be less than the composition time, and
     * the offsets are expressed as unsigned numbers such that</p>
     * <p>CT(n) = DT(n) + CTTS(n) where CTTS(n) is the (uncompressed) table entry for sample n.</p>
     * <p>In version 1 of this box, the composition timeline and the decoding timeline are
     * still derived from each other, but the offsets are signed.
     * It is recommended that for the computed composition timestamps, there is
     * exactly one with the value 0 (zero).</p>
     */
    public class CompositionTimeToSample : AbstractFullBox
    {
        public const string TYPE = "ctts";

        List<Entry> entries = new List<Entry>();

        public CompositionTimeToSample() : base(TYPE)
        { }

        /**
         * Decompresses the list of entries and returns the list of composition times.
         *
         * @param entries composition time to sample entries in compressed form
         * @return decoding time per sample
         */
        public static int[] blowupCompositionTimes(List<CompositionTimeToSample.Entry> entries)
        {
            long numOfSamples = 0;
            foreach (CompositionTimeToSample.Entry entry in entries)
            {
                numOfSamples += entry.getCount();
            }

            Debug.Assert(numOfSamples <= int.MaxValue);
            int[] decodingTime = new int[(int)numOfSamples];

            int current = 0;


            foreach (CompositionTimeToSample.Entry entry in entries)
            {
                for (int i = 0; i < entry.getCount(); i++)
                {
                    decodingTime[current++] = entry.getOffset();
                }
            }

            return decodingTime;
        }

        protected override long getContentSize()
        {
            return 8 + 8 * entries.Count;
        }

        public List<Entry> getEntries()
        {
            return entries;
        }

        public void setEntries(List<Entry> entries)
        {
            this.entries = entries;
        }

        protected override void _parseDetails(ByteBuffer content)
        {
            parseVersionAndFlags(content);
            int numberOfEntries = CastUtils.l2i(IsoTypeReader.readUInt32(content));
            entries = new List<Entry>(numberOfEntries);
            for (int i = 0; i < numberOfEntries; i++)
            {
                Entry e = new Entry(CastUtils.l2i(IsoTypeReader.readUInt32(content)), content.getInt());
                entries.Add(e);
            }
        }

        protected override void getContent(ByteBuffer byteBuffer)
        {
            writeVersionAndFlags(byteBuffer);
            IsoTypeWriter.writeUInt32(byteBuffer, entries.Count);

            foreach (Entry entry in entries)
            {
                IsoTypeWriter.writeUInt32(byteBuffer, entry.getCount());
                byteBuffer.putInt(entry.getOffset());
            }

        }

        public sealed class Entry
        {
            int count;
            int offset;

            public Entry(int count, int offset)
            {
                this.count = count;
                this.offset = offset;
            }

            public int getCount()
            {
                return count;
            }

            public void setCount(int count)
            {
                this.count = count;
            }

            public int getOffset()
            {
                return offset;
            }

            public void setOffset(int offset)
            {
                this.offset = offset;
            }

            public override string ToString()
            {
                return "Entry{" +
                        "count=" + count +
                        ", offset=" + offset +
                        '}';
            }
        }
    }
}