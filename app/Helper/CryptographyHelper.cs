using ExcelAssess.Cryptography.Cryptography;
using Newtonsoft.Json;

namespace ExcelAssess.TestPlayer.ResponseVerification.Console.Helper
{
    public static class CryptographyHelper
    {
        public static T? Decrypt<T>(string encryptedData, string symmetricKey) where T : class
        {
            string decryptedData = Decrypt(encryptedData, symmetricKey);
            var JsonObj = JsonConvert.DeserializeObject<T>(decryptedData);
            return JsonObj;
        }
        public static string Decrypt(string encryptedData, string symmetricKey)
        {
            return SymmetricCryptography.Decrypt(symmetricKey, encryptedData);
        }
        public static string Encrypt<T>(T data, string symmetricKey)
        {
            string jsonStringData = JsonConvert.SerializeObject(data);
            string encryptedData = Encrypt(jsonStringData, symmetricKey);
            return encryptedData;
        }
        public static string Encrypt(string data, string symmetricKey)
        {
            string encryptedData = SymmetricCryptography.Encrypt(symmetricKey, data);
            return encryptedData;
        }

    }
}
