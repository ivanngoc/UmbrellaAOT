
using System.Net;

namespace Umbrella.Networking
{
    public class ClientConfigs
    {
        public required string InputPath { get; set; }
        public required IPAddress ServerHost { get; set; }
        public required int ServerPort { get; set; }
        public required bool TlsEnabled { get; set; }
        public required bool TlsValidateCert { get; set; }

        internal ClientConfigs Merge(ClientConfigs? defaultConfigs)
        {
            return this;
        }
    }
}
