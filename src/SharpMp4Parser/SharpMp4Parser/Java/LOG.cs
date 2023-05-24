using System;
using System.Diagnostics;

namespace SharpMp4Parser.Java
{
    public static class LOG
    {
        public static void warn(string message, Exception ex = null)
        {
            SinkWarn(message, ex);
        }

        public static void error(string message, Exception ex = null)
        {
            SinkError(message, ex);
        }

        public static void trace(string message, Exception ex = null)
        {
            SinkTrace(message, ex);
        }

        public static void debug(string message, Exception ex = null)
        {
            SinkDebug(message, ex);
        }

        public static void finest(string message, Exception ex = null)
        {
            SinkFinest(message, ex);
        }

        public static void info(string message, Exception ex = null)
        {
            SinkInfo(message, ex);
        }

        public static Action<string, Exception> SinkWarn = new Action<string, Exception>((m, ex) => { Debug.WriteLine(m); });
        public static Action<string, Exception> SinkError = new Action<string, Exception>((m, ex) => { Debug.WriteLine(m); });
        public static Action<string, Exception> SinkTrace = new Action<string, Exception>((m, ex) => { });
        public static Action<string, Exception> SinkDebug = new Action<string, Exception>((m, ex) => { });
        public static Action<string, Exception> SinkFinest = new Action<string, Exception>((m, ex) => { });
        public static Action<string, Exception> SinkInfo = new Action<string, Exception>((m, ex) => { });
    }
}
