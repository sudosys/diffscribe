using System.Reflection;
using System.Text.Json;
using ConsoleTables;
using DiffScribe.Configuration.Enums;
using DiffScribe.Encryption;
using DiffScribe.Extensions;

namespace DiffScribe.Configuration;

public class ConfigHandler
{
    private const string ConfigFileName = ".dsc_config";

    private static readonly string DiffScribeFolderPath =
        $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}{Path.DirectorySeparatorChar}DiffScribe";
    
    private static readonly string ConfigFilePath = 
        $"{DiffScribeFolderPath}{Path.DirectorySeparatorChar}{ConfigFileName}";
    
    private readonly JsonSerializerOptions _serializerOptions = new()
    {
        WriteIndented = true,
        AllowTrailingCommas = false
    };

    private const int MaxValueColumnWidth = 25; 
    private const string SetValuePlaceHolder = "<SET>";
    private const string NotSetValuePlaceHolder = "<NOT SET>";

    private readonly AesEncryptor _aesEncryptor;
    private readonly SecretKeyHandler _secretKeyHandler;
    
    public ToolConfiguration Configuration { get; }

    public ConfigHandler(AesEncryptor aesEncryptor, SecretKeyHandler secretKeyHandler)
    {
        Configuration = ReadConfiguration();
        _aesEncryptor = aesEncryptor ?? throw new ArgumentNullException(nameof(aesEncryptor));
        _secretKeyHandler = secretKeyHandler ?? throw new ArgumentNullException(nameof(secretKeyHandler));
    }
    
    private ToolConfiguration ReadConfiguration()
    {
        string serialized;
        try
        {
            serialized = File.ReadAllText(ConfigFilePath);
        }
        catch (FileNotFoundException)
        {
            return TryCreateAndReadConfig();
        }
        catch (DirectoryNotFoundException)
        {
            return TryCreateAndReadConfig();
        }
        
        return Deserialize(serialized);
        ToolConfiguration TryCreateAndReadConfig()
        {
            ConsoleWrapper.Info("Creating default configuration file.");
            TryCreateConfigFile();
            
            return ReadConfiguration();
        }
    }

    public void PrintCurrentConfigAsTable()
    {
        var table = CreateConfigurationInformation();
        
        table.Write(Format.Minimal);
    }
    
    private ConsoleTable CreateConfigurationInformation()
    {
        var type = Configuration.GetType();
        var properties = type.GetProperties();

        var table = new ConsoleTable("Configuration", "Value");
        foreach (var propertyInfo in properties)
        {
            table.AddRow(propertyInfo.Name.PascalToTitleCase(), GetPropertyValue(Configuration, propertyInfo));
        }
        
        return table;
    }
    
    private string GetPropertyValue(ToolConfiguration toolConfig, PropertyInfo propertyInfo)
    {
        var value = propertyInfo.GetValue(toolConfig);

        if (value is not string stringValue)
        {
            return value?.ToString() ?? NotSetValuePlaceHolder;
        }

        return stringValue.Length switch
        {
            > 0 when propertyInfo.Name == nameof(toolConfig.ApiKey) => SetValuePlaceHolder,
            > 0 => TryTruncateValue(stringValue),
            _ => NotSetValuePlaceHolder
        };
    }

    private string TryTruncateValue(string value)
    {
        var maxLength = Math.Min(value.Length, MaxValueColumnWidth);
        var truncated = maxLength == MaxValueColumnWidth;

        return $"{value[..maxLength]}{(truncated ? "..." : string.Empty)}";
    }
    
    public void ResetConfiguration()
    {
        File.Delete(ConfigFilePath);
        
        TryCreateConfigFile();
    }

    public void TryCreateConfigFile()
    {
        if (DoesConfigExist())
        {
            return;
        }
        
        var defaultConfig = GetDefaultConfiguration();
        var serialized = Serialize(defaultConfig);

        Directory.CreateDirectory(DiffScribeFolderPath);
        File.Create(ConfigFilePath).Dispose();
        WriteToFile(serialized);
    }

    private bool DoesConfigExist() => File.Exists(ConfigFilePath);

    private ToolConfiguration GetDefaultConfiguration()
    {
        return new ToolConfiguration(
            commitStyle: nameof(CommitStyle.Standard),
            autoCommit: false,
            apiKey: string.Empty,
            llm: nameof(LlmModel.Gpt4oMini));
    }

    #region API Key
    public void UpdateApiKey(string apiKey)
    {
        _secretKeyHandler.CreateKey();
        
        var encrypted = _aesEncryptor.EncryptText(apiKey);
        
        Configuration.ApiKey = encrypted;
    }

    public string ReadApiKey() 
        => _aesEncryptor.DecryptText(Configuration.ApiKey);
    #endregion

    public void UpdateConfiguration() => WriteToFile(Serialize(Configuration));

    private static void WriteToFile(string serializedConfig) 
        => File.WriteAllText(ConfigFilePath, serializedConfig);

    private string Serialize(ToolConfiguration toolConfiguration) 
        => JsonSerializer.Serialize(toolConfiguration, _serializerOptions);

    private ToolConfiguration Deserialize(string serialized) 
        => JsonSerializer.Deserialize<ToolConfiguration>(serialized, _serializerOptions)!;
    
    public bool IsApiKeySet() => !string.IsNullOrEmpty(Configuration.ApiKey);

    public void RemoveConfiguration()
    {
        try
        {
            Directory.Delete(DiffScribeFolderPath, recursive: true);
        }
        catch (DirectoryNotFoundException e)
        {
            ConsoleWrapper.Error(e.Message);
        }
    }
}