using SharpMp4Parser.IsoParser.Support;
using SharpMp4Parser.IsoParser.Tools;
using SharpMp4Parser.Java;

namespace SharpMp4Parser.IsoParser.Boxes.ThreeGPP.TS26244
{
    /**
     * <h1>4cc = "{@value #TYPE}"</h1>
     * Location Information Box as specified in TS 26.244.
     */
    public class LocationInformationBox : AbstractFullBox
    {
        public const string TYPE = "loci";

        private string language;
        private string name = "";
        private int role;
        private double longitude;
        private double latitude;
        private double altitude;
        private string astronomicalBody = "";
        private string additionalNotes = "";

        public LocationInformationBox() : base(TYPE)
        { }

        public string getLanguage()
        {
            return language;
        }

        public void setLanguage(string language)
        {
            this.language = language;
        }

        public string getName()
        {
            return name;
        }

        public void setName(string name)
        {
            this.name = name;
        }

        public int getRole()
        {
            return role;
        }

        public void setRole(int role)
        {
            this.role = role;
        }

        public double getLongitude()
        {
            return longitude;
        }

        public void setLongitude(double longitude)
        {
            this.longitude = longitude;
        }

        public double getLatitude()
        {
            return latitude;
        }

        public void setLatitude(double latitude)
        {
            this.latitude = latitude;
        }

        public double getAltitude()
        {
            return altitude;
        }

        public void setAltitude(double altitude)
        {
            this.altitude = altitude;
        }

        public string getAstronomicalBody()
        {
            return astronomicalBody;
        }

        public void setAstronomicalBody(string astronomicalBody)
        {
            this.astronomicalBody = astronomicalBody;
        }

        public string getAdditionalNotes()
        {
            return additionalNotes;
        }

        public void setAdditionalNotes(string additionalNotes)
        {
            this.additionalNotes = additionalNotes;
        }

        protected override long getContentSize()
        {
            return 22 + Utf8.convert(name).Length + Utf8.convert(astronomicalBody).Length + Utf8.convert(additionalNotes).Length;
        }

        protected override void _parseDetails(ByteBuffer content)
        {
            parseVersionAndFlags(content);
            language = IsoTypeReader.readIso639(content);
            name = IsoTypeReader.readString(content);
            role = IsoTypeReader.readUInt8(content);
            longitude = IsoTypeReader.readFixedPoint1616(content);
            latitude = IsoTypeReader.readFixedPoint1616(content);
            altitude = IsoTypeReader.readFixedPoint1616(content);
            astronomicalBody = IsoTypeReader.readString(content);
            additionalNotes = IsoTypeReader.readString(content);
        }


        protected override void getContent(ByteBuffer byteBuffer)
        {
            writeVersionAndFlags(byteBuffer);
            IsoTypeWriter.writeIso639(byteBuffer, language);
            byteBuffer.put(Utf8.convert(name));
            byteBuffer.put(0);
            IsoTypeWriter.writeUInt8(byteBuffer, role);
            IsoTypeWriter.writeFixedPoint1616(byteBuffer, longitude);
            IsoTypeWriter.writeFixedPoint1616(byteBuffer, latitude);
            IsoTypeWriter.writeFixedPoint1616(byteBuffer, altitude);
            byteBuffer.put(Utf8.convert(astronomicalBody));
            byteBuffer.put(0);
            byteBuffer.put(Utf8.convert(additionalNotes));
            byteBuffer.put(0);
        }
    }
}