using Renci.SshNet;


internal class Program
{
    private static async Task Main(string[] args)
    {
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine("""



          ########################################################
          ##                                                    ##
          ##                    SWITCHARONI                     ##
          ##                                                    ##
          ##             ___________________________            ##
          ##            /                          /|           ##
          ##           /   by: M. Wilson 8/2025   / |           ##
          ##          /                          /  |           ##
          ##         /__________________________/  /            ##
          ##         |  [ ] [ ] [ ] [ ] [ ] [ ] | /             ##
          ##         |__________________________|/              ##
          ##                                                    ##
          ##                                                    ##
          ##    - Enter the Management IP {###.###.###.###}     ##
          ##       of the Cisco Switch that you want to get     ##
          ##       all of the IP addresses of hosts connected   ##
          ##       switch ports.                                ##
          ##                                                    ##
          ##    - You will be prompted for the ssh USERNAME     ##
          ##       and PASSWORD for the switch.                 ##
          ##                                                    ##
          ########################################################


        """);
        Console.ResetColor();
        if (args.Length > 0 && args[0] == "help")
        {
            Console.WriteLine("Usage: Switcharoni [options]");
            Console.WriteLine("Options:");
            Console.WriteLine("  help       Show this help message");
            Console.WriteLine("  ip         Specify the switch IP address");
            Console.WriteLine("  username   Specify the SSH username");
            Console.WriteLine("  password   Specify the SSH password");
            return;
        }
        var _switchip = GetSwitchIP(); //"172.16.102.6";
        var _username = GetUsername(); // "admin";
        var _password = GetPassword();
        List<string> _commandsToRunOnSwitch =
        [
            "show interface status | include (connected)",
            "show ip arp",
            "show mac address-table dynamic", //dynamic
            ""
        ];
        CiscoSwitchManager switchManager = new(_switchip, _username, _password);
        await switchManager.RunCommandsAsync(_commandsToRunOnSwitch);
        //foreach (var mac in switchManager.MacAddresses) // remove last two chars from the InterfaceName property of each Interface
        //{
        //    if (mac.Port.Length > 2) if(mac.Port.Substring(mac.Port.Length - 2) == "\r") mac.Port = mac.Port.Substring(0, mac.Port.Length - 2);
        //}

        // Merge the Lists into a single List for display and export
        foreach (var ipAddr in switchManager.IpAddresses)
        {
            var swInt = switchManager.SwitchInterfaces.FirstOrDefault(i => i.Mac == ipAddr.Mac);
            if (swInt != null)
            {
                swInt.Ip = ipAddr.Ip;
            }
            else
            {
                //interf.Ip = "screwed up in 1";
            }
        }

        foreach (var ipAddr in switchManager.IpAddresses)
        {
            var mac = switchManager.MacAddresses.FirstOrDefault(m => m.Mac == ipAddr.Mac);
            if (mac != null)
            {
                ipAddr.Port = mac.Port?.Replace("\r", "");
            }
            else
            {
                ipAddr.Port = "Not IN MAT";
            }
        }

        foreach (var swInt in switchManager.SwitchInterfaces)
        {
            var mac = switchManager.MacAddresses.FirstOrDefault(m => m.Port?.Replace("\r", "") == swInt.InterfaceName?.Replace("\r", "")); //.Substring(0, m.Port.Length - 2)
            if (mac != null)
            {
                swInt.Mac = mac.Mac;
            }
            else
            {
                swInt.Mac = "NOT IN MAT";
            }
        }
        var switchPortInfo = switchManager.IpAddresses;


        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        switchManager.SwitchInterfaces.ForEach(i => Console.WriteLine($"Interface: {i.InterfaceName} IP Address: {i.Ip} MAC: {i.Mac}"));
        Console.WriteLine("\n\n");
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        switchManager.IpAddresses.OrderBy(i => i.Port).ToList().ForEach(i => Console.WriteLine($" PORT: {i.Port}  |  IP Address: {i.Ip}  |  MAC: {i.Mac}"));
        Console.WriteLine("\n\n");
        //switchManager.MacAddresses.ForEach(i => Console.WriteLine($"MAC: {i.Mac} Port: {i.Port}"));
        //Console.WriteLine("\n\n");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("Note: Any 'NOT IN MAT' showing as the PORT means the mac address is in the device's arp table but NOT in the device's mac address-table (Most likely purged from inactivity{5minutes} or is a virtual interface like a vlan interface/ip).  Ping the IP of each and the device will add it to it's mac address-table.  Then run Switcharoni again. :)\n");
        //Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine("......  Press [ENTER] key to close window, Bruh!  ........\n");
        //Console.WriteLine();
        Console.ReadLine();

        // TODO: get from user the router IP/username/password (unless the switch is layer 3)

        static string GetSwitchIP()
        {
            Console.Write("Switch Management IP: ");
            Console.ForegroundColor = ConsoleColor.Green;
            var switchip = Console.ReadLine();
            Console.ResetColor();
            if (string.IsNullOrEmpty(switchip))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("You must enter a valid IP address. examples: '172.16.0.99' '10.0.0.7'");
                Console.ResetColor();
                return GetSwitchIP();
            }
            else { return switchip; }
        }

        static string GetUsername()
        {
            Console.Write("ssh USERNAME: ");
            Console.ForegroundColor = ConsoleColor.Green;
            var un = Console.ReadLine();
            Console.ResetColor();
            if (string.IsNullOrEmpty(un))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("ssh USERNAME cannot be blank.");
                Console.ResetColor();
                return GetUsername();
            }
            else { return un; }
        }

        static string GetPassword()
        {
            Console.Write("ssh PASSWORD: ");
            try
            {
                var pw = "";
                do
                {
                    ConsoleKeyInfo key = Console.ReadKey(true);
                    // Backspace Should Not Work  
                    if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
                    {
                        pw += key.KeyChar;
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write("*");
                        Console.ResetColor();
                    }
                    else
                    {
                        if (key.Key == ConsoleKey.Backspace && pw.Length > 0)
                        {
                            pw = pw.Substring(0, pw.Length - 1);
                            Console.Write("\b \b");
                        }
                        else if (key.Key == ConsoleKey.Enter)
                        {
                            if (string.IsNullOrWhiteSpace(pw))
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("ssh PASSWORD cannot be blank.");
                                Console.ResetColor();
                                GetPassword();
                                return pw;
                            }
                            else
                            {
                                Console.WriteLine("");
                                return pw;
                            }
                        }
                    }
                } while (true);
            }
            catch (Exception){ throw; }
        }
    }
}

internal class CiscoSwitchManager
{
    private readonly string _ipAddress;
    private readonly string _userName;
    private readonly string _passWord;
    internal CiscoSwitchManager(string ipAddress, string username, string password)
    {
        _ipAddress = ipAddress;
        _userName = username;
        _passWord = password;
    }

    public List<SwitchInterface> SwitchInterfaces { get; set; } = [];
    public List<MacAddress> MacAddresses { get; set; } = [];
    public List<IpAddress> IpAddresses { get; set; } = [];

    public async Task RunCommandsAsync(List<string> commandsToRunOnSwitch)
    {
        using var sshClient = new SshClient(_ipAddress, _userName, _passWord);
        try
        {
            sshClient.Connect();
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green; Console.WriteLine("Connection Successful, Bruh!"); Console.ResetColor();
            Console.WriteLine();
            string enablePassword = _passWord; // Assuming the enable password is the same as the login password
            string lastCommand = string.Empty;
            string lastMessage = string.Empty;
            using (var shellStream = sshClient.CreateShellStream("vt100", 80, 24, 800, 600, 1024))
            {
                shellStream.WriteLine("enable");
                shellStream.WriteLine(enablePassword);
                shellStream.WriteLine("terminal length 0");
                var result = shellStream.ReadLine();
                await Task.Delay(500);
                string resultlinesforconsole = "";
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                foreach (string command in commandsToRunOnSwitch)
                {
                    if (command == "show interface status | include (connected)")
                    {
                        Console.WriteLine("Grabbing all the active port info from the device....");
                    }
                    else if (command == "show ip arp") Console.WriteLine("Infiltrating  arp data....");
                    else if (command == "show mac address-table dynamic") Console.WriteLine("Stealing entries from mac address table....");
                    shellStream.WriteLine(command);
                    result = shellStream.Read();
                    await Task.Delay(3500);
                    foreach (var l in result.Split('\n'))
                    {
                        if (l.Contains("connected "))
                        {
                            var swInt = new SwitchInterface();
                            var parts = l.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            if (parts.Length > 0)
                            {
                                swInt.InterfaceName = parts[0];
                                SwitchInterfaces.Add(swInt); // Assuming the first part is the interface name
                                resultlinesforconsole += l;
                            }
                        }
                        else if (l.Contains("Internet"))
                        {
                            var ipAddr = new IpAddress();
                            var parts = l.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            if (parts.Length > 3)
                            {
                                ipAddr.Ip = parts[1];
                                ipAddr.Mac = parts[3];
                                IpAddresses.Add(ipAddr); // Assuming the IP address is the second part
                                resultlinesforconsole += l;
                            }
                        }
                        else if (l.Contains("DYNAMIC"))
                        {
                            var macAddr = new MacAddress();
                            var parts = l.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            if (parts.Length > 2)
                            {
                                macAddr.Mac = parts[1];
                                macAddr.Port = parts[3]; // TODO: 3 works on 9300 but not 4507
                                MacAddresses.Add(macAddr); // Assuming the MAC address is the second part
                                resultlinesforconsole += l;
                            }
                        }
                    }
                    lastCommand = command;
                }
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red; Console.WriteLine($"Error: {ex.Message}"); Console.ResetColor();
        }
        finally
        {
            sshClient.Disconnect();
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green; Console.WriteLine("....Disconnected, Bruh!"); Console.ResetColor();
            Console.WriteLine();
        }
    }

    public class SwitchInterface  // Switch Port
    {
        public string? InterfaceName { get; set; }
        public string? Mac { get; set; }
        public string? Ip { get; set; }
    }

    public class MacAddress // Computer/Server/Device mac
    {
        public string? Mac { get; set; }
        public string? Port { get; set; }
        public string? Ip { get; set; }
    }

    public class IpAddress
    {
        public string? Ip { get; set; }
        public string? HostName { get; set; }
        public string? Port { get; set; }
        public string? Mac { get; set; }
    }
}