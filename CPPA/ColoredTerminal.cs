namespace CPPA;

public class ColoredTerminal
{
    public static void DisplayColoredMessage(string message, ConsoleColor textColor, ConsoleColor backgroundColor = ConsoleColor.Black)
    {
        Console.BackgroundColor = backgroundColor;
        Console.ForegroundColor = textColor;
        Console.WriteLine(message);
        Console.ResetColor(); // Resets to the default colors.
    }
}