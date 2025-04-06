﻿using SharpMp4Parser.IsoParser.Support;
using SharpMp4Parser.IsoParser.Tools;
using SharpMp4Parser.Java;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpMp4Parser.IsoParser.Boxes.ISO23001.Part7
{
    public abstract class AbstractSampleEncryptionBox : AbstractFullBox
    {
        protected int algorithmId = -1;
        protected int ivSize = -1;
        protected byte[] kid = new byte[] { 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff };
        List<CencSampleAuxiliaryDataFormat> entries = new List<CencSampleAuxiliaryDataFormat>();

        protected AbstractSampleEncryptionBox(string type) : base(type)
        { }

        public int getOffsetToFirstIV()
        {
            int offset = getSize() > 1L << 32 ? 16 : 8;
            offset += isOverrideTrackEncryptionBoxParameters() ? 4 + kid.Length : 0;
            offset += 4; //num entries
            return offset;
        }

        protected override void _parseDetails(ByteBuffer content)
        {
            parseVersionAndFlags(content);

            if ((getFlags() & 0x1) > 0)
            {
                algorithmId = IsoTypeReader.readUInt24(content);
                ivSize = IsoTypeReader.readUInt8(content);
                kid = new byte[16];
                content.get(kid);
            }

            long numOfEntries = IsoTypeReader.readUInt32(content);
            ByteBuffer parseEight = (ByteBuffer)content.duplicate();
            ByteBuffer parseSixteen = (ByteBuffer)content.duplicate();
            ByteBuffer parseZero = (ByteBuffer)content.duplicate();

            entries = parseEntries(parseEight, numOfEntries, 8);
            if (entries != null)
            {
                ((Java.Buffer)content).position(content.position() + content.remaining() - parseEight.remaining());
                return;
            }

            entries = parseEntries(parseSixteen, numOfEntries, 16);
            if (entries != null)
            {
                ((Java.Buffer)content).position(content.position() + content.remaining() - parseSixteen.remaining());
                return;
            }

            entries = parseEntries(parseZero, numOfEntries, 0);
            if (entries != null)
            {
                ((Java.Buffer)content).position(content.position() + content.remaining() - parseZero.remaining());
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
                        for (int i = 0; i < e.pairs.Length; i++)
                        {
                            e.pairs[i] = e.createPair(
                                    IsoTypeReader.readUInt16(content),
                                    IsoTypeReader.readUInt32(content));
                        }
                    }
                    _entries.Add(e);
                }
            }
            catch (Exception)
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

        protected virtual bool isOverrideTrackEncryptionBoxParameters()
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
                    if (entry.iv.Length != 8 && entry.iv.Length != 16)
                    {
                        throw new Exception("IV must be either 8 or 16 bytes");
                    }
                    byteBuffer.put(entry.iv);
                    if (isSubSampleEncryption())
                    {
                        IsoTypeWriter.writeUInt16(byteBuffer, entry.pairs.Length);
                        foreach (CencSampleAuxiliaryDataFormat.Pair pair in entry.pairs)
                        {
                            IsoTypeWriter.writeUInt16(byteBuffer, pair.Clear);
                            IsoTypeWriter.writeUInt32(byteBuffer, pair.Encrypted);
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

        public override void getBox(ByteStream os)
        {
            base.getBox(os);
        }

        public override bool Equals(object o)
        {
            if (this == o)
            {
                return true;
            }
            if (o == null || GetType() != o.GetType())
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
            if (entries != null ? !Enumerable.SequenceEqual(entries, that.entries) : that.entries != null)
            {
                return false;
            }
            if (!kid.SequenceEqual(that.kid))
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
            List<short> entrySizes = new List<short>(entries.Count);
            foreach (CencSampleAuxiliaryDataFormat entry in entries)
            {
                short size = (short)entry.iv.Length;
                if (isSubSampleEncryption())
                {
                    size += 2; //numPairs
                    size += (short)(entry.pairs.Length * 6);
                }
                entrySizes.Add(size);
            }
            return entrySizes;
        }
    }
}
