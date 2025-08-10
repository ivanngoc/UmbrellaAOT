using Microsoft.Extensions.Configuration;

namespace Umbrella.Networking
{
    public class ClientFactory<T> where T : Client, new()
    {
        private IConfigurationBuilder builder;

        public ClientFactory(string confPath = "configs/config.json")
        {
            this.builder = new ConfigurationBuilder().AddJsonFile(confPath, optional: false, reloadOnChange: false);
        }

        public ClientFactory(IConfigurationBuilder builder)
        {
            this.builder = builder;
        }

        public T Create()
        {
            var config = builder.Build();
            var client = new T();
            client.Config(config);
            return client;
        }
    }
}
