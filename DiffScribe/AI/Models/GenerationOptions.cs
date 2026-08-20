using DiffScribe.Configuration.Enums;

namespace DiffScribe.AI.Models;

/// <summary>
/// One-time generation overrides given through the "generate" command.
/// A null member means "fall back to the configured value".
/// </summary>
public class GenerationOptions
{
    public CommitStyle? CommitStyle { get; init; }

    public int? MaxSubjectLength { get; init; }

    public string? Steer { get; init; }
}
