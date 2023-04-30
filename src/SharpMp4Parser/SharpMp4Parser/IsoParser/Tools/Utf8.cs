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

using System;
using System.Text;

namespace SharpMp4Parser.IsoParser.Tools
{
    /**
     * Converts <code>byte[]</code> -&gt; <code>String</code> and vice versa.
     */
    public sealed class Utf8
    {
        public static byte[] convert(string s)
        {
            try
            {
                if (s != null)
                {
                    return Encoding.UTF8.GetBytes(s);
                }
                else
                {
                    return null;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static string convert(byte[] b)
        {
            try
            {
                if (b != null)
                {
                    return Encoding.UTF8.GetString(b);
                }
                else
                {
                    return null;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static int utf8StringLengthInBytes(string utf8)
        {
            try
            {
                if (utf8 != null)
                {
                    return Encoding.UTF8.GetBytes(utf8).Length;
                }
                else
                {
                    return 0;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
