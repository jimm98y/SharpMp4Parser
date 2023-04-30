﻿/**
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
using System;

namespace SharpMp4Parser.IsoParser
{
    /**
     * Provides rewind() operation to ReadableByteChannel by buffering data up to specified capacity.
     */
    public class RewindableReadableByteChannel : ReadableByteChannel
    {

        private readonly ReadableByteChannel readableByteChannel;
        private readonly ByteBuffer buffer;
        // If 'true', there are more bytes read from |readableByteChannel| than the allocated buffer size.
        // The rewind is not possible in that case.
        private bool passedRewindPoint;
        private int nextBufferWritePosition;
        private int nextBufferReadPosition;

        public RewindableReadableByteChannel(ReadableByteChannel readableByteChannel, int bufferCapacity)
        {
            buffer = allocate(bufferCapacity);
            this.readableByteChannel = readableByteChannel;
        }

        /**
         * @see ReadableByteChannel#read(ByteBuffer)
         */
        public override int read(ByteBuffer dst)
        {
            int initialDstPosition = dst.position();
            // Read data from |readableByteChannel| into |buffer|.
            buffer.limit(buffer.capacity());
            ((Java.Buffer)buffer).position(nextBufferWritePosition);
            if (buffer.capacity() > 0)
            {
                readableByteChannel.read(buffer);
                nextBufferWritePosition = buffer.position();
            }

            // Read data from |buffer| into |dst|.
            ((Java.Buffer)buffer).position(nextBufferReadPosition);
            buffer.limit(nextBufferWritePosition);
            if (buffer.remaining() > dst.remaining())
            {
                buffer.limit(buffer.position() + dst.remaining());
            }
            dst.put(buffer);
            nextBufferReadPosition = buffer.position();

            // If |dst| still has capacity then read data from |readableByteChannel|.
            int bytesRead = readableByteChannel.read(dst);
            if (bytesRead > 0)
            {
                // We passed the buffering capacity. It will not be possible to rewind
                // |readableByteChannel| anymore.
                passedRewindPoint = true;
            }
            else if (bytesRead == -1 && dst.position() - initialDstPosition == 0)
            {
                return -1;
            }
            return dst.position() - initialDstPosition;
        }

        public new void rewind()
        {
            if (passedRewindPoint)
            {
                throw new InvalidOperationException("Passed the rewind point. Increase the buffer capacity.");
            }
            nextBufferReadPosition = 0;
        }

        /**
         * @see ReadableByteChannel#isOpen()
         */
        public override bool isOpen()
        {
            return readableByteChannel.isOpen();
        }

        /**
         * @see ReadableByteChannel#close()
         */
        public override void close()
        {
            readableByteChannel.close();
        }
    }
}