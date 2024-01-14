namespace CPPA;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class NecProjectorController
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
            Console.WriteLine("1. Serial");
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
                    ProjSerial();
                    break;
                case "2":
                    ProjPowerStatus();
                    break;
                case "3":
                    Console.Write("Paste Hex string: ");
                    string hexString = Console.ReadLine();
                    ProjPowerStatusDecodeString(hexString);
                    break;
                case "4":
                    CurrentScene();
                    break;
                case "5":
                    ProjPowerOn();
                    break;
                case "6":
                    ProjPowerOff();
                    break;
                case "7":
                    Console.Write("Enter channel number: ");
                    string channelString = Console.ReadLine();
                    if (Int32.TryParse(channelString, out int c))
                    {
                        ProjInputSwitchChange(c);
                    }
                    else
                    {
                        Console.WriteLine("Not a valid number.");
                    }

                    break;
                case "8":
                    ProjVersionDataRequest();
                    break;
                case "9":
                    Console.Write("Mute On/Off: ");
                    string muteString = Console.ReadLine();
                    if (muteString.ToLower() == "on")
                    {
                        ProjPictureMute("on");
                    }
                    else
                    {
                        ProjPictureMute("off");
                    }
                    ProjMuteStatusRequest();

                    break;
                case "10":
                    ProjMuteStatusRequest();
                    break;
                case "11":
                    Console.Write("Dowser Open/Close: ");
                    string dowserString = Console.ReadLine();
                    if (dowserString.ToLower() == "close")
                    {
                        ProjDowser("close");
                    }
                    else
                    {
                        ProjDowser("open");
                    }
                    break;
                case "12":
                    ProjLampInfoRequest2();
                    break;
                case "13":
                    ProjLampMode();
                    break;
                case "14":
                    ProjInputStatusRequest();
                    break;
                case "15":
                    Console.Write("Power Lamp On/Off: ");
                    string lampOnString = Console.ReadLine();
                    if (lampOnString.ToLower() == "off")
                    {
                        ProjLamp("off");
                    }
                    else
                    {
                        ProjLamp("on");
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

    private static void ProjSerial()
    {
        using (var client = new TcpClient())
        {
            try
            {
                client.Connect(_projectorIp, _networkPort);
                Console.WriteLine($"Connected to {_projectorIp}:{_networkPort}");

                var stream = client.GetStream();
                byte[] message = new byte[] { 0x00, 0x86, 0x00, 0xC0, 0x01, 0x08, 0x4f };
                stream.Write(message, 0, message.Length);

                byte[] buffer = new byte[4096];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string receivedData = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                Console.WriteLine($"Received: {receivedData}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error connecting to projector: {ex.Message}");
            }
        }
    }

    private static void ProjPowerStatus()
    {
        using (var client = new TcpClient())
        {
            try
            {
                client.Connect(_projectorIp, _networkPort);
                Console.WriteLine($"Connected to {_projectorIp}:{_networkPort}");

                var stream = client.GetStream();
                byte[] message = { 0x00, 0x85, 0x00, 0x00, 0x01, 0x01, 0x87 };
                Console.WriteLine($"Message: {BitConverter.ToString(message)}");
                stream.Write(message, 0, message.Length);

                byte[] buffer = new byte[4096];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string responseHex = BitConverter.ToString(buffer, 0, bytesRead).Replace("-", ":");
                Console.WriteLine($"Response: {responseHex}");

                ProcessPowerResponse(buffer, bytesRead);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error connecting to projector: {ex.Message}");
            }
        }
    }

    private static void ProcessPowerResponse(byte[] data, int bytesRead)
    {
        if (bytesRead < 21) return;

        string externalControlStatus = data[6] == 0 ? "Off" : data[6] == 1 ? "On" : "Unknown";
        Console.WriteLine($"External_Control_Status: {externalControlStatus}");

        string powerStatus = data[7] == 0 ? "Off" : data[7] == 1 ? "On" : "Unknown";
        Console.WriteLine($"Power_Status: {powerStatus}");

        string lampCoolingProcessing = data[8] == 0 ? "No execution" : data[8] == 1 ? "During execution" : "Unknown";
        Console.WriteLine($"Lamp_Cooling_Processing: {lampCoolingProcessing}");

        string onOffProcessing = data[9] == 0 ? "No execution" : data[9] == 1 ? "During execution" : "Unknown";
        Console.WriteLine($"On_Off_Processing: {onOffProcessing}");

        string projectorProcessStatus = data[10] switch
        {
            0 => "Standby",
            1 => "Power On Protect (before Lamp(Light) control)",
            2 => "Ignition",
            3 => "Power On Running",
            4 => "Running (Power On / Lamp(Light) On)",
            5 => "Cooling",
            7 => "Reset Wait",
            8 => "Fan Stop Error (before Cooling)",
            9 => "Lamp Retry",
            10 => "Lamp(Light) Error (before Cooling)",
            12 => "Running (Power On / Lamp(Light) Off)",
            _ => "Unknown"
        };
        Console.WriteLine($"Projector_Process_Status: {projectorProcessStatus}");

        string storeProcessing = data[13] == 0 ? "No execution" : data[13] == 1 ? "During execution" : "Unknown";
        Console.WriteLine($"Store_Processing: {storeProcessing}");

        string lampStatus = data[14] switch
        {
            0 => "Lamp(Light) Off",
            1 => "Lamp(Light) On, Dual-Lamp: Lamp1 On/Lamp2 Off",
            2 => "Lamp1 Off/Lamp2 On",
            3 => "Lamp1and2 On",
            _ => "Unknown"
        };
        Console.WriteLine($"Lamp_Status: {lampStatus}");

        string processingOfLamp = data[15] == 0 ? "No execution" : data[15] == 1 ? "During execution" : "Unknown";
        Console.WriteLine($"Processing_of_Lamp: {processingOfLamp}");

        string lampModeSetting = data[16] switch
        {
            0 => "Dual",
            1 => "Lamp1",
            2 => "Lamp2",
            _ => "Unknown"
        };
        Console.WriteLine($"Lamp_Mode_Setting: {lampModeSetting}");
        int coolingRemainingTime = BitConverter.ToInt16(new byte[] { data[18], data[17] }, 0);
        Console.WriteLine($"Cooling_Remaining_Time: {coolingRemainingTime} seconds");

        int remainingTimeOfLamp = BitConverter.ToInt16(new byte[] { data[20], data[19] }, 0);
        Console.WriteLine($"Remaining_Time_of_Lamp: {remainingTimeOfLamp} hours");
    }

    private static void ProjPowerStatusDecodeString(string arg)
    {
        byte[] data;

        try
        {
            data = ConvertHexStringToByteArray(arg);
        }
        catch (FormatException)
        {
            Console.WriteLine("Invalid hex string format.");
            return;
        }

        Console.WriteLine($"Response: {BitConverter.ToString(data).Replace("-", ":")}");

        ProcessPowerResponse(data, data.Length);
    }

    private static byte[] ConvertHexStringToByteArray(string hexString)
    {
        if (hexString.Length % 2 != 0)
        {
            throw new ArgumentException("The hex string must have an even number of digits.");
        }

        byte[] bytes = new byte[hexString.Length / 2];
        for (int i = 0; i < hexString.Length; i += 2)
        {
            bytes[i / 2] = Convert.ToByte(hexString.Substring(i, 2), 16);
        }

        return bytes;
    }

    private static void CurrentScene()
    {
        using (var client = new TcpClient())
        {
            try
            {
                client.Connect(_projectorIp, _networkPort);
                Console.WriteLine($"Connected to {_projectorIp}:{_networkPort}");

                var stream = client.GetStream();
                byte[] message = { 0x00, 0x85, 0x00, 0x00, 0x01, 0xE6, 0x6C };
                stream.Write(message, 0, message.Length);

                byte[] buffer = new byte[4096];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                
                string receivedData = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                receivedData = receivedData.Replace("\x86\x00\xc0\x11\x08", string.Empty);
                receivedData = receivedData.Replace("\x00\x00\x00\x00\x00\x00\x00u", string.Empty);

                Console.WriteLine($"Received: ({receivedData.Trim()})");
            }
            catch (SocketException ex)
            {
                Console.WriteLine($"Error connecting to projector: {ex.Message}");
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Error in communication: {ex.Message}");
            }
        }
    }

    private static void ProjPowerOn()
    {
        using (var client = new TcpClient())
        {
            try
            {
                client.Connect(_projectorIp, _networkPort);
                Console.WriteLine($"Connected to {_projectorIp}:{_networkPort}");

                var stream = client.GetStream();
                byte[] message = { 0x02, 0x00, 0x00, 0x00, 0x00, 0x02 };
                Console.WriteLine($"Message: {BitConverter.ToString(message).Replace("-", ":")}");
                stream.Write(message, 0, message.Length);

                byte[] buffer = new byte[4096];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string responseHex = BitConverter.ToString(buffer, 0, bytesRead).Replace("-", ":");
                Console.WriteLine($"Response: {responseHex}");

                if (ContainsSequence(buffer, new byte[] { 0x00, 0x00, 0xC0, 0x00, 0xE2 }))
                {
                    Console.WriteLine("power_on Command OK");
                }
                else
                {
                    Console.WriteLine("Projector appears to be already ON");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }

    private static void ProjPowerOff()
    {
        using (var client = new TcpClient())
        {
            try
            {
                client.Connect(_projectorIp, _networkPort);
                Console.WriteLine($"Connected to {_projectorIp}:{_networkPort}");

                var stream = client.GetStream();
                byte[] message = { 0x02, 0x01, 0x00, 0x00, 0x00, 0x03 };
                Console.WriteLine($"Message: {BitConverter.ToString(message).Replace("-", ":")}");
                ColoredTerminal.DisplayColoredMessage("Waiting on cooldown...", ConsoleColor.Green);
                stream.Write(message, 0, message.Length);

                byte[] buffer = new byte[4096];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string responseHex = BitConverter.ToString(buffer, 0, bytesRead).Replace("-", ":");
                Console.WriteLine($"Response: {responseHex}");

                if (ContainsSequence(buffer, new byte[] { 0x02, 0x01, 0x00, 0x00, 0x00, 0x03 }))
                {
                    Console.WriteLine("power_off Command OK");
                }
                else
                {
                    Console.WriteLine("Projector appears to be already OFF");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }

    private static bool ContainsSequence(byte[] buffer, byte[] sequence)
    {
        for (int i = 0; i < buffer.Length - sequence.Length + 1; i++)
        {
            bool sequenceFound = true;
            for (int j = 0; j < sequence.Length; j++)
            {
                if (buffer[i + j] != sequence[j])
                {
                    sequenceFound = false;
                    break;
                }
            }

            if (sequenceFound)
            {
                return true;
            }
        }

        return false;
    }

    private static void ProjVersionDataRequest()
    {
        using (var client = new TcpClient())
        {
            try
            {
                client.Connect(_projectorIp, _networkPort);
                Console.WriteLine($"Connected to {_projectorIp}:{_networkPort}");

                var stream = client.GetStream();
                byte[] messageMain = { 0x00, 0x86, 0x00, 0xC0, 0x01, 0x08 };
                byte[] cks = { CalculateCks(messageMain) };
                byte[] message = messageMain.Concat(cks).ToArray();

                Console.WriteLine($"Message: {BitConverter.ToString(message).Replace("-", ":")}");
                stream.Write(message, 0, message.Length);

                byte[] buffer = new byte[4096];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string responseHex = BitConverter.ToString(buffer, 0, bytesRead).Replace("-", ":");
                Console.WriteLine($"Response: {responseHex}");
                
                string serialNo = Encoding.ASCII.GetString(buffer, 6, 10).Trim('\0');
                Console.WriteLine($"Got response, Serial Number: {serialNo}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }

    private static byte CalculateCks(byte[] data)
    {
        return (byte)(data.Sum(b => b) % 256);
    }


    private static void ProjInputSwitchChange(int channelNumber)
    {
        channelNumber -= 1;

        using (var client = new TcpClient())
        {
            try
            {
                client.Connect(_projectorIp, _networkPort);
                Console.WriteLine($"Connected to {_projectorIp}:{_networkPort}");

                var messageMain = new List<byte> { 0x02, 0x03, 0x00, 0x00, 0x02, 0x00 };
                messageMain.Add((byte)channelNumber);
                byte[] cks = { CalculateCks(messageMain.ToArray()) };
                byte[] message = messageMain.Concat(cks).ToArray();

                Console.WriteLine($"Message: {BitConverter.ToString(message).Replace("-", ":")}");
                var stream = client.GetStream();
                stream.Write(message, 0, message.Length);
                byte[] buffer = new byte[4096];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string responseHex = BitConverter.ToString(buffer, 0, bytesRead).Replace("-", ":");
                Console.WriteLine($"Response: {responseHex}");

                if (StartsWith(buffer, new byte[] { 0x22, 0x03, 0x00, 0xC0, 0x01, 0x00 }))
                {
                    Console.WriteLine("Success");
                }
                else
                {
                    Console.WriteLine("Error");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }

    private static bool StartsWith(byte[] array, byte[] sequence)
    {
        if (sequence.Length > array.Length)
            return false;

        for (int i = 0; i < sequence.Length; i++)
        {
            if (array[i] != sequence[i])
                return false;
        }

        return true;
    }

    private static void ProjPictureMute(string mute)
    {
        using (var client = new TcpClient())
        {
            try
            {
                client.Connect(_projectorIp, _networkPort);
                Console.WriteLine($"Connected to {_projectorIp}:{_networkPort}");

                byte[] message;
                if (mute.Equals("off", StringComparison.OrdinalIgnoreCase))
                {
                    message = new byte[] { 0x02, 0x11, 0x00, 0x00, 0x00, 0x13 };
                }
                else if (mute.Equals("on", StringComparison.OrdinalIgnoreCase))
                {
                    message = new byte[] { 0x02, 0x10, 0x00, 0x00, 0x00, 0x12 };
                }
                else
                {
                    Console.WriteLine("Invalid command. Please specify 'on' or 'off'.");
                    return;
                }

                Console.WriteLine($"Message: {BitConverter.ToString(message).Replace("-", ":")}");
                var stream = client.GetStream();
                stream.Write(message, 0, message.Length);

                byte[] buffer = new byte[4096];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string responseHex = BitConverter.ToString(buffer, 0, bytesRead).Replace("-", ":");
                Console.WriteLine($"Response: {responseHex}");

                if (StartsWith(buffer, new byte[] { 0x22, 0x10, 0x00, 0xC0, 0x00 }) ||
                    StartsWith(buffer, new byte[] { 0x22, 0x11, 0x00, 0xC0, 0x00 }))
                {
                    Console.WriteLine("Success");
                }
                else
                {
                    Console.WriteLine("Error");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }

    private static void ProjDowser(string openClose)
    {
        using (var client = new TcpClient())
        {
            try
            {
                client.Connect(_projectorIp, _networkPort);
                Console.WriteLine($"Connected to {_projectorIp}:{_networkPort}");

                byte[] message;
                if (openClose.Equals("open", StringComparison.OrdinalIgnoreCase))
                {
                    message = new byte[] { 0x02, 0x17, 0x00, 0x00, 0x00, 0x19 };
                }
                else if (openClose.Equals("close", StringComparison.OrdinalIgnoreCase))
                {
                    message = new byte[] { 0x02, 0x16, 0x00, 0x00, 0x00, 0x18 };
                }
                else
                {
                    Console.WriteLine("Invalid command. Please specify 'open' or 'close'.");
                    return;
                }

                Console.WriteLine($"Message: {BitConverter.ToString(message).Replace("-", ":")}");
                var stream = client.GetStream();
                stream.Write(message, 0, message.Length);

                byte[] buffer = new byte[4096];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string responseHex = BitConverter.ToString(buffer, 0, bytesRead).Replace("-", ":");
                Console.WriteLine($"Response: {responseHex}");

                if (StartsWith(buffer, new byte[] { 0x22, 0x16, 0x00, 0xC0, 0x00 }) ||
                    StartsWith(buffer, new byte[] { 0x22, 0x17, 0x00, 0xC0, 0x00 }))
                {
                    Console.WriteLine("Success");
                }
                else
                {
                    Console.WriteLine("Error");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }

    private static void ProjLampInfoRequest2()
    {
        using (var client = new TcpClient())
        {
            try
            {
                client.Connect(_projectorIp, _networkPort);
                Console.WriteLine($"Connected to {_projectorIp}:{_networkPort}");

                var stream = client.GetStream();
                byte[] message = { 0x03, 0x94, 0x00, 0x00, 0x00, 0x97 };
                Console.WriteLine($"Message: {BitConverter.ToString(message).Replace("-", ":")}");
                stream.Write(message, 0, message.Length);

                byte[] buffer = new byte[4096];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string responseHex = BitConverter.ToString(buffer, 0, bytesRead).Replace("-", ":");
                Console.WriteLine($"Response: {responseHex}");

                if (bytesRead >= 9 && buffer[0] == 0x23 && buffer[1] == 0x94 && buffer[2] == 0x00 &&
                    buffer[3] == 0xC0 && buffer[4] == 0x05)
                {
                    int lampTimeInSeconds =
                        BitConverter.ToInt32(new byte[] { buffer[8], buffer[7], buffer[6], buffer[5] }, 0);
                    Console.WriteLine($"Lamp time in seconds: {lampTimeInSeconds} seconds");
                }
                else
                {
                    Console.WriteLine("Error");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }

    private static void ProjInputStatusRequest()
    {
        using (var client = new TcpClient())
        {
            try
            {
                client.Connect(_projectorIp, _networkPort);
                Console.WriteLine($"Connected to {_projectorIp}:{_networkPort}");

                var stream = client.GetStream();
                byte[] message = { 0x00, 0x85, 0x00, 0x00, 0x01, 0x02, 0x88 };
                Console.WriteLine($"Message: {BitConverter.ToString(message).Replace("-", ":")}");
                stream.Write(message, 0, message.Length);

                byte[] buffer = new byte[4096];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string responseHex = BitConverter.ToString(buffer, 0, bytesRead).Replace("-", ":");
                Console.WriteLine($"Response: {responseHex}");

                if (bytesRead >= 11 && buffer[0] == 0x20 && buffer[1] == 0x85 && buffer[2] == 0x00 &&
                    buffer[3] == 0xC0 && buffer[4] == 0x10)
                {
                    string selectingSignalProcessing = buffer[5] switch
                    {
                        0 => "No execution (Normal condition)",
                        1 => "During execution",
                        _ => "Unknown"
                    };
                    Console.WriteLine($"Selecting_signal_processing: {selectingSignalProcessing}");

                    int channel = buffer[6] + 1; // 1-based index
                    Console.WriteLine($"Channel Number: {channel}");
                    
                    string portName = GetPortName(buffer[7], buffer[8]);
                    Console.WriteLine($"Port Name: {portName}");

                    string testPatternStatus = buffer[10] switch
                    {
                        0 => "No Display (Normal condition)",
                        1 => "Displaying",
                        _ => "Unknown"
                    };
                    Console.WriteLine($"Test Pattern: {testPatternStatus}");
                }
                else
                {
                    Console.WriteLine("Error");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }

    private static string GetPortName(byte pn1, byte pn2)
    {
        return (pn1, pn2) switch
        {
            (0, 6) => "Test Pattern",
            (1, 6) => "292A",
            (2, 6) => "292B",
            (1, 13) => "292C",
            (2, 13) => "292D",
            (3, 6) => "292Dual(AB)",
            (3, 13) => "292Dual(CD)",
            (4, 13) => "292Quad",
            (1, 12) => "DVIA",
            (2, 12) => "DVIB",
            (3, 12) => "DVI Dual/Twin",
            (4, 12) => "IMB",
            _ => "Unknown"
        };
    }
    
    private static void ProjMuteStatusRequest()
    {
        using (var client = new TcpClient())
        {
            try
            {
                client.Connect(_projectorIp, _networkPort);
                Console.WriteLine($"Connected to {_projectorIp}:{_networkPort}");

                var stream = client.GetStream();
                byte[] message = { 0x00, 0x85, 0x00, 0x00, 0x01, 0x03, 0x89 };
                Console.WriteLine($"Message: {BitConverter.ToString(message).Replace("-", ":")}");
                stream.Write(message, 0, message.Length);

                byte[] buffer = new byte[4096];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string responseHex = BitConverter.ToString(buffer, 0, bytesRead).Replace("-", ":");
                Console.WriteLine($"Response: {responseHex}");

                if (bytesRead >= 6 && buffer[0] == 0x20 && buffer[1] == 0x85 && buffer[2] == 0x00 && buffer[3] == 0xC0 && buffer[4] == 0x10)
                {
                    string pictureMute = buffer[5] switch
                    {
                        0 => "OFF",
                        1 => "ON",
                        _ => "Unknown"
                    };
                    Console.WriteLine($"Picture_mute: {pictureMute}");
                }
                else
                {
                    Console.WriteLine("ERROR");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
    
    private static void ProjLamp(string onOff)
    {
        using (var client = new TcpClient())
        {
            try
            {
                client.Connect(_projectorIp, _networkPort);
                Console.WriteLine($"Connected to {_projectorIp}:{_networkPort}");

                var stream = client.GetStream();

                byte[] message;
                if (onOff.Equals("on", StringComparison.OrdinalIgnoreCase))
                {
                    message = new byte[] { 0x03, 0x2F, 0x00, 0x00, 0x02, 0x12, 0x01 };
                }
                else if (onOff.Equals("off", StringComparison.OrdinalIgnoreCase))
                {
                    message = new byte[] { 0x03, 0x2F, 0x00, 0x00, 0x02, 0x12, 0x02 };
                }
                else
                {
                    Console.WriteLine("Invalid command. Please specify 'on' or 'off'.");
                    return;
                }

                byte[] cks = { CalculateCks(message) };
                byte[] finalMessage = message.Concat(cks).ToArray();
                Console.WriteLine($"Message: {BitConverter.ToString(finalMessage).Replace("-", ":")}");
                stream.Write(finalMessage, 0, finalMessage.Length);

                byte[] buffer = new byte[4096];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string responseHex = BitConverter.ToString(buffer, 0, bytesRead).Replace("-", ":");
                Console.WriteLine($"Response: {responseHex}");

                if (StartsWith(buffer, new byte[] { 0x23, 0x2F, 0x00, 0xC0, 0x02 }))
                {
                    Console.WriteLine("Success");
                }
                else
                {
                    Console.WriteLine("Error");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
    
    private static void ProjLampMode()
    {
        using (var client = new TcpClient())
        {
            try
            {
                client.Connect(_projectorIp, _networkPort);
                Console.WriteLine($"Connected to {_projectorIp}:{_networkPort}");

                var stream = client.GetStream();
                byte[] message = { 0x03, 0x2F, 0x00, 0x00, 0x01, 0x11, 0x44 };
                Console.WriteLine($"Message: {BitConverter.ToString(message).Replace("-", ":")}");
                stream.Write(message, 0, message.Length);

                byte[] buffer = new byte[4096];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string responseHex = BitConverter.ToString(buffer, 0, bytesRead).Replace("-", ":");
                Console.WriteLine($"Response: {responseHex}");

                if (StartsWith(buffer, new byte[] { 0x23, 0x2F, 0x00, 0xC0, 0x02, 0x11 }))
                {
                    string lampControlMode = buffer[6] switch
                    {
                        0 => "Standard mode in conjunction with Power On/Off",
                        1 => "Lamp(Light) On Mode",
                        2 => "Lamp(Light) Off Mode",
                        _ => "Unknown"
                    };
                    Console.WriteLine($"Lamp_Control_Mode: {lampControlMode}");
                }
                else
                {
                    Console.WriteLine("Error");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}