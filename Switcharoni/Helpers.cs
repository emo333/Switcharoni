using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Switcharoni
{
    internal static class Helpers
    {

        internal static Task DisplayInterfacesResultToConsole( List<SwitchManager.SwitchInterface> switchinterfaces)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("[[[[[  SWITCH PORTS  ]]]]]");
            switchinterfaces.ForEach(i => Console.WriteLine($"Interface: {i.InterfaceName} IP Address: {i.Ip} MAC: {i.Mac}"));
            Console.WriteLine("\n\n");

            return Task.CompletedTask;
        }


        internal static Task DisplayIpAddressesResultToConsole(List<SwitchManager.IpAddress> ipaddresses)
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("[[[[[  IP ADDRESSES  ]]]]]");
            ipaddresses.OrderBy(i => i.Port).ToList().ForEach(i => Console.WriteLine($" PORT: {i.Port}  |  IP Address: {i.Ip}  |  MAC: {i.Mac}"));
            Console.WriteLine("\n\n");

            return Task.CompletedTask;
        }


        internal static Task DisplayMacAddressesResultToConsole(List<SwitchManager.MacAddress> macaddresses)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("[[[[[  MAC ADDRESSES  ]]]]]");
            macaddresses.ForEach(i => Console.WriteLine($"MAC: {i.Mac} Port: {i.Port}"));
            Console.WriteLine("\n\n");

            return Task.CompletedTask;
        }



        internal static Task DisplayBanner(string[] args)
        {
            const string Value = """


                ########################################################
                ##                                                    ##
                ##                    SWITCHARONI                     ##
                ##                                                    ##
                ##             ____________________________           ##
                ##            /                           /|          ##
                ##           /   by: M. Wilson 8/2025    / |          ##
                ##          /                           /  |...       ##
                ##         /___________________________/  /....       ##
                ##         |  [+] [ ] [ ] [+] [+] [ ]  | /.....       ##
                ##         |___________________________|/......       ##
                ##                                                    ##
                ##                                                    ##
                ##    - Enter the Management IP ###.###.###.###       ##
                ##       of the Cisco Switch that you want to get     ##
                ##       all of the IP addresses of hosts connected   ##
                ##       switch ports.                                ##
                ##                                                    ##
                ##    - You will be prompted for the ssh USERNAME     ##
                ##       and PASSWORD for the switch.                 ##
                ##                                                    ##
                ########################################################


                """;
            Console.WriteLine(Value);
            Console.ResetColor();
            if (args.Length > 0 && args[0] == "help")
            {
                Console.WriteLine("Usage: Switcharoni [options]");
                Console.WriteLine("Options:");
                Console.WriteLine("  help       Show this help message");
                Console.WriteLine("  ip         Specify the switch IP address");
                Console.WriteLine("  username   Specify the SSH username");
                Console.WriteLine("  password   Specify the SSH password");
                return Task.CompletedTask;
            }

            return Task.CompletedTask;
        }
    }
}
