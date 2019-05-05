using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace DurableEntitiesVoting
{
    public static class HttpFunctions
    {
        public static EntityId DefaultEntityId = new EntityId("Votes", "default");

        [FunctionName(nameof(IncrementVote))]
        public static Task IncrementVote(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "votes/{choice}/incr")] HttpRequest req,
            [OrchestrationClient] IDurableOrchestrationClient client,
            string choice,
            ILogger log)
        {
            if (!VotingEntity.Choices.Contains(choice))
            {
                throw new ArgumentException($"{choice} is not a valid choice");
            }

            return client.SignalEntityAsync(DefaultEntityId, "incr", choice);
        }

        [FunctionName(nameof(GetVotes))]
        public static async Task<Dictionary<string, int>> GetVotes(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "votes")] HttpRequest req,
            [OrchestrationClient] IDurableOrchestrationClient client,
            ILogger log)
        {
            return (await client.ReadEntityStateAsync<Dictionary<string, int>>(DefaultEntityId)).EntityState;
        }

        [FunctionName(nameof(ClearVotes))]
        public static Task ClearVotes(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "votes")] HttpRequest req,
            [OrchestrationClient] IDurableOrchestrationClient client,
            ILogger log)
        {
            return client.SignalEntityAsync(DefaultEntityId, "clear", "");
        }
    }
}
