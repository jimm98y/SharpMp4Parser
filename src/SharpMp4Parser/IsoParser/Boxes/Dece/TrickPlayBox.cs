using SharpMp4Parser.IsoParser.Support;
using SharpMp4Parser.IsoParser.Tools;
using SharpMp4Parser.Java;
using System.Collections.Generic;
using System.Text;

namespace SharpMp4Parser.IsoParser.Boxes.Dece
{
    /**
     * <h1>4cc = "{@value #TYPE}"</h1>
     * <pre>
     * aligned(8) class TrickPlayBox extends FullBox(‘trik’, version=0, flags=0)
     * {
     *  for (i=0; I &lt; sample_count; i++) {
     *   unsigned int(2) pic_type;
     *   unsigned int(6) dependency_level;
     *  }
     * }
     * </pre>
     */
    public class TrickPlayBox : AbstractFullBox
    {
        public const string TYPE = "trik";

        private List<Entry> entries = new List<Entry>();

        public TrickPlayBox() : base(TYPE)
        { }

        public List<Entry> getEntries()
        {
            return entries;
        }

        public void setEntries(List<Entry> entries)
        {
            this.entries = entries;
        }

        protected override long getContentSize()
        {
            return 4 + entries.Count;
        }

        protected override void _parseDetails(ByteBuffer content)
        {
            parseVersionAndFlags(content);
            while (content.remaining() > 0)
            {
                entries.Add(new Entry(IsoTypeReader.readUInt8(content)));
            }
        }

        protected override void getContent(ByteBuffer byteBuffer)
        {
            writeVersionAndFlags(byteBuffer);
            foreach (Entry entry in entries)
            {
                IsoTypeWriter.writeUInt8(byteBuffer, entry.value);
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("TrickPlayBox");
            sb.Append("{entries=").Append(entries);
            sb.Append('}');
            return sb.ToString();
        }

        public sealed class Entry
        {
            public int value;

            public Entry()
            {
            }

            public Entry(int value)
            {
                this.value = value;
            }

            public int getPicType()
            {
                return value >> 6 & 0x03;
            }

            public void setPicType(int picType)
            {
                value = value & 0xff >> 3;
                value = (picType & 0x03) << 6 | value;
            }

            public int getDependencyLevel()
            {
                return value & 0x3f;
            }

            public void setDependencyLevel(int dependencyLevel)
            {
                value = dependencyLevel & 0x3f | value;
            }

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("Entry");
                sb.Append("{picType=").Append(getPicType());
                sb.Append(",dependencyLevel=").Append(getDependencyLevel());
                sb.Append('}');
                return sb.ToString();
            }
        }
    }
}