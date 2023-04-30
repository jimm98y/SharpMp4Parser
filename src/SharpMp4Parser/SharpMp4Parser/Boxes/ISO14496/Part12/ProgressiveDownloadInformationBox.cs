using SharpMp4Parser.Java;
using SharpMp4Parser.Support;
using SharpMp4Parser.Tools;
using System.Collections.Generic;

namespace SharpMp4Parser.Boxes.ISO14496.Part12
{
    /**
     * <h1>4cc = "{@value #TYPE}"</h1>
     */
    public class ProgressiveDownloadInformationBox : AbstractFullBox
    {
        public const string TYPE = "pdin";

        List<Entry> entries = new List<Entry>();

        public ProgressiveDownloadInformationBox() : base(TYPE)
        { }

        protected override long getContentSize()
        {
            return 4 + entries.Count * 8;
        }

        protected override void getContent(ByteBuffer byteBuffer)
        {
            writeVersionAndFlags(byteBuffer);
            foreach (Entry entry in entries)
            {
                IsoTypeWriter.writeUInt32(byteBuffer, entry.getRate());
                IsoTypeWriter.writeUInt32(byteBuffer, entry.getInitialDelay());
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

        protected override void _parseDetails(ByteBuffer content)
        {
            parseVersionAndFlags(content);
            entries = new List<Entry>();
            while (content.remaining() >= 8)
            {
                Entry entry = new Entry(IsoTypeReader.readUInt32(content), IsoTypeReader.readUInt32(content));
                entries.Add(entry);
            }
        }

        public override string ToString()
        {
            return "ProgressiveDownloadInfoBox{" +
                    "entries=" + entries +
                    '}';
        }

        public sealed class Entry
        {
            long rate;
            long initialDelay;

            public Entry(long rate, long initialDelay)
            {
                this.rate = rate;
                this.initialDelay = initialDelay;
            }

            public long getRate()
            {
                return rate;
            }

            public void setRate(long rate)
            {
                this.rate = rate;
            }

            public long getInitialDelay()
            {
                return initialDelay;
            }

            public void setInitialDelay(long initialDelay)
            {
                this.initialDelay = initialDelay;
            }

            public override string ToString()
            {
                return "Entry{" +
                        "rate=" + rate +
                        ", initialDelay=" + initialDelay +
                        '}';
            }

            public override bool Equals(object o)
            {
                if (this == o) return true;
                if (o == null || GetType() != o.GetType()) return false;

                Entry entry = (Entry)o;

                if (initialDelay != entry.initialDelay) return false;
                if (rate != entry.rate) return false;

                return true;
            }

            public override int GetHashCode()
            {
                int result = (int)(rate ^ (long)((ulong)rate >> 32));
                result = 31 * result + (int)(initialDelay ^ (long)((ulong)initialDelay >> 32));
                return result;
            }
        }
    }
}
