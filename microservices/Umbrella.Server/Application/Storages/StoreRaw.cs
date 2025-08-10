using System.Text.Json;
using Umbrella.Domain.Dtos;

namespace Umbrella.Server.Application
{
    public class StoreRaw : Store
    {
        public async Task PushAsync(string line)
        {
            var dto = JsonSerializer.Deserialize<MessageDto>(line, DomainJsonSerializerContext.Default.MessageDto);
            ArgumentNullException.ThrowIfNull(dto);
            await items.Writer.WriteAsync(dto);
        }
    }
}
