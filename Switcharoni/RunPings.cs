using System.Net.NetworkInformation;

namespace Switcharoni
{
    internal class RunPings
    {
        internal static async Task Run(List<string> hostsToPing)
        {
            Console.WriteLine("Pinging hosts in parallel...");
            await  Task.Delay(1500);
            // Use Parallel.ForEach to send pings concurrently
            Parallel.ForEach(hostsToPing, host =>
            {
                using (Ping pingSender = new Ping())
                {
                    try
                    {
                        PingReply reply = pingSender.Send(host, 1500);

                        if (reply.Status == IPStatus.Success)
                        {
                            Console.WriteLine($"Ping successful for {host}: RoundtripTime={reply.RoundtripTime}ms, Address={reply.Address}");
                        }
                        else
                        {
                            Console.WriteLine($"Ping failed for {host}: Status={reply.Status}");
                        }
                    }
                    catch (PingException ex)
                    {
                        Console.WriteLine($"Error pinging {host}: {ex.Message}");
                    }
                }
            });
            Console.WriteLine("Parallel pinging complete.");
        }
    }
}
