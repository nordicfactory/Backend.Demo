using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host.Triggers;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace Microsoft.Azure.WebJobs.Extensions.MongoDb
{
    internal sealed class MongoDbBindingProvider : ITriggerBindingProvider
    {
        private readonly IConfiguration _configuration;

        public MongoDbBindingProvider(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Task<ITriggerBinding> TryCreateAsync(TriggerBindingProviderContext context) => 
            Task.FromResult(TryCreate(context));

        public ITriggerBinding TryCreate(TriggerBindingProviderContext context)
        {
            var parameter = context?.Parameter ?? throw new ArgumentNullException(nameof(context));
            if (parameter.ParameterType.GetGenericTypeDefinition() != typeof(ChangeStreamDocument<>))
                //we only support binding to this type
                return null;

            var attr = parameter.GetCustomAttribute<MongoDbTriggerAttribute>(inherit: false);
            if (attr == null)
                //only bind if parameter has our supported attribute
                return null;

            var triggerConnectionString = ResolveAttributeConnectionString(attr);

            var genericType = parameter.ParameterType.GetGenericArguments().First();

            return new MongoDbTriggerBinding(genericType, attr.CollectionName, triggerConnectionString);
        }

        private string ResolveAttributeConnectionString(MongoDbTriggerAttribute triggerAttribute) => 
            triggerAttribute.ConnectionStringSetting.Contains("%") 
                ? ResolveConnectionString(triggerAttribute.ConnectionStringSetting.Replace("%",""), nameof(MongoDbTriggerAttribute.ConnectionStringSetting)) 
                : triggerAttribute.ConnectionStringSetting;

        private string ResolveConnectionString(string unresolvedConnectionString, string propertyName)
        {
            if (string.IsNullOrEmpty(unresolvedConnectionString)) return null;

            var resolvedString = _configuration.GetConnectionStringOrSetting(unresolvedConnectionString);

            if (string.IsNullOrEmpty(resolvedString))
                throw new InvalidOperationException(
                    $"Unable to resolve app setting for property '{nameof(MongoDbTriggerAttribute)}.{propertyName}'. Make sure the app setting exists and has a valid value.");

            return resolvedString;
        }
    }
}