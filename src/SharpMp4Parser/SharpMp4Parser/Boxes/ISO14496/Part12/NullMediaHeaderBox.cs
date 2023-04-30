/*
 * Copyright 2011 Sebastian Annies, Hamburg, Germany
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

namespace SharpMp4Parser.Boxes.ISO14496.Part12
{
    /**
     * <h1>4cc = "{@value #TYPE}"</h1>
     * Streams other than visual and audio (e.g., timed metadata streams) may use a
     * Null Media Header Box.
     */
    public class NullMediaHeaderBox : AbstractMediaHeaderBox
    {
        public const string TYPE = "nmhd";

        public NullMediaHeaderBox() : base(TYPE)
        { }

        protected override long getContentSize()
        {
            return 4;
        }

        public override void _parseDetails(ByteBuffer content)
        {
            parseVersionAndFlags(content);
        }

        protected override void getContent(ByteBuffer byteBuffer)
        {
            writeVersionAndFlags(byteBuffer);
        }
    }
}