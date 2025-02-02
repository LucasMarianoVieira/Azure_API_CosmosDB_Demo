using System.IO.Compression;
using System.Reflection.Metadata;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace fn_post_data_storage
{
    public class fnPostDataStorage
    {
        private readonly ILogger<fnPostDataStorage> _logger;

        public fnPostDataStorage(ILogger<fnPostDataStorage> logger)
        {
            _logger = logger;
        }

        [Function("DataStorage")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
        {
            _logger.LogInformation("Processing image on storage");

            try
            {
                if(!req.Headers.TryGetValue("file-type", out var fileTypeHeader))
                {
                    return new BadRequestObjectResult("Header 'file-type' is needed!");
                }

                var fileType = fileTypeHeader.ToString();

                var form = await req.ReadFormAsync();
                // Request the file sent
                var file = form.Files["file"];

                if(file == null || file.Length == 0) 
                {
                    return new BadRequestObjectResult("File wasn't sent!");
                }

                string? connection_string = Environment.GetEnvironmentVariable("AzureWebJobsStorage");

                if(connection_string == String.Empty) {
                    _logger.LogInformation("Connection String wasn't set on environmental variable!");
                    return new BadRequestObjectResult("Bad Server Configuration");
                }

                string container_name = fileType;

                BlobClient blob_client = new BlobClient(connection_string, container_name, file.FileName);
                BlobContainerClient container_client = new BlobContainerClient(connection_string, container_name);

                await container_client.CreateIfNotExistsAsync();
                await container_client.SetAccessPolicyAsync(PublicAccessType.BlobContainer);

                string blob_name = file.FileName;
                var blob = container_client.GetBlobClient(blob_name);

                using (var stream = file.OpenReadStream())
                {
                    await blob.UploadAsync(stream, true);
                }

                _logger.LogInformation($"File {file.FileName} stored successfully");

                return new OkObjectResult( new
                {
                    message = $"File {file.FileName} stored successfully",
                    blob_uri = blob.Uri,
                });
            } catch (Exception)
            {
                return new BadRequestObjectResult("Server Failure!");
            }
        }
    }
}
