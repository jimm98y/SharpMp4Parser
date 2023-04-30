using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part1.ObjectDescriptors;
using SharpMp4Parser.IsoParser.Support;
using SharpMp4Parser.Java;
using System.Collections.Generic;

namespace SharpMp4Parser.IsoParser.Boxes.Dolby
{
    /**
     * <h1>4cc = "{@value #TYPE}"</h1>
     */
    public class EC3SpecificBox : AbstractBox
    {
        public const string TYPE = "dec3";
        List<Entry> entries = new List<Entry>();
        int dataRate;
        int numIndSub;

        public EC3SpecificBox() : base(TYPE)
        { }

        protected override long getContentSize()
        {
            long size = 2;
            foreach (Entry entry in entries)
            {
                if (entry.num_dep_sub > 0)
                {
                    size += 4;
                }
                else
                {
                    size += 3;
                }
            }
            return size;
        }

        protected override void _parseDetails(ByteBuffer content)
        {
            BitReaderBuffer brb = new BitReaderBuffer(content);
            dataRate = brb.readBits(13);
            numIndSub = brb.readBits(3) + 1;
            // This field indicates the number of independent substreams that are present in the Enhanced AC-3 bitstream. The value
            // of this field is one less than the number of independent substreams present.


            for (int i = 0; i < numIndSub; i++)
            {
                Entry e = new Entry();
                e.fscod = brb.readBits(2);
                e.bsid = brb.readBits(5);
                e.bsmod = brb.readBits(5);
                e.acmod = brb.readBits(3);
                e.lfeon = brb.readBits(1);
                e.reserved = brb.readBits(3);
                e.num_dep_sub = brb.readBits(4);
                if (e.num_dep_sub > 0)
                {
                    e.chan_loc = brb.readBits(9);
                }
                else
                {
                    e.reserved2 = brb.readBits(1);
                }
                entries.Add(e);
            }
        }

        protected override void getContent(ByteBuffer byteBuffer)
        {
            BitWriterBuffer bwb = new BitWriterBuffer(byteBuffer);
            bwb.writeBits(dataRate, 13);
            bwb.writeBits(entries.Count - 1, 3);
            foreach (Entry e in entries)
            {
                bwb.writeBits(e.fscod, 2);
                bwb.writeBits(e.bsid, 5);
                bwb.writeBits(e.bsmod, 5);
                bwb.writeBits(e.acmod, 3);
                bwb.writeBits(e.lfeon, 1);
                bwb.writeBits(e.reserved, 3);
                bwb.writeBits(e.num_dep_sub, 4);
                if (e.num_dep_sub > 0)
                {
                    bwb.writeBits(e.chan_loc, 9);
                }
                else
                {
                    bwb.writeBits(e.reserved2, 1);
                }
            }
        }

        public List<Entry> getEntries()
        {
            return entries;
        }

        public void setEntries(List<Entry> entries)
        {
            this.entries = entries;
        }

        public void addEntry(Entry entry)
        {
            entries.Add(entry);
        }

        public int getDataRate()
        {
            return dataRate;
        }

        public void setDataRate(int dataRate)
        {
            this.dataRate = dataRate;
        }

        public int getNumIndSub()
        {
            return numIndSub;
        }

        public void setNumIndSub(int numIndSub)
        {
            this.numIndSub = numIndSub;
        }

        public sealed class Entry
        {
            public int fscod;
            public int bsid;
            public int bsmod;
            public int acmod;
            public int lfeon;
            public int reserved;
            public int num_dep_sub;
            public int chan_loc;
            public int reserved2;

            public override string ToString()
            {
                return "Entry{" +
                        "fscod=" + fscod +
                        ", bsid=" + bsid +
                        ", bsmod=" + bsmod +
                        ", acmod=" + acmod +
                        ", lfeon=" + lfeon +
                        ", reserved=" + reserved +
                        ", num_dep_sub=" + num_dep_sub +
                        ", chan_loc=" + chan_loc +
                        ", reserved2=" + reserved2 +
                        '}';
            }
        }
    }
}