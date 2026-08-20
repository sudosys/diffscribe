namespace DiffScribe.Suggestions;

/// <summary>
/// Recommends the closest known command or argument for a mistyped one.
/// </summary>
public static class SuggestionEngine
{
    private const int MaxSuggestions = 3;

    private const int PrefixScore = 0;
    private const int ContainmentScore = 1;
    private const int DistanceScoreOffset = 2;

    public static string BuildRecommendation(string input, IEnumerable<string> candidates)
    {
        var suggestions = FindClosest(input, candidates);

        return suggestions.Length == 0
            ? string.Empty
            : $"Did you mean {string.Join(" or ", suggestions.Select(suggestion => $"\"{suggestion}\""))}?";
    }

    public static string[] FindClosest(string input, IEnumerable<string> candidates)
    {
        var normalizedInput = input.Trim().ToLowerInvariant();

        if (normalizedInput.Length == 0)
        {
            return [];
        }

        var threshold = GetDistanceThreshold(normalizedInput.Length);

        return candidates
            .Where(candidate => !string.IsNullOrWhiteSpace(candidate))
            .Select(candidate => (Candidate: candidate, Score: GetScore(normalizedInput, candidate.ToLowerInvariant(), threshold)))
            .Where(match => match.Score.HasValue)
            .OrderBy(match => match.Score!.Value)
            .ThenBy(match => match.Candidate, StringComparer.Ordinal)
            .Take(MaxSuggestions)
            .Select(match => match.Candidate)
            .ToArray();
    }

    private static int? GetScore(string input, string candidate, int threshold)
    {
        if (candidate.StartsWith(input) || input.StartsWith(candidate))
        {
            return PrefixScore;
        }

        if (candidate.Contains(input))
        {
            return ContainmentScore;
        }

        var distance = GetEditDistance(input, candidate);

        return distance <= threshold ? distance + DistanceScoreOffset : null;
    }

    private static int GetDistanceThreshold(int inputLength) => inputLength switch
    {
        <= 3 => 1,
        <= 6 => 2,
        _ => 3
    };

    /// <summary>
    /// Levenshtein distance calculated with a rolling two-row matrix.
    /// </summary>
    private static int GetEditDistance(string source, string target)
    {
        var previousRow = new int[target.Length + 1];
        var currentRow = new int[target.Length + 1];

        for (var column = 0; column <= target.Length; column++)
        {
            previousRow[column] = column;
        }

        for (var row = 1; row <= source.Length; row++)
        {
            currentRow[0] = row;

            for (var column = 1; column <= target.Length; column++)
            {
                var substitutionCost = source[row - 1] == target[column - 1] ? 0 : 1;

                currentRow[column] = Math.Min(
                    Math.Min(currentRow[column - 1] + 1, previousRow[column] + 1),
                    previousRow[column - 1] + substitutionCost);
            }

            (previousRow, currentRow) = (currentRow, previousRow);
        }

        return previousRow[target.Length];
    }
}
