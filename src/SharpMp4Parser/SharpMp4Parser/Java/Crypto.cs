using System;

namespace SharpMp4Parser.Java
{
    public class Cipher
    {
        public static object ENCRYPT_MODE { get; internal set; }
        public static object DECRYPT_MODE { get; internal set; }

        public static Cipher getInstance(string v)
        {
            throw new NotImplementedException();
        }

        public byte[] doFinal(byte[] fullyEncryptedSample, int v, int encryptedLength)
        {
            throw new NotImplementedException();
        }

        public byte[] doFinal(byte[] fullyEncryptedSample)
        {
            throw new NotImplementedException();
        }

        public void init(object mode, SecretKey cek, IvParameterSpec ivParameterSpec)
        {
            throw new NotImplementedException();
        }

        public void update(byte[] fullSample1, int offset1, int v, byte[] fullSample2, int offset2)
        {
            throw new NotImplementedException();
        }

        public byte[] update(byte[] toBeEncrypted)
        {
            throw new NotImplementedException();
        }

        internal byte[] doFinal()
        {
            throw new NotImplementedException();
        }
    }

    public class SecretKey
    {

    }

    public class SecretKeySpec : SecretKey
    {
        private byte[] bytes;
        private string v;

        public SecretKeySpec(byte[] bytes, string v)
        {
            this.bytes = bytes;
            this.v = v;
        }
    }

    public class IvParameterSpec
    {
        private byte[] fullIv;

        public IvParameterSpec(byte[] fullIv)
        {
            this.fullIv = fullIv;
        }
    }
}
