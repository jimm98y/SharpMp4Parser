using SharpMp4Parser.Java;

namespace SharpMp4Parser.IsoParser.Boxes.Apple
{
    /**
     * Created by sannies on 10/15/13.
     */
    public class AppleTrackNumberBox : AppleDataBox
    {
        int a;
        int b;

        public AppleTrackNumberBox() : base("trkn", 0)
        { }

        public int getA()
        {
            return a;
        }

        public void setA(int a)
        {
            this.a = a;
        }

        public int getB()
        {
            return b;
        }

        public void setB(int b)
        {
            this.b = b;
        }

        protected override byte[] writeData()
        {
            ByteBuffer bb = ByteBuffer.allocate(8);
            bb.putInt(a);
            bb.putInt(b);
            return bb.array();
        }

        protected override void parseData(ByteBuffer data)
        {
            a = data.getInt();
            b = data.getInt();
        }

        protected override int getDataLength()
        {
            return 8;
        }
    }
}