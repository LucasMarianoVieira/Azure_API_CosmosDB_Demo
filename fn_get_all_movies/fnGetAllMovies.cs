using System.Threading.Tasks;
using fn_get_movie_detail;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace Get_All_Movies
{
    public class fn_get_all_movies
    {
        private readonly ILogger<fn_get_all_movies> _logger;
        private readonly CosmosClient _cosmos_client;

        public fn_get_all_movies(ILogger<fn_get_all_movies> logger, CosmosClient cosmos_client)
        {
            _logger = logger;
            _cosmos_client = cosmos_client;
        }

        [Function("get_all_movies")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData req)
        {
            _logger.LogInformation("Requesting Movie information...");
            var container = _cosmos_client.GetContainer("flixdemoDB", "movies");

            var query = $"SELECT c.id, c.title, c.year, c.video, c.thumb FROM c";

            var query_definition = new QueryDefinition(query);
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
            await response_message.WriteAsJsonAsync(results);
            return response_message;
        }
    }
}
