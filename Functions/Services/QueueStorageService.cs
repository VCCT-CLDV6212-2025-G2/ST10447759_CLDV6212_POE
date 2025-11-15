using Azure.Storage.Queues;
using AzureRetailHub.Functions.Settings; // <-- CHANGED
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Threading.Tasks;

namespace AzureRetailHub.Functions.Services // <-- CHANGED
{
    public class QueueStorageService
    {
        private readonly QueueClient _queueClient;
        private readonly StorageOptions _opts;

        public QueueStorageService(IOptions<StorageOptions> options)
        {
            _opts = options.Value;
            _queueClient = new QueueClient(_opts.ConnectionString, _opts.QueueName);
            _queueClient.CreateIfNotExists();
        }

        public async Task SendMessageAsync<T>(T messageObject)
        {
            var json = JsonSerializer.Serialize(messageObject);
            await _queueClient.SendMessageAsync(json);
        }
    }
}