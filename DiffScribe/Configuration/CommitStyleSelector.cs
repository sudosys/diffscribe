using DiffScribe.Configuration.Enums;
using DiffScribe.Extensions;
using Spectre.Console;

namespace DiffScribe.Configuration;

/// <summary>
/// Interactive commit style selection shared by the configuration and the generation commands.
/// </summary>
public static class CommitStyleSelector
{
    private static readonly Dictionary<CommitStyle, string> Selections = new()
    {
        { CommitStyle.Minimal, $"{CommitStyle.Minimal} (Short, one-line commit message.)" },
        { CommitStyle.Standard, $"{CommitStyle.Standard} (Clear commit message with brief context.)" },
        { CommitStyle.Detailed, $"{CommitStyle.Detailed} (Descriptive commit message followed by an in-depth explanation.)" },
    };

    public static CommitStyle Select(string configuredStyle, string title)
    {
        var selections = new Dictionary<CommitStyle, string>(Selections);
        EnumExtensions.UpdateSelectedOption(ref selections, configuredStyle);

        var selection = AnsiConsole.Prompt(new SelectionPrompt<string>()
            .Title(title)
            .AddChoices(selections.Values));

        return selections
            .First(p => p.Value == selection)
            .Key;
    }
}
