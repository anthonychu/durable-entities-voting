using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace DurableEntitiesVoting
{
    public static class VotingEntity
    {
        public static string[] Animals = new string[] { "dog", "rabbit", "horse" };

        [FunctionName(nameof(ProcessOperation))]
        public static async Task ProcessOperation(
            [EntityTrigger(EntityName = "Votes")] IDurableEntityContext context,
            [SignalR(HubName = "votes")] IAsyncCollector<SignalRMessage> signalRMessages,
            ILogger log)
        {
            var votes = context.GetState<Dictionary<string, int>>() ?? InitializeVotes();
            var choice = context.GetInput<string>();

            switch (context.OperationName)
            {
                case "incr":
                    votes[choice] += 1;
                    await signalRMessages.AddAsync(BuildVotesUpdatedMessage(choice, votes[choice]));
                    break;
                case "clear":
                    votes = InitializeVotes();
                    await Task.WhenAll(Animals.Select(a => signalRMessages.AddAsync(BuildVotesUpdatedMessage(a, 0))));
                    break;
            }

            context.SetState(votes);
        }

        private static Dictionary<string, int> InitializeVotes()
        {
            return Animals.ToDictionary(a => a, _ => 0);
        }

        private static SignalRMessage BuildVotesUpdatedMessage(string choice, int votes)
        {
            return new SignalRMessage
            {
                Target = "votesUpdated",
                Arguments = new object[]
                {
                    new { animal = choice, votes }
                }
            };
        }
    }
}