using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using Fdb.Rx.Domain;
using System.Serialization;
using Newtonsoft.Json;
using JsonSerializer = System.Serialization.JsonSerializer;

namespace Fdb.Rx.Persistence.Blob
{
    internal static class DomainExtensions
    {
        private static T DeserializeObjectFromBlob<T>(this Stream blobStream)
        {
            using StreamReader sr = new StreamReader(blobStream);
            using JsonTextReader jtr = new JsonTextReader(sr);
            JsonSerializer ser = new JsonSerializer();
            return ser.Deserialize<T>(jtr)!;
        }

        internal static async Task<T> Get<T>(this BlobServiceClient client, string containerName, string documentId) where T : IAggregate
        {
            try
            {
                return (await client.GetBlobContainerClient(containerName.ToLower()).GetBlobClient(documentId.ToLower()).DownloadAsync().ConfigureAwait(false)).Value.Content.DeserializeObjectFromBlob<T>();
            }
            catch (RequestFailedException)
            {
                return default!;
            }
        }

        internal static async Task Save<T>(this BlobServiceClient client, string containerName, T aggregate) where T : IAggregate
        {
            await client.GetBlobContainerClient(containerName.ToLower()).CreateIfNotExistsAsync();
            var commandContent = new MemoryStream(Encoding.UTF8.GetBytes(aggregate.Serialize()));
            await client.GetBlobContainerClient(containerName.ToLower()).GetBlobClient(aggregate.Id.ToLower()).UploadAsync(commandContent, overwrite: true);
        }

        internal static async Task Delete<T>(this BlobServiceClient client, string containerName, string documentId) where T : IAggregate
        {
            try
            {
                await client.GetBlobContainerClient(containerName.ToLower()).GetBlobClient(documentId.ToLower()).DeleteAsync();
            }
            catch (RequestFailedException exception)
            {
                if (exception.Status != (int)HttpStatusCode.NotFound)
                    throw;
            }
        }
    }
}