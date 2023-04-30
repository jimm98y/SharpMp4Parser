using SharpMp4Parser.IsoParser.Support;
using SharpMp4Parser.IsoParser.Tools;
using SharpMp4Parser.Java;

namespace SharpMp4Parser.IsoParser.Boxes.Dolby
{
    public class DoViConfigurationBox : AbstractBox
    {
        public const string TYPE = "dvcC";

        private int dvVersionMajor;
        private int dvVersionMinor;
        private int dvProfile;
        private int dvLevel;
        private bool rpuPresentFlag;
        private bool elPresentFlag;
        private bool blPresentFlag;
        private long reserved1;
        private long reserved2;
        private long reserved3;
        private long reserved4;
        private long reserved5;

        public DoViConfigurationBox() : base(TYPE)
        { }

        protected override long getContentSize()
        {
            return 24;
        }

        protected override void getContent(ByteBuffer byteBuffer)
        {
            IsoTypeWriter.writeUInt8(byteBuffer, dvVersionMajor);
            IsoTypeWriter.writeUInt8(byteBuffer, dvVersionMinor);

            int x = 0;
            x += (dvProfile & 127) << 9;
            x += (dvProfile & 63) << 3;
            x += rpuPresentFlag ? 0x4 : 0;
            x += elPresentFlag ? 0x2 : 0;
            x += blPresentFlag ? 0x1 : 0;

            IsoTypeWriter.writeUInt16(byteBuffer, x);
            IsoTypeWriter.writeUInt32(byteBuffer, reserved1);
            IsoTypeWriter.writeUInt32(byteBuffer, reserved2);
            IsoTypeWriter.writeUInt32(byteBuffer, reserved3);
            IsoTypeWriter.writeUInt32(byteBuffer, reserved4);
            IsoTypeWriter.writeUInt32(byteBuffer, reserved5);
        }

        protected override void _parseDetails(ByteBuffer content)
        {
            dvVersionMajor = IsoTypeReader.readUInt8(content);
            dvVersionMinor = IsoTypeReader.readUInt8(content);
            int x = IsoTypeReader.readUInt16(content);
            dvProfile = x >> 9 & 127;
            dvLevel = x >> 3 & 63;
            rpuPresentFlag = (x & 0x4) > 0;
            elPresentFlag = (x & 0x2) > 0;
            blPresentFlag = (x & 0x1) > 0;
            reserved1 = IsoTypeReader.readUInt32(content);
            reserved2 = IsoTypeReader.readUInt32(content);
            reserved3 = IsoTypeReader.readUInt32(content);
            reserved4 = IsoTypeReader.readUInt32(content);
            reserved5 = IsoTypeReader.readUInt32(content);
        }

        public int getDvVersionMajor()
        {
            return dvVersionMajor;
        }

        public void setDvVersionMajor(int dvVersionMajor)
        {
            this.dvVersionMajor = dvVersionMajor;
        }

        public int getDvVersionMinor()
        {
            return dvVersionMinor;
        }

        public void setDvVersionMinor(int dvVersionMinor)
        {
            this.dvVersionMinor = dvVersionMinor;
        }

        public int getDvProfile()
        {
            return dvProfile;
        }

        public void setDvProfile(int dvProfile)
        {
            this.dvProfile = dvProfile;
        }

        public int getDvLevel()
        {
            return dvLevel;
        }

        public void setDvLevel(int dvLevel)
        {
            this.dvLevel = dvLevel;
        }

        public bool isRpuPresentFlag()
        {
            return rpuPresentFlag;
        }

        public void setRpuPresentFlag(bool rpuPresentFlag)
        {
            this.rpuPresentFlag = rpuPresentFlag;
        }

        public bool isElPresentFlag()
        {
            return elPresentFlag;
        }

        public void setElPresentFlag(bool elPresentFlag)
        {
            this.elPresentFlag = elPresentFlag;
        }

        public bool isBlPresentFlag()
        {
            return blPresentFlag;
        }

        public void setBlPresentFlag(bool blPresentFlag)
        {
            this.blPresentFlag = blPresentFlag;
        }

        public long getReserved1()
        {
            return reserved1;
        }

        public void setReserved1(long reserved1)
        {
            this.reserved1 = reserved1;
        }

        public long getReserved2()
        {
            return reserved2;
        }

        public void setReserved2(long reserved2)
        {
            this.reserved2 = reserved2;
        }

        public long getReserved3()
        {
            return reserved3;
        }

        public void setReserved3(long reserved3)
        {
            this.reserved3 = reserved3;
        }

        public long getReserved4()
        {
            return reserved4;
        }

        public void setReserved4(long reserved4)
        {
            this.reserved4 = reserved4;
        }

        public long getReserved5()
        {
            return reserved5;
        }

        public void setReserved5(long reserved5)
        {
            this.reserved5 = reserved5;
        }
    }
}