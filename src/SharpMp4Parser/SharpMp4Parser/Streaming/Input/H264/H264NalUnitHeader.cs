using System;
using System.Collections.Generic;
using System.Text;

namespace SharpMp4Parser.Streaming.Input.H264
{
    public class H264NalUnitHeader
    {
        public int nal_ref_idc;
        public int nal_unit_type;
    }
}
