using System.Collections.Concurrent;
using System.Threading.Channels;
using Umbrella.Domain.Dtos;

namespace Umbrella.Server.Application
{
    public abstract class Store
    {
        protected readonly Channel<MessageDto> items = Channel.CreateBounded<MessageDto>(new BoundedChannelOptions(100)
        {
            SingleReader = true,
            SingleWriter = false,
            AllowSynchronousContinuations = false,
        });

        public async Task<MessageDto> PullAsync()
        {
            return await items.Reader.ReadAsync();
        }
    }
}
