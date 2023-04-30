/*  
 * Copyright 2008 CoreMedia AG, Hamburg
 *
 * Licensed under the Apache License, Version 2.0 (the License); 
 * you may not use this file except in compliance with the License. 
 * You may obtain a copy of the License at 
 * 
 *     http://www.apache.org/licenses/LICENSE-2.0 
 * 
 * Unless required by applicable law or agreed to in writing, software 
 * distributed under the License is distributed on an AS IS BASIS, 
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
 * See the License for the specific language governing permissions and 
 * limitations under the License. 
 */

using SharpMp4Parser.Java;

namespace SharpMp4Parser.IsoParser.Boxes.ISO14496.Part12
{
    /**
     * <h1>4cc = "{@value #TYPE}"</h1>
     * This box contains the media data. In video tracks, this box would contain video frames. A presentation may
     * contain zero or more Media Data Boxes. The actual media data follows the type field; its structure is described
     * by the metadata (see {@link SampleTableBox}).<br>
     * In large presentations, it may be desirable to have more data in this box than a 32-bit size would permit. In this
     * case, the large variant of the size field is used.<br>
     * There may be any number of these boxes in the file (including zero, if all the media data is in other files). The
     * metadata refers to media data by its absolute offset within the file (see {@link StaticChunkOffsetBox});
     * so Media Data Box headers and free space may easily be skipped, and files without any box structure may
     * also be referenced and used.
     */
#warning TODO
    public sealed class MediaDataBox : ParsableBox /*, Closeable */
    {
        public const string TYPE = "mdat";
        //ByteBuffer header;
        //File dataFile;

        public string getType()
        {
            return TYPE;
        }

        public void getBox(WritableByteChannel writableByteChannel)
        {
            throw new System.NotImplementedException();
            //writableByteChannel.write((ByteBuffer)((Java.Buffer)header).rewind());
            //using (FileInputStream fis = new FileInputStream(dataFile))
            //{
            //    using (FileChannel fc = fis.getChannel())
            //    {
            //        fc.transferTo(0, dataFile.lastModified(), writableByteChannel);
            //    }
            //}
        }

        public long getSize()
        {
            throw new System.NotImplementedException();
            return 0; /* header.limit() + dataFile.length(); */
        }

        /**
         * {@inheritDoc}
         */
        public void parse(ReadableByteChannel dataSource, ByteBuffer header, long contentSize, BoxParser boxParser)
        {
            throw new System.NotImplementedException();
            //dataFile = File.createTempFile("MediaDataBox", base.ToString());

            //// make sure to clean up temp file
            //dataFile.deleteOnExit();

            //this.header = ByteBuffer.allocate(header.limit());
            //this.header.put(header);
            //using (RandomAccessFile raf = new RandomAccessFile(dataFile, "rw"))
            //{
            //    raf.getChannel().transferFrom(dataSource, 0, contentSize);
            //}
        }

        /*
        public override void close()
        {
            if (dataFile != null)
            {
                dataFile.delete();
            }
        }
        */
    }
}
