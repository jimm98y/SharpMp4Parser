/*
 * Copyright 2012 Sebastian Annies, Hamburg
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

using SharpMp4Parser.Boxes;
using SharpMp4Parser.Tools;
using System.Collections.Generic;
using System;
using System.Linq;
using System.IO;
using SharpMp4Parser.Java;

namespace SharpMp4Parser
{
    /**
     * This BoxParser handles the basic stuff like reading size and extracting box type.
     */
    public abstract class AbstractBoxParser : BoxParser
    {
        private List<string> skippedTypes;

        //private static Logger LOG = LoggerFactory.getLogger(AbstractBoxParser.class.getName());
        ByteBuffer header = ByteBuffer.allocate(32);

        public abstract ParsableBox createBox(string type, byte[] userType, string parent);

        /**
         * Parses the next size and type, creates a box instance and parses the box's content.
         *
         * @param byteChannel the DataSource pointing to the ISO file
         * @param parentType  the current box's parent's type (null if no parent)
         * @return the box just parsed
         * @throws java.io.IOException if reading from <code>in</code> fails
         */
        public ParsableBox parseBox(ReadableByteChannel byteChannel, string parentType)
        {
            ((Java.Buffer)header).rewind().limit(8);

            int bytesRead = 0;
            int b;
            while ((b = byteChannel.read(header)) + bytesRead < 8)
            {
                if (b < 0)
                {
                    throw new EndOfStreamException();
                }
                else
                {
                    bytesRead += b;
                }
            }
                
            ((Java.Buffer)header).rewind();

            long size = IsoTypeReader.readUInt32(header);
            // do plausibility check
            if (size < 8 && size > 1)
            {
                //LOG.error("Plausibility check failed: size < 8 (size = {}). Stop parsing!", size);
                return null;
            }

            string type = IsoTypeReader.read4cc(header);
            //System.err.println(type);
            byte[] usertype = null;
            long contentSize;

            if (size == 1)
            {
                header.limit(16);
                byteChannel.read(header);
                header.position(8);
                size = IsoTypeReader.readUInt64(header);
                contentSize = size - 16;
            }
            else if (size == 0)
            {
                throw new Exception("box size of zero means 'till end of file. That is not yet supported");
            }
            else
            {
                contentSize = size - 8;
            }
            if (UserBox.TYPE.Equals(type))
            {
                header.limit(header.limit() + 16);
                byteChannel.read(header);
                usertype = new byte[16];
                for (int i = header.position() - 16; i < header.position(); i++)
                {
                    usertype[i - (header.position() - 16)] = header.get(i);
                }
                contentSize -= 16;
            }
            ParsableBox parsableBox = null;
            if (skippedTypes != null && skippedTypes.Contains(type))
            {
                //LOG.trace("Skipping box {} {} {}", type, usertype, parentType);
                parsableBox = new SkipBox(type, usertype, parentType);
            }
            else
            {
                //LOG.trace("Creating box {} {} {}", type, usertype, parentType);
                parsableBox = createBox(type, usertype, parentType);
            }

            //LOG.finest("Parsing " + box.getType());
            // System.out.println("parsing " + Mp4Arrays.toString(box.getType()) + " " + box.getClass().getName() + " size=" + size);
            ((Java.Buffer)header).rewind();

            parsableBox.parse(byteChannel, header, contentSize, this);
            return parsableBox;
        }

        public AbstractBoxParser skippingBoxes(params string[] types)
        {
            skippedTypes = types.ToList();
            return this;
        }
    }
}