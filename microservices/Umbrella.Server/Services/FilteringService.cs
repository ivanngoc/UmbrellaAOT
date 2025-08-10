
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Umbrella.Domain.Dtos;
using Umbrella.Server.Application;

namespace Umbrella.Server.Services
{
    public class FilteringService(ILogger<FilteringService> logger, IConfiguration configuration, StoreRaw storeIn, StoreFiltered storeOut) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var filters = configuration.GetSection("Filters").Get<Filter[]>() ?? Array.Empty<Filter>();
                    while (!stoppingToken.IsCancellationRequested)
                    {
                        var message = await storeIn.PullAsync();
                        if (filters.Any() && filters.Any(x => !x.Match(message)))
                        {
                            continue;
                        }
                        await storeOut.PushAsync(message);
                        logger.LogInformation(message.ToStringPretty());
                    }
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex.Message);
                }
            }
        }
    }

    internal class Filter
    {

        [JsonPropertyName("field")]
        public string? Field { get; set; }

        [JsonPropertyName("operator")]
        public string? Operator { get; set; }

        [JsonPropertyName("value")]
        public string? Value { get; set; }

        internal bool Match(MessageDto message)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(Field);
            ArgumentNullException.ThrowIfNullOrEmpty(Operator);

            var jObj = JsonObject.Parse(message.Json);
            if (jObj == null) return false;
            var prop = jObj[Field];
            if (prop == null) return false;
            var left = prop.GetValue<string>();
            var right = Value;
            if (Operator == "equals")
            {
                return left == right;
            }
            else if (Operator == "not_equals")
            {
                return left != right;
            }
            return false;
        }
    }
}
