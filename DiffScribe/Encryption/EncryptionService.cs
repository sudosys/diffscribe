using System.Reflection;
using System.Security.Cryptography;
using System.Security.AccessControl;
using System.Security.Principal;

namespace DiffScribe.Encryption;

#pragma warning disable CA1416

public class EncryptionService
{
    private const string SecretFileName = ".secret";
    
    private readonly string _secretKeyFilePath = Path.Combine(
        Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
        SecretFileName);
    
    public string EncryptText(string plainText)
    {
        using var aes = Aes.Create();
        aes.Key = Convert.FromBase64String(GetSecretKey());
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
        aes.Key = Convert.FromBase64String(GetSecretKey());

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
    
    private string GetSecretKey() => 
        File.Exists(_secretKeyFilePath) ? File.ReadAllText(_secretKeyFilePath) : UpdateSecretKey();

    public string UpdateSecretKey()
    {
        var key = CreateSecretKey();
        WriteSecretKey(key);
        return key;
    }

    private string CreateSecretKey()
    {
        var key = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(key);
        return Convert.ToBase64String(key);
    }

    private void WriteSecretKey(string key)
    {
        if (!File.Exists(_secretKeyFilePath))
        {
            if (OperatingSystem.IsWindows())
            {
                CreateRestrictedFileWindows();
            }
            else
            {
                CreateRestrictedFileUnix();
            }
        }
        
        File.WriteAllText(_secretKeyFilePath, key);
        
        SetFileAttributes();
    }
    
    private void CreateRestrictedFileWindows()
    {
        var fileInfo = new FileInfo(_secretKeyFilePath);
        
        fileInfo.Create().Close();
        fileInfo.SetAccessControl(SetFileAccessRules());
    }

    private FileSecurity SetFileAccessRules()
    {
        var fileSecurity = new FileSecurity();
        fileSecurity.SetAccessRuleProtection(true, false);
        
        var everyone = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
        fileSecurity.AddAccessRule(new FileSystemAccessRule(
            everyone,
            FileSystemRights.FullControl,
            AccessControlType.Deny));
        
        var currentUser = WindowsIdentity.GetCurrent().User;
        if (currentUser != null)
        {
            fileSecurity.AddAccessRule(new FileSystemAccessRule(
                currentUser,
                FileSystemRights.Read | FileSystemRights.Write,
                AccessControlType.Allow));
        }

        return fileSecurity;
    }
    
    private void CreateRestrictedFileUnix()
    {
        File.Create(_secretKeyFilePath).Close();
        File.SetUnixFileMode(_secretKeyFilePath, UnixFileMode.UserRead | UnixFileMode.UserWrite);
    }

    private void SetFileAttributes() => 
        File.SetAttributes(_secretKeyFilePath, FileAttributes.ReadOnly | FileAttributes.Hidden);
}