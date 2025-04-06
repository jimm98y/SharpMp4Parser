using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part12;
using SharpMp4Parser.IsoParser.Tools;
using SharpMp4Parser.IsoParser;
using SharpMp4Parser.Java;
using System.Collections.Generic;

namespace SharpMp4Parser.Tests.IsoParser.Boxes.ISO14496.Part12
{
    [TestClass]
    public class ItemLocationBoxTest
    {

        int[] v = new int[] { 1, 2, 4, 8 };

        [TestMethod]
        public void testSimpleRoundTrip()
        {
            foreach (int i in v)
            {
                foreach (int i1 in v)
                {
                    foreach (int i2 in v)
                    {
                        foreach (int i3 in v)
                        {
                            testSimpleRoundTrip(i, i1, i2, i3);
                        }
                    }
                }
            }
        }

        public void testSimpleRoundTrip(int baseOffsetSize, int indexSize, int lengthSize, int offsetSize)
        {
            ItemLocationBox ilocOrig = new ItemLocationBox();
            ilocOrig.setVersion(1);
            ilocOrig.setBaseOffsetSize(baseOffsetSize);
            ilocOrig.setIndexSize(indexSize);
            ilocOrig.setLengthSize(lengthSize);
            ilocOrig.setOffsetSize(offsetSize);

            ByteStream baos = new ByteStream();


            ilocOrig.getBox(Channels.newChannel(baos));


            IsoFile isoFile = new IsoFile(new ByteBufferByteChannel(baos.toByteArray()));

            ItemLocationBox iloc = (ItemLocationBox)isoFile.getBoxes()[0];

            Assert.AreEqual(ilocOrig.getBaseOffsetSize(), iloc.getBaseOffsetSize());
            Assert.AreEqual(ilocOrig.getIndexSize(), iloc.getIndexSize());
            Assert.AreEqual(ilocOrig.getLengthSize(), iloc.getLengthSize());
            Assert.AreEqual(ilocOrig.getOffsetSize(), iloc.getOffsetSize());
            Assert.AreEqual(System.Text.Json.JsonSerializer.Serialize(ilocOrig.getItems()), System.Text.Json.JsonSerializer.Serialize(iloc.getItems()));


        }


        [TestMethod]
        public void testSimpleRoundWithEntriesTrip()
        {
            foreach (int i in v)
            {
                foreach (int i1 in v)
                {
                    foreach (int i2 in v)
                    {
                        foreach (int i3 in v)
                        {
                            testSimpleRoundWithEntriesTrip(i, i1, i2, i3);
                        }
                    }
                }
            }
        }

        public void testSimpleRoundWithEntriesTrip(int baseOffsetSize, int indexSize, int lengthSize, int offsetSize)
        {
            ItemLocationBox ilocOrig = new ItemLocationBox();
            ilocOrig.setVersion(1);
            ilocOrig.setBaseOffsetSize(baseOffsetSize);
            ilocOrig.setIndexSize(indexSize);
            ilocOrig.setLengthSize(lengthSize);
            ilocOrig.setOffsetSize(offsetSize);
            ItemLocationBox.Item item = ilocOrig.createItem(12, 0, 13, 123, new List<ItemLocationBox.Extent>());
            ilocOrig.setItems(new List<ItemLocationBox.Item>() { item });
            ByteStream baos = new ByteStream();
            ilocOrig.getBox(Channels.newChannel(baos));

            IsoFile isoFile = new IsoFile(new ByteBufferByteChannel(baos.toByteArray()));

            ItemLocationBox iloc = (ItemLocationBox)isoFile.getBoxes()[0];

            Assert.AreEqual(ilocOrig.getBaseOffsetSize(), iloc.getBaseOffsetSize());
            Assert.AreEqual(ilocOrig.getIndexSize(), iloc.getIndexSize());
            Assert.AreEqual(ilocOrig.getLengthSize(), iloc.getLengthSize());
            Assert.AreEqual(ilocOrig.getOffsetSize(), iloc.getOffsetSize());
            Assert.AreEqual(System.Text.Json.JsonSerializer.Serialize(ilocOrig.getItems()), System.Text.Json.JsonSerializer.Serialize(iloc.getItems()));


        }

        /*
        [TestMethod]
        public void testSimpleRoundWithEntriesAndExtentsTrip()
        {
            foreach (int i in v)
            {
                foreach (int i1 in v)
                {
                    foreach (int i2 in v)
                    {
                        foreach (int i3 in v)
                        {
                            testSimpleRoundWithEntriesAndExtentsTrip(i, i1, i2, i3);
                        }
                    }
                }
            }
        }
        */

        /*
        public void testSimpleRoundWithEntriesAndExtentsTrip(int baseOffsetSize, int indexSize, int lengthSize, int offsetSize)
        {
            ItemLocationBox ilocOrig = new ItemLocationBox();
            ilocOrig.setVersion(1);
            ilocOrig.setBaseOffsetSize(baseOffsetSize);
            ilocOrig.setIndexSize(indexSize);
            ilocOrig.setLengthSize(lengthSize);
            ilocOrig.setOffsetSize(offsetSize);
            List<ItemLocationBox.Extent> extents = new List<ItemLocationBox.Extent>();
            ItemLocationBox.Extent extent = ilocOrig.createExtent(12, 13, 1);
            extents.Add(extent);
            ItemLocationBox.Item item = ilocOrig.createItem(12, 0, 13, 123, extents);
            ilocOrig.setItems(new List<ItemLocationBox.Item>() { item });
            
            File f = File.createTempFile(this.getClass().getSimpleName(), "");
            f.deleteOnExit();
            FileChannel fc = new FileByteStreamBase(f).getChannel();
            ilocOrig.getBox(fc);
            fc.close();


            IsoFile isoFile = new IsoFile(new FileByteStreamBase(f).getChannel());

            ItemLocationBox iloc = (ItemLocationBox)isoFile.getBoxes()[0];

            Assert.AreEqual(ilocOrig.getBaseOffsetSize(), iloc.getBaseOffsetSize());
            Assert.AreEqual(ilocOrig.getIndexSize(), iloc.getIndexSize());
            Assert.AreEqual(ilocOrig.getLengthSize(), iloc.getLengthSize());
            Assert.AreEqual(ilocOrig.getOffsetSize(), iloc.getOffsetSize());
            Assert.AreEqual(ilocOrig.getItems(), iloc.getItems());


        }
        */

        [TestMethod]
        public void testExtent()
        {
            testExtent(1, 2, 4, 8);
            testExtent(2, 4, 8, 1);
            testExtent(4, 8, 1, 2);
            testExtent(8, 1, 2, 4);
        }

        public void testExtent(int a, int b, int c, int d)
        {
            ItemLocationBox iloc = new ItemLocationBox();
            iloc.setVersion(1);
            iloc.setBaseOffsetSize(a);
            iloc.setIndexSize(b);
            iloc.setLengthSize(c);
            iloc.setOffsetSize(d);
            ItemLocationBox.Extent e1 = iloc.createExtent(123, 124, 125);
            ByteBuffer bb = ByteBuffer.allocate(e1.getSize());
            e1.getContent(bb);
            Assert.IsTrue(bb.remaining() == 0);
            ((Java.Buffer)bb).rewind();
            ItemLocationBox.Extent e2 = iloc.createExtent(bb);

            Assert.AreEqual(System.Text.Json.JsonSerializer.Serialize(e1), System.Text.Json.JsonSerializer.Serialize(e2));


        }

        [TestMethod]
        public void testItem()
        {
            testItem(1, 2, 4, 8);
            testItem(2, 4, 8, 1);
            testItem(4, 8, 1, 2);
            testItem(8, 1, 2, 4);
        }

        public void testItem(int a, int b, int c, int d)
        {
            ItemLocationBox iloc = new ItemLocationBox();
            iloc.setVersion(1);
            iloc.setBaseOffsetSize(a);
            iloc.setIndexSize(b);
            iloc.setLengthSize(c);
            iloc.setOffsetSize(d);
            ItemLocationBox.Item e1 = iloc.createItem(65, 1, 0, 66, new List<ItemLocationBox.Extent>());
            ByteBuffer bb = ByteBuffer.allocate(e1.getSize());
            e1.getContent(bb);
            Assert.IsTrue(bb.remaining() == 0);
            ((Java.Buffer)bb).rewind();
            ItemLocationBox.Item e2 = iloc.createItem(bb);

            Assert.AreEqual(System.Text.Json.JsonSerializer.Serialize(e1), System.Text.Json.JsonSerializer.Serialize(e2));
        }

        [TestMethod]
        public void testItemVersionZero()
        {
            testItemVersionZero(1, 2, 4, 8);
            testItemVersionZero(2, 4, 8, 1);
            testItemVersionZero(4, 8, 1, 2);
            testItemVersionZero(8, 1, 2, 4);
        }

        public void testItemVersionZero(int a, int b, int c, int d)
        {
            ItemLocationBox iloc = new ItemLocationBox();
            iloc.setVersion(0);
            iloc.setBaseOffsetSize(a);
            iloc.setIndexSize(b);
            iloc.setLengthSize(c);
            iloc.setOffsetSize(d);
            ItemLocationBox.Item e1 = iloc.createItem(65, 0, 1, 66, new List<ItemLocationBox.Extent>());
            ByteBuffer bb = ByteBuffer.allocate(e1.getSize());
            e1.getContent(bb);
            Assert.IsTrue(bb.remaining() == 0);
            ((Java.Buffer)bb).rewind();
            ItemLocationBox.Item e2 = iloc.createItem(bb);

            Assert.AreEqual(System.Text.Json.JsonSerializer.Serialize(e1), System.Text.Json.JsonSerializer.Serialize(e2));
        }
    }
}