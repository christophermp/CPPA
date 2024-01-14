using CPPA.Christie;

namespace CPPA;

public class ChristieProjectorController
{
    private static string _projectorIp = "127.0.0.1";
    private static int _networkPort = 43728;
    private static string _responseString = "";

    public static void Start()
    {
        Console.Clear();
        ColoredTerminal.DisplayColoredMessage("\nChristie Projector Control started.", ConsoleColor.Green);
        ColoredTerminal.DisplayColoredMessage($"Default IP and port: {_projectorIp}:{_networkPort}\n", ConsoleColor.Green);
        while (true)
        {
            Console.WriteLine("\nSelect Projector Series:");
            Console.WriteLine("1. Christie 2 Series");
            Console.WriteLine("2. Christie 4 Series");
            ColoredTerminal.DisplayColoredMessage("0. <- Back..", ConsoleColor.Red);
            Console.Write("\nEnter your choice: ");

            string input = Console.ReadLine();
            if (string.IsNullOrEmpty(input) || input.Equals("0"))
                break;
            Console.Clear();

            switch (input)
            {
                case "1":
                    CP22xx.Start();
                    break;
                case "2":
                    CP4xxx.Start();
                    break;
                default:
                    Console.WriteLine("Invalid choice. Please try again.");
                    break;
            }
        }
    }

    
}