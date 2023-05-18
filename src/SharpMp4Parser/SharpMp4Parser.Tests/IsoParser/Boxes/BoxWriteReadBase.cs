using SharpMp4Parser.IsoParser.Tools;
using SharpMp4Parser.IsoParser;
using SharpMp4Parser.Java;
using SharpMp4Parser.IsoParser.Support;
using System.Diagnostics;
using System.Reflection;
using System.Collections;
using System.Xml.Linq;
using System;

namespace SharpMp4Parser.Tests.IsoParser.Boxes
{
    public static class FieldInfoExtensions
    {
        public static FieldInfo[] GetPrivateFields(this TypeInfo t)
        {
            TypeInfo? tt = t;
            List<FieldInfo> fields = new List<FieldInfo>();
            do
            {
                var field = tt.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
                fields.AddRange(field);
                tt = tt.BaseType?.GetTypeInfo();
            } while (tt != null);

            return fields.Distinct().ToArray();
        }

        private static string ToFirstCharacterUpper(string a)
        {
            return Char.ToUpper(a[0]) + a.Substring(1);
        }

        public static MethodInfo getWriteMethod(this FieldInfo info, TypeInfo beanInfo)
        {
            string writeMethod = $"set{ToFirstCharacterUpper(info.Name)}";
            return beanInfo.GetMethods().First(x => x.Name == writeMethod);
        }

        public static MethodInfo getReadMethod(this FieldInfo info, TypeInfo beanInfo)
        {
            string readMethod = $"get{ToFirstCharacterUpper(info.Name)}";
            return beanInfo.GetMethods().First(x => x.Name == readMethod);
        }
    }

    public abstract class BoxWriteReadBase<T> where T : ParsableBox
    {
        private static readonly List<string> skipList = new List<string>() {
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
        String dummyParent = null;

        protected BoxWriteReadBase(string dummyParent)
        {
            this.dummyParent = dummyParent;
        }

        protected BoxWriteReadBase()
        {
        }

        public abstract Type getBoxUnderTest();

        public abstract void setupProperties(Dictionary<string, object> addPropsHere, T box);


        protected T getInstance(Type clazz)
        {
            return (T)Activator.CreateInstance(clazz);
        }

        [TestMethod]
        public void roundtrip()
        {
            Type clazz = getBoxUnderTest();
            T box = getInstance(clazz);
            TypeInfo beanInfo = box.GetType().GetTypeInfo();
            FieldInfo[] propertyDescriptors = beanInfo.GetPrivateFields();
            Dictionary<string, object> props = new Dictionary<string, object>();
            setupProperties(props, box);
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
                            propertyDescriptor.getWriteMethod(beanInfo).Invoke(box, new object[] { props[property] });
                        }
                        catch (Exception)
                        {

                            Debug.WriteLine(propertyDescriptor.getWriteMethod(beanInfo).Name + "(" + propertyDescriptor.getWriteMethod(beanInfo).GetParameters()[0].ParameterType.Name + ");");
                            Debug.WriteLine("Called with " + props[property].GetType());

                            throw;
                        }

                        // do the next assertion on string level to not trap into the long vs java.lang.Long pitfall
                        if (props[property] is IEnumerable<dynamic> && !(props[property] is string))
                        {
                            Assert.IsTrue(Enumerable.SequenceEqual<dynamic>(props[property] as IEnumerable<dynamic>, (IEnumerable<dynamic>)propertyDescriptor.getReadMethod(beanInfo).Invoke(box, null)), "The symmetry between getter/setter of " + property + " is not given.");
                        }
                        else
                        {
                            Assert.AreEqual(props[property], (Object)propertyDescriptor.getReadMethod(beanInfo).Invoke(box, null), "The symmetry between getter/setter of " + property + " is not given.");
                        }
                    }
                }
                if (!found)
                {
                    Assert.Fail("Could not find any property descriptor for " + property);
                }
            }


            ByteStream baos = new ByteStream();
            ByteStream wbc = Channels.newChannel(baos);
            box.getBox(wbc);
            //wbc.close();
            //baos.close();

            DummyContainerBox singleBoxIsoFile = new DummyContainerBox(dummyParent);
            singleBoxIsoFile.initContainer(new ByteBufferByteChannel(baos.toByteArray()), baos.toByteArray().Length, new PropertyBoxParserImpl());
            Assert.AreEqual(box.getSize(), baos.toByteArray().Length, "Expected box and file size to be the same");
            Assert.AreEqual(1, singleBoxIsoFile.getBoxes().Count, "Expected a single box in the IsoFile structure");
            Assert.AreEqual(clazz, singleBoxIsoFile.getBoxes()[0].GetType(), "Expected to find a box of different type ");

            T parsedBox = (T)singleBoxIsoFile.getBoxes()[0];


            foreach (string property in props.Keys)
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
                            if (props[property] is IEnumerable<dynamic> && !(props[property] is string))
                            {
                                Assert.IsTrue(Enumerable.SequenceEqual<dynamic>(props[property] as IEnumerable<dynamic>, (IEnumerable<dynamic>)propertyDescriptor.getReadMethod(beanInfo).Invoke(parsedBox, null)), "Writing and parsing changed the value of " + property);
                            }
                            else
                            {
                                Assert.AreEqual(props[property], (Object)propertyDescriptor.getReadMethod(beanInfo).Invoke(parsedBox, null), "Writing and parsing changed the value of " + property);
                            }
                        }
                    }
                }
                if (!found)
                {
                    Assert.Fail("Could not find any property descriptor for " + property);
                }
            }

            Assert.AreEqual(box.getSize(), parsedBox.getSize(), "Writing and parsing should not change the box size.");

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
                        Debug.WriteLine(String.Format("addPropsHere.put(\"{0}\", ({1}) );", propertyDescriptor.Name, propertyDescriptor.Name));
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