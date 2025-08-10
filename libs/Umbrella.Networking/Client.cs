using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Umbrella.Domain.Dtos;

namespace Umbrella.Networking
{
    public class Client
    {
        private ClientConfigs? defaultConfigs;
        private ILogger<Client>? logger;
        private ILogger<Client> Logger => logger ?? throw new InvalidOperationException();
        public virtual async Task RunAsync(CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(defaultConfigs);
            using var client = new TcpClient();
            await client.ConnectAsync(defaultConfigs.ServerHost, defaultConfigs.ServerPort);
            using var str = client.GetStream();
            SslStream? ssl = null;
            Stream strCurrent = str;
            byte[] buffer = new byte[512];
            if (defaultConfigs.TlsEnabled)
            {
                ssl = new SslStream(str);
                strCurrent = ssl;
                var opt = new SslClientAuthenticationOptions()
                {
                    RemoteCertificateValidationCallback = (object sender, X509Certificate? certificate, X509Chain? chain, SslPolicyErrors sslPolicyErrors) =>
                    {
                        if (!defaultConfigs.TlsValidateCert)
                        {
                            return true;
                        }
                        if (sslPolicyErrors == SslPolicyErrors.None) return true;
                        return false;
                    }
                };
                await ssl.AuthenticateAsClientAsync(opt, ct);
            }
            var fi = new FileInfo(defaultConfigs.InputPath);
            if (fi.Exists)
            {
                var lines = await File.ReadAllLinesAsync(fi.FullName);
                var messages = lines.Select(x => MessageDto.Create(x)).Select(x => (x, IsValid: x.IsValid())).ToArray();
                var messagesValid = messages.Where(x => x.IsValid).Select(x => JsonSerializer.Serialize(x.x, DomainJsonSerializerContext.Default.MessageDto)).ToArray();
                var messagesInvalid = messages.Where(x => !x.IsValid).Select(x => x.x).ToArray();

                foreach (var messageInvalid in messagesInvalid)
                {
                    logger?.LogError($"Invalid message format. Recieved: [{messageInvalid.Json}]");
                }

                var data = messagesValid.Aggregate((x, y) => x + Environment.NewLine + y);
                await strCurrent.WriteAsync(Encoding.UTF8.GetBytes(data));
                var eof = Encoding.UTF8.GetBytes("\r\n");
                await strCurrent.WriteAsync(eof);
                await strCurrent.WriteAsync(eof);
                var readed = await strCurrent.ReadAtLeastAsync(buffer, 1);
                ssl?.Dispose();
#if DEBUG
                var result = Encoding.UTF8.GetString(buffer.AsSpan().Slice(0, readed));
                Console.WriteLine(result);
#endif
                logger?.LogInformation("Done");
            }
            else
            {
                throw new FileNotFoundException(fi.FullName);
            }
        }

        internal ClientConfigs Parse(IConfigurationRoot config)
        {
            return new ClientConfigs()
            {
                InputPath = config.GetSection("InputPath").Value!,
                ServerHost = IPAddress.Parse(config.GetSection("ServerHost").Value!),
                ServerPort = int.Parse(config.GetSection("ServerPort").Value!),
                TlsEnabled = bool.Parse(config.GetSection("TlsEnabled").Value!),
                TlsValidateCert = bool.Parse(config.GetSection("TlsValidateCert").Value!),
            };
        }

        internal void SetLogger(ILogger<Client> logger)
        {
            this.logger = logger;
        }

        internal ClientConfigs Config(IConfigurationRoot config)
        {
            defaultConfigs = Parse(config);
            return defaultConfigs;
        }
    }
}
