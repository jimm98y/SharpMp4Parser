using SharpMp4Parser.Java;
using SharpMp4Parser.Streaming.Extensions;
using System.Diagnostics;
using System.Linq;

namespace SharpMp4Parser.Streaming.Input.H264
{
    /**
     * Reads H264 data from an Annex B InputStream.
     */
    public class H264AnnexBTrack : H264NalConsumingTrack /*, Callable<Void> */
    {

        private ByteStream inputStream;

        public H264AnnexBTrack(ByteStream inputStream)
        {
            Debug.Assert(inputStream != null);
            this.inputStream = new ByteStream(inputStream); // BufferedInputStream
        }


        public void call()
        {
            byte[] nal;
            NalStreamTokenizer st = new NalStreamTokenizer(inputStream);

            while ((nal = st.getNext()) != null)
            {
                //Debug.WriteLine("NAL before consume");
                consumeNal(ByteBuffer.wrap(nal));
                //Debug.WriteLine("NAL after consume");
            }
            pushSample(createSample(buffered, fvnd.sliceHeader, sliceNalUnitHeader), true, true);
        }

        public override string ToString()
        {
            TrackIdTrackExtension trackIdTrackExtension = this.getTrackExtension< TrackIdTrackExtension>(typeof(TrackIdTrackExtension));
            if (trackIdTrackExtension != null)
            {
                return "H264AnnexBTrack{trackId=" + trackIdTrackExtension.getTrackId() + "}";
            }
            else
            {
                return "H264AnnexBTrack{}";
            }
        }

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

        sealed class MyByteArrayOutputStream : ByteStream
        {
            public byte[] toByteArrayLess3()
            {
                var data = toByteArray();

                if (position() > 3)
                {
                    return data.Take(position() - 3).ToArray();
                }
                else
                {
                    return null;
                }
            }
        }
    }
}
