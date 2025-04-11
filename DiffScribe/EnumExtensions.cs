using DiffScribe.Configuration;

namespace DiffScribe;

public static class EnumExtensions
{
    public static string ToApiName(this LlmModel model) => model switch
    {
        LlmModel.Gpt3_5 => "gpt-3.5-turbo",
        LlmModel.Gpt4o => "gpt-4o",
        LlmModel.Gpt4oMini => "gpt-4o-mini",
        _ => throw new ArgumentOutOfRangeException(nameof(model))
    };
}