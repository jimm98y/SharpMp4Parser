﻿using System;

namespace SharpMp4Parser.Boxes.Adobe
{
    /**
     * <h1>4cc = "{@value #TYPE}"</h1>
     * Sample Entry as used for Action Message Format tracks.
     */
    public class ActionMessageFormat0SampleEntryBox : AbstractSampleEntry
    {
        public const string TYPE = "amf0";

        public ActionMessageFormat0SampleEntryBox() : base(TYPE)
        { }

        public override void parse(ReadableByteChannel dataSource, ByteBuffer header, long contentSize, BoxParser boxParser)
        {
            ByteBuffer bb = ByteBuffer.allocate(8);
            dataSource.read(bb);
            ((Buffer)bb).position(6);// ignore 6 reserved bytes;
            dataReferenceIndex = IsoTypeReader.readUInt16(bb);
            initContainer(dataSource, contentSize - 8, boxParser);
        }

        public override void getBox(WritableByteChannel writableByteChannel)
        {
            writableByteChannel.write(getHeader());
            ByteBuffer bb = ByteBuffer.allocate(8);
            ((Buffer)bb).position(6);
            IsoTypeWriter.writeUInt16(bb, dataReferenceIndex);
            writableByteChannel.write((ByteBuffer)((Buffer)bb).rewind());
            writeContainer(writableByteChannel);
        }

        public override long getSize()
        {
            long s = getContainerSize();
            long t = 8; // bytes to container start
            return s + t + ((largeBox || (s + t) >= (1L << 32)) ? 16 : 8);
        }
    }
}
