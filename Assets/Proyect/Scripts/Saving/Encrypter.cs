using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Burmuruk.Tesis.Saving
{
    public static class Encrypter
    {
        static byte[] key = null;
        static byte[] iv = null;

        public static void EncryptAEs(JObject data, string path)
        {
            SetKeys();

            Convert.ToBase64String(Encrypt(Encoding.UTF8.GetBytes(data.ToString())));
        }

        public static void DecryptAEs(string data)
        {
            Convert.ToBase64String(Encrypt(Encoding.UTF8.GetBytes(data.ToString())));
        }

        private static void SetKeys()
        {
            if (key != null || iv != null) return;

            key = new byte[16];
            iv = new byte[16];

            var cryptToAEs = RandomNumberGenerator.Create();
            cryptToAEs.GetBytes(key);
            cryptToAEs.GetBytes(iv);
        }

        private static byte[] Encrypt(byte[] data)
        {
            using (Aes algorithm = Aes.Create())
            {
                using (ICryptoTransform cryptoTransform = algorithm.CreateEncryptor(key, iv))
                {
                    Crypt(data, cryptoTransform);
                    return null;
                }
            }
        }

        private static byte[] Decrypt(byte[] data)
        {
            using (Aes aes = Aes.Create())
            {
                using (ICryptoTransform decryptor = aes.CreateDecryptor(key, iv))
                {
                    MemoryStream ms = new MemoryStream();
                    using (Stream stream = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    {
                        return null;
                    }
                }
            }
        }

        private static byte[] Crypt(byte[] data, ICryptoTransform cryptoTransform)
        {
            MemoryStream ms = new MemoryStream();
            using (Stream stream = new CryptoStream(ms, cryptoTransform, CryptoStreamMode.Read))
            {
                stream.Write(data, 0, data.Length);
                return null;
            }
        }
    }
}
