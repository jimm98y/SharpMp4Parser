/*
 * Copyright 2012 Sebastian Annies, Hamburg
 *
 * Licensed under the Apache License, Version 2.0 (the License);
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an AS IS BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System.Text;
using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using SharpMp4Parser.IsoParser.Boxes.Adobe;
using SharpMp4Parser.IsoParser.Boxes.Apple;
using SharpMp4Parser.IsoParser.Boxes.Dece;
using SharpMp4Parser.IsoParser.Boxes.Dolby;
using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part12;
using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part14;
using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part15;
using SharpMp4Parser.IsoParser.Boxes.ISO14496.Part30;
using SharpMp4Parser.IsoParser.Boxes.ISO23001.Part7;
using SharpMp4Parser.IsoParser.Boxes.ISO23009.Part1;
using SharpMp4Parser.IsoParser.Boxes.Microsoft;
using SharpMp4Parser.IsoParser.Boxes.Oma;
using SharpMp4Parser.IsoParser.Boxes.SampleEntry;
using SharpMp4Parser.IsoParser.Boxes.SampleGrouping;
using SharpMp4Parser.IsoParser.Boxes.ThreeGPP.TS26244;
using SharpMp4Parser.IsoParser.Boxes.ThreeGPP.TS26245;
using SharpMp4Parser.IsoParser.Boxes.WebM;
using SharpMp4Parser.IsoParser.Boxes;
using SharpMp4Parser.IsoParser.Tools;

namespace SharpMp4Parser.IsoParser
{
    /**
     * A Property file based BoxFactory
     */
    public class PropertyBoxParserImpl : AbstractBoxParser
    {
        public Dictionary<string, Type> mapping = new Dictionary<string, Type>
        {
            { "meta-ilst" , typeof(AppleItemListBox) },
            { "meta-ilst", typeof(AppleItemListBox) },
            { "rmra", typeof(AppleReferenceMovieBox) },
            { "rmda", typeof(AppleReferenceMovieDescriptorBox) },
            { "rmdr", typeof(AppleDataRateBox) },
            { "rdrf", typeof(AppleDataReferenceBox) },
            { "wave", typeof(AppleWaveBox) },
            //{ "udta-ccid", typeof(OmaDrmContentIdBox) },
            { "udta-yrrc", typeof(RecordingYearBox) },
            { "udta-titl", typeof(TitleBox) },
            { "udta-dscp", typeof(DescriptionBox) },
            { "udta-albm", typeof(AlbumBox) },
            { "udta-cprt", typeof(CopyrightBox) },
            { "udta-gnre", typeof(GenreBox) },
            { "udta-perf", typeof(PerformerBox) },
            { "udta-auth", typeof(AuthorBox) },
            { "udta-kywd", typeof(KeywordsBox) },
            { "udta-loci", typeof(LocationInformationBox) },
            { "udta-rtng", typeof(RatingBox) },
            { "udta-clsf", typeof(ClassificationBox) },
            { "tx3g", typeof(TextSampleEntry) },
            { "stsd-text", typeof(QuicktimeTextSampleEntry) },
            { "enct", typeof(TextSampleEntry) },
            { "samr", typeof(AudioSampleEntry) },
            { "sawb", typeof(AudioSampleEntry) },
            { "mp4a", typeof(AudioSampleEntry) },
            { "drms", typeof(AudioSampleEntry) },
            { "stsd-alac", typeof(AudioSampleEntry) },
            { "mp4s", typeof(MpegSampleEntry) },
            { "owma", typeof(AudioSampleEntry) },
            { "ac-3", typeof(AudioSampleEntry) },
            { "ac-4", typeof(AudioSampleEntry) },
            { "dac3", typeof(AC3SpecificBox) },
            { "ec-3", typeof(AudioSampleEntry) },
            { "dec3", typeof(EC3SpecificBox) },
            { "stsd-lpcm", typeof(AudioSampleEntry) },
            { "stsd-dtsc", typeof(AudioSampleEntry) },
            { "stsd-dtsh", typeof(AudioSampleEntry) },
            { "stsd-dtsl", typeof(AudioSampleEntry) },
            { "ddts", typeof(DTSSpecificBox) },
            { "stsd-dtse", typeof(AudioSampleEntry) },
            { "stsd-mlpa", typeof(AudioSampleEntry) },
            { "dmlp", typeof(MLPSpecificBox) },
            { "enca", typeof(AudioSampleEntry) },
            { "sowt", typeof(AudioSampleEntry) },
            { "vp08", typeof(VisualSampleEntry) },
            { "vp09", typeof(VisualSampleEntry) },
            { "vp10", typeof(VisualSampleEntry) },
            { "encv", typeof(VisualSampleEntry) },
            { "apcn", typeof(VisualSampleEntry) },
            { "mp4v", typeof(VisualSampleEntry) },
            { "s263", typeof(VisualSampleEntry) },
            { "avc1", typeof(VisualSampleEntry) },
            { "avc2", typeof(VisualSampleEntry) },
            { "dvhe", typeof(VisualSampleEntry) },
            { "dvav", typeof(VisualSampleEntry) },
            { "avc3", typeof(VisualSampleEntry) },
            { "avc4", typeof(VisualSampleEntry) },
            { "hev1", typeof(VisualSampleEntry) },
            { "hvc1", typeof(VisualSampleEntry) },
            { "ovc1", typeof(Ovc1VisualSampleEntryImpl) },
            { "vpcC", typeof(VPCodecConfigurationBox) },
            { "stpp", typeof(XMLSubtitleSampleEntry) },
            { "avcC", typeof(AvcConfigurationBox) },
            { "hvcC", typeof(HevcConfigurationBox) },
            { "alac", typeof(AppleLosslessSpecificBox) },
            { "btrt", typeof(BitRateBox) },
            { "ftyp", typeof(FileTypeBox) },
            { "mdat", typeof(MediaDataBox) },
            { "moov", typeof(MovieBox) },
            { "mvhd", typeof(MovieHeaderBox) },
            { "trak", typeof(TrackBox) },
            { "tkhd", typeof(TrackHeaderBox) },
            { "edts", typeof(EditBox) },
            { "elst", typeof(EditListBox) },
            { "mdia", typeof(MediaBox) },
            { "mdhd", typeof(MediaHeaderBox) },
            { "hdlr", typeof(HandlerBox) },
            { "minf", typeof(MediaInformationBox) },
            { "vmhd", typeof(VideoMediaHeaderBox) },
            { "smhd", typeof(SoundMediaHeaderBox) },
            { "sthd", typeof(SubtitleMediaHeaderBox) },
            { "hmhd", typeof(HintMediaHeaderBox) },
            { "dinf", typeof(DataInformationBox) },
            { "dref", typeof(DataReferenceBox) },
            { "url ", typeof(DataEntryUrlBox) },
            { "urn ", typeof(DataEntryUrnBox) },
            { "stbl", typeof(SampleTableBox) },
            { "ctts", typeof(CompositionTimeToSample) },
            { "stsd", typeof(SampleDescriptionBox) },
            { "stts", typeof(TimeToSampleBox) },
            { "stss", typeof(SyncSampleBox) },
            { "stsc", typeof(SampleToChunkBox) },
            { "stsz", typeof(SampleSizeBox) },
            { "stco", typeof(StaticChunkOffsetBox) },
            { "subs", typeof(SubSampleInformationBox) },
            { "udta", typeof(UserDataBox) },
            { "skip", typeof(FreeSpaceBox) },
            { "tref", typeof(TrackReferenceBox) },
            { "iloc", typeof(ItemLocationBox) },
            { "idat", typeof(ItemDataBox) },
            { "damr", typeof(AmrSpecificBox) },
            { "meta", typeof(MetaBox) },
            { "ipro", typeof(ItemProtectionBox) },
            { "sinf", typeof(ProtectionSchemeInformationBox) },
            { "frma", typeof(OriginalFormatBox) },
            { "schi", typeof(SchemeInformationBox) },
            { "odaf", typeof(OmaDrmAccessUnitFormatBox) },
            { "schm", typeof(SchemeTypeBox) },
            { "uuid", typeof(UserBox) }, //userType
            { "free", typeof(FreeBox) },
            { "styp", typeof(SegmentTypeBox) },
            { "mvex", typeof(MovieExtendsBox) },
            { "mehd", typeof(MovieExtendsHeaderBox) },
            { "trex", typeof(TrackExtendsBox) },
            { "moof", typeof(MovieFragmentBox) },
            { "mfhd", typeof(MovieFragmentHeaderBox) },
            { "traf", typeof(TrackFragmentBox) },
            { "tfhd", typeof(TrackFragmentHeaderBox) },
            { "trun", typeof(TrackRunBox) },
            { "sdtp", typeof(SampleDependencyTypeBox) },
            { "mfra", typeof(MovieFragmentRandomAccessBox) },
            { "tfra", typeof(TrackFragmentRandomAccessBox) },
            { "mfro", typeof(MovieFragmentRandomAccessOffsetBox) },
            { "tfdt", typeof(TrackFragmentBaseMediaDecodeTimeBox) },
            { "nmhd", typeof(NullMediaHeaderBox) },
            { "gmhd", typeof(GenericMediaHeaderAtom) },
            { "gmhd-text", typeof(GenericMediaHeaderTextAtom) },
            { "gmin", typeof(BaseMediaInfoAtom) },
            { "cslg", typeof(CompositionToDecodeBox) },
            { "pdin", typeof(ProgressiveDownloadInformationBox) },
            { "bloc", typeof(BaseLocationBox) },
            { "ftab", typeof(FontTableBox) },
            { "co64", typeof(ChunkOffset64BitBox) },
            { "xml ", typeof(XmlBox) },
            { "avcn", typeof(AvcNalUnitStorageBox) },
            { "ainf", typeof(AssetInformationBox) },
            { "pssh", typeof(ProtectionSystemSpecificHeaderBox) },
            { "trik", typeof(TrickPlayBox) },
            { "uuid[A2394F525A9B4F14A2446C427C648DF4]", typeof(PiffSampleEncryptionBox) },
            { "uuid[8974DBCE7BE74C5184F97148F9882554]", typeof(PiffTrackEncryptionBox) },
            { "uuid[D4807EF2CA3946958E5426CB9E46A79F]", typeof(TfrfBox) },
            { "uuid[6D1D9B0542D544E680E2141DAFF757B2]", typeof(TfxdBox) },
            { "uuid[D08A4F1810F34A82B6C832D8ABA183D3]", typeof(UuidBasedProtectionSystemSpecificHeaderBox) },
            { "senc", typeof(SampleEncryptionBox) },
            { "tenc", typeof(TrackEncryptionBox) },
            { "amf0", typeof(ActionMessageFormat0SampleEntryBox) },
            { "esds", typeof(ESDescriptorBox) },
            { "tmcd", typeof(TimeCodeBox) },
            { "sidx", typeof(SegmentIndexBox) },
            { "sbgp", typeof(SampleToGroupBox) },
            { "sgpd", typeof(SampleGroupDescriptionBox) },
            { "tapt", typeof(TrackApertureModeDimensionAtom) },
            { "clef", typeof(CleanApertureAtom) },
            { "prof", typeof(TrackProductionApertureDimensionsAtom) },
            { "enof", typeof(TrackEncodedPixelsDimensionsAtom) },
            { "pasp", typeof(PixelAspectRationAtom) },
            { "load", typeof(TrackLoadSettingsAtom) },
            { "default", typeof(UnknownBox) },
            { "\u00A9nam", typeof(AppleNameBox) },
            { "\u00A9ART", typeof(AppleArtistBox) },
            { "aART", typeof(AppleArtist2Box) },
            { "\u00A9alb", typeof(AppleAlbumBox) },
            { "\u00A9gen", typeof(AppleGenreBox) },
            { "gnre", typeof(AppleGenreIDBox) },
            { "#\u00A9day", typeof(AppleRecordingYearBox) },
            { "\u00A9day", typeof(AppleRecordingYear2Box) },
            { "trkn", typeof(AppleTrackNumberBox) },
            { "cpil", typeof(AppleCompilationBox) },
            { "pgap", typeof(AppleGaplessPlaybackBox) },
            { "disk", typeof(AppleDiskNumberBox) },
            { "apID", typeof(AppleAppleIdBox) },
            { "cprt", typeof(AppleCopyrightBox) },
            { "atID", typeof(Apple_atIDBox) },
            { "geID", typeof(Apple_geIDBox) },
            { "sfID", typeof(AppleCountryTypeBoxBox) },
            { "desc", typeof(AppleDescriptionBox) },
            { "tvnn", typeof(AppleTVNetworkBox) },
            { "tvsh", typeof(AppleTVShowBox) },
            { "tven", typeof(AppleTVEpisodeNumberBox) },
            { "tvsn", typeof(AppleTVSeasonBox) },
            { "tves", typeof(AppleTVEpisodeBox) },
            { "xid ", typeof(Apple_xid_Box) },
            { "flvr", typeof(Apple_flvr_Box) },
            { "sdes", typeof(AppleShortDescriptionBox) },
            { "ldes", typeof(AppleLongDescriptionBox) },
            { "soal", typeof(AppleSortAlbumBox) },
            { "purd", typeof(ApplePurchaseDateBox) },
            { "stik", typeof(AppleMediaTypeBox) },
            { "\u00A9cmt", typeof(AppleCommentBox) },
            { "tmpo", typeof(AppleTempoBox) },
            { "\u00A9too", typeof(AppleEncoderBox) },
            { "\u00A9wrt", typeof(AppleTrackAuthorBox) },
            { "\u00A9grp", typeof(AppleGroupingBox) },
            { "covr", typeof(AppleCoverBox) },
            { "\u00A9lyr", typeof(AppleLyricsBox) },
            { "cinf", typeof(ContentInformationBox) },
            { "tibr", typeof(TierBitRateBox) },
            { "tiri", typeof(TierInfoBox) },
            { "svpr", typeof(PriotityRangeBox) },
            { "emsg", typeof(EventMessageBox) },
            { "saio", typeof(SampleAuxiliaryInformationOffsetsBox) },
            { "saiz", typeof(SampleAuxiliaryInformationSizesBox) },
            { "vttC", typeof(WebVTTConfigurationBox) },
            { "vlab", typeof(WebVTTSourceLabelBox) },
            { "wvtt", typeof(WebVTTSampleEntry) },
            { "Xtra", typeof(XtraBox) },
            { "\u00A9xyz", typeof(AppleGPSCoordinatesBox) },
            { "hint", typeof(TrackReferenceTypeBox) },
            { "cdsc", typeof(TrackReferenceTypeBox) },
            { "hind", typeof(TrackReferenceTypeBox) },
            { "vdep", typeof(TrackReferenceTypeBox) },
            { "vplx", typeof(TrackReferenceTypeBox) },
            { "rtp ", typeof(HintSampleEntry) },
            { "srtp", typeof(HintSampleEntry) },
            { "stdp", typeof(DegradationPriorityBox) },
            { "dvcC", typeof(DoViConfigurationBox) },
            { "dfxp", typeof(DfxpSampleEntry) },
            { "CoLL", typeof(ContentLightLevelBox) },
            { "SmDm", typeof(SMPTE2086MasteringDisplayMetadataBox) },
        };

        StringBuilder buildLookupStrings = new StringBuilder();
        Type clazzName = null;

        public PropertyBoxParserImpl()
        { }

        private static string GetParam(Type type)
        {
            if (type == typeof(TextSampleEntry) ||
               type == typeof(AudioSampleEntry) ||
               type == typeof(MpegSampleEntry) ||
               type == typeof(VisualSampleEntry) ||
               type == typeof(UnknownBox) ||
               type == typeof(TrackReferenceTypeBox) ||
               type == typeof(HintSampleEntry))
            {
                return "type";
            }
            else if (type == typeof(UserBox))
            {
                return "userType";
            }
            else
            {
                return "";
            }
        }

        public override ParsableBox createBox(string type, byte[] userType, string parent)
        {
            invoke(type, userType, parent);
            try
            {
                Type clazz = clazzName;
                string param = GetParam(clazz);
                if (!string.IsNullOrEmpty(param))
                {
                    // right now all we have to support are classes with a single param
                    Type[] constructorArgsClazz = new Type[1];
                    object[] constructorArgs = new object[1];
                    for (int i = 0; i < param.Length; i++)
                    {
                        if ("userType".Equals(param[i]))
                        {
                            constructorArgs[i] = userType;
                            constructorArgsClazz[i] = typeof(byte[]);
                        }
                        else if ("type".Equals(param[i]))
                        {
                            constructorArgs[i] = type;
                            constructorArgsClazz[i] = typeof(string);
                        }
                        else if ("parent".Equals(param[i]))
                        {
                            constructorArgs[i] = parent;
                            constructorArgsClazz[i] = typeof(string);
                        }
                        else
                        {
                            throw new Exception("No such param: " + param[i]);
                        }
                    }

                    //Constructor<ParsableBox> constructorObject = clazz.getConstructor(constructorArgsClazz);
                    //return constructorObject.newInstance(constructorArgs);
                    return (ParsableBox)Activator.CreateInstance(clazz, constructorArgs);
                }
                else
                {
                    return (ParsableBox)Activator.CreateInstance(clazz);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void invoke(string type, byte[] userType, string parent)
        {
            Type constructor;
            if (userType != null)
            {
                if (!"uuid".Equals(type))
                {
                    throw new Exception("we have a userType but no uuid box type. Something's wrong");
                }
                constructor = mapping["uuid[" + Hex.encodeHex(userType).ToUpperInvariant() + "]"];
                if (constructor == null)
                {
                    constructor = mapping[parent + "-uuid[" + Hex.encodeHex(userType).ToUpperInvariant() + "]"];
                }
                if (constructor == null)
                {
                    constructor = mapping["uuid"];
                }
            }
            else
            {
                constructor = mapping[type];
                if (constructor == null)
                {
                    string lookup = buildLookupStrings.Append(parent).Append('-').Append(type).ToString();
                    buildLookupStrings.Length = 0;
                    constructor = mapping[lookup];
                }
            }
            if (constructor == null)
            {
                constructor = mapping["default"];
            }
            if (constructor == null)
            {
                throw new Exception("No box object found for " + type);
            }

            clazzName = constructor;
        }
    }
}
