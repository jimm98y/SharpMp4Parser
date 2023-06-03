using System.IO;

namespace SharpMp4Parser.Streaming.Input
{
    /**
    * Created by Jimm98y on 5/29/2023.
    */
    public static class AnnexBUtils
    {
        public static byte[] AddEmulationPreventionBytes(byte[] nal)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                int i;
                for (i = 0; i < nal.Length; i++)
                {
                    if (i >= 2)
                    {
                        if (nal[i - 1] == 0)
                        {
                            if (nal[i - 2] == 0)
                            {
                                if (nal[i] == 0x00 || nal[i] == 0x01 || nal[i] == 0x02)
                                {
                                    ms.WriteByte(0x03);
                                }
                            }
                        }
                    }

                    ms.WriteByte(nal[i]);
                }

                return ms.ToArray();
            }
        }

        public static byte[] RemoveEmulationPreventionBytes(byte[] nal)
        {
            /*
             The NAL units are encoded in such a way that the sequences of 00 00 00, 00 00 01, 00 00 02, 00 00 03 bytes are illegal. If such 
              sequence of bytes is found, the encoder will insert a 03 byte to "escape it". We have to analyze the byte stream, find all the
              sequences of 00 00 03 and remove the 03 byte beofre passing the NAL unit further into the encoder. NalStreamTokenizer does it
              for the input stream.
             */
            byte a;
            bool removeNext3 = false;

            using (MemoryStream ms = new MemoryStream())
            {
                int i;
                for (i = 0; i < nal.Length; i++)
                {
                    a = nal[i];

                    if (a == 0)
                    {
                        if (i < nal.Length - 2)
                        {
                            if (nal[i + 1] == 0)
                            {
                                if (nal[i + 2] == 3)
                                {
                                    removeNext3 = true;
                                }
                            }
                        }
                    }

                    if (a != 3 || !removeNext3)
                    {
                        ms.WriteByte(a);
                    }
                    else
                    {
                        removeNext3 = false;
                    }
                }

                return ms.ToArray();
            }
        }
    }
}
