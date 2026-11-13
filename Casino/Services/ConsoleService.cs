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
        int selected = 0;

        while (true)
        {
            Console.Clear();

            for (int i = 0; i < options.Length; i++)
            {
                if (i == selected)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write("-> ");
                }
                else
                    Console.Write("   ");

                Console.WriteLine($"{i+1}. {options[i]}");
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
                    return selected;
            }
        }
    }

}
