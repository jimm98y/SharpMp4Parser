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

using SharpMp4Parser.Tools;
using System.Text;
using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using SharpMp4Parser.Boxes.SampleEntry;
using SharpMp4Parser.Boxes;
using SharpMp4Parser.Boxes.ISO14496.Part12;

namespace SharpMp4Parser
{
    /**
     * A Property file based BoxFactory
     */
    public class PropertyBoxParserImpl : AbstractBoxParser
    {
        public Dictionary<string, Type> mapping = new Dictionary<string, Type>
        {
            { "meta-ilst" , typeof(Boxes.Apple.AppleItemListBox) },
            { "meta-ilst", typeof(Boxes.Apple.AppleItemListBox) },
            { "rmra", typeof(Boxes.Apple.AppleReferenceMovieBox) },
            { "rmda", typeof(Boxes.Apple.AppleReferenceMovieDescriptorBox) },
            { "rmdr", typeof(Boxes.Apple.AppleDataRateBox) },
            { "rdrf", typeof(Boxes.Apple.AppleDataReferenceBox) },
            { "wave", typeof(Boxes.Apple.AppleWaveBox) },
            //{ "udta-ccid", typeof(OmaDrmContentIdBox) },
            { "udta-yrrc", typeof(Boxes.ThreeGPP.TS26244.RecordingYearBox) },
            { "udta-titl", typeof(Boxes.ThreeGPP.TS26244.TitleBox) },
            { "udta-dscp", typeof(Boxes.ThreeGPP.TS26244.DescriptionBox) },
            { "udta-albm", typeof(Boxes.ThreeGPP.TS26244.AlbumBox) },
            { "udta-cprt", typeof(Boxes.ThreeGPP.TS26244.CopyrightBox) },
            { "udta-gnre", typeof(Boxes.ThreeGPP.TS26244.GenreBox) },
            { "udta-perf", typeof(Boxes.ThreeGPP.TS26244.PerformerBox) },
            { "udta-auth", typeof(Boxes.ThreeGPP.TS26244.AuthorBox) },
            { "udta-kywd", typeof(Boxes.ThreeGPP.TS26244.KeywordsBox) },
            { "udta-loci", typeof(Boxes.ThreeGPP.TS26244.LocationInformationBox) },
            { "udta-rtng", typeof(Boxes.ThreeGPP.TS26244.RatingBox) },
            { "udta-clsf", typeof(Boxes.ThreeGPP.TS26244.ClassificationBox) },
            { "tx3g", typeof(Boxes.SampleEntry.TextSampleEntry) },
            { "stsd-text", typeof(Boxes.Apple.QuicktimeTextSampleEntry) },
            { "enct", typeof(Boxes.SampleEntry.TextSampleEntry) },
            { "samr", typeof(Boxes.SampleEntry.AudioSampleEntry) },
            { "sawb", typeof(Boxes.SampleEntry.AudioSampleEntry) },
            { "mp4a", typeof(Boxes.SampleEntry.AudioSampleEntry) },
            { "drms", typeof(Boxes.SampleEntry.AudioSampleEntry) },
            { "stsd-alac", typeof(Boxes.SampleEntry.AudioSampleEntry) },
            { "mp4s", typeof(Boxes.SampleEntry.MpegSampleEntry) },
            { "owma", typeof(Boxes.SampleEntry.AudioSampleEntry) },
            { "ac-3", typeof(Boxes.SampleEntry.AudioSampleEntry) },
            { "ac-4", typeof(Boxes.SampleEntry.AudioSampleEntry) },
            { "dac3", typeof(Boxes.Dolby.AC3SpecificBox) },
            { "ec-3", typeof(Boxes.SampleEntry.AudioSampleEntry) },
            { "dec3", typeof(Boxes.Dolby.EC3SpecificBox) },
            { "stsd-lpcm", typeof(Boxes.SampleEntry.AudioSampleEntry) },
            { "stsd-dtsc", typeof(Boxes.SampleEntry.AudioSampleEntry) },
            { "stsd-dtsh", typeof(Boxes.SampleEntry.AudioSampleEntry) },
            { "stsd-dtsl", typeof(Boxes.SampleEntry.AudioSampleEntry) },
            { "ddts", typeof(Boxes.Dolby.DTSSpecificBox) },
            { "stsd-dtse", typeof(Boxes.SampleEntry.AudioSampleEntry) },
            { "stsd-mlpa", typeof(Boxes.SampleEntry.AudioSampleEntry) },
            { "dmlp", typeof(Boxes.Dolby.MLPSpecificBox) },
            { "enca", typeof(Boxes.SampleEntry.AudioSampleEntry) },
            { "sowt", typeof(Boxes.SampleEntry.AudioSampleEntry) },
            { "vp08", typeof(Boxes.SampleEntry.VisualSampleEntry) },
            { "vp09", typeof(Boxes.SampleEntry.VisualSampleEntry) },
            { "vp10", typeof(Boxes.SampleEntry.VisualSampleEntry) },
            { "encv", typeof(Boxes.SampleEntry.VisualSampleEntry) },
            { "apcn", typeof(Boxes.SampleEntry.VisualSampleEntry) },
            { "mp4v", typeof(Boxes.SampleEntry.VisualSampleEntry) },
            { "s263", typeof(Boxes.SampleEntry.VisualSampleEntry) },
            { "avc1", typeof(Boxes.SampleEntry.VisualSampleEntry) },
            { "avc2", typeof(Boxes.SampleEntry.VisualSampleEntry) },
            { "dvhe", typeof(Boxes.SampleEntry.VisualSampleEntry) },
            { "dvav", typeof(Boxes.SampleEntry.VisualSampleEntry) },
            { "avc3", typeof(Boxes.SampleEntry.VisualSampleEntry) },
            { "avc4", typeof(Boxes.SampleEntry.VisualSampleEntry) },
            { "hev1", typeof(Boxes.SampleEntry.VisualSampleEntry) },
            { "hvc1", typeof(Boxes.SampleEntry.VisualSampleEntry) },
            { "ovc1", typeof(Boxes.SampleEntry.Ovc1VisualSampleEntryImpl) },
            { "vpcC", typeof(Boxes.WebM.VPCodecConfigurationBox) },
            { "stpp", typeof(Boxes.ISO14496.Part30.XMLSubtitleSampleEntry) },
            { "avcC", typeof(Boxes.ISO14496.Part15.AvcConfigurationBox) },
            { "hvcC", typeof(Boxes.ISO14496.Part15.HevcConfigurationBox) },
            { "alac", typeof(Boxes.Apple.AppleLosslessSpecificBox) },
            { "btrt", typeof(Boxes.ISO14496.Part12.BitRateBox) },
            { "ftyp", typeof(Boxes.ISO14496.Part12.FileTypeBox) },
            { "mdat", typeof(Boxes.ISO14496.Part12.MediaDataBox) },
            { "moov", typeof(Boxes.ISO14496.Part12.MovieBox) },
            { "mvhd", typeof(Boxes.ISO14496.Part12.MovieHeaderBox) },
            { "trak", typeof(Boxes.ISO14496.Part12.TrackBox) },
            { "tkhd", typeof(Boxes.ISO14496.Part12.TrackHeaderBox) },
            { "edts", typeof(Boxes.ISO14496.Part12.EditBox) },
            { "elst", typeof(Boxes.ISO14496.Part12.EditListBox) },
            { "mdia", typeof(Boxes.ISO14496.Part12.MediaBox) },
            { "mdhd", typeof(Boxes.ISO14496.Part12.MediaHeaderBox) },
            { "hdlr", typeof(Boxes.ISO14496.Part12.HandlerBox) },
            { "minf", typeof(Boxes.ISO14496.Part12.MediaInformationBox) },
            { "vmhd", typeof(Boxes.ISO14496.Part12.VideoMediaHeaderBox) },
            { "smhd", typeof(Boxes.ISO14496.Part12.SoundMediaHeaderBox) },
            { "sthd", typeof(Boxes.ISO14496.Part12.SubtitleMediaHeaderBox) },
            { "hmhd", typeof(Boxes.ISO14496.Part12.HintMediaHeaderBox) },
            { "dinf", typeof(Boxes.ISO14496.Part12.DataInformationBox) },
            { "dref", typeof(Boxes.ISO14496.Part12.DataReferenceBox) },
            { "url ", typeof(Boxes.ISO14496.Part12.DataEntryUrlBox) },
            { "urn ", typeof(Boxes.ISO14496.Part12.DataEntryUrnBox) },
            { "stbl", typeof(Boxes.ISO14496.Part12.SampleTableBox) },
            { "ctts", typeof(Boxes.ISO14496.Part12.CompositionTimeToSample) },
            { "stsd", typeof(Boxes.ISO14496.Part12.SampleDescriptionBox) },
            { "stts", typeof(Boxes.ISO14496.Part12.TimeToSampleBox) },
            { "stss", typeof(Boxes.ISO14496.Part12.SyncSampleBox) },
            { "stsc", typeof(Boxes.ISO14496.Part12.SampleToChunkBox) },
            { "stsz", typeof(Boxes.ISO14496.Part12.SampleSizeBox) },
            { "stco", typeof(Boxes.ISO14496.Part12.StaticChunkOffsetBox) },
            { "subs", typeof(Boxes.ISO14496.Part12.SubSampleInformationBox) },
            { "udta", typeof(Boxes.ISO14496.Part12.UserDataBox) },
            { "skip", typeof(Boxes.ISO14496.Part12.FreeSpaceBox) },
            { "tref", typeof(Boxes.ISO14496.Part12.TrackReferenceBox) },
            { "iloc", typeof(Boxes.ISO14496.Part12.ItemLocationBox) },
            { "idat", typeof(Boxes.ISO14496.Part12.ItemDataBox) },
            { "damr", typeof(Boxes.SampleEntry.AmrSpecificBox) },
            { "meta", typeof(Boxes.ISO14496.Part12.MetaBox) },
            { "ipro", typeof(Boxes.ISO14496.Part12.ItemProtectionBox) },
            { "sinf", typeof(Boxes.ISO14496.Part12.ProtectionSchemeInformationBox) },
            { "frma", typeof(Boxes.ISO14496.Part12.OriginalFormatBox) },
            { "schi", typeof(Boxes.ISO14496.Part12.SchemeInformationBox) },
            { "odaf", typeof(Boxes.Oma.OmaDrmAccessUnitFormatBox) },
            { "schm", typeof(Boxes.ISO14496.Part12.SchemeTypeBox) },
            { "uuid", typeof(Boxes.UserBox) }, //userType
            { "free", typeof(Boxes.ISO14496.Part12.FreeBox) },
            { "styp", typeof(Boxes.ISO14496.Part12.SegmentTypeBox) },
            { "mvex", typeof(Boxes.ISO14496.Part12.MovieExtendsBox) },
            { "mehd", typeof(Boxes.ISO14496.Part12.MovieExtendsHeaderBox) },
            { "trex", typeof(Boxes.ISO14496.Part12.TrackExtendsBox) },
            { "moof", typeof(Boxes.ISO14496.Part12.MovieFragmentBox) },
            { "mfhd", typeof(Boxes.ISO14496.Part12.MovieFragmentHeaderBox) },
            { "traf", typeof(Boxes.ISO14496.Part12.TrackFragmentBox) },
            { "tfhd", typeof(Boxes.ISO14496.Part12.TrackFragmentHeaderBox) },
            { "trun", typeof(Boxes.ISO14496.Part12.TrackRunBox) },
            { "sdtp", typeof(Boxes.ISO14496.Part12.SampleDependencyTypeBox) },
            { "mfra", typeof(Boxes.ISO14496.Part12.MovieFragmentRandomAccessBox) },
            { "tfra", typeof(Boxes.ISO14496.Part12.TrackFragmentRandomAccessBox) },
            { "mfro", typeof(Boxes.ISO14496.Part12.MovieFragmentRandomAccessOffsetBox) },
            { "tfdt", typeof(Boxes.ISO14496.Part12.TrackFragmentBaseMediaDecodeTimeBox) },
            { "nmhd", typeof(Boxes.ISO14496.Part12.NullMediaHeaderBox) },
            { "gmhd", typeof(Boxes.Apple.GenericMediaHeaderAtom) },
            { "gmhd-text", typeof(Boxes.Apple.GenericMediaHeaderTextAtom) },
            { "gmin", typeof(Boxes.Apple.BaseMediaInfoAtom) },
            { "cslg", typeof(Boxes.ISO14496.Part12.CompositionToDecodeBox) },
            { "pdin", typeof(Boxes.ISO14496.Part12.ProgressiveDownloadInformationBox) },
            { "bloc", typeof(Boxes.Dece.BaseLocationBox) },
            { "ftab", typeof(Boxes.ThreeGPP.TS26245.FontTableBox) },
            { "co64", typeof(Boxes.ISO14496.Part12.ChunkOffset64BitBox) },
            { "xml ", typeof(Boxes.ISO14496.Part12.XmlBox) },
            { "avcn", typeof(Boxes.Dece.AvcNalUnitStorageBox) },
            { "ainf", typeof(Boxes.Dece.AssetInformationBox) },
            { "pssh", typeof(Boxes.ISO23001.Part7.ProtectionSystemSpecificHeaderBox) },
            { "trik", typeof(Boxes.Dece.TrickPlayBox) },
            { "uuid[A2394F525A9B4F14A2446C427C648DF4]", typeof(Boxes.Microsoft.PiffSampleEncryptionBox) },
            { "uuid[8974DBCE7BE74C5184F97148F9882554]", typeof(Boxes.Microsoft.PiffTrackEncryptionBox) },
            { "uuid[D4807EF2CA3946958E5426CB9E46A79F]", typeof(Boxes.Microsoft.TfrfBox) },
            { "uuid[6D1D9B0542D544E680E2141DAFF757B2]", typeof(Boxes.Microsoft.TfxdBox) },
            { "uuid[D08A4F1810F34A82B6C832D8ABA183D3]", typeof(Boxes.Microsoft.UuidBasedProtectionSystemSpecificHeaderBox) },
            { "senc", typeof(Boxes.ISO23001.Part7.SampleEncryptionBox) },
            { "tenc", typeof(Boxes.ISO23001.Part7.TrackEncryptionBox) },
            { "amf0", typeof(Boxes.Adobe.ActionMessageFormat0SampleEntryBox) },
            { "esds", typeof(Boxes.ISO14496.Part14.ESDescriptorBox) },
            { "tmcd", typeof(Boxes.Apple.TimeCodeBox) },
            { "sidx", typeof(Boxes.ISO14496.Part12.SegmentIndexBox) },
            { "sbgp", typeof(Boxes.SampleGrouping.SampleToGroupBox) },
            { "sgpd", typeof(Boxes.SampleGrouping.SampleGroupDescriptionBox) },
            { "tapt", typeof(Boxes.Apple.TrackApertureModeDimensionAtom) },
            { "clef", typeof(Boxes.Apple.CleanApertureAtom) },
            { "prof", typeof(Boxes.Apple.TrackProductionApertureDimensionsAtom) },
            { "enof", typeof(Boxes.Apple.TrackEncodedPixelsDimensionsAtom) },
            { "pasp", typeof(Boxes.Apple.PixelAspectRationAtom) },
            { "load", typeof(Boxes.Apple.TrackLoadSettingsAtom) },
            { "default", typeof(Boxes.UnknownBox) },
            { "\u00A9nam", typeof(Boxes.Apple.AppleNameBox) },
            { "\u00A9ART", typeof(Boxes.Apple.AppleArtistBox) },
            { "aART", typeof(Boxes.Apple.AppleArtist2Box) },
            { "\u00A9alb", typeof(Boxes.Apple.AppleAlbumBox) },
            { "\u00A9gen", typeof(Boxes.Apple.AppleGenreBox) },
            { "gnre", typeof(Boxes.Apple.AppleGenreIDBox) },
            { "#\u00A9day", typeof(Boxes.Apple.AppleRecordingYearBox) },
            { "\u00A9day", typeof(Boxes.Apple.AppleRecordingYear2Box) },
            { "trkn", typeof(Boxes.Apple.AppleTrackNumberBox) },
            { "cpil", typeof(Boxes.Apple.AppleCompilationBox) },
            { "pgap", typeof(Boxes.Apple.AppleGaplessPlaybackBox) },
            { "disk", typeof(Boxes.Apple.AppleDiskNumberBox) },
            { "apID", typeof(Boxes.Apple.AppleAppleIdBox) },
            { "cprt", typeof(Boxes.Apple.AppleCopyrightBox) },
            { "atID", typeof(Boxes.Apple.Apple_atIDBox) },
            { "geID", typeof(Boxes.Apple.Apple_geIDBox) },
            { "sfID", typeof(Boxes.Apple.AppleCountryTypeBoxBox) },
            { "desc", typeof(Boxes.Apple.AppleDescriptionBox) },
            { "tvnn", typeof(Boxes.Apple.AppleTVNetworkBox) },
            { "tvsh", typeof(Boxes.Apple.AppleTVShowBox) },
            { "tven", typeof(Boxes.Apple.AppleTVEpisodeNumberBox) },
            { "tvsn", typeof(Boxes.Apple.AppleTVSeasonBox) },
            { "tves", typeof(Boxes.Apple.AppleTVEpisodeBox) },
            { "xid ", typeof(Boxes.Apple.Apple_xid_Box) },
            { "flvr", typeof(Boxes.Apple.Apple_flvr_Box) },
            { "sdes", typeof(Boxes.Apple.AppleShortDescriptionBox) },
            { "ldes", typeof(Boxes.Apple.AppleLongDescriptionBox) },
            { "soal", typeof(Boxes.Apple.AppleSortAlbumBox) },
            { "purd", typeof(Boxes.Apple.ApplePurchaseDateBox) },
            { "stik", typeof(Boxes.Apple.AppleMediaTypeBox) },
            { "\u00A9cmt", typeof(Boxes.Apple.AppleCommentBox) },
            { "tmpo", typeof(Boxes.Apple.AppleTempoBox) },
            { "\u00A9too", typeof(Boxes.Apple.AppleEncoderBox) },
            { "\u00A9wrt", typeof(Boxes.Apple.AppleTrackAuthorBox) },
            { "\u00A9grp", typeof(Boxes.Apple.AppleGroupingBox) },
            { "covr", typeof(Boxes.Apple.AppleCoverBox) },
            { "\u00A9lyr", typeof(Boxes.Apple.AppleLyricsBox) },
            { "cinf", typeof(Boxes.Dece.ContentInformationBox) },
            { "tibr", typeof(Boxes.ISO14496.Part15.TierBitRateBox) },
            { "tiri", typeof(Boxes.ISO14496.Part15.TierInfoBox) },
            { "svpr", typeof(Boxes.ISO14496.Part15.PriotityRangeBox) },
            { "emsg", typeof(Boxes.ISO23009.Part1.EventMessageBox) },
            { "saio", typeof(Boxes.ISO14496.Part12.SampleAuxiliaryInformationOffsetsBox) },
            { "saiz", typeof(Boxes.ISO14496.Part12.SampleAuxiliaryInformationSizesBox) },
            { "vttC", typeof(Boxes.ISO14496.Part30.WebVTTConfigurationBox) },
            { "vlab", typeof(Boxes.ISO14496.Part30.WebVTTSourceLabelBox) },
            { "wvtt", typeof(Boxes.ISO14496.Part30.WebVTTSampleEntry) },
            { "Xtra", typeof(Boxes.Microsoft.XtraBox) },
            { "\u00A9xyz", typeof(Boxes.Apple.AppleGPSCoordinatesBox) },
            { "hint", typeof(Boxes.ISO14496.Part12.TrackReferenceTypeBox) },
            { "cdsc", typeof(Boxes.ISO14496.Part12.TrackReferenceTypeBox) },
            { "hind", typeof(Boxes.ISO14496.Part12.TrackReferenceTypeBox) },
            { "vdep", typeof(Boxes.ISO14496.Part12.TrackReferenceTypeBox) },
            { "vplx", typeof(Boxes.ISO14496.Part12.TrackReferenceTypeBox) },
            { "rtp ", typeof(Boxes.ISO14496.Part12.HintSampleEntry) },
            { "srtp", typeof(Boxes.ISO14496.Part12.HintSampleEntry) },
            { "stdp", typeof(Boxes.ISO14496.Part12.DegradationPriorityBox) },
            { "dvcC", typeof(Boxes.Dolby.DoViConfigurationBox) },
            { "dfxp", typeof(Boxes.SampleEntry.DfxpSampleEntry) },
            { "CoLL", typeof(Boxes.WebM.ContentLightLevelBox) },
            { "SmDm", typeof(Boxes.WebM.SMPTE2086MasteringDisplayMetadataBox) },
        };

        StringBuilder buildLookupStrings = new StringBuilder();
        Type clazzName = null;

        public PropertyBoxParserImpl()
        {  }

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
                Type clazz = (Type)clazzName;
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
                    constructor = mapping[(parent) + "-uuid[" + Hex.encodeHex(userType).ToUpperInvariant() + "]"];
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
