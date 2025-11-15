using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;
using AzureRetailHub.Functions.Settings;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace AzureRetailHub.Functions.Services
{
    public class FileStorageService
    {
        private readonly ShareServiceClient _shareServiceClient;
        private readonly StorageOptions _opts;

        public FileStorageService(IOptions<StorageOptions> options)
        {
            _opts = options.Value;
            _shareServiceClient = new ShareServiceClient(_opts.ConnectionString);
        }

        // --- NEW HELPER ---
        public async Task<ShareClient> GetShareClientAsync(string shareName)
        {
            var client = _shareServiceClient.GetShareClient(shareName);
            await client.CreateIfNotExistsAsync();
            return client;
        }

        public async Task<ShareFileClient> GetFileClientAsync(string shareName, string fileName)
        {
            var share = await GetShareClientAsync(shareName);
            var directory = share.GetRootDirectoryClient();
            return directory.GetFileClient(fileName);
        }

        public async Task UploadFileAsync(string shareName, string fileName, Stream stream)
        {
            var file = await GetFileClientAsync(shareName, fileName);
            stream.Position = 0;
            await file.CreateAsync(stream.Length);
            await file.UploadRangeAsync(new Azure.HttpRange(0, stream.Length), stream);
        }

        public async Task<Stream> GetFileStreamAsync(string shareName, string fileName)
        {
            var file = await GetFileClientAsync(shareName, fileName);
            var download = await file.DownloadAsync();
            return download.Value.Content;
        }

        public async Task DeleteFileAsync(string shareName, string fileName)
        {
            var file = await GetFileClientAsync(shareName, fileName);
            await file.DeleteIfExistsAsync();
        }

        public async IAsyncEnumerable<ShareFileItem> ListFilesAsync(string shareName)
        {
            var share = await GetShareClientAsync(shareName);
            var directory = share.GetRootDirectoryClient();

            await foreach (var item in directory.GetFilesAndDirectoriesAsync())
            {
                if (item.IsDirectory) continue;
                yield return (ShareFileItem)item;
            }
        }
    }
}