using qBittorrentCompanion.Services;
using Splat;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace qBittorrentCompanion.Helpers
{
    public class NoLoginDataException : Exception
    {
        public NoLoginDataException() { }

        public NoLoginDataException(string message) : base(message) { }

        public NoLoginDataException(string message, Exception inner) : base(message, inner) { }
    }

    public class SecureStorage
    {
        private const string FilePath = "ConnectionData.txt";

        public static void SaveData(string username, string password, string ip, string port, bool useHttps = false)
        {
            var passwordEncrypted = EncryptString(password);
            var data = $"{username}\n{passwordEncrypted}\n{ip}\n{port}\n{useHttps}";
            File.WriteAllText(FilePath, data);
            AppLoggerService.AddLogMessage(
                LogLevel.Info,
                GetFullTypeName<SecureStorage>(),
                Resources.Resources.SecureStorage_SavedLoginDataToLocalStorage,
                String.Format(Resources.Resources.SecureStorage_AddedLoginDataToFile, FilePath)
            );
        }

        public static bool HasSavedData() => File.Exists(FilePath);

        public static (string username, string password, string ip, string port, bool useHttps) LoadData()
        {
            if (!HasSavedData())
                throw new NoLoginDataException(
                    String.Format(Resources.Resources.SecureStorage_FileDoesNotExist, FilePath)
                );

            var data = File.ReadAllText(FilePath).Split('\n');
            var password = DecryptString(data[1]);

            AppLoggerService.AddLogMessage(
                level: LogLevel.Info,
                source: GetFullTypeName<SecureStorage>(),
                title: Resources.Resources.SecureStorage_LoginDataRequested,
                message: String.Format(Resources.Resources.SecureStorage_LoadedUserCredentials, data[0], FilePath)
            );

            return (data[0], password, data[2], data[3], bool.Parse(data[4]));
        }

        private static string EncryptString(string text)
        {
            var plainBytes = Encoding.UTF8.GetBytes(text);

            if (OperatingSystem.IsWindows())
            {
                var cipherBytes = ProtectedData.Protect(plainBytes, null, DataProtectionScope.CurrentUser);
                return Convert.ToBase64String(cipherBytes);
            }

            if (OperatingSystem.IsLinux())
            {
                return AesEncrypt(text, GetLinuxMachineKey());
            }

            // Android fallback — OS sandboxing provides security
            return Convert.ToBase64String(plainBytes);
        }

        private static string DecryptString(string cipherText)
        {
            var cipherBytes = Convert.FromBase64String(cipherText);

            if (OperatingSystem.IsWindows())
            {
                var plainBytes = ProtectedData.Unprotect(cipherBytes, null, DataProtectionScope.CurrentUser);
                return Encoding.UTF8.GetString(plainBytes);
            }

            if (OperatingSystem.IsLinux())
            {
                return AesDecrypt(cipherText, GetLinuxMachineKey());
            }

            // Android fallback
            return Encoding.UTF8.GetString(cipherBytes);
        }

        private static string AesEncrypt(string text, string keyString)
        {
            var key = Encoding.UTF8.GetBytes(keyString);
            using var aesAlg = Aes.Create();
            using var encryptor = aesAlg.CreateEncryptor(key, aesAlg.IV);
            using var msEncrypt = new MemoryStream();
            using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
            using (var swEncrypt = new StreamWriter(csEncrypt))
                swEncrypt.Write(text);

            var iv = aesAlg.IV;
            var encrypted = msEncrypt.ToArray();
            var result = new byte[iv.Length + encrypted.Length];
            Buffer.BlockCopy(iv, 0, result, 0, iv.Length);
            Buffer.BlockCopy(encrypted, 0, result, iv.Length, encrypted.Length);
            return Convert.ToBase64String(result);
        }

        private static string AesDecrypt(string cipherText, string keyString)
        {
            var fullCipher = Convert.FromBase64String(cipherText);
            var iv = new byte[16];
            var cipher = new byte[fullCipher.Length - iv.Length];
            Buffer.BlockCopy(fullCipher, 0, iv, 0, iv.Length);
            Buffer.BlockCopy(fullCipher, iv.Length, cipher, 0, cipher.Length);

            var key = Encoding.UTF8.GetBytes(keyString);
            using var aesAlg = Aes.Create();
            using var decryptor = aesAlg.CreateDecryptor(key, iv);
            using var msDecrypt = new MemoryStream(cipher);
            using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
            using var srDecrypt = new StreamReader(csDecrypt);
            return srDecrypt.ReadToEnd();
        }

        private static string GetLinuxMachineKey()
        {
            // /etc/machine-id is a stable unique ID generated at OS install time
            var machineId = File.ReadAllText("/etc/machine-id").Trim();
            var username = Environment.UserName;
            var combined = $"{machineId}:{username}";

            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(combined));
            return Convert.ToHexString(hash)[..32]; // 32 hex chars = 16 bytes = valid AES-128 key
        }

        public static void DestroyData()
        {
            if (File.Exists(FilePath))
            {
                File.Delete(FilePath);
                AppLoggerService.AddLogMessage(
                    LogLevel.Info,
                    GetFullTypeName<SecureStorage>(),
                    String.Format(Resources.Resources.SecureStorage_FileDeleted, FilePath),
                    String.Format(Resources.Resources.SecureStorage_LoginFileMissing, FilePath)
                );
            }
        }
    }
}