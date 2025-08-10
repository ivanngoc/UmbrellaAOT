using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration.Json;
using Scalar.AspNetCore;
using Umbrella.Server.Application;
using Umbrella.Server.Application.Pipeline;
using Umbrella.Server.Services;

namespace Umbrella.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateSlimBuilder(args);

            builder.Services.ConfigureHttpJsonOptions(options =>
            {
                options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
            });

            foreach (var source in builder.Configuration.Sources)
            {
                if (source is JsonConfigurationSource jsonSource)
                {
                    Console.WriteLine(jsonSource.Path);
                }
            }

            Console.WriteLine($"BaseDirectory: {AppContext.BaseDirectory}");
            Console.WriteLine($"EnvironmentName: {builder.Environment.EnvironmentName}");
            Console.WriteLine($"Configuration: {builder.Configuration.GetSection("ListenAddr").Value}");
            builder.Services.AddOpenApi();
            builder.Services.AddHostedService<TcpService>();
            builder.Services.AddHostedService<FilteringService>();
            builder.Services.AddSingleton<SessionsManager>();
            builder.Services.AddSingleton<PipelineFactory>();
            builder.Services.AddSingleton<StoreFiltered>();
            builder.Services.AddSingleton<StoreRaw>();
            builder.Services.AddScoped<PipelineRunner>();
            builder.Services.AddScoped<ProducerNode>();
            builder.Services.AddScoped<StartNode>();
            builder.Services.AddScoped<TlsNode>();

            var app = builder.Build();

            app.MapOpenApi();
            app.MapScalarApiReference();

            var sampleTodos = new Todo[] {
                new(1, "Walk the dog"),
                new(2, "Do the dishes", DateOnly.FromDateTime(DateTime.Now)),
                new(3, "Do the laundry", DateOnly.FromDateTime(DateTime.Now.AddDays(1))),
                new(4, "Clean the bathroom"),
                new(5, "Clean the car", DateOnly.FromDateTime(DateTime.Now.AddDays(2)))
            };

            var todosApi = app.MapGroup("/todos");
            todosApi.MapGet("/", () => sampleTodos);
            todosApi.MapGet("/{id}", (int id) =>
                sampleTodos.FirstOrDefault(a => a.Id == id) is { } todo
                    ? Results.Ok(todo)
                    : Results.NotFound());

            var storeApi = app.MapGroup("/store");

            storeApi.MapGet("/pull", async (StoreFiltered store) =>
            {
                var result = await store.PullAsync();
                return Results.Ok(result.Json);
            });

            app.Run();
        }
    }

    public record Todo(int Id, string? Title, DateOnly? DueBy = null, bool IsComplete = false);

    [JsonSerializable(typeof(Todo[]))]
    [JsonSerializable(typeof(Filter[]))]
    internal partial class AppJsonSerializerContext : JsonSerializerContext
    {

    }
}
