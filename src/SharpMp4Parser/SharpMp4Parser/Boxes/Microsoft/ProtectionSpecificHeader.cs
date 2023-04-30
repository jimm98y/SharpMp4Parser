using System;
using System.Collections.Generic;
using System.Text;

namespace SharpMp4Parser.Boxes.Microsoft
{
    public abstract class ProtectionSpecificHeader
    {
        protected static Dictionary<UUID, Type> uuidRegistry = new Dictionary<UUID, Type>();

        public static ProtectionSpecificHeader createFor(UUID systemId, ByteBuffer bufferWrapper)
        {
            Type aClass = uuidRegistry[systemId];

            ProtectionSpecificHeader protectionSpecificHeader = null;
            if (aClass != null)
            {
                try
                {
                    protectionSpecificHeader = (ProtectionSpecificHeader)Activator.CreateInstance(aClass);
                }
                catch (Exception e)
                {
                    throw;
                }
            }

            if (protectionSpecificHeader == null)
            {
                protectionSpecificHeader = new GenericHeader();
            }
            protectionSpecificHeader.parse(bufferWrapper);
            return protectionSpecificHeader;

        }

        public abstract UUID getSystemId();

        public override bool Equals(object obj)
        {
            throw new Exception("somebody called equals on me but that's not supposed to happen.");
        }

        public abstract void parse(ByteBuffer byteBuffer);

        public abstract ByteBuffer getData();

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("ProtectionSpecificHeader");
            sb.Append("{data=");
            ByteBuffer data = getData().duplicate();
            ((Buffer)data).rewind();
            byte[] bytes = new byte[data.limit()];
            data.get(bytes);
            sb.Append(Hex.encodeHex(bytes));
            sb.Append('}');
            return sb.ToString();
        }
    }
}
