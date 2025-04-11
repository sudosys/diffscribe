using System.Text.RegularExpressions;

namespace DiffScribe;

public static class RegexExtensions
{
    public static string PascalToTitleCase(this string input) => 
        Regex.Replace(input, "(?<!^)([A-Z])", " $1");
}