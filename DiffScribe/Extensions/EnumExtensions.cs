using DiffScribe.Configuration.Enums;

namespace DiffScribe.Extensions;

public static class EnumExtensions
{
    public static string ToApiName(this LlmModel model) => model switch
    {
        LlmModel.Gpt4o => "gpt-4o",
        LlmModel.Gpt4oMini => "gpt-4o-mini",
        LlmModel.Gpt4_1 => "gpt-4.1",
        LlmModel.Gpt4_1Mini => "gpt-4.1-mini",
        LlmModel.Gpt4_1Nano => "gpt-4.1-nano",
        _ => throw new ArgumentOutOfRangeException(nameof(model))
    };

    public static string ToDisplayName(this LlmModel model) => model switch
    {
        LlmModel.Gpt4o => "GPT-4o",
        LlmModel.Gpt4oMini => "GPT-4o mini",
        LlmModel.Gpt4_1 => "GPT-4.1",
        LlmModel.Gpt4_1Mini => "GPT-4.1 mini",
        LlmModel.Gpt4_1Nano => "GPT-4.1 nano",
        _ => throw new ArgumentOutOfRangeException(nameof(model))
    };
        
    public static string GetStats(this LlmModel model) => model switch
    {
        LlmModel.Gpt4o => $"Intelligence: {new string('\u2022', 3)}, Cost: {new string('\u2022', 5)}",
        LlmModel.Gpt4oMini => $"Intelligence: {new string('\u2022', 2)}, Cost: {new string('\u2022', 2)}",
        LlmModel.Gpt4_1 => $"Intelligence: {new string('\u2022', 4)}, Cost: {new string('\u2022', 4)}",
        LlmModel.Gpt4_1Mini => $"Intelligence: {new string('\u2022', 3)}, Cost: {new string('\u2022', 3)}",
        LlmModel.Gpt4_1Nano => $"Intelligence: {new string('\u2022', 2)}, Cost: {new string('\u2022', 1)}",
        _ => throw new ArgumentOutOfRangeException(nameof(model))
    };
    
    public static void UpdateSelectedOption<T>(this string[] selections, string value) where T : struct, Enum
    {
        if (Enum.TryParse(value, out T enumValue))
        {
            var configuredOptionIdx = Convert.ToInt32(enumValue);
            selections[configuredOptionIdx] = $"[X] {selections[configuredOptionIdx]}";
        }
    }

    public static string ParseApiName(this string configuredLlm) =>
        Enum.TryParse<LlmModel>(configuredLlm, out var model) ? 
            model.ToApiName() :
            Enum.GetValues<LlmModel>()[0].ToApiName();
}