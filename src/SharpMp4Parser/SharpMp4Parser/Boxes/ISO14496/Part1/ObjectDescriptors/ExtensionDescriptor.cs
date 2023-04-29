using System;
using System.Text;

namespace SharpMp4Parser.Boxes.ISO14496.Part1.ObjectDescriptors
{
    /**
     * abstract class ExtensionDescriptor extends BaseDescriptor
     * : bit(8) tag = ExtensionProfileLevelDescrTag, ExtDescrTagStartRange ..
     * ExtDescrTagEndRange {
     * // empty. To be filled by classes extending this class.
     * }
     */
    [Descriptor(Tags = new int[] { 0x13, 106, 107, 108, 109, 110, 111, 112, 113, 114, 115, 116, 117, 118, 119, 120, 121, 122, 123, 124, 125, 126, 127, 128, 129, 130, 131, 132, 133, 134, 135, 136, 137, 138, 139, 140, 141, 142, 143, 144, 145, 146, 147, 148, 149, 150, 151, 152, 153, 154, 155, 156, 157, 158, 159, 160, 161, 162, 163, 164, 165, 166, 167, 168, 169, 170, 171, 172, 173, 174, 175, 176, 177, 178, 179, 180, 181, 182, 183, 184, 185, 186, 187, 188, 189, 190, 191, 192, 193, 194, 195, 196, 197, 198, 199, 200, 201, 202, 203, 204, 205, 206, 207, 208, 209, 210, 211, 212, 213, 214, 215, 216, 217, 218, 219, 220, 221, 222, 223, 224, 225, 226, 227, 228, 229, 230, 231, 232, 233, 234, 235, 236, 237, 238, 239, 240, 241, 242, 243, 244, 245, 246, 247, 248, 249, 250, 251, 252, 253 })]
    public class ExtensionDescriptor : BaseDescriptor
    {
        //private static Logger LOG = LoggerFactory.getLogger(ExtensionDescriptor.class.getName());
        ByteBuffer data;


        //todo: add this better to the tags list?
        //14496-1:2010 p.20:
        //0x6A-0xBF Reserved for ISO use
        //0xC0-0xFE User private
        //
        //ExtDescrTagStartRange = 0x6A
        //ExtDescrTagEndRange = 0xFE
        public static int[] allTags()
        {
            int[] ints = new int[0xFE - 0x6A];

            for (int i = 0x6A; i < 0xFE; i++)
            {
                int pos = i - 0x6A;
                //LOG.trace("pos: {}", pos);
                ints[pos] = i;
            }
            return ints;
        }

        public override void parseDetail(ByteBuffer bb)
        {
            data = bb.slice();
            ((Buffer)bb).position(bb.position() + data.remaining());
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("ExtensionDescriptor");
            sb.Append("tag=").Append(tag);
            sb.Append(",bytes=").Append(Hex.encodeHex(data.array()));
            sb.Append('}');
            return sb.ToString();
        }

        public override ByteBuffer serialize()
        {
            ByteBuffer output = ByteBuffer.allocate(getSize());
            IsoTypeWriter.writeUInt8(output, tag);
            writeSize(output, getContentSize());
            output.put(data.duplicate());
            return output;
        }

        public int getContentSize()
        {
            return data.remaining();
        }
    }
}
