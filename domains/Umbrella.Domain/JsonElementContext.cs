using System.Text.Json;
using System.Text.Json.Serialization;

namespace Umbrella.Domain.Dtos
{
    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(JsonElement))]
    internal partial class JsonElementContext : JsonSerializerContext
    {
    }
}
