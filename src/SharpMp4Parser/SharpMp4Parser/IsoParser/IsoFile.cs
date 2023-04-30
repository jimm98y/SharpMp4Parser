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

using System;
using SharpMp4Parser.Java;
using System.Text;
using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part12;
using SharpMp4Parser.IsoParser.Support;

namespace SharpMp4Parser.IsoParser
{
    /**
     * The most upper container for ISO Boxes. It is a container box that is a file.
     * Uses IsoBufferWrapper  to access the underlying file.
     */
#warning TODO
    [DoNotParseDetail]
    public class IsoFile : BasicContainer /*, Closeable */
    {
        private readonly ReadableByteChannel readableByteChannel;

        //private FileInputStream fis;

        //public IsoFile(string file) : this(new File(file))
        //{  }

        //public IsoFile(File file)
        //{
        //    this.fis = new FileInputStream(file);
        //    this.readableByteChannel = fis.getChannel();
        //    initContainer(readableByteChannel, -1, new PropertyBoxParserImpl());
        //}

        /**
         * @param readableByteChannel the data source
         * @throws IOException in case I/O error
         */
        public IsoFile(ReadableByteChannel readableByteChannel) : this(readableByteChannel, new PropertyBoxParserImpl())
        { }

        public IsoFile(ReadableByteChannel readableByteChannel, BoxParser boxParser)
        {
            this.readableByteChannel = readableByteChannel;
            initContainer(readableByteChannel, -1, boxParser);
        }

        public static byte[] fourCCtoBytes(string fourCC)
        {
            byte[] result = new byte[4];
            if (fourCC != null)
            {
                for (int i = 0; i < Math.Min(4, fourCC.Length); i++)
                {
                    result[i] = (byte)fourCC[i];
                }
            }
            return result;
        }

        public static string bytesToFourCC(byte[] type)
        {
            byte[] result = new byte[] { 0, 0, 0, 0 };
            if (type != null)
            {
                Array.Copy(type, 0, result, 0, Math.Min(type.Length, 4));
            }
            return Encoding.GetEncoding("ISO-8859-1").GetString(result);
        }


        public long getSize()
        {
            return getContainerSize();
        }

        /**
         * Shortcut to get the MovieBox since it is often needed and present in
         * nearly all ISO 14496 files (at least if they are derived from MP4 ).
         *
         * @return the MovieBox or <code>null</code>
         */
        public MovieBox getMovieBox()
        {
            foreach (Box box in getBoxes())
            {
                if (box is MovieBox)
                {
                    return (MovieBox)box;
                }
            }
            return null;
        }

        public void getBox(WritableByteChannel os)
        {
            writeContainer(os);
        }

        public void close()
        {
            readableByteChannel.Close();
            /*
            if (this.fis != null)
            {
                this.fis.close();
            }
            
            foreach (Box box in getBoxes())
            {
                if (box is Closeable)
                {
                    ((Closeable)box).close();
                }
            }
            */
        }

        public override string ToString()
        {
            return "model(" + readableByteChannel + ")";
        }
    }
}
