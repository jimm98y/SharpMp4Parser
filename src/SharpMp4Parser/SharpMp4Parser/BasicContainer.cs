using SharpMp4Parser.Java;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SharpMp4Parser
{
    public class BasicContainer : Container
    {
        private List<Box> boxes = new List<Box>();

        public BasicContainer()
        {
        }

        public BasicContainer(List<Box> boxes)
        {
            this.boxes = boxes;
        }

        public List<Box> getBoxes()
        {
            return boxes;
        }

        public virtual void setBoxes(List<Box> boxes)
        {
            this.boxes = new List<Box>(boxes);
        }

        protected long getContainerSize()
        {
            long contentSize = 0;
            for (int i = 0; i < getBoxes().Count; i++)
            {
                // it's quicker to iterate an array list like that since no iterator
                // needs to be instantiated
                contentSize += boxes[i].getSize();
            }
            return contentSize;
        }

        public virtual List<T> getBoxes<T>(Type clazz) where T: Box
        {
            List<T> boxesToBeReturned = null;
            T oneBox = default;
            List<Box> boxes = getBoxes();
            foreach (Box boxe in boxes)
            {
                //clazz.isInstance(boxe) / clazz == boxe.getClass()?
                // I hereby finally decide to use isInstance

                if (clazz.IsInstanceOfType(boxe))
                {
                    if (oneBox == null)
                    {
                        oneBox = (T)boxe;
                    }
                    else
                    {
                        if (boxesToBeReturned == null)
                        {
                            boxesToBeReturned = new List<T>(2);
                            boxesToBeReturned.Add(oneBox);
                        }
                        boxesToBeReturned.Add((T)boxe);
                    }
                }
            }
            if (boxesToBeReturned != null)
            {
                return boxesToBeReturned;
            }
            else if (oneBox != null)
            {
                return new List<T>() { oneBox };
            }
            else
            {
                return new List<T>();
            }
        }

        public List<T> getBoxes<T>(Type clazz, bool recursive) where T: Box
        {
            List<T> boxesToBeReturned = new List<T>(2);
            List<Box> boxes = getBoxes();
            for (int i = 0; i < boxes.Count; i++)
            {
                Box boxe = boxes[i];
                //clazz.isInstance(boxe) / clazz == boxe.getClass()?
                // I hereby finally decide to use isInstance

                if (clazz.IsInstanceOfType(boxe))
                {
                    boxesToBeReturned.Add((T)boxe);
                }

                if (recursive && boxe is Container)
                {
                    boxesToBeReturned.AddRange(((Container)boxe).getBoxes<T>(clazz, recursive));
                }
            }
            return boxesToBeReturned;
        }

        /**
         * Add <code>box</code> to the container and sets the parent correctly. If <code>box</code> is <code>null</code>
         * nochange will be performed and no error thrown.
         *
         * @param box will be added to the container
         */
        public virtual void addBox(Box box)
        {
            if (box != null)
            {
                boxes = new List<Box>(getBoxes());
                boxes.Add(box);
            }
        }

        public void initContainer(ReadableByteChannel readableByteChannel, long containerSize, BoxParser boxParser)
        {
            long contentProcessed = 0;

            while (containerSize < 0 || contentProcessed < containerSize)
            {
                try
                {
                    ParsableBox b = boxParser.parseBox(readableByteChannel, (this is ParsableBox) ? ((ParsableBox)this).getType() : null);
                    boxes.Add(b);
                    contentProcessed += b.getSize();
                }
                catch (EndOfStreamException e)
                {
                    if (containerSize < 0)
                    {
                        return;
                    }
                    else
                    {
                        throw e;
                    }
                }
            }
        }

        public override string ToString()
        {
            StringBuilder buffer = new StringBuilder();

            buffer.Append(this.GetType().Name).Append("[");
            for (int i = 0; i < boxes.Count; i++)
            {
                if (i > 0)
                {
                    buffer.Append(";");
                }
                buffer.Append(boxes[i]);
            }
            buffer.Append("]");
            return buffer.ToString();
        }

        public void writeContainer(WritableByteChannel bb)
        {
            foreach (Box box in getBoxes())
            {
                box.getBox(bb);
            }
        }
    }
}
