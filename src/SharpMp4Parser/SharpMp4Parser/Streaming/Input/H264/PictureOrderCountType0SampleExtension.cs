
using SharpMp4Parser.Muxer.Tracks.H264;

namespace SharpMp4Parser.Streaming.Input.H264
{
    public class PictureOrderCountType0SampleExtension : SampleExtension
    {
        public int picOrderCntMsb;
        public int picOrderCountLsb;

        public PictureOrderCountType0SampleExtension(SliceHeader currentSlice, PictureOrderCountType0SampleExtension previous)
        {
            int prevPicOrderCntLsb = 0;
            int prevPicOrderCntMsb = 0;
            if (previous != null)
            {
                prevPicOrderCntLsb = previous.picOrderCountLsb;
                prevPicOrderCntMsb = previous.picOrderCntMsb;
            }

            int max_pic_order_count_lsb = (1 << (currentSlice.sps.log2_max_pic_order_cnt_lsb_minus4 + 4));
            // System.out.print(" pic_order_cnt_lsb " + pic_order_cnt_lsb + " " + max_pic_order_count);
            picOrderCountLsb = currentSlice.pic_order_cnt_lsb;
            picOrderCntMsb = 0;
            if ((picOrderCountLsb < prevPicOrderCntLsb) &&
                    ((prevPicOrderCntLsb - picOrderCountLsb) >= (max_pic_order_count_lsb / 2)))
            {
                picOrderCntMsb = prevPicOrderCntMsb + max_pic_order_count_lsb;
            }
            else if ((picOrderCountLsb > prevPicOrderCntLsb) &&
                    ((picOrderCountLsb - prevPicOrderCntLsb) > (max_pic_order_count_lsb / 2)))
            {
                picOrderCntMsb = prevPicOrderCntMsb - max_pic_order_count_lsb;
            }
            else
            {
                picOrderCntMsb = prevPicOrderCntMsb;
            }
        }

        public int getPoc()
        {
            return picOrderCntMsb + picOrderCountLsb;
        }

        public override string ToString()
        {
            return "picOrderCntMsb=" + picOrderCntMsb + ", picOrderCountLsb=" + picOrderCountLsb;
        }
    }
}