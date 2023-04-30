using SharpMp4Parser.Boxes.ISO14496.Part1.ObjectDescriptors;
using System;
using System.Collections.Generic;

namespace SharpMp4Parser.Boxes.ISO14496.Part15
{
    /**
     *
     */
    public class AvcDecoderConfigurationRecord
    {
        public int configurationVersion;
        public int avcProfileIndication;
        public int profileCompatibility;
        public int avcLevelIndication;
        public int lengthSizeMinusOne;
        public List<ByteBuffer> sequenceParameterSets = new List<ByteBuffer>();
        public List<ByteBuffer> pictureParameterSets = new List<ByteBuffer>();

        public bool hasExts = true;
        public int chromaFormat = 1;
        public int bitDepthLumaMinus8 = 0;
        public int bitDepthChromaMinus8 = 0;
        public List<ByteBuffer> sequenceParameterSetExts = new List<ByteBuffer>();

        /**
         * Just for non-spec-conform encoders
         */
        public int lengthSizeMinusOnePaddingBits = 63;
        public int numberOfSequenceParameterSetsPaddingBits = 7;
        public int chromaFormatPaddingBits = 31;
        public int bitDepthLumaMinus8PaddingBits = 31;
        public int bitDepthChromaMinus8PaddingBits = 31;

        public AvcDecoderConfigurationRecord()
        {
        }

        public AvcDecoderConfigurationRecord(ByteBuffer content)
        {
            configurationVersion = IsoTypeReader.readUInt8(content);
            avcProfileIndication = IsoTypeReader.readUInt8(content);
            profileCompatibility = IsoTypeReader.readUInt8(content);
            avcLevelIndication = IsoTypeReader.readUInt8(content);
            BitReaderBuffer brb = new BitReaderBuffer(content);
            lengthSizeMinusOnePaddingBits = brb.readBits(6);
            lengthSizeMinusOne = brb.readBits(2);
            numberOfSequenceParameterSetsPaddingBits = brb.readBits(3);
            int numberOfSeuqenceParameterSets = brb.readBits(5);
            for (int i = 0; i < numberOfSeuqenceParameterSets; i++)
            {
                int sequenceParameterSetLength = IsoTypeReader.readUInt16(content);

                byte[] sequenceParameterSetNALUnit = new byte[sequenceParameterSetLength];
                content.get(sequenceParameterSetNALUnit);
                sequenceParameterSets.Add(ByteBuffer.wrap(sequenceParameterSetNALUnit));
            }
            long numberOfPictureParameterSets = IsoTypeReader.readUInt8(content);
            for (int i = 0; i < numberOfPictureParameterSets; i++)
            {
                int pictureParameterSetLength = IsoTypeReader.readUInt16(content);
                byte[] pictureParameterSetNALUnit = new byte[pictureParameterSetLength];
                content.get(pictureParameterSetNALUnit);
                pictureParameterSets.Add(ByteBuffer.wrap(pictureParameterSetNALUnit));
            }
            if (content.remaining() < 4)
            {
                hasExts = false;
            }
            if (hasExts && (avcProfileIndication == 100 || avcProfileIndication == 110 || avcProfileIndication == 122 || avcProfileIndication == 144))
            {
                // actually only some bits are interesting so masking with & x would be good but not all Mp4 creating tools set the reserved bits to 1.
                // So we need to store all bits
                brb = new BitReaderBuffer(content);
                chromaFormatPaddingBits = brb.readBits(6);
                chromaFormat = brb.readBits(2);
                bitDepthLumaMinus8PaddingBits = brb.readBits(5);
                bitDepthLumaMinus8 = brb.readBits(3);
                bitDepthChromaMinus8PaddingBits = brb.readBits(5);
                bitDepthChromaMinus8 = brb.readBits(3);
                long numOfSequenceParameterSetExt = IsoTypeReader.readUInt8(content);
                for (int i = 0; i < numOfSequenceParameterSetExt; i++)
                {
                    int sequenceParameterSetExtLength = IsoTypeReader.readUInt16(content);
                    byte[] sequenceParameterSetExtNALUnit = new byte[sequenceParameterSetExtLength];
                    content.get(sequenceParameterSetExtNALUnit);
                    sequenceParameterSetExts.Add(ByteBuffer.wrap(sequenceParameterSetExtNALUnit));
                }
            }
            else
            {
                chromaFormat = -1;
                bitDepthLumaMinus8 = -1;
                bitDepthChromaMinus8 = -1;
            }
        }

        public void getContent(ByteBuffer byteBuffer)
        {
            IsoTypeWriter.writeUInt8(byteBuffer, configurationVersion);
            IsoTypeWriter.writeUInt8(byteBuffer, avcProfileIndication);
            IsoTypeWriter.writeUInt8(byteBuffer, profileCompatibility);
            IsoTypeWriter.writeUInt8(byteBuffer, avcLevelIndication);
            BitWriterBuffer bwb = new BitWriterBuffer(byteBuffer);
            bwb.writeBits(lengthSizeMinusOnePaddingBits, 6);
            bwb.writeBits(lengthSizeMinusOne, 2);
            bwb.writeBits(numberOfSequenceParameterSetsPaddingBits, 3);
            bwb.writeBits(sequenceParameterSets.size(), 5);
            foreach (ByteBuffer sequenceParameterSetNALUnit in sequenceParameterSets)
            {
                IsoTypeWriter.writeUInt16(byteBuffer, sequenceParameterSetNALUnit.limit());
                byteBuffer.put((ByteBuffer)((Buffer)sequenceParameterSetNALUnit).rewind());
            }
            IsoTypeWriter.writeUInt8(byteBuffer, pictureParameterSets.size());
            foreach (ByteBuffer pictureParameterSetNALUnit in pictureParameterSets)
            {
                IsoTypeWriter.writeUInt16(byteBuffer, pictureParameterSetNALUnit.limit());
                byteBuffer.put((ByteBuffer)((Buffer)pictureParameterSetNALUnit).rewind());
            }
            if (hasExts && (avcProfileIndication == 100 || avcProfileIndication == 110 || avcProfileIndication == 122 || avcProfileIndication == 144))
            {

                bwb = new BitWriterBuffer(byteBuffer);
                bwb.writeBits(chromaFormatPaddingBits, 6);
                bwb.writeBits(chromaFormat, 2);
                bwb.writeBits(bitDepthLumaMinus8PaddingBits, 5);
                bwb.writeBits(bitDepthLumaMinus8, 3);
                bwb.writeBits(bitDepthChromaMinus8PaddingBits, 5);
                bwb.writeBits(bitDepthChromaMinus8, 3);
                foreach (ByteBuffer sequenceParameterSetExtNALUnit in sequenceParameterSetExts)
                {
                    IsoTypeWriter.writeUInt16(byteBuffer, sequenceParameterSetExtNALUnit.limit());
                    byteBuffer.put((ByteBuffer)sequenceParameterSetExtNALUnit.reset());
                }
            }
        }

        public long getContentSize()
        {
            long size = 5;
            size += 1; // sequenceParamsetLength
            foreach (ByteBuffer sequenceParameterSetNALUnit in sequenceParameterSets)
            {
                size += 2; //lengthSizeMinusOne field
                size += sequenceParameterSetNALUnit.limit();
            }
            size += 1; // pictureParamsetLength
            foreach (ByteBuffer pictureParameterSetNALUnit in pictureParameterSets)
            {
                size += 2; //lengthSizeMinusOne field
                size += pictureParameterSetNALUnit.limit();
            }
            if (hasExts && (avcProfileIndication == 100 || avcProfileIndication == 110 || avcProfileIndication == 122 || avcProfileIndication == 144))
            {
                size += 4;
                foreach (ByteBuffer sequenceParameterSetExtNALUnit in sequenceParameterSetExts)
                {
                    size += 2;
                    size += sequenceParameterSetExtNALUnit.limit();
                }
            }

            return size;
        }


        public List<string> getSequenceParameterSetsAsStrings()
        {
            List<string> result = new List<string>(sequenceParameterSets.size());
            foreach (ByteBuffer parameterSet in sequenceParameterSets)
            {
                result.Add(Hex.encodeHex(parameterSet));
            }
            return result;
        }

        public List<string> getSequenceParameterSetExtsAsStrings()
        {
            List<string> result = new List<string>(sequenceParameterSetExts.size());
            foreach (ByteBuffer parameterSet in sequenceParameterSetExts)
            {
                result.Add(Hex.encodeHex(parameterSet));
            }
            return result;
        }

        public List<string> getPictureParameterSetsAsStrings()
        {
            List<string> result = new List<string>(pictureParameterSets.size());
            foreach (ByteBuffer parameterSet in pictureParameterSets)
            {
                result.Add(Hex.encodeHex(parameterSet));
            }
            return result;
        }

        public override string ToString()
        {
            return "AvcDecoderConfigurationRecord{" +
                    "configurationVersion=" + configurationVersion +
                    ", avcProfileIndication=" + avcProfileIndication +
                    ", profileCompatibility=" + profileCompatibility +
                    ", avcLevelIndication=" + avcLevelIndication +
                    ", lengthSizeMinusOne=" + lengthSizeMinusOne +
                    ", hasExts=" + hasExts +
                    ", chromaFormat=" + chromaFormat +
                    ", bitDepthLumaMinus8=" + bitDepthLumaMinus8 +
                    ", bitDepthChromaMinus8=" + bitDepthChromaMinus8 +
                    ", lengthSizeMinusOnePaddingBits=" + lengthSizeMinusOnePaddingBits +
                    ", numberOfSequenceParameterSetsPaddingBits=" + numberOfSequenceParameterSetsPaddingBits +
                    ", chromaFormatPaddingBits=" + chromaFormatPaddingBits +
                    ", bitDepthLumaMinus8PaddingBits=" + bitDepthLumaMinus8PaddingBits +
                    ", bitDepthChromaMinus8PaddingBits=" + bitDepthChromaMinus8PaddingBits +
                    '}';
        }
    }
}
