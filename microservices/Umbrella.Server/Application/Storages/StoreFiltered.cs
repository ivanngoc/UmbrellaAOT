using Umbrella.Domain.Dtos;

namespace Umbrella.Server.Application
{
    public class StoreFiltered : Store
    {
        public async Task PushAsync(MessageDto message)
        {
            await items.Writer.WriteAsync(message);
        }
    }
}
