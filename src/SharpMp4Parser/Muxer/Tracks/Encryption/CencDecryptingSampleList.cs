using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part12;
using SharpMp4Parser.IsoParser.Boxes.ISO23001.Part7;
using SharpMp4Parser.IsoParser.Boxes.SampleEntry;
using SharpMp4Parser.IsoParser.Tools;
using SharpMp4Parser.Java;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SharpMp4Parser.Muxer.Tracks.Encryption
{
    public class CencDecryptingSampleList : AbstractList<Sample>
    {
        private RangeStartMap<int, SampleEntry> sampleEntries;
        private List<CencSampleAuxiliaryDataFormat> sencInfo;
        private RangeStartMap<int, SecretKey> keys = new RangeStartMap<int, SecretKey>();
        private IList<Sample> parent;

        public CencDecryptingSampleList(
                RangeStartMap<int, SecretKey> keys,
                RangeStartMap<int, SampleEntry> sampleEntries,
                IList<Sample> parent,
                List<CencSampleAuxiliaryDataFormat> sencInfo
        )
        {
            this.sampleEntries = sampleEntries;
            this.sencInfo = sencInfo;
            this.keys = keys;
            this.parent = parent;
        }

        private string getSchemeType(SampleEntry s)
        {
            SchemeTypeBox schm = Path.getPath< SchemeTypeBox>((IsoParser.Container)s, "sinf/schm");

            Debug.Assert(schm != null, "Cannot get cipher without schemetypebox");
            return schm.getSchemeType();
        }

        private Cipher getCipher(SecretKey sk, byte[] iv, SampleEntry se)
        {

            byte[] fullIv = new byte[16];
            System.Array.Copy(iv, 0, fullIv, 0, iv.Length);
            // The IV
            try
            {
                string schemeType = getSchemeType(se);
                if ("cenc".Equals(schemeType) || "piff".Equals(schemeType))
                {
                    Cipher c = Cipher.getInstance("AES/CTR/NoPadding");
                    c.init(Cipher.DECRYPT_MODE, sk, new IvParameterSpec(fullIv));
                    return c;
                }
                else if ("cbc1".Equals(schemeType))
                {
                    Cipher c = Cipher.getInstance("AES/CBC/NoPadding");
                    c.init(Cipher.DECRYPT_MODE, sk, new IvParameterSpec(fullIv));
                    return c;
                }
                else
                {
                    throw new Exception("Only cenc & cbc1 is supported as encryptionAlgo");
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public override Sample get(int index)
        {
            if (keys[index] != null)
            {
                Sample encSample = parent[index];
                ByteBuffer encSampleBuffer = encSample.asByteBuffer();
                ((Java.Buffer)encSampleBuffer).rewind();
                ByteBuffer decSampleBuffer = ByteBuffer.allocate(encSampleBuffer.limit());
                CencSampleAuxiliaryDataFormat sencEntry = sencInfo[index];
                Cipher cipher = getCipher(keys[index], sencEntry.iv, encSample.getSampleEntry());

                try
                {
                    if (sencEntry.pairs != null && sencEntry.pairs.Length > 0)
                    {

                        foreach (CencSampleAuxiliaryDataFormat.Pair pair in sencEntry.pairs)
                        {
                            int clearBytes = pair.Clear;
                            int encrypted = CastUtils.l2i(pair.Encrypted);

                            byte[] clears = new byte[clearBytes];
                            encSampleBuffer.get(clears);
                            decSampleBuffer.put(clears);
                            if (encrypted > 0)
                            {
                                byte[] encs = new byte[encrypted];
                                encSampleBuffer.get(encs);
                                byte[] decr = cipher.update(encs);
                                decSampleBuffer.put(decr);
                            }

                        }
                        if (encSampleBuffer.remaining() > 0)
                        {
                            Java.LOG.debug("Decrypted sample " + index + " but still data remaining: " + encSample.getSize());
                        }
                        decSampleBuffer.put(cipher.doFinal());
                    }
                    else
                    {
                        byte[] fullyEncryptedSample = new byte[encSampleBuffer.limit()];
                        encSampleBuffer.get(fullyEncryptedSample);
                        string schemeType = getSchemeType(encSample.getSampleEntry());
                        if ("cbc1".Equals(schemeType))
                        {
                            int encryptedLength = fullyEncryptedSample.Length / 16 * 16;
                            decSampleBuffer.put(cipher.doFinal(fullyEncryptedSample, 0, encryptedLength));
                            decSampleBuffer.put(fullyEncryptedSample, encryptedLength, fullyEncryptedSample.Length - encryptedLength);
                        }
                        else if ("cenc".Equals(schemeType))
                        {
                            decSampleBuffer.put(cipher.doFinal(fullyEncryptedSample));
                        }
                        else if ("piff".Equals(schemeType))
                        {
                            decSampleBuffer.put(cipher.doFinal(fullyEncryptedSample));
                        }
                        else
                        {
                            throw new Exception("unknown encryption algo");
                        }
                    }
                    ((Java.Buffer)encSampleBuffer).rewind();
                }
                catch (Exception)
                {
                    throw;
                }
                ((Java.Buffer)decSampleBuffer).rewind();
                return new SampleImpl(decSampleBuffer, sampleEntries[index]);
            }
            else
            {
                return parent[index];
            }
        }

        public override int size()
        {
            return parent.Count;
        }
    }
}
