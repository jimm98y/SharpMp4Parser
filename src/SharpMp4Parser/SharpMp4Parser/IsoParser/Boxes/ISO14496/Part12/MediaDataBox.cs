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
using System.IO;

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
#warning TODO refactoring of the file access
    public sealed class MediaDataBox : ParsableBox, Closeable
    {
        public const string TYPE = "mdat";
        ByteBuffer header;
        System.IO.FileStream dataFile;
        long dataFilelength = 0;

        public string getType()
        {
            return TYPE;
        }

        public void getBox(ByteStream writableByteChannel)
        {
            writableByteChannel.write((ByteBuffer)((Java.Buffer)header).rewind());
            
            dataFile.Seek(0, SeekOrigin.Begin);

            using(MemoryStream ms = new MemoryStream())
            {
                dataFile.CopyTo(ms);
                writableByteChannel.write(ms.ToArray());
            }            
        }

        public long getSize()
        {
            return header.limit() + dataFilelength;
        }

        /**
         * {@inheritDoc}
         */
        public void parse(ByteStream dataSource, ByteBuffer header, long contentSize, BoxParser boxParser)
        {
            // fileName: "MediaDataBox" + base.ToString()
            dataFile = System.IO.File.Create(System.IO.Path.GetRandomFileName(), (int)contentSize, FileOptions.DeleteOnClose);

            this.header = ByteBuffer.allocate(header.limit());
            this.header.put(header);

            dataFilelength = contentSize;
            var data = ByteBuffer.allocate((int)contentSize); // we have to use ByteBuffer instead of byte[] because of ByteBufferByteChannel
            dataSource.read(data);

            dataFile.Write(data.array(), 0, (int)contentSize);
            dataFile.Flush();
        }

        public void close()
        {
            if (dataFile != null)
            {
                dataFile.Close();
            }
        }
    }
}
