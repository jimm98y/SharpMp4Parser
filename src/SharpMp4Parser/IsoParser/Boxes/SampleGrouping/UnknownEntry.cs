using SharpMp4Parser.IsoParser.Tools;
using SharpMp4Parser.Java;

namespace SharpMp4Parser.IsoParser.Boxes.SampleGrouping
{
    /**
      *
      */
    public class UnknownEntry : GroupEntry
    {
        private ByteBuffer content;
        private string type;

        public UnknownEntry(string type)
        {
            this.type = type;
        }

        public override string getType()
        {
            return type;
        }

        public ByteBuffer getContent()
        {
            return content;
        }

        public void setContent(ByteBuffer content)
        {
            this.content = (ByteBuffer)content.duplicate().rewind();
        }

        public override void parse(ByteBuffer byteBuffer)
        {
            content = (ByteBuffer)byteBuffer.duplicate().rewind();
        }

        public override ByteBuffer get()
        {
            return (ByteBuffer)content.duplicate();
        }

        public override string ToString()
        {
            ByteBuffer bb = (ByteBuffer)content.duplicate();
            bb.rewind();
            byte[] b = new byte[bb.limit()];
            bb.get(b);
            return "UnknownEntry{" +
                    "content=" + Hex.encodeHex(b) +
                    '}';
        }

        public override bool Equals(object o)
        {
            if (this == o)
            {
                return true;
            }
            if (o == null || GetType() != o.GetType())
            {
                return false;
            }

            UnknownEntry that = (UnknownEntry)o;

            if (content != null ? !content.Equals(that.content) : that.content != null)
            {
                return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            return content != null ? content.GetHashCode() : 0;
        }
    }
}
