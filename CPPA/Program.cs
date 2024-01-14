using System;

public class Program
{
    static void Main(string[] args)
    {
        while (true)
        {
            Console.WriteLine("Select an option:");
            Console.WriteLine("1. NEC Projector Control");
            Console.WriteLine("2. Other Program 1");
            Console.WriteLine("3. Other Program 2");
            Console.WriteLine("4. Exit");
            Console.Write("Enter your choice: ");

            var choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    NecProjectorController.Start();
                    break;
                case "2":
                    // Other program logic
                    break;
                // Other cases
            }
        }
    }
}