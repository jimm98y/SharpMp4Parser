using System.Threading;

namespace SharpMp4Parser.Java
{
    public class CountDownLatch
    {
        private object lockObj = new object();
        private int counter;

        public CountDownLatch(int counter)
        {
            this.counter = counter;
        }

        public void await()
        {
            lock (lockObj)
            {
                while (counter > 0)
                {
                    Monitor.Wait(lockObj);
                }
            }
        }

        public int getCount()
        {
            return counter;
        }

        public void countDown()
        {
            lock (lockObj)
            {
                counter--;
                Monitor.PulseAll(lockObj);
            }
        }
    }
}
