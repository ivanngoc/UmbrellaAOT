
namespace Umbrella.Server.Application.Pipeline
{
    public abstract class PipelineNode
    {
        private PipelineRunner? pipeline;
        protected PipelineRunner Pipeline => pipeline ?? throw new InvalidOperationException();
        public virtual Task PrepareAsync(PipelineRunner pipeline, Session session, CancellationToken ct = default)
        {
            this.pipeline = pipeline;
            return Task.CompletedTask;
        }

        public virtual Task ExecuteAsync(CancellationToken ct = default)
        {
            return Task.CompletedTask;
        }
    }
}
