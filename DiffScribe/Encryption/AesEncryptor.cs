using System.Security.Cryptography;

namespace DiffScribe.Encryption;

#pragma warning disable CA1416

public class AesEncryptor(SecretKeyHandler secretKeyHandler)
{

    public string EncryptText(string plainText)
    {
        using var aes = Aes.Create();
        aes.Key = Convert.FromBase64String(secretKeyHandler.GetKey());
        aes.GenerateIV();

        using MemoryStream memoryStream = new();
        using CryptoStream cryptoStream = new(memoryStream, aes.CreateEncryptor(), CryptoStreamMode.Write);
        using (StreamWriter writer = new(cryptoStream))
        {
            writer.Write(plainText);
        }

        byte[] iv = aes.IV;
        byte[] encryptedContent = memoryStream.ToArray();
        byte[] result = new byte[iv.Length + encryptedContent.Length];

        Buffer.BlockCopy(iv, 0, result, 0, iv.Length);
        Buffer.BlockCopy(encryptedContent, 0, result, iv.Length, encryptedContent.Length);

        return Convert.ToBase64String(result);
    }

    public string DecryptText(string encryptedText)
    {
        using var aes = Aes.Create();
        aes.Key = Convert.FromBase64String(secretKeyHandler.GetKey());

        byte[] fullText = Convert.FromBase64String(encryptedText);

        byte[] iv = new byte[aes.BlockSize / 8];
        Buffer.BlockCopy(fullText, 0, iv, 0, iv.Length);
        aes.IV = iv;

        byte[] cipherText = new byte[fullText.Length - iv.Length];
        Buffer.BlockCopy(fullText, iv.Length, cipherText, 0, cipherText.Length);

        using MemoryStream memoryStream = new(cipherText);
        using CryptoStream cryptoStream = new(memoryStream, aes.CreateDecryptor(), CryptoStreamMode.Read);
        using StreamReader reader = new(cryptoStream);

        return reader.ReadToEnd();
    }
}