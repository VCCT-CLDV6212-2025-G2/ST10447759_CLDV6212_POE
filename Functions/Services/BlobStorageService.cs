using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using AzureRetailHub.Functions.Settings; // <-- CHANGED
using Microsoft.Extensions.Options;
using System.IO;
using System.Threading.Tasks;

namespace AzureRetailHub.Functions.Services // <-- CHANGED
{
    public class BlobStorageService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly StorageOptions _opts;

        public BlobStorageService(IOptions<StorageOptions> options)
        {
            _opts = options.Value;
            _blobServiceClient = new BlobServiceClient(_opts.ConnectionString);
        }

        public async Task<BlobContainerClient> GetContainerClientAsync()
        {
            var client = _blobServiceClient.GetBlobContainerClient(_opts.BlobContainer);
            await client.CreateIfNotExistsAsync(PublicAccessType.Blob);
            return client;
        }

        public async Task<string> UploadFileAsync(Stream stream, string fileName, string contentType)
        {
            var container = await GetContainerClientAsync();
            var blob = container.GetBlobClient(fileName);

            stream.Position = 0;
            await blob.UploadAsync(stream, new BlobHttpHeaders { ContentType = contentType });

            return blob.Uri.ToString();
        }

        public async Task DeleteFileAsync(string fileName)
        {
            var container = await GetContainerClientAsync();
            var blob = container.GetBlobClient(fileName);
            await blob.DeleteIfExistsAsync();
        }
    }
}