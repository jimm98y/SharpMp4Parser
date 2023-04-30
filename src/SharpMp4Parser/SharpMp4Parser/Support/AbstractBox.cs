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
using SharpMp4Parser.Java;
using SharpMp4Parser.Tools;
using System.Diagnostics;

namespace SharpMp4Parser.Support
{
    /**
     * A basic on-demand parsing box. Requires the implementation of three methods to become a fully working box:
     * <ol>
     * <li>{@link #_parseDetails(java.nio.ByteBuffer)}</li>
     * <li>{@link #getContent(java.nio.ByteBuffer)}</li>
     * <li>{@link #getContentSize()}</li>
     * </ol>
     * additionally this new box has to be put into the <code>isoparser2-default.properties</code> file so that
     * it is accessible by the <code>PropertyBoxParserImpl</code>
     */
    public abstract class AbstractBox : ParsableBox
    {
        //private static Logger LOG = LoggerFactory.getLogger(AbstractBox.class);

        protected string type;
        private ByteBuffer content;
        protected bool isParsed;
        private byte[] userType;
        private ByteBuffer deadBytes = null;

        protected AbstractBox(string type)
        {
            this.type = type;
            isParsed = true;
        }

        protected AbstractBox(string type, byte[] userType)
        {
            this.type = type;
            this.userType = userType;
            isParsed = true;
        }

        /**
         * Get the box's content size without its header. This must be the exact number of bytes
         * that <code>getContent(ByteBuffer)</code> writes.
         *
         * @return Gets the box's content size in bytes
         * @see #getContent(java.nio.ByteBuffer)
         */
        protected abstract long getContentSize();

        /**
         * Write the box's content into the given <code>ByteBuffer</code>. This must include flags
         * and version in case of a full box. <code>byteBuffer</code> has been initialized with
         * <code>getSize()</code> bytes.
         *
         * @param byteBuffer the sink for the box's content
         */
        protected abstract void getContent(ByteBuffer byteBuffer);

        /**
         * Parse the box's fields and child boxes if any.
         *
         * @param content the box's raw content beginning after the 4-cc field.
         */
        protected abstract void _parseDetails(ByteBuffer content);

        /**
         * {@inheritDoc}
         */
        public void parse(ReadableByteChannel dataSource, ByteBuffer header, long contentSize, BoxParser boxParser)
        {
            content = ByteBuffer.allocate(CastUtils.l2i(contentSize));

            while ((content.position() < contentSize))
            {
                if (dataSource.read(content) == -1)
                {
                    //LOG.error("{} might have been truncated by file end. bytesRead={} contentSize={}", this, content.position(), contentSize);
                    break;
                }
            }

            ((Java.Buffer)content).position(0);
            isParsed = false;
        }

        public virtual void getBox(WritableByteChannel os)
        {
            if (isParsed)
            {
                ByteBuffer bb = ByteBuffer.allocate(CastUtils.l2i(getSize()));
                getHeader(bb);
                getContent(bb);
                if (deadBytes != null)
                {
                    ((Java.Buffer)deadBytes).rewind();
                    while (deadBytes.remaining() > 0)
                    {
                        bb.put(deadBytes);
                    }
                }
                os.write((ByteBuffer)((Java.Buffer)bb).rewind());
            }
            else
            {
                ByteBuffer header = ByteBuffer.allocate((isSmallBox() ? 8 : 16) + (UserBox.TYPE.Equals(getType()) ? 16 : 0));
                getHeader(header);
                os.write((ByteBuffer)((Java.Buffer)header).rewind());
                os.write((ByteBuffer)((Java.Buffer)content).position(0));
            }
        }

        private readonly object _syncRoot = new object();

        /**
         * Parses the raw content of the box. It surrounds the actual parsing
         * which is done
         */
        public void parseDetails()
        {
            lock (_syncRoot)
            {
                //LOG.debug("parsing details of {}", this.getType());
                if (content != null)
                {
                    ByteBuffer content = this.content;
                    isParsed = true;
                    ((Java.Buffer)content).rewind();
                    _parseDetails(content);
                    if (content.remaining() > 0)
                    {
                        deadBytes = content.slice();
                    }
                    this.content = null;
                    Debug.Assert(verify(content));
                }
            }
        }


        /**
         * Gets the full size of the box including header and content.
         *
         * @return the box's size
         */
        public long getSize()
        {
            long size = isParsed ? getContentSize() : content.limit();
            size += (8 + // size|type
                    (size >= ((1L << 32) - 8) ? 8 : 0) + // 32bit - 8 byte size and type
                    (UserBox.TYPE.Equals(getType()) ? 16 : 0));
            size += (deadBytes == null ? 0 : deadBytes.limit());
            return size;
        }

        public string getType()
        {
            return type;
        }

        public virtual byte[] getUserType()
        {
            return userType;
        }

        /**
         * Check if details are parsed.
         *
         * @return <code>true</code> whenever the content <code>ByteBuffer</code> is not <code>null</code>
         */
        public bool IsParsed()
        {
            return isParsed;
        }

        /**
         * Verifies that a box can be reconstructed byte-exact after parsing.
         *
         * @param content the raw content of the box
         * @return <code>true</code> if raw content exactly matches the reconstructed content
         */
        private bool verify(ByteBuffer content)
        {
            ByteBuffer bb = ByteBuffer.allocate(CastUtils.l2i(getContentSize() + (deadBytes != null ? deadBytes.limit() : 0)));
            getContent(bb);
            if (deadBytes != null)
            {
                ((Java.Buffer)deadBytes).rewind();
                while (deadBytes.remaining() > 0)
                {
                    bb.put(deadBytes);
                }
            }
            ((Java.Buffer)content).rewind();
            ((Java.Buffer)bb).rewind();


            if (content.remaining() != bb.remaining())
            {
                //LOG.error("{}: remaining differs {}  vs. {}", this.getType(), content.remaining(), bb.remaining());
                return false;
            }
            int p = content.position();
            for (int i = content.limit() - 1, j = bb.limit() - 1; i >= p; i--, j--)
            {
                byte v1 = content.get(i);
                byte v2 = bb.get(j);
                if (v1 != v2)
                {
                    //LOG.error("{}: buffers differ at {}: {}/{}", this.getType(), i, v1, v2);
                    byte[] b1 = new byte[content.remaining()];
                    byte[] b2 = new byte[bb.remaining()];
                    content.get(b1);
                    bb.get(b2);
                    //LOG.error("original      : {}", Hex.encodeHex(b1, 4));
                    //LOG.error("reconstructed : {}", Hex.encodeHex(b2, 4));
                    return false;
                }
            }
            return true;

        }

        private bool isSmallBox()
        {
            int baseSize = 8;
            if (UserBox.TYPE.Equals(getType()))
            {
                baseSize += 16;
            }
            if (isParsed)
            {
                return (getContentSize() + (deadBytes != null ? deadBytes.limit() : 0) + baseSize) < (1L << 32);
            }
            else
            {
                return content.limit() + baseSize < (1 << 32);
            }

        }

        private void getHeader(ByteBuffer byteBuffer)
        {
            if (isSmallBox())
            {
                IsoTypeWriter.writeUInt32(byteBuffer, this.getSize());
                byteBuffer.put(IsoFile.fourCCtoBytes(getType()));
            }
            else
            {
                IsoTypeWriter.writeUInt32(byteBuffer, 1);
                byteBuffer.put(IsoFile.fourCCtoBytes(getType()));
                IsoTypeWriter.writeUInt64(byteBuffer, getSize());
            }
            if (UserBox.TYPE.Equals(getType()))
            {
                byteBuffer.put(getUserType());
            }
        }
    }
}