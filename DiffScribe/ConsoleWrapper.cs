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
        Console.CursorVisible = false;

        Task.Run(async () =>
        {
            try
            {
                await StartLoadingTextLoop(text, cancellationToken);
            }
            finally
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    Console.CursorVisible = true;
                }
            }
        }, cancellationToken);
    }

    private static async Task StartLoadingTextLoop(string text, CancellationToken cancellationToken)
    {        
        const int ellipsisLimit = 3;
        var numberOfDots = 0;
        while (true)
        {
            numberOfDots = numberOfDots % ellipsisLimit + 1;
            var dots = new string('.', numberOfDots);
            var padding = new string(' ', ellipsisLimit);
            Console.Write($"\r{text}{dots}{padding}");

            await Task.Delay(500, cancellationToken);

            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }
        }
    }

    public static bool ShowConfirmation(string text)
    {
        Console.Write($"{text} [y/N]: ");

        var input = Console.ReadLine();

        switch (input?.ToLower())
        {
            case "y":
                return true;
            case "n" or "":
                return false;
            default:
                return ShowConfirmation(text);
        }
    }

    public static async Task ShowProgressBar(string title, Func<ProgressContext, Task> ctxAction)
    {
        AnsiConsole.MarkupLine(title);
        await AnsiConsole.Progress()
            .Columns(
                new PercentageColumn(),
                new ProgressBarColumn(),
                new DownloadedColumn())
            .StartAsync(ctxAction);
    }
}