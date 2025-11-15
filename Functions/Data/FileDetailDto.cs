using System;

namespace AzureRetailHub.Functions.Data
{
    // DTO for sending file details to the web app
    public class FileDetailDto
    {
        public string Name { get; set; } = "";
        public long Size { get; set; }
        public DateTimeOffset UploadedOn { get; set; }
    }
}