using System.Drawing;

namespace CSharp_Exam.Casino.Services;

static class ConsoleService
{
    public static void WriteColoredLine(string message, ConsoleColor color = ConsoleColor.White)
    {
        var prevColor = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.WriteLine(message);
        Console.ForegroundColor = prevColor;
    }

    public static void WriteColored(string message, ConsoleColor color = ConsoleColor.White)
    {
        var prevColor = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.Write(message);
        Console.ForegroundColor = prevColor;
    }


    public static int ChooseOption(params string[] options)
    {
        if (options.Length == 0)
            throw new ArgumentException("At least one option is required");

        int selected = 0;
        int startLine = Console.CursorTop;


        for (int i = 0; i < options.Length; i++)
            Console.WriteLine();
        startLine = Console.CursorTop - options.Length;  
        if (startLine < 0) startLine = 0;

        Console.CursorVisible = false;
        try
        {
            while (true)
            {
                for (int i = 0; i < options.Length; i++)
                {
                    Console.SetCursorPosition(0, startLine + i);
                    Console.Write(new string(' ', Console.WindowWidth - 1));
                    Console.SetCursorPosition(0, startLine + i);

                    if (i == selected)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write("-> ");
                    }
                    else
                        Console.Write("   ");

                    Console.Write($"{i + 1}. {options[i]}");
                    Console.ResetColor();
                }

                ConsoleKey key = Console.ReadKey(true).Key;

                switch (key)
                {
                    case ConsoleKey.UpArrow:
                        selected = (selected - 1 + options.Length) % options.Length;
                        break;

                    case ConsoleKey.DownArrow:
                        selected = (selected + 1) % options.Length;
                        break;

                    case ConsoleKey.Enter:
                        Console.SetCursorPosition(0, startLine + options.Length);
                        return selected;
                }
            }
        }
        finally
        {
            Console.CursorVisible = true;
        }
    }
}
