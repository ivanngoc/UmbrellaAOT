using System.Text.Json.Serialization;

namespace Umbrella.Domain.Dtos
{
    [JsonSerializable(typeof(MessageDto))]
    public partial class DomainJsonSerializerContext : JsonSerializerContext
    {

    }
}
