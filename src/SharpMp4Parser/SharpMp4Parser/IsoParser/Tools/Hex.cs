/*
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System.Text;
using System;
using SharpMp4Parser.Java;

namespace SharpMp4Parser.IsoParser.Tools
{
    /**
     * Converts hexadecimal Strings.
     */
    public class Hex
    {
        private static readonly char[] DIGITS = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };

        public static string encodeHex(byte[] data)
        {
            return encodeHex(data, 0);
        }

        public static string encodeHex(ByteBuffer data)
        {
            ByteBuffer byteBuffer = data.duplicate();
            StringBuilder sb = new StringBuilder();
            while (byteBuffer.remaining() > 0)
            {
                byte b = byteBuffer.get();
                sb.Append(DIGITS[(byte)(0xF0 & b) >> 4]);
                sb.Append(DIGITS[0x0F & b]);
            }
            return sb.ToString();
        }

        public static string encodeHex(byte[] data, int group)
        {
            int l = data.Length;
            char[] output = new char[(l << 1) + (group > 0 ? l / group : 0)];
            // two characters form the hex value.
            for (int i = 0, j = 0; i < l; i++)
            {
                if (group > 0 && i % group == 0 && j > 0)
                {
                    output[j++] = '-';
                }

                output[j++] = DIGITS[(byte)(0xF0 & data[i]) >> 4];
                output[j++] = DIGITS[0x0F & data[i]];
            }
            return new string(output);
        }


        public static byte[] decodeHex(string hexString)
        {
            ByteArrayOutputStream bas = new ByteArrayOutputStream();
            for (int i = 0; i < hexString.Length; i += 2)
            {
                int b = Convert.ToInt32(hexString.Substring(i, i + 2), 16);
                bas.write(b);
            }
            return bas.toByteArray();
        }
    }
}
