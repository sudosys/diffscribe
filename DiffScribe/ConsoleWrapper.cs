using Spectre.Console;

namespace DiffScribe;

public static class ConsoleWrapper
{
    public static void Warning(string line)
    {
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.Write("\u26a0  Warning ");
        Console.ResetColor();
        Console.WriteLine(line);
    }
    
    public static void Error(string line)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Write("\u274c  Error ");
        Console.ResetColor();
        Console.WriteLine(line);
    }
    
    public static void Info(string line)
    {
        Console.WriteLine($"\u2139\uFE0F {line}");
    }
    
    public static void Success(string line)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write("\u2705  Success ");
        Console.ResetColor();
        Console.WriteLine(line);
    }

    public static void ShowLoadingText(string text, CancellationToken cancellationToken)
    {
        AnsiConsole
            .Status()
            .Spinner(Spinner.Known.Dots)
            .SpinnerStyle(Style.Parse("blue"))
            .StartAsync(text, async _ =>
            {
                while (true)
                {
                    await Task.Delay(500, cancellationToken);

                    if (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }
                }
            });
    }

    public static bool ShowConfirmation(string text)
    {
        return AnsiConsole.Prompt(
            new TextPrompt<bool>(text)
                .AddChoice(true)
                .AddChoice(false)
                .DefaultValue(false)
                .ShowChoices(true)
                .WithConverter(choice => choice ? "y" : "n"));
    }

    public static async Task ShowProgressBar(Func<ProgressContext, Task> ctxAction)
    {
        await AnsiConsole.Progress()
            .Columns(
                new PercentageColumn(),
                new ProgressBarColumn(),
                new DownloadedColumn())
            .StartAsync(ctxAction);
    }
}