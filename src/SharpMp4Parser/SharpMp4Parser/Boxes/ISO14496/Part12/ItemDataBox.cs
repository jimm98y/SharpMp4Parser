using System;

namespace SharpMp4Parser.Boxes.ISO14496.Part12
{
    /**
     * <h1>4cc = "{@value #TYPE}"</h1>
     */
    public class ItemDataBox : AbstractBox
    {
        public const string TYPE = "idat";
        ByteBuffer data = ByteBuffer.allocate(0);

        public ItemDataBox() : base(TYPE)
        { }

        public ByteBuffer getData()
        {
            return data;
        }

        public void setData(ByteBuffer data)
        {
            this.data = data;
        }

        protected override long getContentSize()
        {
            return data.limit();
        }

        public override void _parseDetails(ByteBuffer content)
        {
            data = content.slice();
            ((Buffer)content).position(content.position() + content.remaining());
        }

        protected override void getContent(ByteBuffer byteBuffer)
        {
            byteBuffer.put(data);
        }
    }
}
