using System.Net.Sockets;
using Umbrella.Server.Application.Pipeline;

namespace Umbrella.Server.Application
{

    public class Session : IDisposable
    {
        public readonly TcpClient client;
        public PipelineRunner? Runner { get; set; }
        public Guid Id { get; private set; }

        public Session(TcpClient client)
        {
            Id = Guid.NewGuid();
            this.client = client;
        }

        public void Dispose()
        {
            client.Dispose();
        }
    }
}
