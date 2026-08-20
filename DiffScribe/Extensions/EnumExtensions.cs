using DiffScribe.Configuration.Enums;
using Spectre.Console;

namespace DiffScribe.Extensions;

public static class EnumExtensions
{
    private const LlmModel DefaultModel = LlmModel.Gpt5_6Luna;
    
    public static string ToDisplayName(this LlmModel model) => model switch
    {
        LlmModel.Gpt5_6Terra => "GPT-5.6 Terra",
        LlmModel.Gpt5_6Luna => "GPT-5.6 Luna",
        LlmModel.Gpt5_4Mini => "GPT-5.4 mini",
        LlmModel.Gpt5_4Nano => "GPT-5.4 nano",
        _ => throw new ArgumentOutOfRangeException(nameof(model))
    };
        
    public static string GetStats(this LlmModel model) => model switch
    {
        LlmModel.Gpt5_6Terra => FormatStats(intelligence: 4, speed: 3, cost: 3),
        LlmModel.Gpt5_6Luna => FormatStats(intelligence: 3, speed: 5, cost: 1),
        LlmModel.Gpt5_4Mini => FormatStats(intelligence: 3, speed: 4, cost: 2),
        LlmModel.Gpt5_4Nano => FormatStats(intelligence: 2, speed: 5, cost: 1),
        _ => throw new ArgumentOutOfRangeException(nameof(model))
    };

    private static string FormatStats(int intelligence, int speed, int cost)
        => $"Intelligence: {new string('\u2022', intelligence)}, "
           + $"Speed: {new string('\u2022', speed)}, "
           + $"Cost: {new string('\u2022', cost)}";
    
    /// <summary>
    /// Upper bound of characters the subject line of the commit message may use.
    /// </summary>
    public static int ToMaxSubjectLength(this CommitLength length) => length switch
    {
        CommitLength.Short => 50,
        CommitLength.Standard => 72,
        CommitLength.Long => 100,
        _ => throw new ArgumentOutOfRangeException(nameof(length))
    };

    public static string GetDescription(this CommitLength length) => length switch
    {
        CommitLength.Short => "Compact subject line that stays readable in every git client.",
        CommitLength.Standard => "Subject line length recommended by git.",
        CommitLength.Long => "Roomy subject line for changes that need more wording.",
        _ => throw new ArgumentOutOfRangeException(nameof(length))
    };
    
    public static void UpdateSelectedOption<T>(ref Dictionary<T, string> selections, string value) where T : struct, Enum
    {
        if (!Enum.TryParse(value, out T enumValue))
        {
            return;
        }
        
        var configuredOptionIdx = Convert.ToInt32(enumValue);

        selections = selections
            .Select(p 
                => (p.Key, Convert.ToInt32(p.Key) == configuredOptionIdx ? 
                            Markup.Escape($"[X] {p.Value}") :
                            p.Value))
            .ToDictionary();
    }

    public static string ParseApiName(this string configuredLlm)
    {
        if (Enum.TryParse<LlmModel>(configuredLlm, out var model) && Enum.IsDefined(model))
        {
            return model.ToApiName();
        }

        ConsoleWrapper.Warning($"The configured model \"{configuredLlm}\" is not available anymore.");
        ConsoleWrapper.Info($"Proceeding with \"{DefaultModel.ToDisplayName()}\". "
                            + $"Run \"{AppInfo.ExecutableName} config --llm\" to pick another model.");

        return DefaultModel.ToApiName();
    }

    private static string ToApiName(this LlmModel model) => model switch
    {
        LlmModel.Gpt5_6Terra => "gpt-5.6-terra",
        LlmModel.Gpt5_6Luna => "gpt-5.6-luna",
        LlmModel.Gpt5_4Mini => "gpt-5.4-mini",
        LlmModel.Gpt5_4Nano => "gpt-5.4-nano",
        _ => throw new ArgumentOutOfRangeException(nameof(model))
    };
}
