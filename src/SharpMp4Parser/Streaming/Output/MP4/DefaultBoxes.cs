using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part12;
using SharpMp4Parser.IsoParser;
using SharpMp4Parser.Streaming.Extensions;
using System.Collections.Generic;

namespace SharpMp4Parser.Streaming.Output.MP4
{
    public abstract class DefaultBoxes
    {
        public Box createFtyp()
        {
            List<string> minorBrands = new List<string>();
            minorBrands.Add("isom");
            minorBrands.Add("iso2");
            minorBrands.Add("avc1");
            minorBrands.Add("iso6");
            minorBrands.Add("mp41");
            return new FileTypeBox("isom", 512, minorBrands);
        }

        protected Box createMdiaHdlr(StreamingTrack streamingTrack)
        {
            HandlerBox hdlr = new HandlerBox();
            hdlr.setHandlerType(streamingTrack.getHandler());
            return hdlr;
        }

        protected Box createMdia(StreamingTrack streamingTrack)
        {
            MediaBox mdia = new MediaBox();
            mdia.addBox(createMdhd(streamingTrack));
            mdia.addBox(createMdiaHdlr(streamingTrack));
            mdia.addBox(createMinf(streamingTrack));
            return mdia;
        }

        abstract protected Box createMdhd(StreamingTrack streamingTrack);

        abstract protected Box createMvhd();

        protected Box createMinf(StreamingTrack streamingTrack)
        {
            MediaInformationBox minf = new MediaInformationBox();
            if (streamingTrack.getHandler().Equals("vide"))
            {
                minf.addBox(new VideoMediaHeaderBox());
            }
            else if (streamingTrack.getHandler().Equals("soun"))
            {
                minf.addBox(new SoundMediaHeaderBox());
            }
            else if (streamingTrack.getHandler().Equals("text"))
            {
                minf.addBox(new NullMediaHeaderBox());
            }
            else if (streamingTrack.getHandler().Equals("subt"))
            {
                minf.addBox(new SubtitleMediaHeaderBox());
            }
            else if (streamingTrack.getHandler().Equals("hint"))
            {
                minf.addBox(new HintMediaHeaderBox());
            }
            else if (streamingTrack.getHandler().Equals("sbtl"))
            {
                minf.addBox(new NullMediaHeaderBox());
            }
            minf.addBox(createDinf());
            minf.addBox(createStbl(streamingTrack));
            return minf;
        }

        protected Box createStbl(StreamingTrack streamingTrack)
        {
            SampleTableBox stbl = new SampleTableBox();

            stbl.addBox(streamingTrack.getSampleDescriptionBox());
            stbl.addBox(new TimeToSampleBox());
            stbl.addBox(new SampleToChunkBox());
            stbl.addBox(new SampleSizeBox());
            stbl.addBox(new StaticChunkOffsetBox());
            return stbl;
        }

        protected DataInformationBox createDinf()
        {
            DataInformationBox dinf = new DataInformationBox();
            DataReferenceBox dref = new DataReferenceBox();
            dinf.addBox(dref);
            DataEntryUrlBox url = new DataEntryUrlBox();
            url.setFlags(1);
            dref.addBox(url);
            return dinf;
        }

        protected Box createTrak(StreamingTrack streamingTrack)
        {
            TrackBox trackBox = new TrackBox();
            trackBox.addBox(createTkhd(streamingTrack));
            trackBox.addBox(createMdia(streamingTrack));
            return trackBox;
        }

        protected Box createTkhd(StreamingTrack streamingTrack)
        {
            TrackHeaderBox tkhd = new TrackHeaderBox();
            tkhd.setTrackId(streamingTrack.getTrackExtension< TrackIdTrackExtension>(typeof(TrackIdTrackExtension)).getTrackId());
            DimensionTrackExtension dte = streamingTrack.getTrackExtension< DimensionTrackExtension>(typeof(DimensionTrackExtension));
            if (dte != null)
            {
                tkhd.setHeight(dte.getHeight());
                tkhd.setWidth(dte.getWidth());
            }
            return tkhd;
        }

    }
}
