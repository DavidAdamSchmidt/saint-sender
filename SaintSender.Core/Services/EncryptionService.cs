using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SaintSender.Core.Services
{
    public static class EncryptionService
    {
        private static string FilePath;

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
            catch (Exception e) when(e is ArgumentException || e is IOException)
            {
                MessageBox.Show("The directory or file could not be loaded!", "Directory Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception e) when(e is NotSupportedException)
            {
                MessageBox.Show("Your operation is not supported, please report this error!", "Unknown Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception e) when(e is UnauthorizedAccessException)
            {
                MessageBox.Show("You are trying to open files not supported by your Account!", "Unauthorized Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return path;
        }

        private static void EncryptFile()
        {
            File.Encrypt(FilePath);
        }

        private static void CreateFile(string fileName)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(fileName);
            var dataDirectory = CreateDirectory();
            FilePath = dataDirectory + $@"\{string.Join("", bytes)}.txt";
            if (!File.Exists(FilePath))
            {
                File.Create(FilePath);
            }
        }

        public static void SaveData(string DataOne, string DataTwo)
        {
            CreateFile(DataOne);
            var formatter = new BinaryFormatter();
            var saveDataOne = RsaEncrypt(DataOne);
            var saveDataTwo = RsaEncrypt(DataTwo);
            using (var stream = new FileStream(FilePath, FileMode.Open, FileAccess.Write))
            {
                formatter.Serialize(stream, saveDataOne);
                formatter.Serialize(stream, saveDataTwo);
            }
            EncryptFile();
        }

        public static string[] RetreiveData()
        {
            var retreiveDataOne = string.Empty;
            var retreiveDataTwo = string.Empty;

            var formatter = new BinaryFormatter();
            using (var fileStream = new FileStream(DecryptFile(FilePath), FileMode.Open, FileAccess.Read))
            {
                if (!(fileStream.Length == 0))
                {
                    retreiveDataOne = RsaDecrypt((string)formatter.Deserialize(fileStream));
                    retreiveDataTwo = RsaDecrypt((string)formatter.Deserialize(fileStream));
                }
                EncryptFile();
            }
            return new string[] { retreiveDataOne, retreiveDataTwo};
        }

        public static Dictionary<string, string> RetreiveAllData()
        {
            var userData = new Dictionary<string, string>();
            var directory = CreateDirectory();
            var filePaths = Directory.GetFiles(directory).ToList();

            foreach (var file in filePaths)
            {
                FilePath = file;
                var userInfo = RetreiveData();
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
                KeyContainerName = "MPPOFKWE12312312"
            };
            using (var rsa = new RSACryptoServiceProvider(2048, cspParams))
            {
                var encryptedData = rsa.Encrypt(plaintext, false);
                return Convert.ToBase64String(encryptedData);
            }
        }

        private static string RsaDecrypt(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                var encryptedData = Convert.FromBase64String(value);

                var cspParams = new CspParameters
                {
                    KeyContainerName = "MPPOFKWE12312312"
                };
                using (var rsa = new RSACryptoServiceProvider(2048, cspParams))
                {
                    var decryptedData = rsa.Decrypt(encryptedData, false);
                    return Encoding.Unicode.GetString(decryptedData);
                }
            }
            return string.Empty;
        }
    }
}
