using SharpMp4Parser.Muxer.Tracks.H264.Parsing.Read;

namespace SharpMp4Parser.Muxer.Tracks.H265
{
    public class SubLayerHrdParameters
    {
        public SubLayerHrdParameters(int subLayerId, int[] cpb_cnt_minus1, bool sub_pic_hrd_params_present_flag, CAVLCReader bsr)
        {
            int CpbCnt = cpb_cnt_minus1[subLayerId];
            int[] bit_rate_value_minus1 = new int[CpbCnt + 1];
            int[] cpb_size_value_minus1 = new int[CpbCnt + 1];
            int[] cpb_size_du_value_minus1 = new int[CpbCnt + 1];
            int[] bit_rate_du_value_minus1 = new int[CpbCnt + 1];
            bool[] cbr_flag = new bool[CpbCnt + 1];
            for (int i = 0; i <= CpbCnt; i++)
            {
                bit_rate_value_minus1[i] = bsr.readUE("bit_rate_value_minus1[i]");
                cpb_size_value_minus1[i] = bsr.readUE("cpb_size_value_minus1[i]");
                if (sub_pic_hrd_params_present_flag)
                {
                    cpb_size_du_value_minus1[i] = bsr.readUE("cpb_size_du_value_minus1[i]");
                    bit_rate_du_value_minus1[i] = bsr.readUE("bit_rate_du_value_minus1[i]");
                }
                cbr_flag[i] = bsr.readBool("cbr_flag[i]");
            }
        }
    }
}
