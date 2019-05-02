using System.Collections.Generic;
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
        [FunctionName(nameof(ProcessOperation))]
        public static async Task ProcessOperation(
            [EntityTrigger(EntityName = "Choice")] IDurableEntityContext context,
            [SignalR(HubName = "votes")] IAsyncCollector<SignalRMessage> signalRMessages,
            ILogger log)
        {
            var votes = context.GetState<int>();

            switch (context.OperationName)
            {
                case "incr":
                    votes += 1;
                    break;
                case "clear":
                    votes = 0;
                    break;
            }

            log.LogInformation($"{context.Key} {votes}");
            context.SetState(votes);

            await signalRMessages.AddAsync(new SignalRMessage
            {
                Target = "votesUpdated",
                Arguments = new object[]
                {
                    new { animal = context.Key, votes }
                }
            });
        }
    }
}