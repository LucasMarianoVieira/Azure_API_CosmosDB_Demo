using System.Threading.Tasks;
using fn_get_movie_detail;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace get_movie_detail
{
    public class fnGetMovieDetail
    {
        private readonly ILogger<fnGetMovieDetail> _logger;
        private readonly CosmosClient _cosmos_client;

        public fnGetMovieDetail(ILogger<fnGetMovieDetail> logger, CosmosClient cosmos_client)
        {
            _logger = logger;
            _cosmos_client = cosmos_client;
        }

        [Function("detail")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData req)
        {
            _logger.LogInformation("Requesting Movie information...");
            var container = _cosmos_client.GetContainer("flixdemoDB", "movies");

            var id = req.Query["id"];
            var query = $"SELECT c.id, c.title, c.year, c.video, c.thumb FROM c WHERE c.id = @id";

            var query_definition = new QueryDefinition(query).WithParameter("@id", id);
            var result  = container.GetItemQueryIterator<Movie_Result>(query_definition);
            var results  = new List<Movie_Result>();
            while (result.HasMoreResults)
            {
                foreach (var item in await result.ReadNextAsync())
                {
                    results.Add(item);
                }
            }

            var response_message = req.CreateResponse(System.Net.HttpStatusCode.OK);
            await response_message.WriteAsJsonAsync(results.FirstOrDefault());
            return response_message;
        }
    }
}
