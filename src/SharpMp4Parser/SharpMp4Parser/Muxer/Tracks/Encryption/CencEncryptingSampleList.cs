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
    public class CencEncryptingSampleList : AbstractList<Sample>
    {
        private readonly RangeStartMap<int, SampleEntry> sampleEntries;
        private List<CencSampleAuxiliaryDataFormat> auxiliaryDataFormats;
        private RangeStartMap<int, KeyIdKeyPair> keys = new RangeStartMap<int, KeyIdKeyPair>();
        private IList<Sample> parent;
        private Dictionary<string, Cipher> ciphers = new Dictionary<string, Cipher>();

        public CencEncryptingSampleList(
                RangeStartMap<int, KeyIdKeyPair> keys,
                RangeStartMap<int, SampleEntry> sampleEntries,
                IList<Sample> parent,
                List<CencSampleAuxiliaryDataFormat> auxiliaryDataFormats)
        {
            this.sampleEntries = sampleEntries;
            this.auxiliaryDataFormats = auxiliaryDataFormats;
            this.keys = keys;
            this.parent = parent;

            try
            {
                ciphers.Add("cenc", Cipher.getInstance("AES/CTR/NoPadding"));
                ciphers.Add("cbc1", Cipher.getInstance("AES/CBC/NoPadding"));
            }
            catch (Exception)
            {
                throw;
            }
        }

        public override Sample get(int index)
        {
            Sample clearSample = parent[index];
            if (keys[index] != null && keys[index].getKeyId() != null)
            {
                return new EncryptedSampleImpl(clearSample, index, this);
            }
            else
            {
                return clearSample;
            }
        }

        private void initCipher(Cipher cipher, byte[] iv, SecretKey cek)
        {
            try
            {
                byte[] fullIv = new byte[16];
                System.Array.Copy(iv, 0, fullIv, 0, iv.Length);
                // The IV
                cipher.init(Cipher.ENCRYPT_MODE, cek, new IvParameterSpec(fullIv));
            }
            catch (Exception)
            {
                throw;
            }
        }

        public override int size()
        {
            return parent.Count;
        }

        private class EncryptedSampleImpl : Sample
        {
            private readonly Sample clearSample;
            private int index;
            private CencEncryptingSampleList that;

            public EncryptedSampleImpl(Sample clearSample, int index, CencEncryptingSampleList cencEncryptingSampleList)
            {
                this.that = cencEncryptingSampleList;
                this.clearSample = clearSample;
                this.index = index;
            }

            public void writeTo(WritableByteChannel channel)
            {

                ByteBuffer sample = (ByteBuffer)((Java.Buffer)clearSample.asByteBuffer()).rewind();
                SampleEntry se = that.sampleEntries[index];
                var keyIdKeyPair = that.keys[index];
                CencSampleAuxiliaryDataFormat entry = that.auxiliaryDataFormats[index];
                SchemeTypeBox schm = Path.getPath<SchemeTypeBox>((IsoParser.Container)se, "sinf[0]/schm[0]");
                Debug.Assert(schm != null);
                string encryptionAlgo = schm.getSchemeType();
                Cipher cipher = that.ciphers[encryptionAlgo];
                that.initCipher(cipher, entry.iv, keyIdKeyPair.getKey());
                try
                {
                    if (entry.pairs != null && entry.pairs.Length > 0)
                    {
                        byte[] fullSample = new byte[sample.limit()];
                        sample.get(fullSample);
                        int offset = 0;

                        foreach (CencSampleAuxiliaryDataFormat.Pair pair in entry.pairs)
                        {
                            offset += pair.Clear;
                            if (pair.Encrypted > 0)
                            {
                                cipher.update(fullSample,
                                        offset,
                                        CastUtils.l2i(pair.Encrypted),
                                        fullSample,
                                        offset);
                                offset += (int)pair.Encrypted;
                            }
                        }
                        channel.write(ByteBuffer.wrap(fullSample));
                    }
                    else
                    {
                        byte[] fullyEncryptedSample = new byte[sample.limit()];
                        sample.get(fullyEncryptedSample);
                        if ("cbc1".Equals(encryptionAlgo))
                        {
                            int encryptedLength = fullyEncryptedSample.Length / 16 * 16;
                            channel.write(ByteBuffer.wrap(cipher.doFinal(fullyEncryptedSample, 0, encryptedLength)));
                            channel.write(ByteBuffer.wrap(fullyEncryptedSample, encryptedLength, fullyEncryptedSample.Length - encryptedLength));
                        }
                        else if ("cenc".Equals(encryptionAlgo))
                        {
                            channel.write(ByteBuffer.wrap(cipher.doFinal(fullyEncryptedSample)));
                        }
                    }
                    ((Java.Buffer)sample).rewind();
                }
                catch (Exception)
                {
                    throw;
                }
            }

            public long getSize()
            {
                return clearSample.getSize();
            }

            public ByteBuffer asByteBuffer()
            {
                ByteBuffer sample = (ByteBuffer)((Java.Buffer)clearSample.asByteBuffer()).rewind();
                ByteBuffer encSample = ByteBuffer.allocate(sample.limit());

                SampleEntry se = that.sampleEntries[index];
                KeyIdKeyPair keyIdKeyPair = that.keys[index];
                CencSampleAuxiliaryDataFormat entry = that.auxiliaryDataFormats[index];
                SchemeTypeBox schm = Path.getPath<SchemeTypeBox>((IsoParser.Container)se, "sinf[0]/schm[0]");
                Debug.Assert(schm != null);
                string encryptionAlgo = schm.getSchemeType();
                Cipher cipher = that.ciphers[encryptionAlgo];
                that.initCipher(cipher, entry.iv, keyIdKeyPair.getKey());
                try
                {
                    if (entry.pairs != null)
                    {
                        foreach (CencSampleAuxiliaryDataFormat.Pair pair in entry.pairs)
                        {
                            byte[] clears = new byte[pair.Clear];
                            sample.get(clears);
                            encSample.put(clears);
                            if (pair.Encrypted > 0)
                            {
                                byte[] toBeEncrypted = new byte[CastUtils.l2i(pair.Encrypted)];
                                sample.get(toBeEncrypted);
                                Debug.Assert((toBeEncrypted.Length % 16) == 0);
                                byte[] encrypted = cipher.update(toBeEncrypted);
                                Debug.Assert(encrypted.Length == toBeEncrypted.Length);
                                encSample.put(encrypted);
                            }
                        }
                    }
                    else
                    {

                        byte[] fullyEncryptedSample = new byte[sample.limit()];
                        sample.get(fullyEncryptedSample);
                        if ("cbc1".Equals(encryptionAlgo))
                        {
                            int encryptedLength = fullyEncryptedSample.Length / 16 * 16;
                            encSample.put(cipher.doFinal(fullyEncryptedSample, 0, encryptedLength));
                            encSample.put(fullyEncryptedSample, encryptedLength, fullyEncryptedSample.Length - encryptedLength);
                        }
                        else if ("cenc".Equals(encryptionAlgo))
                        {
                            encSample.put(cipher.doFinal(fullyEncryptedSample));
                        }
                    }
                    ((Java.Buffer)sample).rewind();
                }
                catch (Exception)
                {
                    throw;
                }
                ((Java.Buffer)encSample).rewind();
                return encSample;
            }

            public SampleEntry getSampleEntry()
            {
                return that.sampleEntries[index];
            }
        }
    }
}
