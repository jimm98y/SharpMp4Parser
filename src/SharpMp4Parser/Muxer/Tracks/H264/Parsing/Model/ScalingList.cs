﻿/*
Copyright (c) 2011 Stanislav Vitvitskiy

Permission is hereby granted, free of charge, to any person obtaining a copy of this
software and associated documentation files (the "Software"), to deal in the Software
without restriction, including without limitation the rights to use, copy, modify,
merge, publish, distribute, sublicense, and/or sell copies of the Software, and to
permit persons to whom the Software is furnished to do so, subject to the following
conditions:

The above copyright notice and this permission notice shall be included in all copies or
substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE
OR OTHER DEALINGS IN THE SOFTWARE.
*/

using SharpMp4Parser.Muxer.Tracks.H264.Parsing.Read;
using SharpMp4Parser.Muxer.Tracks.H264.Parsing.Write;

namespace SharpMp4Parser.Muxer.Tracks.H264.Parsing.Model
{
    /**
     * Scaling list entity
     * <p>
     * capable to serialize / deserialize with CAVLC bitstream</p>
     *
     * @author Stanislav Vitvitskiy
     */
    public class ScalingList
    {

        public int[] scalingList;
        public bool useDefaultScalingMatrixFlag;

        public static ScalingList read(IByteBufferReader input, int sizeOfScalingList)
        {

            ScalingList sl = new ScalingList();
            sl.scalingList = new int[sizeOfScalingList];
            int lastScale = 8;
            int nextScale = 8;
            for (int j = 0; j < sizeOfScalingList; j++)
            {
                if (nextScale != 0)
                {
                    int deltaScale = input.readSE("deltaScale");
                    nextScale = (lastScale + deltaScale + 256) % 256;
                    sl.useDefaultScalingMatrixFlag = (j == 0 && nextScale == 0);
                }
                sl.scalingList[j] = nextScale == 0 ? lastScale : nextScale;
                lastScale = sl.scalingList[j];
            }
            return sl;
        }

        public void write(CAVLCWriter output)
        {
            if (useDefaultScalingMatrixFlag)
            {
                output.writeSE(0, "SPS: ");
                return;
            }

            int lastScale = 8;
            int nextScale = 8;
            for (int j = 0; j < scalingList.Length; j++)
            {
                if (nextScale != 0)
                {
                    int deltaScale = scalingList[j] - lastScale - 256;
                    output.writeSE(deltaScale, "SPS: ");
                }
                lastScale = scalingList[j];
            }
        }

        public override string ToString()
        {
            return "ScalingList{" +
                    "scalingList=" + scalingList +
                    ", useDefaultScalingMatrixFlag=" + useDefaultScalingMatrixFlag +
                    '}';
        }
    }
}
