using System.ComponentModel;
using System.Diagnostics;

namespace SharpMp4Parser.Support
{
    /**
     * Compares boxes for testing purposes.
     */
    public class BoxComparator
    {
        public static bool isIgnore(Container refc, Box b, string[] ignores)
        {
            foreach (string ignore in ignores)
            {
                if (Path.isContained(refc, b, ignore))
                {
                    return true;
                }
            }
            return false;
        }

        public static void check(Container root1, Box b1, Container root2, Box b2, params string[] ignores)
        {
            //System.err.println(b1.getType() + " - " + b2.getType());
            Debug.Assert(b1.getType().equals(b2.getType()));
            if (!isIgnore(root1, b1, ignores))
            {
                //    System.err.println(b1.getType());
                Debug.Assert(b1.getType().Equals(b2.getType()), "Type differs. \ntypetrace ref : " + b1 + "\ntypetrace new : " + b2);
                if (b1 is Container ^ !(b2 is Container))
                {
                    if (b1 is Container)
                    {
                        check(root1, (Container)b1, root2, (Container)b2, ignores);
                    }
                    else
                    {
                        checkBox(root1, b1, root2, b2, ignores);
                    }
                }
                else
                {
                    Debug.Assert(false, "Either both boxes are container boxes or none");
                }
            }
        }

        private static void checkBox(Container root1, Box b1, Container root2, Box b2, string[] ignores)
        {
            if (!isIgnore(root1, b1, ignores))
            {
                ByteArrayOutputStream baos1 = new ByteArrayOutputStream();
                ByteArrayOutputStream baos2 = new ByteArrayOutputStream();

                b1.getBox(Channels.newChannel(baos1));
                b2.getBox(Channels.newChannel(baos2));

                baos1.close();
                baos2.close();

                Debug.Assert(Base64.getEncoder().encodeToString(baos1.toByteArray()).Equals(Base64.getEncoder().encodeToString(baos2.toByteArray())), "Box at " + b1 + " differs from reference\n\n" + b1.ToString() + "\n" + b2.ToString());
            }
        }

        public static void check(Container cb1, Container cb2, params string[] ignores)
        {
            check(cb1, cb1, cb2, cb2, ignores);
        }

        public static void check(Container root1, Container cb1, Container root2, Container cb2, params string[] ignores)
        {
            Iterator<Box> it1 = cb1.getBoxes().iterator();
            Iterator<Box> it2 = cb2.getBoxes().iterator();

            while (it1.hasNext() && it2.hasNext())
            {
                Box b1 = it1.next();
                Box b2 = it2.next();

                check(root1, b1, root2, b2, ignores);
            }
            Debug.Assert(!it1.hasNext(), "There is a box missing in the current output of the tool: " + it1.next());
            Debug.Assert(!it2.hasNext(), "There is a box too much in the current output of the tool: " + it2.next());
        }
    }
}
