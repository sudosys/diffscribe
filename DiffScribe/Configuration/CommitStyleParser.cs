using DiffScribe.Configuration.Enums;

namespace DiffScribe.Configuration;

public static class CommitStyleParser
{
    public static bool TryParse(string? value, out CommitStyle style)
    {
        style = CommitStyle.Standard;

        var given = value?.Trim();
        
        // A numeric value would be parsed into an undefined enum member, so it is rejected upfront.
        if (string.IsNullOrEmpty(given) || int.TryParse(given, out _))
        {
            return false;
        }

        if (!Enum.TryParse<CommitStyle>(given, ignoreCase: true, out var parsedStyle) 
            || !Enum.IsDefined(parsedStyle))
        {
            return false;
        }

        style = parsedStyle;
        return true;
    }
}
