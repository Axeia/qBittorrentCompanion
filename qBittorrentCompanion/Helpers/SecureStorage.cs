using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace qBittorrentCompanion.Helpers
{
    public class NoLoginDataException : Exception
    {
        public NoLoginDataException()
        {
        }

        public NoLoginDataException(string message)
            : base(message)
        {
        }

        public NoLoginDataException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }

    public class SecureStorage
    {
        private const string FilePath = "connection_data.txt";
        private const string Key = "15CHARSARENEEDED"; // TODO base this on something

        public void SaveData(string username, string password, string ip, string port)
        {
            var passwordEncrypted = EncryptString(password, Key);
            var data = $"{username}\n{passwordEncrypted}\n{ip}\n{port}";
            File.WriteAllText(FilePath, data);
        }

        public bool HasSavedData()
        {
            return File.Exists(FilePath);
        }

        public (string username, string password, string ip, string port) LoadData()
        {
            if (!HasSavedData())
            {
                throw new NoLoginDataException($"The file {FilePath} does not exist.");
            }
            var data = File.ReadAllText(FilePath).Split('\n');
            var password = DecryptString(data[1], Key);
            return (data[0], password, data[2], data[3]);
        }

        private static string EncryptString(string text, string keyString)
        {
            var key = Encoding.UTF8.GetBytes(keyString);

            using (var aesAlg = Aes.Create())
            {
                using (var encryptor = aesAlg.CreateEncryptor(key, aesAlg.IV))
                {
                    using (var msEncrypt = new MemoryStream())
                    {
                        using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        using (var swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(text);
                        }

                        var iv = aesAlg.IV;

                        var decryptedContent = msEncrypt.ToArray();

                        var result = new byte[iv.Length + decryptedContent.Length];

                        Buffer.BlockCopy(iv, 0, result, 0, iv.Length);
                        Buffer.BlockCopy(decryptedContent, 0, result, iv.Length, decryptedContent.Length);

                        return Convert.ToBase64String(result);
                    }
                }
            }
        }

        private static string DecryptString(string cipherText, string keyString)
        {
            var fullCipher = Convert.FromBase64String(cipherText);

            var iv = new byte[16];
            var cipher = new byte[fullCipher.Length - iv.Length]; // Change this line

            Buffer.BlockCopy(fullCipher, 0, iv, 0, iv.Length);
            Buffer.BlockCopy(fullCipher, iv.Length, cipher, 0, cipher.Length); // And this line
            var key = Encoding.UTF8.GetBytes(keyString);

            using (var aesAlg = Aes.Create())
            {
                using (var decryptor = aesAlg.CreateDecryptor(key, iv))
                {
                    string result;
                    using (var msDecrypt = new MemoryStream(cipher))
                    {
                        using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {
                            using (var srDecrypt = new StreamReader(csDecrypt))
                            {
                                result = srDecrypt.ReadToEnd();
                            }
                        }
                    }

                    return result;
                }
            }
        }

        public static void DestroyData()
        {
            if (File.Exists(FilePath))
            {
                File.Delete(FilePath);
                Debug.WriteLine($"Deleted {FilePath}");
            }
        }
    }
}
