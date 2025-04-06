using SharpMp4Parser.Java;

namespace SharpMp4Parser.Muxer.Tracks.Encryption
{
    /**
     * Pairs up KeyId with Key.
     */
    public class KeyIdKeyPair
    {
        private SecretKey key;
        private Uuid keyId;

        public KeyIdKeyPair(Uuid keyId, SecretKey key)
        {
            this.key = key;
            this.keyId = keyId;
        }

        public SecretKey getKey()
        {
            return key;
        }

        public Uuid getKeyId()
        {
            return keyId;
        }
    }

}
