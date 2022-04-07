# rabbitMQ_KEDA_Csharp

# Requisites
> https://github.com/developer-onizuka/rabbitMQ_KEDA#0-install-keda <br>
> https://github.com/developer-onizuka/rabbitMQ_KEDA#1-install-rabbitmq-with-helm
> https://github.com/developer-onizuka/AzureFunctionsOnKubernetesWithKEDA#1-run-the-registry-somewhere
> https://github.com/developer-onizuka/AzureFunctionsOnKubernetesWithKEDA#2-install-azure-functions-core-tools-in-kubernetes-master-node

# 1. Install dotnet-sdk-6.0
```
wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
dpkg -i packages-microsoft-prod.deb 
sudo apt-get update
sudo apt-get install -y apt-transport-https dotnet-sdk-6.0
```

# 2. Deploy a RabbitMQ consumer with Azure Functions

```
$ mkdir rabbitmq-consumer

$ cd rabbitmq-consumer

$ func init --docker
Select a number for worker runtime:
1. dotnet
2. dotnet (isolated process)
3. node
4. python
5. powershell
6. custom
Choose option: 1
dotnet

Writing /home/vagrant/rabbitmq-consumer/.vscode/extensions.json
Writing Dockerfile
Writing .dockerignore

$ func new
Select a number for template:
1. QueueTrigger
2. HttpTrigger
3. BlobTrigger
4. TimerTrigger
5. DurableFunctionsOrchestration
6. SendGrid
7. EventHubTrigger
8. ServiceBusQueueTrigger
9. ServiceBusTopicTrigger
10. EventGridTrigger
11. CosmosDBTrigger
12. IotHubTrigger
Choose option: 1
Function name: rabbitmq-consumer
rabbitmq-consumer

The function "rabbitmq-consumer" was created successfully from the "QueueTrigger" template.

$ dotnet add package Microsoft.Azure.WebJobs.Extensions.RabbitMQ

$ dotnet add package Microsoft.Azure.WebJobs.Extensions.Storage
```
```
$ vi local.settings.json 
```
```
{
    "IsEncrypted": false,
    "Values": {
        "AzureWebJobsStorage": "UseDevelopmentStorage=true",
        "FUNCTIONS_WORKER_RUNTIME": "dotnet",
	"RabbitMQConnection": "amqp://user:PASSWORD@rabbitmq.default.svc.cluster.local:5672"
    }
}
```
```
$ vi rabbitmq_consumer.cs
```
```
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
```
```
$ func kubernetes deploy --name rabbitmq-consumer --registry 192.168.1.5:5000
Running 'docker build -t 192.168.1.5:5000/rabbitmq-consumer:latest /home/vagrant/rabbitmq-consumer'...................done
secret/rabbitmq-consumer created
deployment.apps/rabbitmq-consumer created
scaledobject.keda.sh/rabbitmq-consumer created
```

# 3. Check ScaledObjects
```
$ kubectl get scaledobjects
NAME                SCALETARGETKIND      SCALETARGETNAME     MIN   MAX   TRIGGERS   AUTHENTICATION   READY   ACTIVE   FALLBACK   AGE
rabbitmq-consumer   apps/v1.Deployment   rabbitmq-consumer               rabbitmq                    True    False    Unknown    55s
```

# 4. Check the consumer
```
$ kubectl get deploy
NAME                READY   UP-TO-DATE   AVAILABLE   AGE
rabbitmq-consumer   0/0     0            0           44s
```

# 5. Publish messages
https://github.com/developer-onizuka/rabbitMQ_KEDA#3-publish-messages-to-the-queue


