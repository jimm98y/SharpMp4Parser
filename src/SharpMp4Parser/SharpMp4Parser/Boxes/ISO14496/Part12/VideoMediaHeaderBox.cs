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

namespace SharpMp4Parser.Boxes.ISO14496.Part12
{
    /**
     * <h1>4cc = "{@value #TYPE}"</h1>
     * The video media header contains general presentation information, independent of the coding, for video
     * media. Note that the flags field has the value 1.
     */
    public class VideoMediaHeaderBox : AbstractMediaHeaderBox
    {
        public const string TYPE = "vmhd";
        private int graphicsmode = 0;
        private int[] opcolor = new int[] { 0, 0, 0 };

        public VideoMediaHeaderBox() : base(TYPE)
        {
            this.flags = 1;
        }

        public int getGraphicsmode()
        {
            return graphicsmode;
        }

        public void setGraphicsmode(int graphicsmode)
        {
            this.graphicsmode = graphicsmode;
        }

        public int[] getOpcolor()
        {
            return opcolor;
        }

        public void setOpcolor(int[] opcolor)
        {
            this.opcolor = opcolor;
        }

        protected long getContentSize()
        {
            return 12;
        }

        public override void _parseDetails(ByteBuffer content)
        {
            parseVersionAndFlags(content);
            graphicsmode = IsoTypeReader.readUInt16(content);
            opcolor = new int[3];
            for (int i = 0; i < 3; i++)
            {
                opcolor[i] = IsoTypeReader.readUInt16(content);
            }
        }

        protected override void getContent(ByteBuffer byteBuffer)
        {
            writeVersionAndFlags(byteBuffer);
            IsoTypeWriter.writeUInt16(byteBuffer, graphicsmode);
            foreach (int anOpcolor in opcolor)
            {
                IsoTypeWriter.writeUInt16(byteBuffer, anOpcolor);
            }
        }

        public override string ToString()
        {
            return "VideoMediaHeaderBox[graphicsmode=" + getGraphicsmode() + ";opcolor0=" + getOpcolor()[0] + ";opcolor1=" + getOpcolor()[1] + ";opcolor2=" + getOpcolor()[2] + "]";
        }
    }
}
