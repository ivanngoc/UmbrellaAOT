using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Umbrella.Domain.Dtos
{
    public class MessageDto
    {
        public required Guid Id { get; set; }

        [StringSyntax(StringSyntaxAttribute.Json)]
        public required string Json { get; set; }

        public static MessageDto Create(string line)
        {
            return new MessageDto() { Id = Guid.NewGuid(), Json = line };
        }

        public bool IsValid()
        {
            if (string.IsNullOrWhiteSpace(Json))
            {
                return false;
            }
            try
            {
                var res = JsonObject.Parse(Json);
                return res != null;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public string ToStringPretty()
        {
            if (string.IsNullOrWhiteSpace(Json)) return string.Empty;
            using var doc = JsonDocument.Parse(Json);
            return JsonSerializer.Serialize(doc.RootElement, JsonElementContext.Default.JsonElement);
        }
    }
}
