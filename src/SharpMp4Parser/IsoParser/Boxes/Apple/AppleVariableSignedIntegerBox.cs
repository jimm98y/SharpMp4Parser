using SharpMp4Parser.IsoParser.Tools;
using SharpMp4Parser.Java;

namespace SharpMp4Parser.IsoParser.Boxes.Apple
{
    /**
     * Created by sannies on 10/22/13.
     */
    public abstract class AppleVariableSignedIntegerBox : AppleDataBox
    {
        long value;
        int intLength = 1;

        protected AppleVariableSignedIntegerBox(string type) : base(type, 15)
        { }

        public int getIntLength()
        {
            return intLength;
        }

        public void setIntLength(int intLength)
        {
            this.intLength = intLength;
        }

        public long getValue()
        {
            //patched by Tobias Bley / UltraMixer (04/25/2014)
            if (!IsParsed())
            {
                parseDetails();
            }
            return value;
        }

        public void setValue(long value)
        {

            if (value <= 127 && value > -128)
            {
                intLength = 1;
            }
            else if (value <= 32767 && value > -32768 && intLength < 2)
            {
                intLength = 2;
            }
            else if (value <= 8388607 && value > -8388608 && intLength < 3)
            {
                intLength = 3;
            }
            else
            {
                intLength = 4;
            }

            this.value = value;
        }

        protected override byte[] writeData()
        {
            int dLength = getDataLength();
            ByteBuffer b = ByteBuffer.wrap(new byte[dLength]);
            IsoTypeWriterVariable.write(value, b, dLength);
            return b.array();
        }

        protected override void parseData(ByteBuffer data)
        {
            int intLength = data.remaining();
            value = IsoTypeReaderVariable.read(data, intLength);
            this.intLength = intLength;
        }

        protected override int getDataLength()
        {
            return intLength;
        }
    }
}