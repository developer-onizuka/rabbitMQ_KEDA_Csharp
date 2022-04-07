using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace rabbitmq_consumer
{
    public class rabbitmq_consumer
    {
        [FunctionName("rabbitmq_consumer")]
        //public void Run([QueueTrigger("myqueue-items", Connection = "")]string myQueueItem, ILogger log)
	public void Run([RabbitMQTrigger("hello", ConnectionStringSetting = "RabbitMQConnection")] string myQueueItem, ILogger log)
        {
            log.LogInformation($"C# Queue trigger function processed: {myQueueItem}");
        }
    }
}
