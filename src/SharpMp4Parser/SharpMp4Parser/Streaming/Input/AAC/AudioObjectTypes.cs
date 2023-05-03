using System.ComponentModel;

namespace SharpMp4Parser.Streaming.Input.AAC
{
    /**
     * Created by sannies on 01.09.2015.
     */
    public enum AudioObjectTypes
    {
        [Description(("n/a"))]
        NOT_APPLICABLE,
        [Description("AAC Main")]
        AAC_Main,
        [Description("AAC LC (Low Complexity)")]
        AAC_LC,
        [Description("AAC SSR (Scalable Sample Rate)")]
        AAC_SSR,
        [Description("AAC LTP (Long Term Prediction)")]
        AAC_LTP,
        [Description("SBR (Spectral Band Replication)")]
        SBR,
        [Description("AAC Scalable")]
        AAC_Scalable,
        [Description("TwinVQ")]
        TwinVQ,
        [Description("CELP (Code Excited Linear Prediction)")]
        CELP,
        [Description("HXVC (Harmonic Vector eXcitation Coding)")]
        HXVC,
        [Description("Reserved")]
        Reserved1,
        [Description("Reserved")]
        Reserved2,
        [Description("TTSI (Text-To-Speech Interface)")]
        TTSI,
        [Description("Main Synthesis")]
        Main_Synthesis,
        [Description("Wavetable Synthesis")]
        Wavetable_Synthesis,
        [Description("General MIDI")]
        General_MIDI,
        [Description("Algorithmic Synthesis and Audio Effects")]
        Algorithmic_Synthesis_and_Audio_Effects,
        [Description("ER (Error Resilient) AAC LC")]
        ER_AAC_LC,
        [Description("Reserved")]
        Reserved3,
        [Description("ER AAC LTP")]
        ER_AAC_LTP,
        [Description("ER AAC Scalable")]
        ER_AAC_Scalable,
        [Description("ER TwinVQ")]
        ER_TwinVQ,
        [Description("ER BSAC (Bit-Sliced Arithmetic Coding)")]
        ER_BSAC,
        [Description("ER AAC LD (Low Delay)")]
        ER_AAC_LD,
        [Description("ER CELP")]
        ER_CELP,
        [Description("ER HVXC")]
        ER_HVXC,
        [Description("ER HILN (Harmonic and Individual Lines plus Noise)")]
        ER_HILN,
        [Description("ER Parametric")]
        ER_Parametric,
        [Description("SSC (SinuSoidal Coding)")]
        SSC,
        [Description("PS (Parametric Stereo)")]
        PS,
        [Description("MPEG Surround")]
        MPEG_Surround,
        [Description("(Escape value)")]
        Escape_value,
        [Description("Layer-1")]
        Layer_1,
        [Description("Layer-2")]
        Layer_2,
        [Description("Layer-3")]
        Layer_3,
        [Description("DST (Direct Stream Transfer)")]
        DST,
        [Description("ALS (Audio Lossless)")]
        ALS,
        [Description("SLS (Scalable LosslesS)")]
        SLS,
        [Description("SLS non-core")]
        SLS_non_core,
        [Description("ER AAC ELD (Enhanced Low Delay)")]
        ER_AAC_ELD,
        [Description("SMR (Symbolic Music Representation) Simple")]
        SMR_Simple,
        [Description("SMR Main")]
        SMR_Main,
        [Description("USAC (Unified Speech and Audio Coding) (no SBR)")]
        USAC_NO_SBR,
        [Description("SAOC (Spatial Audio Object Coding)")]
        SAOC,
        [Description("LD MPEG Surround")]
        LD_MPEG_Surround,
        [Description("USAC")]
        USAC
    }
}
