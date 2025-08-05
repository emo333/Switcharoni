namespace Switcharoni
{
    public class Program
    {
        private static async Task Main(string[] args)
        {
            await Helpers.DisplayBanner(args);
            Console.ForegroundColor = ConsoleColor.Magenta;
            
            List<string> ipAddresses =
            [
                "4.2.2.2",
                "172.16.50.10",
                "4.4.4.4",
                "1.1.1.1",
                "8.8.8.8",
                "invalidhost.example.com" // Example of an invalid host
            ];
            await ResolveIpToHostname.Resolve(ipAddresses); // Run the pinging of hosts in parallel

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();


            List<string> hostsToPing =
            [
                "google.com",
                "172.16.10.1",
                "github.com",
                "broadcom.com",
                "linux.com",
                "youtube.com",
                "facebook.com",
                "amazon.com",
                "stackoverflow.com",

                "invalidhost.example.com" // Example of an invalid host
            ];
            await RunPings.Run(hostsToPing); // Run the pinging of hosts in parallel

            Console.ReadLine(); // temp for testing


            var _switchip = GetSwitchIP(); //e.g. "172.16.102.6"
            var _username = GetUsername(); //e.g. "admin"
            var _password = GetPassword();
            List<string> _commandsToRunOnSwitch =
            [
                "show interface status | include (connected)",
                "show ip arp",
                "show mac address-table dynamic", //dynamic
                ""  // TODO: remove this line ??
            ];
            SwitchManager switchManager = new(_switchip, _username, _password);
            await switchManager.RunCommandsAsync(_commandsToRunOnSwitch);
            //foreach (var mac in switchManager.MacAddresses) // remove last two chars from the InterfaceName property of each Interface
            //{
            //    if (mac.Port.Length > 2) if(mac.Port.Substring(mac.Port.Length - 2) == "\r") mac.Port = mac.Port.Substring(0, mac.Port.Length - 2);
            //}

            // Merge the Lists into a single List for display TODO: and export
            foreach (var ipAddr in switchManager.IpAddresses)
            {
                var swInt = switchManager.SwitchInterfaces.FirstOrDefault(i => i.Mac == ipAddr.Mac);
                if (swInt != null)
                {
                    swInt.Ip = ipAddr.Ip;
                }
                else
                {
                    // TODO: handle IP address missing in IpAddresses
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
            //var switchPortInfo = switchManager.IpAddresses;

            await Helpers.DisplayInterfacesResultToConsole(switchManager.SwitchInterfaces);


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
                catch (Exception) { throw; }
            }
        }
    }
}