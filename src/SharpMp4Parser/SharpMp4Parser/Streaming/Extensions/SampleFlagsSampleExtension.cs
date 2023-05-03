using System.Collections.Concurrent;

namespace SharpMp4Parser.Streaming.Extensions
{
    public class SampleFlagsSampleExtension : SampleExtension
    {
        public static ConcurrentDictionary<long, SampleFlagsSampleExtension> pool =
                new ConcurrentDictionary<long, SampleFlagsSampleExtension>();

        private byte isLeading, sampleDependsOn, sampleIsDependedOn, sampleHasRedundancy, samplePaddingValue;
        private bool sampleIsNonSyncSample;
        private int sampleDegradationPriority;

        public static SampleFlagsSampleExtension create(
                byte isLeading, byte sampleDependsOn, byte sampleIsDependedOn,
                byte sampleHasRedundancy, byte samplePaddingValue, bool sampleIsNonSyncSample, int sampleDegradationPriority)
        {
            long key = isLeading + (sampleDependsOn << 2) + (sampleIsDependedOn << 4) + (sampleHasRedundancy << 6);
            key += (samplePaddingValue << 8);
            key += (sampleDegradationPriority << 11);
            key += (sampleIsNonSyncSample ? 1 : 0) << 27;

            SampleFlagsSampleExtension c;
            if (!pool.TryGetValue(key, out c))
            {
                c = new SampleFlagsSampleExtension();
                c.isLeading = isLeading;
                c.sampleDependsOn = sampleDependsOn;
                c.sampleIsDependedOn = sampleIsDependedOn;
                c.sampleHasRedundancy = sampleHasRedundancy;
                c.samplePaddingValue = samplePaddingValue;
                c.sampleIsNonSyncSample = sampleIsNonSyncSample;
                c.sampleDegradationPriority = sampleDegradationPriority;
                pool.TryAdd(key, c);
            }
            return c;
        }

        public override string ToString()
        {
            return "isLeading=" + isLeading +
                    ", dependsOn=" + sampleDependsOn +
                    ", isDependedOn=" + sampleIsDependedOn +
                    ", hasRedundancy=" + sampleHasRedundancy +
                    ", paddingValue=" + samplePaddingValue +
                    ", isSyncSample=" + !sampleIsNonSyncSample +
                    ", sampleDegradationPriority=" + sampleDegradationPriority;
        }

        public byte getIsLeading()
        {
            return isLeading;
        }

        public void setIsLeading(byte isLeading)
        {
            this.isLeading = isLeading;
        }

        public byte getSampleDependsOn()
        {
            return sampleDependsOn;
        }

        public void setSampleDependsOn(int sampleDependsOn)
        {
            this.sampleDependsOn = (byte)sampleDependsOn;
        }

        public byte getSampleIsDependedOn()
        {
            return sampleIsDependedOn;
        }

        public void setSampleIsDependedOn(int sampleIsDependedOn)
        {
            this.sampleIsDependedOn = (byte)sampleIsDependedOn;
        }

        public byte getSampleHasRedundancy()
        {
            return sampleHasRedundancy;
        }

        public void setSampleHasRedundancy(byte sampleHasRedundancy)
        {
            this.sampleHasRedundancy = sampleHasRedundancy;
        }

        public byte getSamplePaddingValue()
        {
            return samplePaddingValue;
        }

        public void setSamplePaddingValue(byte samplePaddingValue)
        {
            this.samplePaddingValue = samplePaddingValue;
        }

        public bool isSampleIsNonSyncSample()
        {
            return sampleIsNonSyncSample;
        }

        public void setSampleIsNonSyncSample(bool sampleIsNonSyncSample)
        {
            this.sampleIsNonSyncSample = sampleIsNonSyncSample;
        }

        public bool isSyncSample()
        {
            return !sampleIsNonSyncSample;
        }

        public int getSampleDegradationPriority()
        {
            return sampleDegradationPriority;
        }

        public void setSampleDegradationPriority(int sampleDegradationPriority)
        {
            this.sampleDegradationPriority = sampleDegradationPriority;
        }
    }
}