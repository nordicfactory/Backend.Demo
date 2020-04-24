using System;
using Microsoft.Azure.WebJobs.Extensions.MongoDb;

namespace Microsoft.Azure.WebJobs.Extensions
{
    public static class MongoTriggerExtensions
    {
        /// <summary>
        /// Adds the MongoDbTrigger extension to the provided <see cref="IWebJobsBuilder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IWebJobsBuilder"/> to configure.</param>
        public static IWebJobsBuilder AddMongoDbTrigger(this IWebJobsBuilder builder)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            //todo bind objects or w/e
            builder.AddExtension<MongoDbExtensionsProvider>();
            return builder;
        }
    }
}