
namespace Umbrella.Server.Application.Pipeline
{
    public class PipelineFactory(IServiceProvider serviceProvider)
    {
        internal PipelineRunner Create(Session session)
        {
            var scope = serviceProvider.CreateScope();
            var runner = scope.ServiceProvider.GetRequiredService<PipelineRunner>();
            runner.Init(session, scope);
            session.Runner = runner;
            return runner;
        }
    }
}
