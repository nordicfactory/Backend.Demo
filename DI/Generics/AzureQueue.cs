using System.Threading.Tasks;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Queue;

namespace DI
{
    public class AzureQueue<T> : IQueue<T> where T : IMessage
    {
        private readonly CloudQueue _queue;

        public AzureQueue()
        {
            var storageAccount = CloudStorageAccount.Parse("UseDevelopmentStorage=true");
            var queueClient = storageAccount.CreateCloudQueueClient();

            // Retrieve a reference to a queue

            var name = typeof(T).Name.ToLowerInvariant();
            _queue = queueClient.GetQueueReference(name);
        }

        public Task Send(T message) => _queue.AddMessageAsync(new CloudQueueMessage(message.Content));
    }
}