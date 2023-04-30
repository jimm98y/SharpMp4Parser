using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpMp4Parser.Boxes.ISO23001.Part7
{
    public abstract class AbstractSampleEncryptionBox : AbstractFullBox
    {
        protected int algorithmId = -1;
        protected int ivSize = -1;
        protected sbyte[] kid = new sbyte[] { -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 };
        List<CencSampleAuxiliaryDataFormat> entries = new List<CencSampleAuxiliaryDataFormat>();

        protected AbstractSampleEncryptionBox(string type) : base(type)
        { }

        public int getOffsetToFirstIV()
        {
            int offset = (getSize() > (1L << 32) ? 16 : 8);
            offset += isOverrideTrackEncryptionBoxParameters() ? (4 + kid.Length) : 0;
            offset += 4; //num entries
            return offset;
        }

        public override void _parseDetails(ByteBuffer content)
        {
            parseVersionAndFlags(content);

            if ((getFlags() & 0x1) > 0)
            {
                algorithmId = IsoTypeReader.readUInt24(content);
                ivSize = IsoTypeReader.readUInt8(content);
                kid = new sbyte[16];
                content.get(kid);
            }

            long numOfEntries = IsoTypeReader.readUInt32(content);
            ByteBuffer parseEight = content.duplicate();
            ByteBuffer parseSixteen = content.duplicate();
            ByteBuffer parseZero = content.duplicate();

            entries = parseEntries(parseEight, numOfEntries, 8);
            if (entries != null)
            {
                ((Buffer)content).position(content.position() + content.remaining() - parseEight.remaining());
                return;
            }

            entries = parseEntries(parseSixteen, numOfEntries, 16);
            if (entries != null)
            {
                ((Buffer)content).position(content.position() + content.remaining() - parseSixteen.remaining());
                return;
            }

            entries = parseEntries(parseZero, numOfEntries, 0);
            if (entries != null)
            {
                ((Buffer)content).position(content.position() + content.remaining() - parseZero.remaining());
                return;
            }

            throw new Exception("Cannot parse SampleEncryptionBox");
        }

        private List<CencSampleAuxiliaryDataFormat> parseEntries(ByteBuffer content, long numOfEntries, int ivSize)
        {
            List<CencSampleAuxiliaryDataFormat> _entries = new List<CencSampleAuxiliaryDataFormat>();
            try
            {
                long remainingNumOfEntries = numOfEntries;
                while (remainingNumOfEntries-- > 0)
                {
                    CencSampleAuxiliaryDataFormat e = new CencSampleAuxiliaryDataFormat();
                    e.iv = new byte[ivSize];
                    content.get(e.iv);
                    if ((getFlags() & 0x2) > 0)
                    {
                        int numOfPairs = IsoTypeReader.readUInt16(content);
                        e.pairs = new CencSampleAuxiliaryDataFormat.Pair[numOfPairs];
                        for (int i = 0; i < e.pairs.length; i++)
                        {
                            e.pairs[i] = e.createPair(
                                    IsoTypeReader.readUInt16(content),
                                    IsoTypeReader.readUInt32(content));
                        }
                    }
                    _entries.Add(e);
                }
            }
            catch (Exception bue)
            {
                return null;
            }
            return _entries;
        }

        public List<CencSampleAuxiliaryDataFormat> getEntries()
        {
            return entries;
        }

        public void setEntries(List<CencSampleAuxiliaryDataFormat> entries)
        {
            this.entries = entries;
        }

        public bool isSubSampleEncryption()
        {
            return (getFlags() & 0x2) > 0;
        }

        public void setSubSampleEncryption(bool b)
        {
            if (b)
            {
                setFlags(getFlags() | 0x2);
            }
            else
            {
                setFlags(getFlags() & (0xffffff ^ 0x2));
            }
        }

        protected bool isOverrideTrackEncryptionBoxParameters()
        {
            return (getFlags() & 0x1) > 0;
        }

        protected override void getContent(ByteBuffer byteBuffer)
        {
            writeVersionAndFlags(byteBuffer);
            if (isOverrideTrackEncryptionBoxParameters())
            {
                IsoTypeWriter.writeUInt24(byteBuffer, algorithmId);
                IsoTypeWriter.writeUInt8(byteBuffer, ivSize);
                byteBuffer.put(kid);
            }
            IsoTypeWriter.writeUInt32(byteBuffer, getNonEmptyEntriesNum());
            foreach (CencSampleAuxiliaryDataFormat entry in entries)
            {
                if (entry.getSize() > 0)
                {
                    if (entry.iv.length != 8 && entry.iv.length != 16)
                    {
                        throw new Exception("IV must be either 8 or 16 bytes");
                    }
                    byteBuffer.put(entry.iv);
                    if (isSubSampleEncryption())
                    {
                        IsoTypeWriter.writeUInt16(byteBuffer, entry.pairs.length);
                        foreach (CencSampleAuxiliaryDataFormat.Pair pair in entry.pairs)
                        {
                            IsoTypeWriter.writeUInt16(byteBuffer, pair.clear());
                            IsoTypeWriter.writeUInt32(byteBuffer, pair.encrypted());
                        }
                    }
                }
            }
        }

        private int getNonEmptyEntriesNum()
        {
            int n = 0;
            foreach (CencSampleAuxiliaryDataFormat entry in entries)
            {
                if (entry.getSize() > 0)
                {
                    n++;
                }
            }

            return n;
        }

        protected override long getContentSize()
        {
            long contentSize = 4;
            if (isOverrideTrackEncryptionBoxParameters())
            {
                contentSize += 4;
                contentSize += kid.Length;
            }
            contentSize += 4;
            foreach (CencSampleAuxiliaryDataFormat entry in entries)
            {
                contentSize += entry.getSize();
            }
            return contentSize;
        }

        public override void getBox(WritableByteChannel os)
        {
            base.getBox(os);
        }

        public override bool Equals(object o)
        {
            if (this == o)
            {
                return true;
            }
            if (o == null || getClass() != o.getClass())
            {
                return false;
            }

            AbstractSampleEncryptionBox that = (AbstractSampleEncryptionBox)o;

            if (algorithmId != that.algorithmId)
            {
                return false;
            }
            if (ivSize != that.ivSize)
            {
                return false;
            }
            if (entries != null ? !entries.Equals(that.entries) : that.entries != null)
            {
                return false;
            }
            if (!Enumerable.SequenceEqual(kid, that.kid))
            {
                return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            int result = algorithmId;
            result = 31 * result + ivSize;
            result = 31 * result + (kid != null ? Arrays.hashCode(kid) : 0);
            result = 31 * result + (entries != null ? entries.GetHashCode() : 0);
            return result;
        }

        public List<short> getEntrySizes()
        {
            List<short> entrySizes = new List<short>(entries.size());
            foreach (CencSampleAuxiliaryDataFormat entry in entries)
            {
                short size = (short)entry.iv.length;
                if (isSubSampleEncryption())
                {
                    size += 2; //numPairs
                    size += entry.pairs.length * 6;
                }
                entrySizes.Add(size);
            }
            return entrySizes;
        }
    }
}
