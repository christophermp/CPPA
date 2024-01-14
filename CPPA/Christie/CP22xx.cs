using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

namespace CPPA.Christie;

public class CP22xx
{
    private static string _projectorIp = "127.0.0.1";
    private static int _networkPort = 43728;
    private static string _responseString = "";

    public static void Start()
    {
        Console.Clear();
        ColoredTerminal.DisplayColoredMessage("\nNEC Projector Control started.", ConsoleColor.Green);
        ColoredTerminal.DisplayColoredMessage($"Default IP and port: {_projectorIp}:{_networkPort}\n", ConsoleColor.Green);
        while (true)
        {
            Console.WriteLine("\nSelect a command:");
            Console.WriteLine("1. Serial"); // See SST - SERI in CP2220 API manual
            Console.WriteLine("2. Power Status");
            Console.WriteLine("3. Power Status Decode String");
            Console.WriteLine("4. Get Current Scene");
            Console.WriteLine("5. Power On");
            Console.WriteLine("6. Power Off");
            Console.WriteLine("7. Change Channel");
            Console.WriteLine("8. Projector Version Data");
            Console.WriteLine("9. Mute Image");
            Console.WriteLine("10. Mute Status");
            Console.WriteLine("11. Control Dowser");
            Console.WriteLine("12. Lamp Data");
            Console.WriteLine("13. Lamp Mode");
            Console.WriteLine("14. Get Channel and Image Port");
            Console.WriteLine("15. Lamp Power Control");
            Console.WriteLine("20. Settings");
            ColoredTerminal.DisplayColoredMessage("0. <- Back..", ConsoleColor.Red);
            Console.Write("\nEnter your choice: ");

            string input = Console.ReadLine();
            if (string.IsNullOrEmpty(input) || input.Equals("0"))
                break;
            Console.Clear();

            switch (input)
            {
                case "1":
                    //ProjSerial();
                    break;
                case "2":
                    ProjPowerStatus();
                    break;
                case "3":
                    Console.Write("Paste Hex string: ");
                    string hexString = Console.ReadLine();
                    //ProjPowerStatusDecodeString(hexString);
                    break;
                case "4":
                    //CurrentScene();
                    break;
                case "5":
                    //ProjPowerOn();
                    break;
                case "6":
                    //ProjPowerOff();
                    break;
                case "7":
                    Console.Write("Enter channel number: ");
                    string channelString = Console.ReadLine();
                    if (Int32.TryParse(channelString, out int c))
                    {
                        //ProjInputSwitchChange(c);
                    }
                    else
                    {
                        Console.WriteLine("Not a valid number.");
                    }

                    break;
                case "8":
                    //ProjVersionDataRequest();
                    break;
                case "9":
                    Console.Write("Mute On/Off: ");
                    string muteString = Console.ReadLine();
                    if (muteString.ToLower() == "on")
                    {
                        //ProjPictureMute("on");
                    }
                    else
                    {
                        //ProjPictureMute("off");
                    }
                    //ProjMuteStatusRequest();

                    break;
                case "10":
                    //ProjMuteStatusRequest();
                    break;
                case "11":
                    Console.Write("Dowser Open/Close: ");
                    string dowserString = Console.ReadLine();
                    if (dowserString.ToLower() == "close")
                    {
                        //ProjDowser("close");
                    }
                    else
                    {
                        //ProjDowser("open");
                    }
                    break;
                case "12":
                    GetLampHistory();
                    break;
                case "13":
                    //ProjLampMode();
                    break;
                case "14":
                    //ProjInputStatusRequest();
                    break;
                case "15":
                    Console.Write("Power Lamp On/Off: ");
                    string lampOnString = Console.ReadLine();
                    if (lampOnString.ToLower() == "off")
                    {
                        //ProjLamp("off");
                    }
                    else
                    {
                        //ProjLamp("on");
                    }
                    break;
                case "20":
                    ChangeSettings();
                    break;
                default:
                    Console.WriteLine("Invalid choice. Please try again.");
                    break;
            }
        }
    }
    
    public static void ProjPowerStatus()
    {
        using (var client = new TcpClient(_projectorIp, _networkPort))
        {
            var stream = client.GetStream();
            byte[] message = Encoding.ASCII.GetBytes("(PWR+STAT?)");
            stream.Write(message, 0, message.Length);

            byte[] buffer = new byte[1024];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            string response = Encoding.ASCII.GetString(buffer, 0, bytesRead);

            InterpretPowerResponse(response);
        }
    }

    private static void InterpretPowerResponse(string response)
    {
        ColoredTerminal.DisplayColoredMessage($"Recived: {response}\n", ConsoleColor.DarkGreen);
        if (response.Contains("(PWR+STAT!000)"))
        {
            Console.WriteLine("Projector State: Power On, Light Source Off");
        }
        else if (response.Contains("(PWR+STAT!003)"))
        {
            Console.WriteLine("Projector State: Power Off");
        }
        else if (response.Contains("(PWR+STAT!001)"))
        {
            Console.WriteLine("Projector State: Full Power");
        }
        else if (response.Contains("(PWR+STAT!010)"))
        {
            Console.WriteLine("Projector State: Cooling Down");
        }
        else if (response.Contains("(PWR+STAT!011)"))
        {
            Console.WriteLine("Projector State: Warm Up");
        }
        else
        {
            Console.WriteLine("Unknown Projector State");
        }
    }
    
    public static void GetLampHistory()
    {
        using (var client = new TcpClient(_projectorIp, _networkPort))
        {
            var stream = client.GetStream();
            byte[] message = Encoding.ASCII.GetBytes("(HIS?)");
            stream.Write(message, 0, message.Length);

            byte[] buffer = new byte[4096];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            string response = Encoding.ASCII.GetString(buffer, 0, bytesRead);

            InterpretLampHistoryResponse(response);
        }
    }

    private static void InterpretLampHistoryResponse(string response)
    {
        var matches = Regex.Matches(response, @"\((HIS!\d{3} ""[^""]*"" ""[^""]*"" ""[^""]*"" \d{3} \d{3} \d{3} \d{3} \d{5} \d{3} \d{3})\)");

        foreach (Match match in matches)
        {
            string entry = match.Groups[1].Value;
            string[] parts = entry.Split(new string[] { "\" \"", "\"" }, StringSplitOptions.RemoveEmptyEntries);

            string lampNumber = parts[0].Substring(4);
            string dateInstalled = parts[1];
            string serialNumber = parts[2];
            string type = parts[3];
            string strikes = parts[4];
            string failedStrikes = parts[5];
            string failedRestrikes = parts[6];
            string unexpectedLampOff = parts[7];
            string preInstalledHours = parts[8];
            string lampHours = parts[9];
            string lampRotation = parts[10].TrimEnd(')');

            Console.WriteLine($"Lamp Number: {lampNumber}");
            Console.WriteLine($"Date Installed: {dateInstalled}");
            Console.WriteLine($"Serial Number: {serialNumber}");
            Console.WriteLine($"Type: {type}");
            Console.WriteLine($"Strikes: {strikes}");
            Console.WriteLine($"Failed Strikes: {failedStrikes}");
            Console.WriteLine($"Failed Restrikes: {failedRestrikes}");
            Console.WriteLine($"Unexpected Lamp Off: {unexpectedLampOff}");
            Console.WriteLine($"Pre-installed Hours: {preInstalledHours}");
            Console.WriteLine($"Lamp Hours: {lampHours}");
            Console.WriteLine($"Lamp Rotation: {lampRotation}");
            Console.WriteLine("-------------------");
        }
    }

    
    private static void ChangeSettings()
    {
        Console.Write($"Current IP: {_projectorIp}. Change? (Y/n): ");
        string changeIp = Console.ReadLine();
        if (!string.IsNullOrEmpty(changeIp) && changeIp.Trim().ToLower() == "y")
        {
            Console.Write("Enter new IP: ");
            string newIp = Console.ReadLine();

            if (IPAddress.TryParse(newIp, out IPAddress parsedIp))
            {
                _projectorIp = parsedIp.ToString();
                Console.WriteLine($"IP changed to: {_projectorIp}");
            }
            else
            {
                Console.WriteLine("Invalid IP address. No changes made.");
            }
        }
        Console.Write($"Current port: {_networkPort}. Change? (Y/n): ");
        string changePort = Console.ReadLine();
        if (!string.IsNullOrEmpty(changePort) && changePort.Trim().ToLower() == "y")
        {
            Console.Write("Enter new Port: ");
            string newPort = Console.ReadLine();

            if (int.TryParse(newPort, out int parsedPort) && parsedPort > 0 && parsedPort <= 65535)
            {
                _networkPort = parsedPort;
                Console.WriteLine($"Port changed to: {_networkPort}");
            }
            else
            {
                Console.WriteLine("Invalid port number. No changes made.");
            }
        }
    }
}