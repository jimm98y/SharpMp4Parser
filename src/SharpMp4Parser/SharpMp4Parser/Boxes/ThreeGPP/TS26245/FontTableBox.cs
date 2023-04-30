using System.Collections.Generic;

namespace SharpMp4Parser.Boxes.ThreeGPP.TS26245
{
    /**
     * <h1>4cc = "{@value #TYPE}"</h1>
     */
    public class FontTableBox : AbstractBox
    {
        public const string TYPE = "ftab";

        List<FontRecord> entries = new List<FontRecord>();

        public FontTableBox() : base("ftab")
        { }

        protected override long getContentSize()
        {
            int size = 2;
            foreach (FontRecord fontRecord in entries)
            {
                size += fontRecord.getSize();
            }
            return size;
        }


        public override void _parseDetails(ByteBuffer content)
        {
            int numberOfRecords = IsoTypeReader.readUInt16(content);
            for (int i = 0; i < numberOfRecords; i++)
            {
                FontRecord fr = new FontRecord();
                fr.parse(content);
                entries.Add(fr);
            }
        }

        protected override void getContent(ByteBuffer byteBuffer)
        {
            IsoTypeWriter.writeUInt16(byteBuffer, entries.size());
            foreach (FontRecord record in entries)
            {
                record.getContent(byteBuffer);
            }
        }

        public List<FontRecord> getEntries()
        {
            return entries;
        }

        public void setEntries(List<FontRecord> entries)
        {
            this.entries = entries;
        }

        public sealed class FontRecord
        {
            int fontId;
            string fontname;

            public FontRecord()
            {
            }

            public FontRecord(int fontId, string fontname)
            {
                this.fontId = fontId;
                this.fontname = fontname;
            }

            public void parse(ByteBuffer bb)
            {
                fontId = IsoTypeReader.readUInt16(bb);
                int length = IsoTypeReader.readUInt8(bb);
                fontname = IsoTypeReader.readString(bb, length);
            }

            public void getContent(ByteBuffer bb)
            {
                IsoTypeWriter.writeUInt16(bb, fontId);
                IsoTypeWriter.writeUInt8(bb, fontname.length());
                bb.put(Utf8.convert(fontname));
            }

            public int getSize()
            {
                return Utf8.utf8StringLengthInBytes(fontname) + 3;
            }

            public override string toString()
            {
                return "FontRecord{" +
                        "fontId=" + fontId +
                        ", fontname='" + fontname + '\'' +
                        '}';
            }
        }
    }
}
