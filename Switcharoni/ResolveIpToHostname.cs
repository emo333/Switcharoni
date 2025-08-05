using System.Net;
using System.Net.Sockets;

namespace Switcharoni
{
    internal class ResolveIpToHostname
    {
        internal static async Task Resolve(List<string> ipAddresses)
        {
            await Task.Delay(200);

            // Use Parallel.ForEachAsync to resolve IP addresses concurrently
            await Parallel.ForEachAsync(ipAddresses, CancellationToken.None, async (ipAddress, ct) =>
            {
                try
                {
                    IPAddress ip = IPAddress.Parse(ipAddress);
                    IPHostEntry hostEntry = await Dns.GetHostEntryAsync(ip);
                    Console.WriteLine(hostEntry.HostName + $" ( {ipAddress} )");
                }
                catch (FormatException)
                {
                    Console.WriteLine($"Error: '{ipAddress}' is not a valid IP address format.");
                }
                catch (SocketException ex)
                {
                    Console.WriteLine($"Error resolving IP '{ipAddress}': {ex.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An unexpected error occurred: {ex.Message}");
                }
            });
        }
    }
}
