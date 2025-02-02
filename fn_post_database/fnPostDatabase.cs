using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace fnPostDatabase
{
    public class fnPostDatabase
    {
        private readonly ILogger<fnPostDatabase> _logger;

        public fnPostDatabase(ILogger<fnPostDatabase> logger)
        {
            _logger = logger;
        }

        [Function("movie")]
        [CosmosDBOutput("%DatabaseName%", "movies", Connection = "CosmosDBConnection", CreateIfNotExists = true, PartitionKey = "title")]
        public async Task<object?> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
        {
            _logger.LogInformation("Movie saved!");

            Movie_Request movie = null;

            string content = await new StreamReader(req.Body).ReadToEndAsync();

            try
            {
                movie = JsonConvert.DeserializeObject<Movie_Request>(content);
            }
            catch (System.Exception ex)
            {
                return new BadRequestObjectResult("Error processing file: " + ex.Message);
            }

            return JsonConvert.SerializeObject(movie);
        }
    }
}
