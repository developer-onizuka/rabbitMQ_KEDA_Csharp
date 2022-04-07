# rabbitMQ_KEDA_Csharp

```
wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
dpkg -i packages-microsoft-prod.deb 
sudo apt-get update
sudo apt-get install -y apt-transport-https dotnet-sdk-6.0

mkdir myfunction
cd myfunction

func init
func new
dotnet add package Microsoft.Azure.WebJobs.Extensions.RabbitMQ
dotnet add package Microsoft.Azure.WebJobs.Extensions.Storage

cat myfuncapp.cs
using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Newtonsoft.Json;
using RabbitMQ.Client.Events;

cat local.settings.json 
{
    "IsEncrypted": false,
    "Values": {
        "AzureWebJobsStorage": "UseDevelopmentStorage=true",
        "FUNCTIONS_WORKER_RUNTIME": "dotnet",
	"queueconnection" : "amqp://user:PASSWORD@rabbitmq.default.svc.cluster.local:5672"
    }
}


namespace myfunction
{
    public class myfuncapp
    {
        [FunctionName("myfuncapp")]
        public void Run([QueueTrigger("hello", Connection = "queueconnection")]string myQueueItem, ILogger log)
        {
            log.LogInformation($"C# Queue trigger function processed: {myQueueItem}");
        }
    }
}



func start

```
