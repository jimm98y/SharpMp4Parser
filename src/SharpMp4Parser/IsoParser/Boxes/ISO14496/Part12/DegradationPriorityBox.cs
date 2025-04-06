using SharpMp4Parser.IsoParser.Support;
using SharpMp4Parser.IsoParser.Tools;
using SharpMp4Parser.Java;

namespace SharpMp4Parser.IsoParser.Boxes.ISO14496.Part12
{
    public class DegradationPriorityBox : AbstractFullBox
    {
        int[] priorities = new int[0];

        public DegradationPriorityBox() : base("stdp")
        { }

        protected override long getContentSize()
        {
            return 4 + priorities.Length * 2;
        }

        protected override void getContent(ByteBuffer byteBuffer)
        {
            writeVersionAndFlags(byteBuffer);
            foreach (int priority in priorities)
            {
                IsoTypeWriter.writeUInt16(byteBuffer, priority);
            }
        }

        protected override void _parseDetails(ByteBuffer content)
        {
            parseVersionAndFlags(content);
            priorities = new int[content.remaining() / 2];
            for (int i = 0; i < priorities.Length; i++)
            {
                priorities[i] = IsoTypeReader.readUInt16(content);
            }
        }

        public int[] getPriorities()
        {
            return priorities;
        }

        public void setPriorities(int[] priorities)
        {
            this.priorities = priorities;
        }

        /*
        aligned(8) class DegradationPriorityBox
         extends FullBox(‘stdp’, version = 0, 0) {
        int i;
         for (i=0; i < sample_count; i++) {
        unsigned int(16) priority;
        }
        }
         */
    }
}
