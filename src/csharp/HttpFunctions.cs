using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using System;

namespace DurableEntitiesVoting
{
    public static class HttpFunctions
    {
        private static string[] animals = new string[] { "dog", "rabbit", "horse" };

        [FunctionName(nameof(IncrementVote))]
        public static Task IncrementVote(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "votes/{choice}/incr")] HttpRequest req,
            [OrchestrationClient] IDurableOrchestrationClient client,
            string choice,
            ILogger log)
        {
            if (!animals.Contains(choice))
            {
                throw new ArgumentException($"{choice} is not a valid animal");
            }

            return client.SignalEntityAsync(new EntityId("Choice", choice), "incr");
        }

        [FunctionName(nameof(GetVotes))]
        public static async Task<IActionResult> GetVotes(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "votes")] HttpRequest req,
            [OrchestrationClient] IDurableOrchestrationClient client,
            ILogger log)
        {
            var results = new
            {
                dog = (await client.ReadEntityStateAsync<int>(new EntityId("Choice", "dog"))).EntityState,
                rabbit = (await client.ReadEntityStateAsync<int>(new EntityId("Choice", "rabbit"))).EntityState,
                horse = (await client.ReadEntityStateAsync<int>(new EntityId("Choice", "horse"))).EntityState
            };
            return new OkObjectResult(results);
        }

        [FunctionName(nameof(ClearVotes))]
        public static async Task ClearVotes(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "votes")] HttpRequest req,
            [OrchestrationClient] IDurableOrchestrationClient client,
            ILogger log)
        {
            await Task.WhenAll(new Task[]
            {
                client.SignalEntityAsync(new EntityId("Choice", "dog"), "clear"),
                client.SignalEntityAsync(new EntityId("Choice", "rabbit"), "clear"),
                client.SignalEntityAsync(new EntityId("Choice", "horse"), "clear")
            });
        }
    }
}
