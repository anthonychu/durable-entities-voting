using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Extensions.Logging;

namespace DurableEntitiesVoting
{
    public static class VotingEntity
    {
        public static string[] Choices = new string[] { "1", "2", "3" };

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
                    await signalRMessages.AddAsync(CreateVotesUpdatedMessage(choice, votes[choice]));
                    break;
                case "clear":
                    votes = InitializeVotes();
                    await Task.WhenAll(Choices.Select(a => signalRMessages.AddAsync(CreateVotesUpdatedMessage(a, 0))));
                    break;
            }

            context.SetState(votes);
        }

        private static Dictionary<string, int> InitializeVotes()
        {
            return Choices.ToDictionary(a => a, _ => 0);
        }

        private static SignalRMessage CreateVotesUpdatedMessage(string choice, int votes)
        {
            return new SignalRMessage
            {
                Target = "votesUpdated",
                Arguments = new object[] { new { choice, votes } }
            };
        }
    }
}