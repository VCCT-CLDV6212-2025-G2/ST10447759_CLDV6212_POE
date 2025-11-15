/*
 * This is the FINAL, corrected ContractsFunctions file.
 * It correctly injects StorageOptions and passes the
 * FileShareName to the FileStorageService.
 */
using AzureRetailHub.Functions.Data;
using AzureRetailHub.Functions.Services;
using AzureRetailHub.Functions.Settings; // <-- ADD THIS
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options; // <-- ADD THIS
using System.Collections.Generic;
using System.IO;
using System.Linq; // <-- ADD THIS
using System.Threading.Tasks;

namespace AzureRetailHub.Functions.Functions
{
    public class ContractsFunctions
    {
        private readonly FileStorageService _files;
        private readonly ILogger<ContractsFunctions> _logger;
        private readonly StorageOptions _opts; // <-- ADD THIS

        // Inject IOptions<StorageOptions>
        public ContractsFunctions(
            FileStorageService files,
            ILogger<ContractsFunctions> logger,
            IOptions<StorageOptions> options) // <-- ADD THIS
        {
            _files = files;
            _logger = logger;
            _opts = options.Value; // <-- ADD THIS
        }

        [Function("ContractList")]
        public async Task<IActionResult> List(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "contracts")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request to list contracts.");

            var fileList = new List<FileDetailDto>();

            // --- THIS IS THE FIX ---
            // We must pass the shareName, as you correctly identified.
            //var files = await _files.ListFilesAsync(_opts.FileShareName);

            // The 'ListFilesAsync' in your original service returns a type
            // that we need to loop through.
            await foreach (var item in _files.ListFilesAsync(_opts.FileShareName))
            {
                // We must get properties for each file
                var fileClient = await _files.GetFileClientAsync(_opts.FileShareName, item.Name);
                var properties = await fileClient.GetPropertiesAsync();

                fileList.Add(new FileDetailDto
                {
                    Name = item.Name,
                    Size = properties.Value.ContentLength,
                    UploadedOn = properties.Value.LastModified
                });
            }

            return new OkObjectResult(fileList);
        }

        [Function("ContractUpload")]
        public async Task<IActionResult> Upload(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "contracts")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request to upload a contract.");

            var formdata = await req.ReadFormAsync();
            var file = req.Form.Files["file"];

            if (file == null || file.Length == 0)
            {
                return new BadRequestObjectResult("No file received.");
            }

            await using var stream = file.OpenReadStream();

            // --- THIS IS THE FIX ---
            // We must pass the shareName here as well.
            await _files.UploadFileAsync(_opts.FileShareName, file.FileName, stream);

            return new OkObjectResult(new { message = "File uploaded successfully." });
        }
    }
}