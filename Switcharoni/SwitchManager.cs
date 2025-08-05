using Renci.SshNet;

namespace Switcharoni
{
    internal class SwitchManager
    {
        private readonly string _ipAddress;
        private readonly string _userName;
        private readonly string _passWord;
        internal SwitchManager(string ipAddress, string username, string password)
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
}