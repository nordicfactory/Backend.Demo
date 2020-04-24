using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host.Executors;
using Microsoft.Azure.WebJobs.Host.Listeners;
using MongoDB.Driver;

namespace Microsoft.Azure.WebJobs.Extensions.MongoDb
{
    [Singleton(Mode = SingletonMode.Listener)]
    internal sealed class MongoDbListener : IListener
    {
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        private readonly Type _genericType;
        private readonly ITriggeredFunctionExecutor _contextExecutor;
        private readonly string _connectionString;
        private readonly string _collectionName;

        private bool _disposed;
        private Task _watchTask;

        public MongoDbListener(Type genericType, string collectionName, string connectionString, ITriggeredFunctionExecutor contextExecutor)
        {
            _genericType = genericType;
            _contextExecutor = contextExecutor;
            _collectionName = collectionName;
            _connectionString = connectionString;
        }

        public void Cancel()
        {
            ThrowIfDisposed();
            _cancellationTokenSource.Cancel();
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                // Running callers might still be using the cancellation token.
                // Mark it canceled but don't dispose of the source while the callers are running.
                // Otherwise, callers would receive ObjectDisposedException when calling token.Register.
                // For now, rely on finalization to clean up _cancellationTokenSource's wait handle (if allocated).
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.Dispose();
 
                _disposed = true;
            }
        }

        public Task StartAsync(CancellationToken cancellationToken) => WatchAsync(MongoUrl.Create(_connectionString), _cancellationTokenSource.Token);

        private async Task WatchAsync(MongoUrl url, CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            var db = new MongoClient(url).GetDatabase(url.DatabaseName);

            var method = typeof(IMongoDatabase).GetMethod(nameof(IMongoDatabase.GetCollection));

            dynamic collection = method.MakeGenericMethod(_genericType)
                .Invoke(db, new object[] {_collectionName, new MongoCollectionSettings() });

            await Watch(collection, cancellationToken);
        }

        private async Task Watch<T>(IMongoCollection<T> collection, CancellationToken cancellationToken)
        {         
            var cursor = await collection.WatchAsync(cancellationToken: cancellationToken);
            _watchTask = cursor.ForEachAsync(document => WatchChange(document, cancellationToken), cancellationToken);
        }

        private async Task WatchChange<T>(ChangeStreamDocument<T> arg, CancellationToken cancellationToken)
        {
            var input = new TriggeredFunctionData
            {
                TriggerValue = arg,
            };
            try
            {
                await _contextExecutor.TryExecuteAsync(input, cancellationToken);
            }
            catch
            {
                // We don't want any function errors to stop the execution
                // schedule. Errors will be logged to Dashboard already.
            }
        }


        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _cancellationTokenSource.Cancel();

            var cancellationTaskSource = new TaskCompletionSource<object>();
            using (cancellationToken.Register(() => cancellationTaskSource.SetCanceled()))
            {
                // Wait for all pending command tasks to complete (or cancellation of the token) before returning.
                await Task.WhenAny(_watchTask, cancellationTaskSource.Task);
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(null);
            }
        }
    }
}