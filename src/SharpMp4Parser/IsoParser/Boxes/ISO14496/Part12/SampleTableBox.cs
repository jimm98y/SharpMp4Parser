﻿/*  
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

using SharpMp4Parser.IsoParser.Support;
using SharpMp4Parser.IsoParser.Tools;

namespace SharpMp4Parser.IsoParser.Boxes.ISO14496.Part12
{
    /**
     * <h1>4cc = "{@value #TYPE}"</h1>
     * The sample table contains all the time and data indexing of the media samples in a track. Using the tables
     * here, it is possible to locate samples in time, determine their type (e.g. I-frame or not), and determine their
     * size, container, and offset into that container.  <br>
     * If the track that contains the Sample Table Box references no data, then the Sample Table Box does not need
     * to contain any sub-boxes (this is not a very useful media track).                                          <br>
     * If the track that the Sample Table Box is contained in does reference data, then the following sub-boxes are
     * required: Sample Description, Sample Size, Sample To Chunk, and Chunk Offset. Further, the Sample
     * Description Box shall contain at least one entry. A Sample Description Box is required because it contains the
     * data reference index field which indicates which Data Reference Box to use to retrieve the media samples.
     * Without the Sample Description, it is not possible to determine where the media samples are stored. The Sync
     * Sample Box is optional. If the Sync Sample Box is not present, all samples are sync samples.<br>
     * Annex A provides a narrative description of random access using the structures defined in the Sample Table
     * Box.
     */
    public class SampleTableBox : AbstractContainerBox
    {
        public const string TYPE = "stbl";
        //private SampleToChunkBox sampleToChunkBox;

        public SampleTableBox() : base(TYPE)
        { }

        public SampleDescriptionBox getSampleDescriptionBox()
        {
            return Path.getPath<SampleDescriptionBox>(this, "stsd");
        }

        public SampleSizeBox getSampleSizeBox()
        {
            return Path.getPath<SampleSizeBox>(this, "stsz");
        }

        public SampleToChunkBox getSampleToChunkBox()
        {
            return Path.getPath<SampleToChunkBox>(this, "stsc");
        }

        public ChunkOffsetBox getChunkOffsetBox()
        {
            foreach (Box box in getBoxes())
            {
                if (box is ChunkOffsetBox)
                {
                    return (ChunkOffsetBox)box;
                }
            }
            return null;
        }

        public TimeToSampleBox getTimeToSampleBox()
        {
            return Path.getPath<TimeToSampleBox>(this, "stts");
        }

        public SyncSampleBox getSyncSampleBox()
        {
            return Path.getPath<SyncSampleBox>(this, "stss");
        }

        public CompositionTimeToSample getCompositionTimeToSample()
        {
            return Path.getPath<CompositionTimeToSample>(this, "ctts");
        }

        public SampleDependencyTypeBox getSampleDependencyTypeBox()
        {
            return Path.getPath<SampleDependencyTypeBox>(this, "sdtp");
        }
    }
}
