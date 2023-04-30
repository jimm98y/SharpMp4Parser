using SharpMp4Parser.Java;
using System;

namespace SharpMp4Parser.IsoParser
{
    public class SkipBox : ParsableBox
    {
        private string type;
        private long size;
        private long sourcePosition = -1;

        public SkipBox(string type, byte[] usertype, string parentType)
        {
            this.type = type;
        }

        public string getType()
        {
            return type;
        }

        public long getSize()
        {
            return size;
        }

        public long getContentSize()
        {
            return size - 8;
        }

        /**
         * Get the seekable position of the content for this box within the source data.
         * @return The data offset, or -1 if it is not known
         */
        public long getSourcePosition()
        {
            return sourcePosition;
        }

        public void getBox(WritableByteChannel writableByteChannel)
        {
            throw new Exception("Cannot retrieve a skipped box - type " + type);
        }

        public void parse(ReadableByteChannel dataSource, ByteBuffer header, long contentSize, BoxParser boxParser)
        {
            size = contentSize + 8;

            //if (dataSource is FileChannel)
            //{
            //FileChannel seekable = (FileChannel)dataSource;
            ReadableByteChannel seekable = dataSource;
            sourcePosition = seekable.position();
            long newPosition = sourcePosition + contentSize;
            seekable.position(newPosition);
            //}
            //else
            //{
            //    throw new Exception("Cannot skip box " + type + " if data source is not seekable");
            //}
        }
    }
}