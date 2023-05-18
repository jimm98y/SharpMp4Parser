using SharpMp4Parser.IsoParser.Tools;
using SharpMp4Parser.IsoParser;
using SharpMp4Parser.Java;
using System.Reflection;
using System.Diagnostics;
using SharpMp4Parser.IsoParser.Support;

namespace SharpMp4Parser.Tests.IsoParser.Boxes
{
    public abstract class BoxRoundtripTest
    {

        /*


        @Parameterized.Parameters
        public static Collection<Object[]> data() {


            return Collections.singletonList(
                    new Object[]{new Box(),
                            new Map.Entry[]{
                                    new E("prop", 1),
                                    new E("prop2", 21)}
                    });
        }



         */

        private static readonly List<String> skipList = new List<string>() {
                "class",
                "flags",
                "isoFile",
                "parent",
                "parsed",
                "path",
                "size",
                "offset",
                "type",
                "userType",
                "version" };

        protected void roundtrip(ParsableBox parsableBoxUnderTest, params KeyValuePair<String, Object>[] properties)
        {
            var props = new Dictionary<string, object>();
            foreach (var property in properties)
            {
                props.Add(property.Key, property.Value);
            }

            TypeInfo beanInfo = parsableBoxUnderTest.GetType().GetTypeInfo();
            FieldInfo[] propertyDescriptors = beanInfo.GetPrivateFields();
            foreach (string property in props.Keys)
            {
                bool found = false;
                foreach (FieldInfo propertyDescriptor in propertyDescriptors)
                {
                    if (property.Equals(propertyDescriptor.Name))
                    {
                        found = true;
                        try
                        {
                            propertyDescriptor.getWriteMethod(beanInfo).Invoke(parsableBoxUnderTest, new object[] { props[property] });
                        }
                        catch (Exception e)
                        {

                            Debug.WriteLine(propertyDescriptor.getWriteMethod(beanInfo).Name + "(" + propertyDescriptor.getWriteMethod(beanInfo).GetParameters()[0].ParameterType.Name + ");");
                            Debug.WriteLine("Called with " + props[property].GetType());


                            throw;
                        }
                        // do the next assertion on string level to not trap into the long vs java.lang.Long pitfall
                        Assert.AreEqual(props[property], propertyDescriptor.getReadMethod(beanInfo).Invoke(parsableBoxUnderTest, null), "The symmetry between getter/setter of " + property + " is not given.");
                    }
                }
                if (!found)
                {
                    Assert.Fail("Could not find any property descriptor for " + property);
                }
            }


            ByteStream baos = new ByteStream();
            ByteStream wbc = Channels.newChannel(baos);
            parsableBoxUnderTest.getBox(wbc);
            //wbc.close();
            //baos.close();

            IsoFile singleBoxIsoFile = new IsoFile(new ByteBufferByteChannel(baos.toByteArray()));

            Assert.AreEqual(parsableBoxUnderTest.getSize(), baos.position(), "Expected box and file size to be the same");
            Assert.AreEqual(1, singleBoxIsoFile.getBoxes().Count, "Expected a single box in the IsoFile structure");
            Assert.AreEqual(parsableBoxUnderTest.GetType(), singleBoxIsoFile.getBoxes()[0].GetType(), "Expected to find a box of different type ");

            Box parsedBox = singleBoxIsoFile.getBoxes()[0];


            foreach (String property in props.Keys)
            {
                bool found = false;
                foreach (FieldInfo propertyDescriptor in propertyDescriptors)
                {
                    if (property.Equals(propertyDescriptor.Name))
                    {
                        found = true;
                        if (props[property] is int[])
                        {
                            Assert.IsTrue(Enumerable.SequenceEqual((int[])props[property], (int[])propertyDescriptor.getReadMethod(beanInfo).Invoke(parsedBox, null)), "Writing and parsing changed the value of " + property);
                        }
                        else if (props[property] is byte[])
                        {
                            Assert.IsTrue(Enumerable.SequenceEqual((byte[])props[property], (byte[])propertyDescriptor.getReadMethod(beanInfo).Invoke(parsedBox, null)), "Writing and parsing changed the value of " + property);
                        }
                        else if (props[property] is long[])
                        {
                            Assert.IsTrue(Enumerable.SequenceEqual((long[])props[property], (long[])propertyDescriptor.getReadMethod(beanInfo).Invoke(parsedBox, null)), "Writing and parsing changed the value of " + property);
                        }
                        else
                        {
                            Assert.AreEqual(props[property], propertyDescriptor.getReadMethod(beanInfo).Invoke(parsedBox, null), "Writing and parsing changed the value of " + property);
                        }
                    }
                }
                if (!found)
                {
                    Assert.Fail("Could not find any property descriptor for " + property);
                }
            }

            Assert.AreEqual(parsableBoxUnderTest.getSize(), parsedBox.getSize(), "Writing and parsing should not change the box size.");

            bool output = false;
            foreach (FieldInfo propertyDescriptor in propertyDescriptors)
            {
                if (!props.ContainsKey(propertyDescriptor.Name))
                {
                    if (!skipList.Contains(propertyDescriptor.Name))
                    {
                        if (!output)
                        {
                            Debug.WriteLine("No value given for the following properties: ");
                            output = true;
                        }
                        Debug.WriteLine(String.Format("new E(\"{0}\", ({1})) ),", propertyDescriptor.Name, propertyDescriptor.FieldType.Name));
                    }
                }
            }

        }

        class DummyContainerBox : AbstractContainerBox
        {

            public DummyContainerBox(String type) : base(type)
            {

            }
        }
    }
}
