using System;

namespace SharpMp4Parser.Java
{
    public static class DateTimeExtensions
    {
        public static long getTime(this DateTime date)
        {
            DateTime oldTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan diff = date.ToUniversalTime() - oldTime;
            return (long)Math.Floor(diff.TotalMilliseconds);
        }
    }
}
