using Umbrella.Networking;

namespace Umbrella.ClientApp
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var factoryClient = new ClientFactory<Client>(args[0]);
            var client = factoryClient.Create();
            await client.RunAsync(CancellationToken.None);
            Console.WriteLine("Finish");
        }
    }
}
