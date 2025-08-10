
using System.Net.Security;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.Json.Nodes;

namespace Umbrella.Server.Application.Pipeline
{
    public class ProducerNode(IConfiguration configuration, ILogger<ProducerNode> logger, StoreRaw store) : PipelineNode
    {
        public override async Task ExecuteAsync(CancellationToken ct = default)
        {
            var filtersStr = configuration.GetSection("Filters").Value;
            using var str = Pipeline.GetStream();
            using var reader = new StreamReader(str);
            ArgumentNullException.ThrowIfNull(reader);
            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                if (string.IsNullOrEmpty(line))
                {
                    break;
                }
                logger.LogInformation(line);
                await store.PushAsync(line);
            }
            await str.WriteAsync(Encoding.UTF8.GetBytes("OK"));
        }
    }
}
