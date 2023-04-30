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
     * UUID from/to byte array.
     */
    public class UUIDConverter
    {
        public static byte[] convert(Uuid uuid)
        {
            long msb = uuid.MostSignificantBits;
            long lsb = uuid.LeastSignificantBits;
            byte[] buffer = new byte[16];

            for (int i = 0; i < 8; i++)
            {
                buffer[i] = (byte)((ulong)msb >> 8 * (7 - i));
            }
            for (int i = 8; i < 16; i++)
            {
                buffer[i] = (byte)((ulong)lsb >> 8 * (7 - i));
            }

            return buffer;

        }

        public static Uuid convert(byte[] uuidBytes)
        {
            ByteBuffer b = ByteBuffer.wrap(uuidBytes);
            b.order(ByteOrder.BIG_ENDIAN);
            return new Uuid(b.getLong(), b.getLong());
        }
    }
}
