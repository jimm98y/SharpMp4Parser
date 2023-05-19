using SharpMp4Parser.IsoParser.Boxes.SampleGrouping;
using SharpMp4Parser.Java;

namespace SharpMp4Parser.Tests.IsoParser.Boxes.SampleGrouping
{
    [TestClass]
    public class TestRateShareEntryV1 : BoxWriteReadBase<SampleGroupDescriptionBox>
    {
        public override Type getBoxUnderTest()
        {
            return typeof(SampleGroupDescriptionBox);
        }

        public override void setupProperties(Dictionary<String, Object> addPropsHere, SampleGroupDescriptionBox box)
        {
            RateShareEntry rateShareEntry = new RateShareEntry();
            rateShareEntry.setDiscardPriority((short)56);
            rateShareEntry.setMaximumBitrate(1000);
            rateShareEntry.setMinimumBitrate(100);
            rateShareEntry.setOperationPointCut((short)2);
            rateShareEntry.setEntries(new List<RateShareEntry.Entry>() {
                        new RateShareEntry.Entry(100, (short)50),
                        new RateShareEntry.Entry(1000, (short)90)
                });


            addPropsHere.Add("defaultLength", rateShareEntry.size());
            addPropsHere.Add("version", 1);
            addPropsHere.Add("groupEntries", new List<GroupEntry>() { rateShareEntry, rateShareEntry });
            addPropsHere.Add("groupingType", RateShareEntry.TYPE);
        }
    }

    [TestClass]
    public class TestRateShareEntryV0 : BoxWriteReadBase<SampleGroupDescriptionBox>
    {

        public override Type getBoxUnderTest()
        {
            return typeof(SampleGroupDescriptionBox);
        }

        public override void setupProperties(Dictionary<String, Object> addPropsHere, SampleGroupDescriptionBox box)
        {
            RateShareEntry rateShareEntry = new RateShareEntry();
            rateShareEntry.setDiscardPriority((short)56);
            rateShareEntry.setMaximumBitrate(1000);
            rateShareEntry.setMinimumBitrate(100);
            rateShareEntry.setOperationPointCut((short)2);
            rateShareEntry.setEntries(new List<RateShareEntry.Entry>() {
                        new RateShareEntry.Entry(100, (short)50),
                        new RateShareEntry.Entry(1000, (short)90)
                });


            addPropsHere.Add("defaultLength", 0);
            addPropsHere.Add("version", 0);
            addPropsHere.Add("groupEntries", new List<GroupEntry>() { rateShareEntry, rateShareEntry });
            addPropsHere.Add("groupingType", RateShareEntry.TYPE);
        }
    }

    [TestClass]
    public class TestUnknownEntryV1 : BoxWriteReadBase<SampleGroupDescriptionBox>
    {

        public override Type getBoxUnderTest()
        {
            return typeof(SampleGroupDescriptionBox);
        }

        public override void setupProperties(Dictionary<String, Object> addPropsHere, SampleGroupDescriptionBox box)
        {
            UnknownEntry unknownEntry = new UnknownEntry("abcd");
            unknownEntry.setContent(ByteBuffer.wrap(new byte[] { 1, 2, 3, 4, 5, 6 }));

            addPropsHere.Add("defaultLength", unknownEntry.size());
            addPropsHere.Add("version", 1);
            addPropsHere.Add("groupEntries", new List<GroupEntry>() { unknownEntry, unknownEntry });
            addPropsHere.Add("groupingType", "unkn");
        }
    }

    [TestClass]
    public class TestRollRecoveryEntryV1 : BoxWriteReadBase<SampleGroupDescriptionBox>
    {
        public override Type getBoxUnderTest()
        {
            return typeof(SampleGroupDescriptionBox);
        }

        public override void setupProperties(Dictionary<String, Object> addPropsHere, SampleGroupDescriptionBox box)
        {
            RollRecoveryEntry entry = new RollRecoveryEntry();
            entry.setRollDistance((short)6);

            addPropsHere.Add("defaultLength", entry.size());
            addPropsHere.Add("version", 1);
            addPropsHere.Add("groupEntries", new List<GroupEntry>() { entry, entry });
            addPropsHere.Add("groupingType", "roll");
        }
    }

    [TestClass]
    public class TestRollRecoveryEntryV0 : BoxWriteReadBase<SampleGroupDescriptionBox>
    {

        public override Type getBoxUnderTest()
        {
            return typeof(SampleGroupDescriptionBox);
        }


        public override void setupProperties(Dictionary<String, Object> addPropsHere, SampleGroupDescriptionBox box)
        {
            RollRecoveryEntry entry = new RollRecoveryEntry();
            entry.setRollDistance((short)6);

            addPropsHere.Add("defaultLength", 0);
            addPropsHere.Add("version", 0);
            addPropsHere.Add("groupEntries", new List<GroupEntry>() { entry, entry });
            addPropsHere.Add("groupingType", "roll");
        }
    }

    [TestClass]
    public class TestDeadBytesV1 : BoxWriteReadBase<SampleGroupDescriptionBox>
    {
        public override Type getBoxUnderTest()
        {
            return typeof(SampleGroupDescriptionBox);
        }

        public override void setupProperties(Dictionary<String, Object> addPropsHere, SampleGroupDescriptionBox box)
        {
            RollRecoveryEntry entry = new RollRecoveryEntry();
            entry.setRollDistance((short)6);

            addPropsHere.Add("defaultLength", 100);
            addPropsHere.Add("version", 1);
            addPropsHere.Add("groupEntries", new List<GroupEntry>() { entry, entry });
            addPropsHere.Add("groupingType", "roll");
        }
    }

    [TestClass]
    public class TestVariableLengthV1 : BoxWriteReadBase<SampleGroupDescriptionBox>
    {

        public override Type getBoxUnderTest()
        {
            return typeof(SampleGroupDescriptionBox);
        }

        public override void setupProperties(Dictionary<String, Object> addPropsHere, SampleGroupDescriptionBox box)
        {
            UnknownEntry entry1 = new UnknownEntry("abcd");
            entry1.setContent(ByteBuffer.wrap(new byte[] { 1, 2, 3 }));

            UnknownEntry entry2 = new UnknownEntry("abcd");
            entry2.setContent(ByteBuffer.wrap(new byte[] { 1, 2, 3, 4, 5, 6 }));

            addPropsHere.Add("defaultLength", 0);
            addPropsHere.Add("version", 1);
            addPropsHere.Add("groupEntries", new List<GroupEntry>() { entry1, entry2 });
            addPropsHere.Add("groupingType", "abcd");
        }
    }

}
