using System;
using Microsoft.Azure.WebJobs.Description;

namespace Microsoft.Azure.WebJobs.Extensions.MongoDb
{
    [AttributeUsage(AttributeTargets.Parameter)]
    [Binding]
    public sealed class MongoDbTriggerAttribute : Attribute 
    {
        public MongoDbTriggerAttribute(string collectionName)
        {
            if (string.IsNullOrWhiteSpace(collectionName))
            {
                throw new ArgumentException("Missing information for the collection to monitor", nameof(collectionName));
            }

            CollectionName = collectionName;
        }

        /// <summary>
        /// Connection string for the service containing the database and collection to monitor
        /// </summary>
        public string ConnectionStringSetting { get; set; }

        /// <summary>
        /// Name of the collection to monitor for changes
        /// </summary>
        public string CollectionName { get; private set; }
    }
}