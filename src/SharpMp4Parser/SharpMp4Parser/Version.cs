namespace SharpMp4Parser
{
    /**
      * The classic version object.
      */
    public class Version
    {
        public static readonly string VERSION;
        //private static final Logger LOG = LoggerFactory.getLogger(Version.class);

        static Version()
        {
            //LineNumberReader lnr = new LineNumberReader(new InputStreamReader(Version.class.getResourceAsStream("/version2.txt")));
            //String version;
            //try {
            //    version = lnr.readLine();
            //} catch (IOException e) {
            //    LOG.warn(e.getMessage());
            //    version = "unknown";
            //}
            VERSION = "1.0.0";
        }
    }
}
