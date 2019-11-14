using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Windows;

namespace SaintSender.Core.Services
{
    public static class EncryptionService
    {
        private static string _filePath;
        private const string KeyContainerName = "MPPOFKWE12312312";

        private static string CreateDirectory()
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            var dataDirectory = currentDirectory + @"\Data";
            Directory.CreateDirectory(dataDirectory);
            return dataDirectory;
        }

        private static string DecryptFile(string path)
        {
            try
            {
                File.Decrypt(path);
            }
            catch (Exception e) when (e is ArgumentException || e is IOException)
            {
                MessageBox.Show("The directory or file could not be loaded!", "Directory Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (NotSupportedException)
            {
                MessageBox.Show("Your operation is not supported, please report this error!", "Unknown Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("You are trying to open files not supported by your Account!", "Unauthorized Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return path;
        }

        private static void EncryptFile()
        {
            File.Encrypt(_filePath);
        }

        private static void CreateFile(string fileName)
        {
            var bytes = Encoding.ASCII.GetBytes(fileName);
            var dataDirectory = CreateDirectory();
            _filePath = dataDirectory + $@"\{string.Join("", bytes)}.txt";
            if (!File.Exists(_filePath))
            {
                File.Create(_filePath).Dispose();
            }
        }

        public static void SaveData(string dataOne, string dataTwo)
        {
            CreateFile(dataOne);
            var formatter = new BinaryFormatter();
            var saveDataOne = RsaEncrypt(dataOne);
            var saveDataTwo = RsaEncrypt(dataTwo);
            using (var stream = new FileStream(_filePath, FileMode.Open, FileAccess.Write))
            {
                formatter.Serialize(stream, saveDataOne);
                formatter.Serialize(stream, saveDataTwo);
            }
            EncryptFile();
        }

        public static string[] RetrieveData()
        {
            var retrieveDataOne = string.Empty;
            var retrieveDataTwo = string.Empty;

            var formatter = new BinaryFormatter();
            using (var fileStream = new FileStream(DecryptFile(_filePath), FileMode.Open, FileAccess.Read))
            {
                if (fileStream.Length != 0)
                {
                    retrieveDataOne = RsaDecrypt((string)formatter.Deserialize(fileStream));
                    retrieveDataTwo = RsaDecrypt((string)formatter.Deserialize(fileStream));
                }
                EncryptFile();
            }
            return new[] { retrieveDataOne, retrieveDataTwo };
        }

        public static Dictionary<string, string> RetrieveAllData()
        {
            var userData = new Dictionary<string, string>();
            var directory = CreateDirectory();
            var filePaths = Directory.GetFiles(directory).ToList();

            foreach (var file in filePaths)
            {
                _filePath = file;
                var userInfo = RetrieveData();
                if (!string.IsNullOrEmpty(userInfo[0]))
                {
                    userData.Add(userInfo[0], userInfo[1]);
                }
            }

            return userData;
        }

        private static string RsaEncrypt(string value)
        {
            var plaintext = Encoding.Unicode.GetBytes(value);

            var cspParams = new CspParameters
            {
                KeyContainerName = KeyContainerName
            };
            using (var rsa = new RSACryptoServiceProvider(2048, cspParams))
            {
                var encryptedData = rsa.Encrypt(plaintext, false);
                return Convert.ToBase64String(encryptedData);
            }
        }

        private static string RsaDecrypt(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            var encryptedData = Convert.FromBase64String(value);

            var cspParams = new CspParameters
            {
                KeyContainerName = KeyContainerName
            };
            using (var rsa = new RSACryptoServiceProvider(2048, cspParams))
            {
                var decryptedData = rsa.Decrypt(encryptedData, false);
                return Encoding.Unicode.GetString(decryptedData);
            }
        }
    }
}
