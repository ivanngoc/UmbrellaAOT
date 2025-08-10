

namespace Umbrella.Server.Application.Pipeline
{
    public class PipelineRunner : IDisposable
    {
        private readonly List<PipelineNode> nodes = new List<PipelineNode>();
        private IServiceScope? scope;
        private Session? session;
        private ILogger<PipelineRunner>? logger;
        private Task? task;
        private Stream? stream;
        public Task Run(CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(scope);
            ArgumentNullException.ThrowIfNull(session);
            var start = scope.ServiceProvider.GetRequiredService<StartNode>();
            Add(start);
            var processing = scope.ServiceProvider.GetRequiredService<ProducerNode>();
            Add(processing);

            this.task = Task.Run(async () =>
            {
                await start.PrepareAsync(this, session, ct);
                await processing.PrepareAsync(this, session, ct);
                await processing.ExecuteAsync(ct);
            }).ContinueWith(x =>
            {
                if (x.Exception != null)
                {
                    logger?.LogDebug(x.Exception.Message);
                }
                Dispose();
            });
            return task;
        }

        internal void Add(PipelineNode node)
        {
            nodes.Add(node);
        }

        internal void Init(Session session, IServiceScope scope)
        {
            this.scope = scope;
            this.session = session;
            this.logger = scope.ServiceProvider.GetRequiredService<ILogger<PipelineRunner>>();
            SetStream(session.client.GetStream());
        }

        internal Stream GetStream()
        {
            ArgumentNullException.ThrowIfNull(stream);
            return stream;
        }

        internal void SetStream(Stream stream)
        {
            this.stream = stream;
        }

        public void Dispose()
        {
            scope?.Dispose();
            scope = null;
            session?.Dispose();
            session = null;
        }
    }
}
