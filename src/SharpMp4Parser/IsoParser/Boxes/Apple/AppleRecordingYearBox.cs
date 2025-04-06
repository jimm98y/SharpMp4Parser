using SharpMp4Parser.IsoParser.Tools;
using SharpMp4Parser.Java;
using System;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace SharpMp4Parser.IsoParser.Boxes.Apple
{
    /**
     * Created by sannies on 10/22/13.
     */
    public class AppleRecordingYearBox : AppleDataBox
    {
        DateTimeFormat df;

        DateTime date = DateTime.UtcNow;

        public AppleRecordingYearBox() : base("©day", 1)
        {
            df = new DateTimeFormat("yyyy-MM-dd'T'kk:mm:ssZ");
        }

        protected static string iso8601toRfc822Date(string iso8601)
        {
            iso8601 = Regex.Replace(iso8601, "Z$", "+0000");
            iso8601 = Regex.Replace(iso8601, "([0-9][0-9]):([0-9][0-9])$", "$1$2");
            return iso8601;
        }

        protected static string rfc822toIso8601Date(string rfc622)
        {
            rfc622 = Regex.Replace(rfc622, "\\+0000$", "Z");
            return rfc622;
        }

        public DateTime getDate()
        {
            return date;
        }

        public void setDate(DateTime date)
        {
            this.date = date;
        }

        protected override byte[] writeData()
        {
            return Utf8.convert(rfc822toIso8601Date(date.ToString(df.FormatProvider)));
        }

        protected override void parseData(ByteBuffer data)
        {
            string dateString = IsoTypeReader.readString(data, data.remaining());
            try
            {
                date = DateTime.Parse(iso8601toRfc822Date(dateString), df.FormatProvider);
            }
            catch (Exception)
            {
                throw;
            }
        }

        protected override int getDataLength()
        {
            return Utf8.convert(rfc822toIso8601Date(date.ToString(df.FormatProvider))).Length;
        }
    }
}