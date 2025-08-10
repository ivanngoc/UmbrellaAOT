
using System.Net;
using System.Net.Sockets;
using Umbrella.Server.Application;
using Umbrella.Server.Application.Pipeline;

namespace Umbrella.Server.Services
{
    public class TcpService(ILogger<TcpService> logger, IConfiguration configuration, SessionsManager sessionsManager, IServiceProvider serviceProvider) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var addrStr = configuration.GetSection("ListenAddr").Value;
                    ArgumentException.ThrowIfNullOrWhiteSpace(addrStr);
                    var addr = IPAddress.Parse(addrStr);
                    var portStr = configuration.GetSection("ListenPort").Value;
                    ArgumentException.ThrowIfNullOrWhiteSpace(portStr);
                    var port = int.Parse(portStr);
                    TcpListener tcpListener = new TcpListener(addr, port);
                    tcpListener.Start();

                    while (!stoppingToken.IsCancellationRequested)
                    {
                        var client = await tcpListener.AcceptTcpClientAsync();
                        var session = new Session(client);
                        sessionsManager.Add(session);
                        var factory = serviceProvider.GetRequiredService<PipelineFactory>();
                        var pipeline = factory.Create(session);
                        var fireAndForget = pipeline.Run();
                    }
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, ex.Message);
                    await Task.Delay(TimeSpan.FromMinutes(1));
                }
            }
        }
    }
}
