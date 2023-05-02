using SharpMp4Parser.IsoParser.Boxes.SampleEntry;
using SharpMp4Parser.IsoParser.Tools;
using SharpMp4Parser.Java;
using System;

namespace SharpMp4Parser.IsoParser.Boxes.ISO14496.Part30
{
    public class XMLSubtitleSampleEntry : AbstractSampleEntry
    {
        public const string TYPE = "stpp";

        private string ns = "";
        private string schemaLocation = "";
        private string auxiliaryMimeTypes = "";

        public XMLSubtitleSampleEntry() : base(TYPE)
        { }

        public override long getSize()
        {
            long s = getContainerSize();
            long t = 8 + ns.Length + schemaLocation.Length + auxiliaryMimeTypes.Length + 3;
            return s + t + (largeBox || s + t + 8 >= 1L << 32 ? 16 : 8);
        }

        public override void parse(ReadableByteChannel dataSource, ByteBuffer header, long contentSize, BoxParser boxParser)
        {
            ByteBuffer byteBuffer = ByteBuffer.allocate(8);
            dataSource.read((ByteBuffer)byteBuffer.rewind());
            ((Java.Buffer)byteBuffer).position(6);
            dataReferenceIndex = IsoTypeReader.readUInt16(byteBuffer);


            byte[]
            namespaceBytes = new byte[0];
            int read;
            while ((read = Channels.newInputStream(dataSource).read()) != 0)
            {
                namespaceBytes = Mp4Arrays.copyOfAndAppend(namespaceBytes, (byte)read);
            }
            ns = Utf8.convert(namespaceBytes);


            byte[] schemaLocationBytes = new byte[0];

            while ((read = Channels.newInputStream(dataSource).read()) != 0)
            {
                schemaLocationBytes = Mp4Arrays.copyOfAndAppend(schemaLocationBytes, (byte)read);
            }
            schemaLocation = Utf8.convert(schemaLocationBytes);


            byte[] auxiliaryMimeTypesBytes = new byte[0];

            while ((read = Channels.newInputStream(dataSource).read()) != 0)
            {
                auxiliaryMimeTypesBytes = Mp4Arrays.copyOfAndAppend(auxiliaryMimeTypesBytes, (byte)read);
            }
            auxiliaryMimeTypes = Utf8.convert(auxiliaryMimeTypesBytes);

            initContainer(dataSource, contentSize - (header.remaining() + ns.Length + schemaLocation.Length + auxiliaryMimeTypes.Length + 3), boxParser);
        }

        public override void getBox(WritableByteChannel writableByteChannel)
        {
            writableByteChannel.write(getHeader());
            ByteBuffer byteBuffer = ByteBuffer.allocate(8 + ns.Length + schemaLocation.Length + auxiliaryMimeTypes.Length + 3);
            ((Java.Buffer)byteBuffer).position(6);
            IsoTypeWriter.writeUInt16(byteBuffer, dataReferenceIndex);
            IsoTypeWriter.writeZeroTermUtf8String(byteBuffer, ns);
            IsoTypeWriter.writeZeroTermUtf8String(byteBuffer, schemaLocation);
            IsoTypeWriter.writeZeroTermUtf8String(byteBuffer, auxiliaryMimeTypes);
            writableByteChannel.write(byteBuffer.rewind());
            writeContainer(writableByteChannel);
        }

        public string getNamespace()
        {
            return ns;
        }

        public void setNamespace(string ns)
        {
            this.ns = ns;
        }

        public string getSchemaLocation()
        {
            return schemaLocation;
        }

        public void setSchemaLocation(string schemaLocation)
        {
            this.schemaLocation = schemaLocation;
        }

        public string getAuxiliaryMimeTypes()
        {
            return auxiliaryMimeTypes;
        }

        public void setAuxiliaryMimeTypes(string auxiliaryMimeTypes)
        {
            this.auxiliaryMimeTypes = auxiliaryMimeTypes;
        }
    }
}