namespace Umbrella.Server.Application.Pipeline
{
    public class StartNode(IConfiguration configuration, IServiceProvider provider) : PipelineNode
    {
        public override async Task PrepareAsync(PipelineRunner pipeline, Session session, CancellationToken ct = default)
        {
            var tlsEnabledStr = configuration.GetSection("TlsEnabled").Value;
            var tlsEnabled = string.IsNullOrWhiteSpace(tlsEnabledStr) ? false : bool.Parse(tlsEnabledStr);
            if (tlsEnabled)
            {
                var nodeTls = provider.GetRequiredService<TlsNode>();
                pipeline.Add(nodeTls);
                await nodeTls.PrepareAsync(pipeline, session);
            }
        }
    }
}
