using DiffScribe.Configuration.Enums;
using DiffScribe.Extensions;

namespace DiffScribe.Configuration;

/// <summary>
/// Turns a configured or given commit length into the maximum number of characters
/// the subject line may use. The resolved value is an upper bound, never a target.
/// Accepted values are the <see cref="CommitLength"/> presets or a custom character count.
/// </summary>
public static class CommitLengthResolver
{
    public const int MinCustomLength = 20;
    public const int MaxCustomLength = 120;

    public static readonly CommitLength DefaultPreset = CommitLength.Standard;

    public static bool TryResolve(string? value, out int maxSubjectLength, out string? error)
    {
        maxSubjectLength = DefaultPreset.ToMaxSubjectLength();
        error = null;

        var given = value?.Trim();
        if (string.IsNullOrEmpty(given))
        {
            error = $"A commit length must be given as a preset ({GetPresetNames()}) or as a character count.";
            return false;
        }

        if (int.TryParse(given, out var customLength))
        {
            if (!IsValidCustomLength(customLength))
            {
                error = $"A custom commit length must be between {MinCustomLength} and {MaxCustomLength} characters. \"{given}\" is out of range.";
                return false;
            }

            maxSubjectLength = customLength;
            return true;
        }

        if (!Enum.TryParse<CommitLength>(given, ignoreCase: true, out var preset) || !Enum.IsDefined(preset))
        {
            error = $"\"{given}\" is not a valid commit length. Use one of the presets ({GetPresetNames()}) "
                    + $"or a character count between {MinCustomLength} and {MaxCustomLength}.";
            return false;
        }

        maxSubjectLength = preset.ToMaxSubjectLength();
        return true;
    }

    public static bool IsValidCustomLength(int length) 
        => length is >= MinCustomLength and <= MaxCustomLength;

    public static string GetPresetNames() => string.Join(", ", Enum.GetNames<CommitLength>());
}
