using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;

namespace DurableEntitiesVoting.Util
{
    public static class KeepAlive
    {
        [FunctionName("KeepAlive")]
        public static Task Run(
            [TimerTrigger("*/15 * * * * *")]TimerInfo myTimer,
            [OrchestrationClient] IDurableOrchestrationClient client)
        {
            return client.SignalEntityAsync(HttpFunctions.DefaultEntityId, "noop", "");
        }
    }
}
