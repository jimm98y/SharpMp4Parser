using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using System;

namespace SharpMp4Parser.Java
{
    public abstract class Cipher
    {
        public const int ENCRYPT_MODE = 0;
        public const int DECRYPT_MODE = 1;

        protected int opmode;
        protected byte[] key;
        protected byte[] iv;

        public static Cipher getInstance(string transformation)
        {
            switch (transformation)
            {
                case "AES/CTR/NoPadding":
                    return new AesCtrCipher();

                case "AES/CBC/NoPadding":
                    return new AesCbcCipher();

                default:
                    throw new NotSupportedException(transformation);
            }
        }

        public virtual void init(int opmode, SecretKey key, IvParameterSpec param)
        {
            this.opmode = opmode;
            this.iv = param.IV;

            if (key != null)
            {
                this.key = key.Key;
            }
        }

        public abstract void update(byte[] input, int inputOffset, int inputLen, byte[] output, int outputOffset);

        public abstract byte[] update(byte[] input);

        public abstract byte[] doFinal(byte[] input, int inputOffset, int inputLen);

        public abstract byte[] doFinal(byte[] input);

        public abstract byte[] doFinal();
    }

    public class AesCbcCipher : Cipher
    {
        BufferedBlockCipher aes;
        ParametersWithIV ivAndKey;

        public override void init(int opmode, SecretKey key, IvParameterSpec param)
        {
            base.init(opmode, key, param);

            aes = new BufferedBlockCipher(new CbcBlockCipher(new AesEngine()));
            ivAndKey = new ParametersWithIV(new KeyParameter(base.key), iv);
            aes.Init(base.opmode == Cipher.ENCRYPT_MODE, ivAndKey);
        }

        public override byte[] doFinal(byte[] input, int inputOffset, int inputLen)
        {
            return aes.DoFinal(input, inputOffset, inputLen);
        }

        public override byte[] doFinal()
        {
            return aes.DoFinal();
        }

        public override void update(byte[] input, int inputOffset, int inputLen, byte[] output, int outputOffset)
        {
            aes.ProcessBytes(input, inputOffset, inputLen, output, outputOffset);
        }

        public override byte[] update(byte[] input)
        {
            return aes.ProcessBytes(input);
        }

        public override byte[] doFinal(byte[] input)
        {
            return aes.DoFinal(input);
        }
    }

    public class AesCtrCipher : Cipher
    {
        BufferedBlockCipher aes;
        ParametersWithIV ivAndKey;

        public override void init(int opmode, SecretKey key, IvParameterSpec param)
        {
            base.init(opmode, key, param);

            aes = new BufferedBlockCipher(new SicBlockCipher(new AesEngine()));
            ivAndKey = new ParametersWithIV(new KeyParameter(base.key), iv);
            aes.Init(base.opmode == Cipher.ENCRYPT_MODE, ivAndKey);
        }

        public override byte[] doFinal(byte[] input, int inputOffset, int inputLen)
        {
            return aes.DoFinal(input, inputOffset, inputLen);
        }

        public override byte[] doFinal()
        {
            return aes.DoFinal();
        }

        public override void update(byte[] input, int inputOffset, int inputLen, byte[] output, int outputOffset)
        {
            aes.ProcessBytes(input, inputOffset, inputLen, output, outputOffset);
        }

        public override byte[] update(byte[] input)
        {
            return aes.ProcessBytes(input);
        }

        public override byte[] doFinal(byte[] input)
        {
            return aes.DoFinal(input);
        }
    }

    public class SecretKey
    {
        private byte[] key;
        private string algorithm;

        public byte[] Key
        {
            get => key;
        }

        public string Algorithm
        {
            get => algorithm;
        }

        public SecretKey(byte[] key, string algorithm)
        {
            this.key = key;
            this.algorithm = algorithm;
        }
    }

    public class IvParameterSpec
    {
        private byte[] iv;

        public byte[] IV
        {
            get => iv;
        }

        public IvParameterSpec(byte[] iv)
        {
            this.iv = iv;
        }
    }
}
