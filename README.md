# rabbitMQ_KEDA_Csharp

```
wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
dpkg -i packages-microsoft-prod.deb 
sudo apt-get update
sudo apt-get install -y apt-transport-https dotnet-sdk-6.0

mkdir myfunction
cd myfunction

func init --docker
func new
dotnet add package Microsoft.Azure.WebJobs.Extensions.RabbitMQ
dotnet add package Microsoft.Azure.WebJobs.Extensions.Storage


# cat local.settings.json 
{
    "IsEncrypted": false,
    "Values": {
        "AzureWebJobsStorage": "UseDevelopmentStorage=true",
        "FUNCTIONS_WORKER_RUNTIME": "dotnet",
	"RabbitMQ": "amqp://user:PASSWORD@rabbitmq.default.svc.cluster.local:5672"
    }
}

# cat myfunc.cs 
using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;


namespace myfunction
{
    public class myfunc
    {
        [FunctionName("myfunc")]
        public void Run([QueueTrigger("hello", Connection = "RabbitMQ")]string myQueueItem, ILogger log)
        {
            log.LogInformation($"C# Queue trigger function processed: {myQueueItem}");
        }
    }
}


docker build -t myfunc:v1 .
func kubernetes deploy --name myfunc --registry 192.168.1.5:5000


```
