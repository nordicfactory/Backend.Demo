using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Description;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebJobs.Host.Listeners;
using Microsoft.Azure.WebJobs.Host.Protocols;
using Microsoft.Azure.WebJobs.Host.Triggers;
using MongoDB.Driver;

namespace Microsoft.Azure.WebJobs.Extensions.MongoDb
{
    [Extension("MongoDb")]
    internal sealed class MongoDbTriggerBinding : ITriggerBinding
    {
        private readonly Type _genericType;
        private readonly string _collectionName;
        private readonly string _connectionString;

        public MongoDbTriggerBinding(Type genericType, string collectionName, string connectionString)
        {
            _genericType = genericType;
            _collectionName = collectionName;
            _connectionString = connectionString;
        }

        public Task<ITriggerData> BindAsync(object value, ValueBindingContext context)
        {
            ITriggerData triggerData = new TriggerData(new ValueProvider(value, _genericType),  new Dictionary<string, object>());
            return Task.FromResult(triggerData);
        }

        public Task<IListener> CreateListenerAsync(ListenerFactoryContext context) =>
            Task.FromResult<IListener>(new MongoDbListener(_genericType, _collectionName, _connectionString, context.Executor));

        public ParameterDescriptor ToParameterDescriptor() =>
            new ParameterDescriptor
            {
                Name = "MongoDocumentTriggerBinding",
                DisplayHints = new ParameterDisplayHints
                {
                    Description = "what is the purpose of this",
                    Prompt = "p"
                }
            };

        public Type TriggerValueType => typeof(ChangeStreamDocument<>).MakeGenericType(_genericType);

        public IReadOnlyDictionary<string, Type> BindingDataContract { get; } = null;

        private class ValueProvider : IValueProvider
        {
            private readonly object _value;

            public ValueProvider(object value, Type triggerValueType)
            {
                _value = value;
                Type = typeof(ChangeStreamDocument<>).MakeGenericType(triggerValueType);
            }

            public Type Type { get; }

            public Task<object> GetValueAsync() => Task.FromResult(_value);

            public string ToInvokeString() => DateTime.Now.ToString("o");
        }
    }
}