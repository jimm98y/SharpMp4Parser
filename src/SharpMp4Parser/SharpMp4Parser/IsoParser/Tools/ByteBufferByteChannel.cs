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

using SharpMp4Parser.Java;

namespace SharpMp4Parser.IsoParser.Tools
{
    /**
     * Creates a <code>ByteStreamBase</code> that is backed by a <code>ByteBuffer</code>.
     */
    public class ByteBufferByteChannel : ByteStream /* : ByteChannel */
    {
        ByteBuffer byteBuffer;

        public ByteBufferByteChannel(byte[] byteArray) : this(ByteBuffer.wrap(byteArray))
        { }

        public ByteBufferByteChannel(ByteBuffer byteBuffer) : base(byteBuffer.array())
        {
            this.byteBuffer = byteBuffer;
        }

        public override int read(ByteBuffer dst)
        {
            int rem = dst.remaining();
            if (byteBuffer.remaining() <= 0)
            {
                return -1;
            }

            dst.put((ByteBuffer)byteBuffer.duplicate().limit(byteBuffer.position() + dst.remaining()));
            ((Buffer)byteBuffer).position(byteBuffer.position() + rem);
            return rem;
        }

        public override bool isOpen()
        {
            return true;
        }

        public override void close()
        {
            base.close();
        }

        public override int write(ByteBuffer src)
        {
            int r = src.remaining();
            byteBuffer.put((ByteBuffer)src);
            return r;
        }
    }
}
