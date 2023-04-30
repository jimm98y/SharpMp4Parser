using SharpMp4Parser.Boxes.Microsoft.ContentProtection;
using SharpMp4Parser.Java;
using SharpMp4Parser.Tools;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpMp4Parser.Boxes.Microsoft
{
    public abstract class ProtectionSpecificHeader
    {
        protected static Dictionary<Uuid, Type> uuidRegistry = new Dictionary<Uuid, Type>();

        public static ProtectionSpecificHeader createFor(Uuid systemId, ByteBuffer bufferWrapper)
        {
            Type aClass = uuidRegistry[systemId];

            ProtectionSpecificHeader protectionSpecificHeader = null;
            if (aClass != null)
            {
                try
                {
                    protectionSpecificHeader = (ProtectionSpecificHeader)Activator.CreateInstance(aClass);
                }
                catch (Exception)
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

        public abstract Uuid getSystemId();

        public override bool Equals(object obj)
        {
            throw new Exception("somebody called equals on me but that's not supposed to happen.");
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public abstract void parse(ByteBuffer byteBuffer);

        public abstract ByteBuffer getData();

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("ProtectionSpecificHeader");
            sb.Append("{data=");
            ByteBuffer data = getData().duplicate();
            ((Java.Buffer)data).rewind();
            byte[] bytes = new byte[data.limit()];
            data.get(bytes);
            sb.Append(Hex.encodeHex(bytes));
            sb.Append('}');
            return sb.ToString();
        }
    }
}
