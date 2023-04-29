namespace SharpMp4Parser.Boxes.Apple
{
    /**
     * Created by sannies on 10/15/13.
     */
    public class AppleDiskNumberBox : AppleDataBox
    {
        int a;
        short b;

        public AppleDiskNumberBox() : base("disk", 0)
        { }

        public int getA()
        {
            return a;
        }

        public void setA(int a)
        {
            this.a = a;
        }

        public short getB()
        {
            return b;
        }

        public void setB(short b)
        {
            this.b = b;
        }

        protected override byte[] writeData()
        {
            ByteBuffer bb = ByteBuffer.allocate(6);
            bb.putInt(a);
            bb.putShort(b);
            return bb.array();
        }

        protected override void parseData(ByteBuffer data)
        {
            a = data.getInt();
            b = data.getShort();
        }

        protected override int getDataLength()
        {
            return 6;
        }
    }
}
