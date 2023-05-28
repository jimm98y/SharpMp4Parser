using SharpMp4Parser.Java;

namespace SharpMp4Parser.Tests.Streaming.Input
{
    public sealed class NalStreamTokenizer
    {
        //private static Logger LOG = LoggerFactory.getLogger(typeof(NalStreamTokenizer).getName());
        MyByteArrayOutputStream next;
        int pattern = 0;
        private ByteStream inputStream;

        public NalStreamTokenizer(ByteStream inputStream)
        {
            this.inputStream = inputStream;
            this.next = new MyByteArrayOutputStream();
        }

        public byte[] getNext()
        {
            //System.err.println("getNext() called");
            //if (LOG.isDebugEnabled()) {
            //    LOG.debug("getNext() called");
            //}
            int c;


            while ((c = inputStream.read()) != -1)
            {
                if (!(pattern == 2 && c == 3))
                {
                    next.write(c);


                    if (pattern == 0 && c == 0)
                    {
                        pattern = 1;
                    }
                    else if (pattern == 1 && c == 0)
                    {
                        pattern = 2;
                    }
                    else if (pattern == 2 && c == 0)
                    {
                        byte[] sss = next.toByteArrayLess3();
                        next.reset();
                        if (sss != null)
                        {
                            return sss;
                        }
                    }
                    else if (pattern == 2 && c == 1)
                    {
                        byte[] sss = next.toByteArrayLess3();
                        next.reset();
                        pattern = 0;
                        if (sss != null)
                        {
                            return sss;
                        }
                    }
                    else if (pattern != 0)
                    {
                        pattern = 0;
                    }
                }
                else
                {
                    pattern = 0;
                }
            }
            byte[] s = next.toByteArray();
            next.reset();
            if (s.Length > 0)
            {
                return s;
            }
            else
            {
                return null;
            }
        }
    }
}
