using System.Collections.Immutable;

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
    
    public static void Success(string line)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write("\u2705  Success ");
        Console.ResetColor();
        Console.WriteLine(line);
    }

    public static int ShowSelectionList(ImmutableArray<string> options, string title = "Make a selection, then press ENTER")
    {
        ConsoleKeyInfo keyInfo;
        var selectionIdx = 0;
        
        do
        {
            Console.Clear();
            
            WriteLineNoColor(title);

            for (var i = 0; i < options.Length; i++)
            {
                if (i == selectionIdx)
                {
                    Console.ForegroundColor = ConsoleColor.Blue;
                }
                else
                {
                    Console.ResetColor();
                }
                
                Console.WriteLine($"{i + 1}. {options[i]}");
            }
            
            keyInfo = Console.ReadKey(intercept: true);

            UpdateSelectionIdx(ref selectionIdx, options.Length, keyInfo.Key);
            
        } while (keyInfo.Key != ConsoleKey.Enter);
        
        return selectionIdx;
    }

    private static void WriteLineNoColor(string line)
    {
        Console.ResetColor();
        Console.WriteLine(line);
    }

    private static void UpdateSelectionIdx(ref int selectionIdx, int optionsLength, ConsoleKey key)
    {
        switch (key)
        {
            case ConsoleKey.UpArrow:
            {
                selectionIdx--;
                if (selectionIdx < 0)
                {
                    selectionIdx = optionsLength - 1;
                }

                break;
            }
            case ConsoleKey.DownArrow:
            {
                selectionIdx++;
                
                if (selectionIdx >= optionsLength)
                {
                    selectionIdx = 0;
                }

                break;
            }
        }
    }

    public static void ShowLoadingText(string text)
    {
        Console.CursorVisible = false;
        var ellipsisLimit = 3;
        var numberOfDots = 0;

        Task.Run(() =>
        {
            while (true)
            {
                numberOfDots = numberOfDots % ellipsisLimit + 1;
                var dots = new string('.', numberOfDots);
                var padding = new string(' ', ellipsisLimit);
                Console.Write($"\r{text}{dots}{padding}");
                Thread.Sleep(500);
            }
        });
    }
}