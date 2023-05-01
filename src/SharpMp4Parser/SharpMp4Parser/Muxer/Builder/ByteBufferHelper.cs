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
using System.Collections.Generic;

namespace SharpMp4Parser.Muxer.Builder
{
    /**
     * Used to merge adjacent byte buffers.
     */
    public class ByteBufferHelper
    {
        public static List<ByteBuffer> mergeAdjacentBuffers(List<ByteBuffer> samples)
        {
            List<ByteBuffer> nuSamples = new List<ByteBuffer>(samples.Count);
            foreach (ByteBuffer buffer in samples)
            {
                int lastIndex = nuSamples.Count - 1;
                if (lastIndex >= 0 && buffer.hasArray() && nuSamples[lastIndex].hasArray() && buffer.array() == nuSamples[lastIndex].array() &&
                        nuSamples[lastIndex].arrayOffset() + nuSamples[lastIndex].limit() == buffer.arrayOffset())
                {
                    ByteBuffer oldBuffer = nuSamples[lastIndex];
                    nuSamples.RemoveAt(lastIndex);
                    ByteBuffer nu = ByteBuffer.wrap(buffer.array(), oldBuffer.arrayOffset(), ((Buffer)oldBuffer).limit() + ((Buffer)buffer).limit()).slice();
                    // We need to slice here since wrap([], offset, length) just sets position and not the arrayOffset.
                    nuSamples.Add(nu);
                }
                else if (lastIndex >= 0 &&
                        buffer is MappedByteBuffer && nuSamples[lastIndex] is MappedByteBuffer &&
                        nuSamples[lastIndex].limit() == nuSamples[lastIndex].capacity() - buffer.capacity()) {
                // This can go wrong - but will it?
                ByteBuffer oldBuffer = nuSamples[lastIndex];
                ((Buffer)oldBuffer).limit(buffer.limit() + oldBuffer.limit());
            } 
            else
            {
                buffer.reset();
                nuSamples.Add(buffer);
            }
        }
        return nuSamples;
    }
}
}
