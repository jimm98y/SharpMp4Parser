using SharpMp4Parser.Java;
using System.Linq;

namespace SharpMp4Parser.Tests.Streaming.Input
{
    internal sealed class MyByteArrayOutputStream : ByteStream
    {
        public byte[] toByteArrayLess3()
        {
            var data = toByteArray();

            if (position() > 3)
            {
                return data.Take(position() - 3).ToArray();
            }
            else
            {
                return null;
            }
        }
    }
}
