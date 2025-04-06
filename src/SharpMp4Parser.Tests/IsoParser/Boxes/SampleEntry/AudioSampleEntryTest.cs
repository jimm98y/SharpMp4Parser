using SharpMp4Parser.IsoParser;
using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part12;
using SharpMp4Parser.IsoParser.Boxes.SampleEntry;
using System;
using System.Collections.Generic;

namespace SharpMp4Parser.Tests.IsoParser.Boxes.SampleEntry
{
    /**
     * Created by sannies on 22.05.13.
     */
    [TestClass]
    public class AudioSampleEntryTest : BoxWriteReadBase<AudioSampleEntry>
    {

        protected override AudioSampleEntry getInstance(Type clazz)
        {
            return new AudioSampleEntry(AudioSampleEntry.TYPE2);
        }

        public override Type getBoxUnderTest()
        {
            return typeof(AudioSampleEntry);
        }

        public override void setupProperties(Dictionary<String, Object> addPropsHere, AudioSampleEntry box)
        {
            addPropsHere.Add("boxes", new List<Box>() { new FreeBox(100) });
            addPropsHere.Add("bytesPerFrame", (long)1);
            addPropsHere.Add("bytesPerPacket", (long)2);
            addPropsHere.Add("bytesPerSample", (long)3);
            addPropsHere.Add("channelCount", 4);
            addPropsHere.Add("compressionId", 5);
            addPropsHere.Add("dataReferenceIndex", 7);
            addPropsHere.Add("packetSize", 8);
            addPropsHere.Add("reserved1", 9);
            addPropsHere.Add("reserved2", (long)10);
            addPropsHere.Add("sampleRate", (long)11);
            addPropsHere.Add("sampleSize", 12);
            addPropsHere.Add("samplesPerPacket", (long)13);
            addPropsHere.Add("soundVersion", 1);
            addPropsHere.Add("soundVersion2Data", null);
        }
    }
}
