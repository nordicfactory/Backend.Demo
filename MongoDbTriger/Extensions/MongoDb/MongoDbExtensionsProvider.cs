using Microsoft.Azure.WebJobs.Description;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Azure.WebJobs.Extensions.MongoDb
{
    [Extension("MongoDb")]
    internal sealed class MongoDbExtensionsProvider : IExtensionConfigProvider
    {
        private readonly IConfiguration _configuration;


        public MongoDbExtensionsProvider(IConfiguration configuration) => _configuration = configuration;

        /// <summary>
        /// This callback is invoked by the WebJobs framework before the host starts execution. 
        /// It should add the binding rules and converters for our new <see cref="MongoDbTriggerAttribute"/>
        /// </summary>
        public void Initialize(ExtensionConfigContext context) =>
            context
                .AddBindingRule<MongoDbTriggerAttribute>()
                .BindToTrigger(new MongoDbBindingProvider(_configuration));
    }
}