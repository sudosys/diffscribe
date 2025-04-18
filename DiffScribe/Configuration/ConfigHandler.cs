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
    private const string NotSetValuePlaceHolder = "<NOT SET>";

    public ToolConfiguration Configuration { get; }

    private readonly EncryptionService _encryptionService;

    public ConfigHandler(EncryptionService encryptionService)
    {
        Configuration = GetConfigurationFromFile();
        _encryptionService = encryptionService ?? throw new ArgumentNullException(nameof(encryptionService));
    }

    public void ReadConfigFile()
    {
        var table = CreateConfigurationInformation();
        
        table.Write(Format.Minimal);
    }
    
    private ToolConfiguration GetConfigurationFromFile()
    {
        var serialized = string.Empty;
        try
        {
            serialized = File.ReadAllText(ConfigFilePath);
        }
        catch (FileNotFoundException)
        {
            HandleNonExistence();
        }
        catch (DirectoryNotFoundException)
        {
            HandleNonExistence();
        }
        
        return Deserialize(serialized);
        void HandleNonExistence()
        {
            ConsoleWrapper.Warning("Configuration not found. Creating default configuration file.");
            TryCreateConfigFile();

            ReadConfigFile();
        }
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

        if (value is string stringValue)
        {
            return stringValue.Length == 0 ? 
                NotSetValuePlaceHolder :
                TryTruncateValue(stringValue);
        }
        
        return value?.ToString() ?? NotSetValuePlaceHolder;
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
            commitStyle: CommitStyle.Standard.ToString(),
            autoCommit: false,
            apiKey: string.Empty,
            llm: LlmModel.Gpt4oMini.ToApiName());
    }

    #region API Key
    public void UpdateApiKey(string apiKey)
    {
        var encrypted = _encryptionService.EncryptText(apiKey);
        
        Configuration.ApiKey = encrypted;
    }

    public string ReadApiKey() 
        => _encryptionService.DecryptText(Configuration.ApiKey);
    #endregion

    public void UpdateConfiguration() => WriteToFile(Serialize(Configuration));

    private string Serialize(ToolConfiguration toolConfiguration) => 
        JsonSerializer.Serialize(toolConfiguration, _serializerOptions);

    private ToolConfiguration Deserialize(string serialized) => 
        JsonSerializer.Deserialize<ToolConfiguration>(serialized, _serializerOptions)!;

    private void WriteToFile(string serializedConfig) => File.WriteAllText(ConfigFilePath, serializedConfig);

    public bool IsApiKeySet() => !string.IsNullOrEmpty(Configuration.ApiKey);
}