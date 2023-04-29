using System;

namespace SharpMp4Parser.Boxes.Apple
{
    /**
     * Created by sannies on 10/22/13.
     */
    public class AppleRecordingYearBox : AppleDataBox
    {
        DateFormat df;

        Date date = new Date();

        public AppleRecordingYearBox() : base("©day", 1)
        {
            df = new SimpleDateFormat("yyyy-MM-dd'T'kk:mm:ssZ");
            df.setTimeZone(TimeZone.getTimeZone("UTC"));
        }

        protected static string iso8601toRfc822Date(string iso8601)
        {
            iso8601 = iso8601.replaceAll("Z$", "+0000");
            iso8601 = iso8601.replaceAll("([0-9][0-9]):([0-9][0-9])$", "$1$2");
            return iso8601;
        }

        protected static string rfc822toIso8601Date(string rfc622)
        {
            rfc622 = rfc622.replaceAll("\\+0000$", "Z");
            return rfc622;
        }

        public Date getDate()
        {
            return date;
        }

        public void setDate(Date date)
        {
            this.date = date;
        }

        protected override byte[] writeData()
        {

            return Utf8.convert(rfc822toIso8601Date(df.format(date)));
        }

        protected override void parseData(ByteBuffer data)
        {
            string dateString = IsoTypeReader.readString(data, data.remaining());
            try
            {
                date = df.parse(iso8601toRfc822Date(dateString));
            }
            catch (ParseException e)
            {
                throw new RuntimeException(e);
            }
        }

        protected override int getDataLength()
        {
            return Utf8.convert(rfc822toIso8601Date(df.format(date))).length;
        }
    }
}