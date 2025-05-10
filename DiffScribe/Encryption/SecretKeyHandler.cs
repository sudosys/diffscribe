using System.Security.Cryptography;

namespace DiffScribe.Encryption;

public abstract class SecretKeyHandler
{
    private const string SecretFileName = ".secret";
    
    private static readonly string SecretKeyDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        ".dsc");
    
    protected readonly string SecretKeyFilePath = Path.Combine(SecretKeyDirectory, SecretFileName);
    
    public string GetKey() => 
        File.Exists(SecretKeyFilePath) ? File.ReadAllText(SecretKeyFilePath) : CreateKey();

    public string CreateKey()
    {
        var key = GenerateKey();
        WriteSecretKey(key);
        return key;
    }
    
    private string GenerateKey()
    {
        var key = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(key);
        return Convert.ToBase64String(key);
    }

    private void WriteSecretKey(string key)
    {
        if (!Directory.Exists(SecretKeyDirectory))
        {
            Directory.CreateDirectory(SecretKeyDirectory);
        }
        
        if (!File.Exists(SecretKeyFilePath))
        {
            CreateRestrictedFile();
        }
        
        File.WriteAllText(SecretKeyFilePath, key);
        
        SetFileAttributes();
    }
    
    protected abstract void CreateRestrictedFile();

    private void SetFileAttributes()
    {
        File.SetAttributes(SecretKeyDirectory, FileAttributes.Hidden);
        File.SetAttributes(SecretKeyFilePath, FileAttributes.Hidden);
    }
    
    public void DeleteKey()
    {
        try
        {
            Directory.Delete(SecretKeyDirectory, recursive: true);
        }
        catch (Exception)
        {
            // ignored
        }
    }
}