using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Nodes;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Images;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Umbrella.Networking;
using Umbrella.Server.Application;
namespace Umbrella.IntegrationTests
{
    public class PipelineRunner_IntegrationTests
    {
        private const string expected0 = "{\"type\":\"event\",\"level\":\"info\",\"message\":\"Service started\"}";
        private const string expected1 = "{\"type\":\"event\",\"level\":\"error\",\"message\":\"Something went wrong\"}";
        private const string expected2 = "{\"type\":\"event\",\"level\":\"info\",\"message\":\"All systems operational\"}";
#if !WINDOWS
        [Fact]
        public async Task Should_dequeue_expected_messages()
        {
            var factory = new WebApplicationFactory<Umbrella.Server.Program>();
            var serv = factory.CreateClient();
            var services = factory.Services;

            var factoryClient = new ClientFactory<Client>();
            var client = factoryClient.Create();
            await client.RunAsync(CancellationToken.None);

            var manager = services.GetRequiredService<SessionsManager>();
            var store = services.GetRequiredService<StoreFiltered>();
            var session = Assert.Single(manager.Sessions);
            var runner = session.Runner;

            var fact0 = await store.PullAsync();
            Assert.Equal(expected0, fact0.Json);
            var fact1 = await store.PullAsync();
            Assert.Equal(expected1, fact1.Json);
            var fact2 = await store.PullAsync();
            Assert.Equal(expected2, fact2.Json);
        }
#endif
        [Fact]
        public async Task Should_dequeue_expected_messages_linux()
        {
            var builder = new ContainerBuilder()
           .WithImage("yoojie/umbrella-server:latest")
           .WithName("umbrella-server")
           .WithCleanUp(true)
           .WithImagePullPolicy(PullPolicy.Always)
           .WithPortBinding(8080, true)
           .WithPortBinding(5555, true)
           .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(8080));

            var container = builder.Build();
            await container.StartAsync();
            var portHttp = container.GetMappedPublicPort(8080);
            var port = container.GetMappedPublicPort(5555);

            var text = await File.ReadAllTextAsync("configs/config.json");
            var jObj = JsonObject.Parse(text);
            ArgumentNullException.ThrowIfNull(jObj);
            jObj["ServerPort"] = port;
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(jObj.ToJsonString()));
            var confBuilder = new ConfigurationBuilder().AddJsonStream(stream);
            var factoryClient = new ClientFactory<Client>(confBuilder);
            var client = factoryClient.Create();
            await client.RunAsync(CancellationToken.None);

            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri($"http://localhost:{portHttp}");

            var fact0 = await httpClient.GetFromJsonAsync<string>("/store/pull");
            var fact1 = await httpClient.GetFromJsonAsync<string>("/store/pull");
            var fact2 = await httpClient.GetFromJsonAsync<string>("/store/pull");

            Assert.Equal(expected0, fact0);
            Assert.Equal(expected1, fact1);
            Assert.Equal(expected2, fact2);
        }
    }
}
