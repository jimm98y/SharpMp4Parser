using SharpMp4Parser.IsoParser.Support;
using SharpMp4Parser.IsoParser;
using Container = SharpMp4Parser.IsoParser.Container;
using System.Diagnostics;
using System.Reflection;

namespace SharpMp4Parser.Tests.Streaming.Input
{
    /**
     * Walks through a Container and its children to see that no getter throws any exception.
     */
    public sealed class Walk
    {
        private Walk()
        {
        }

        public static void through(Container container)
        {
            foreach (Box b in container.getBoxes())
            {
                List<Box> myBoxes = container.getBoxes<Box>(b.GetType());
                bool found = false;
                foreach (Box myBox in myBoxes)
                {
                    if (myBox == b)
                    {
                        found = true;
                    }
                }
                if (!found)
                {
                    throw new Exception("Didn't find the box");
                }

                if (b is Container)
                {
                    through((Container)b);
                }

                b.ToString(); // Just test if some execption is trown

                var beanInfo = b.GetType().GetTypeInfo();
                MethodInfo[] propertyDescriptors = beanInfo.GetMethods();

                foreach (MethodInfo propertyDescriptor in propertyDescriptors)
                {
                    string name = propertyDescriptor.Name;
                    if (name.StartsWith("get") && propertyDescriptor.GetParameters().Length == 0)
                    {
                        propertyDescriptor.Invoke(b, null);
                    }
                }
                if (b is AbstractBox)
                {
                    Debug.Assert(((AbstractBox)b).IsParsed(), "Box (" + b.GetType().Name + ") is not parsed.");
                }
            }
        }
    }
}