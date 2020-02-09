using SaintSender.Core.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using SaintSender.Core.Exceptions;

namespace SaintSender.Core.Services
{
    public class EncryptionService
    {
        private const string KeyContainerName = "MPPOFKWE12312312";

        public List<UserInfo> RetrieveAllData()
        {
            var directory = GetDirectory();
            var filePaths = Directory.GetFiles(directory).ToList();

            return filePaths
                .Select(RetrieveData)
                .Where(userInfo => !string.IsNullOrEmpty(userInfo.Email))
                .ToList();
        }

        public string RetrievePassword(string emailAddress)
        {
            return RetrieveData(GetFilePath(emailAddress)).Password;
        }

        public void SaveData(string emailAddress, string password)
        {
            var path = CreateFile(emailAddress);

            var saveDataOne = RsaEncrypt(emailAddress);
            var saveDataTwo = RsaEncrypt(password);

            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Write))
            {
                var formatter = new BinaryFormatter();

                formatter.Serialize(stream, saveDataOne);
                formatter.Serialize(stream, saveDataTwo);
            }

            File.Encrypt(path);
        }

        private UserInfo RetrieveData(string filePath)
        {
            var formatter = new BinaryFormatter();

            try
            {
                File.Decrypt(filePath);
            }
            catch (Exception e) when (e is ArgumentException || e is IOException)
            {
                throw new DataRetrievalException("The directory or file could not be loaded.");
            }
            catch (NotSupportedException e)
            {
                throw new DataRetrievalException("Unsupported operation.", e);
            }
            catch (UnauthorizedAccessException e)
            {
                throw new DataRetrievalException("Access denied.", e);
            }

            using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

            try
            {
                if (fileStream.Length == 0)
                {
                    return null;
                }

                return new UserInfo
                {
                    Email = RsaDecrypt((string) formatter.Deserialize(fileStream)),
                    Password = RsaDecrypt((string) formatter.Deserialize(fileStream))

                };
            }
            catch (SerializationException e)
            {
                throw new DataRetrievalException("Unsuccessful serialization.", e);
            }
            finally
            {
                File.Encrypt(filePath);
            }
        }

        private string GetDirectory()
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            var dataDirectory = currentDirectory + @"\Data";

            Directory.CreateDirectory(dataDirectory);

            return dataDirectory;
        }

        private string GetFilePath(string emailAddress)
        {
            var bytes = Encoding.ASCII.GetBytes(emailAddress);
            var dataDirectory = GetDirectory();

            return $@"{dataDirectory}\{string.Join("", bytes)}.txt";
        }

        private string CreateFile(string fileName)
        {
            var bytes = Encoding.ASCII.GetBytes(fileName);
            var dataDirectory = GetDirectory();
            var filePath = $@"{dataDirectory}\{string.Join("", bytes)}.txt";

            if (!File.Exists(filePath))
            {
                File.Create(filePath).Dispose();
            }

            return filePath;
        }

        private string RsaEncrypt(string value)
        {
            var plaintext = Encoding.Unicode.GetBytes(value);

            var cspParams = new CspParameters
            {
                KeyContainerName = KeyContainerName
            };
            using var rsa = new RSACryptoServiceProvider(2048, cspParams);
            var encryptedData = rsa.Encrypt(plaintext, false);

            return Convert.ToBase64String(encryptedData);
        }

        private string RsaDecrypt(string value)
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

            using var rsa = new RSACryptoServiceProvider(2048, cspParams);
            var decryptedData = rsa.Decrypt(encryptedData, false);

            return Encoding.Unicode.GetString(decryptedData);
        }
    }
}
