# rabbitMQ_KEDA_Csharp
Azure Functions is a service on Azure that consists of a runtime part that executes functions and a part that controls scaling, of which the latter scaling control can be replaced with Kubernetes and KEDA. <br>
Azure Functions can run on Kubernetes with KEDA, so you can use Azure Functions outside of your Azure platform, such as your on-premises environment.

# 0. Prerequisites
# 0-1. Install KEDA with helm
> https://github.com/developer-onizuka/rabbitMQ_KEDA#0-install-keda <br>

# 0-2. Install RabbitMQ with helm
> https://github.com/developer-onizuka/rabbitMQ_KEDA#1-install-rabbitmq-with-helm <br>

# 0-3. Install Azure-functions-core-tools
> https://github.com/developer-onizuka/AzureFunctionsOnKubernetesWithKEDA#1-run-the-registry-somewhere <br>
> https://github.com/developer-onizuka/AzureFunctionsOnKubernetesWithKEDA#2-install-azure-functions-core-tools-in-kubernetes-master-node <br>

# 0-4. Create the private registry
> https://github.com/developer-onizuka/AzureFunctionsOnKubernetesWithKEDA#1-run-the-registry-somewhere <br>

# 0-5. Create StorageClass for MongoDB's pvc
This step is needed to Write RabbitMQ's messages to MongoDB in #8.<br>
> https://github.com/developer-onizuka/persistentVolume-CSI <br>

<br>

# 1. Install dotnet-sdk-6.0
```
wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb 
sudo apt-get update
sudo apt-get install -y apt-transport-https dotnet-sdk-6.0
```

# 2. Create a sample of Class library and edit it

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
https://docs.microsoft.com/en-us/azure/azure-functions/functions-develop-local#local-settings-file
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

# 3. Deploy a RabbitMQ consumer with Azure Functions
```
$ func kubernetes deploy --name rabbitmq-consumer --registry 192.168.1.5:5000 --max-replicas 16 --polling-interval 5 --cooldown-period 30
Running 'docker build -t 192.168.1.5:5000/rabbitmq-consumer:latest /home/vagrant/rabbitmq-consumer'..done
secret/rabbitmq-consumer created
deployment.apps/rabbitmq-consumer created
scaledobject.keda.sh/rabbitmq-consumer created
```

# 4. Check ScaledObjects
```
$ kubectl get scaledobjects
NAME                SCALETARGETKIND      SCALETARGETNAME     MIN   MAX   TRIGGERS   AUTHENTICATION   READY   ACTIVE   FALLBACK   AGE
rabbitmq-consumer   apps/v1.Deployment   rabbitmq-consumer               rabbitmq                    True    False    Unknown    55s
```

# 5. Check the consumer's scalability
You should see rabbitmq-consumer deployment with 0 pods as there currently aren't any queue messages.
```
# kubectl get hpa
NAME                         REFERENCE                      TARGETS              MINPODS   MAXPODS   REPLICAS   AGE
keda-hpa-rabbitmq-consumer   Deployment/rabbitmq-consumer   <unknown>/20 (avg)   1         16        0          15m

$ kubectl get deploy
NAME                READY   UP-TO-DATE   AVAILABLE   AGE
rabbitmq-consumer   0/0     0            0           44s
```
You might see the following instead above.
```
$ kubectl get deploy
NAME                READY   UP-TO-DATE   AVAILABLE   AGE
rabbitmq-consumer   1/1     1            1           44s
```

# 6. Publish a large number of messages
https://github.com/developer-onizuka/rabbitMQ_KEDA#3-publish-messages-to-the-queue
```
$ git clone https://github.com/kedacore/sample-go-rabbitmq

$ cd sample-go-rabbitmq

$ kubectl apply -f deploy/deploy-publisher-job.yaml
job.batch/rabbitmq-publish created

$ kubectl exec -it rabbitmq-0 -- rabbitmqctl list_queues
Timeout: 60.0 seconds ...
Listing queues for vhost / ...
name	messages
hello	30000
```

# 7. Automatic Scale Out of KEDA
```
kubectl get deploy -w
NAME                READY   UP-TO-DATE   AVAILABLE   AGE
rabbitmq-consumer   0/0     0            0           3s
rabbitmq-consumer   0/1     0            0           31s
rabbitmq-consumer   0/1     0            0           31s
rabbitmq-consumer   0/1     0            0           31s
rabbitmq-consumer   0/1     1            0           31s
rabbitmq-consumer   1/1     1            1           41s
rabbitmq-consumer   1/4     1            1           46s
rabbitmq-consumer   1/4     1            1           46s
rabbitmq-consumer   1/4     1            1           46s
rabbitmq-consumer   1/4     4            1           46s
rabbitmq-consumer   2/4     4            2           56s
rabbitmq-consumer   3/4     4            3           57s
rabbitmq-consumer   4/4     4            4           58s
rabbitmq-consumer   4/8     4            4           61s
rabbitmq-consumer   4/8     4            4           61s
rabbitmq-consumer   4/8     4            4           61s
rabbitmq-consumer   4/8     7            4           61s
rabbitmq-consumer   4/8     8            4           61s
rabbitmq-consumer   5/8     8            5           71s
rabbitmq-consumer   6/8     8            6           72s
rabbitmq-consumer   7/8     8            7           72s
rabbitmq-consumer   8/8     8            8           72s
rabbitmq-consumer   8/16    8            8           76s
rabbitmq-consumer   8/16    8            8           76s
rabbitmq-consumer   8/16    8            8           76s
rabbitmq-consumer   8/16    16           8           76s
rabbitmq-consumer   8/0     16           8           86s
rabbitmq-consumer   8/0     16           8           86s
rabbitmq-consumer   8/0     16           8           86s
rabbitmq-consumer   0/0     0            0           86s
```

<img src="https://github.com/developer-onizuka/rabbitMQ_KEDA_Csharp/blob/main/rabbitMQ1.png" width="720">

# 7-1. Clean up
```
$ func kubernetes delete --name rabbitmq-consumer --registry 192.168.1.5:5000 --max-replicas 16 --polling-interval 5 --cooldown-period 30
```

# 8. Write RabbitMQ's messages to MongoDB
I already uploaded a C# code on this repo. But, it is almost same as above to create it from scratch. <br>
If you are not familier with MongoDB, the see also https://github.com/developer-onizuka/mvc_containers2. <br>
C# is a very convenient object oriented language to handle MongoDB.

# 8-1. Deploy the Function of rabbitmq-to-mongodb to AzureFunctions on Onprem-Kubernetes
```
$ git clone https://github.com/developer-onizuka/rabbitMQ_KEDA_Csharp

$ cd rabbitMQ_KEDA_Csharp/rabbitmq-to-mongodb
```
- Prepare the MongoDB in the Kubernetes Cluster.
```
$ kubectl apply -f mongodb.yaml

$ cat <<EOF > local.settings.json
{
    "IsEncrypted": false,
    "Values": {
        "AzureWebJobsStorage": "UseDevelopmentStorage=true",
        "FUNCTIONS_WORKER_RUNTIME": "dotnet",
        "RabbitMQConnection": "amqp://user:PASSWORD@rabbitmq.default.svc.cluster.local:5672",
        "PrimaryConnection": "mongodb://mongo-svc:27017"
    }
}
EOF
```
- The case of mongoDB replica-set:<br>
> https://github.com/developer-onizuka/mongoDB_replicaSet#1-create-replicaset-among-mongo-0-mongo-1-and-mongo-2-with-mongodb-opsmanager.<br>
> https://docs.microsoft.com/en-us/azure/azure-functions/functions-develop-local#local-settings-file
```
$ cat <<EOF > local.settings.json
{
    "IsEncrypted": false,
    "Values": {
        "AzureWebJobsStorage": "UseDevelopmentStorage=true",
        "FUNCTIONS_WORKER_RUNTIME": "dotnet",
        "RabbitMQConnection": "amqp://user:PASSWORD@rabbitmq.default.svc.cluster.local:5672",
        "PrimaryConnection": "mongodb://mongo-0:27017,mongo-1:27017,mongo-2:27017/?replicaSet=myReplicaSet"
    }
}
EOF
```
- The case of mongoDB in Onprem and Azure Cosmos DB API for MongoDB:<br>
```
$ cat <<EOF > local.settings.json
{
    "IsEncrypted": false,
    "Values": {
        "AzureWebJobsStorage": "UseDevelopmentStorage=true",
        "FUNCTIONS_WORKER_RUNTIME": "dotnet",
        "RabbitMQConnection": "amqp://user:PASSWORD@rabbitmq.default.svc.cluster.local:5672",
        "PrimaryConnection": "mongodb://mongo-0:27017,mongo-1:27017,mongo-2:27017/?replicaSet=myReplicaSet",
        "SecondaryConnection": "mongodb://myfirstcosmosdb:XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX"
    }
}
EOF
```

- Let's begin deploying a function in the Kubernetes Cluster.
```
$ func kubernetes deploy --name rabbitmq-to-mongodb --registry 192.168.1.5:5000 --max-replicas 16 --polling-interval 5 --cooldown-period 30
Running 'docker build -t 192.168.1.5:5000/rabbitmq-to-mongodb:latest /home/vagrant/rabbitMQ_KEDA_Csharp/rabbitmq-to-mongodb'..........................done
secret/rabbitmq-to-mongodb created
deployment.apps/rabbitmq-to-mongodb created
scaledobject.keda.sh/rabbitmq-to-mongodb created

$ kubectl get deploy
NAME                  READY   UP-TO-DATE   AVAILABLE   AGE
mongo-test            1/1     1            1           42m
rabbitmq-to-mongodb   0/0     0            0           7m5s
```

- For compiled languages (C# or Go etc), the function.json file is generated automatically from annotations in your code. It defines the function's trigger, bindings, and other configuration settings.<br>
> https://docs.microsoft.com/en-us/azure/azure-functions/functions-dotnet-class-library?tabs=v2%2Ccmd#functions-class-library-project
```
# cd /home/site/wwwroot/rabbitmq_to_mongodb
# cat function.json 
{
  "generatedBy": "Microsoft.NET.Sdk.Functions.Generator-4.0.1",
  "configurationSource": "attributes",
  "bindings": [
    {
      "type": "rabbitMQTrigger",
      "connectionStringSetting": "RabbitMQConnection",
      "queueName": "employee-queue",
      "name": "emp"
    }
  ],
  "disabled": false,
  "scriptFile": "../bin/rabbitmq-to-mongodb.dll",
  "entryPoint": "rabbitmq_to_mongodb.rabbitmq_to_mongodb.Run"
```


# 8-2. Publish some Messages as a test
You might use the Json below:
```
{"EmployeeID":1,"FirstName":"Yukichi","LastName":"Fukuzawa"}
{"EmployeeID":2,"FirstName":"Shoin","LastName":"Yoshida"}
```

# 8-3. Check if the documents are written correctly
```
$ kubectl exec -it mongo-test-dd8d57b87-4ffj7 -- /bin/bash
root@mongo-test-dd8d57b87-4ffj7:/# mongo

> show dbs
admin   0.000GB
config  0.000GB
local   0.000GB
mydb    0.000GB

> use mydb
switched to db mydb

> show collections
Employee

> db.Employee.find()
{ "_id" : ObjectId("62500698ec309b7f992ec51b"), "EmployeeID" : 1, "FirstName" : "Yukichi", "LastName" : "Fukuzawa" }
{ "_id" : ObjectId("6250096dcc145fdac5410531"), "EmployeeID" : 2, "FirstName" : "Shoin", "LastName" : "Yoshida" }
```

# 8-4. Publish a large number of messages
There is a C# code which publish messages controlled with  environment below in the rabbitMQ_KEDA_Csharp/send-to-rabbitmq directory.<br>
Use it if you like.
```
export RABBITMQ_IPADDR="192.168.33.220"
export RABBITMQ_QUEUE="employee-queue"
export RABBITMQ_MESSAGECOUNT="100000"
```
```
$ cd rabbitMQ_KEDA_Csharp/send-to-rabbitmq
$ dotnet run
 [x] Sent {"EmployeeID":1,"FirstName":"xxxxx","LastName":"xxxxx"}
 ...
 [x] Sent {"EmployeeID":99998,"FirstName":"xxxxx","LastName":"xxxxx"}
 [x] Sent {"EmployeeID":99999,"FirstName":"xxxxx","LastName":"xxxxx"}
 Press [enter] to exit.
```

Confirm the count of record in MongoDB collection.
```
> db.Employee.find().count()
100000
```

# 9. Store messages as dead letters if it fails
# 9-1. Create Queue with message-TTL
<img src="https://github.com/developer-onizuka/rabbitMQ_KEDA_Csharp/blob/main/rabbitMQ-dlx1.png" width="480"> <br>

# 9-2. Create Failure-Queue
<img src="https://github.com/developer-onizuka/rabbitMQ_KEDA_Csharp/blob/main/rabbitMQ-dlx2.png" width="480"> <br>

# 9-3. Create DLXs and Bind it for each queue
# Employee-queue
<img src="https://github.com/developer-onizuka/rabbitMQ_KEDA_Csharp/blob/main/rabbitMQ-dlx3.png" width="480"> <br>
<img src="https://github.com/developer-onizuka/rabbitMQ_KEDA_Csharp/blob/main/rabbitMQ-dlx5.png" width="480"> <br>

# Employee-queue-failure
<img src="https://github.com/developer-onizuka/rabbitMQ_KEDA_Csharp/blob/main/rabbitMQ-dlx4.png" width="480"> <br>
<img src="https://github.com/developer-onizuka/rabbitMQ_KEDA_Csharp/blob/main/rabbitMQ-dlx6.png" width="480"> <br>

# 9-4. Publish a large number of messages via DLX
```
export RABBITMQ_IPADDR="192.168.33.220"
export RABBITMQ_DLX="dlx.employee-queue"
export RABBITMQ_MESSAGECOUNT="20000"
```
```
$ cd rabbitMQ_KEDA_Csharp/send-to-rabbitmqDLX
$ dotnet run
```

# 9-5. Consume them thru the DLX aware App
If the App fails by some reasons, the messages will be left on the dead letter queue.
```
$ cd rabbitMQ_KEDA_Csharp/rabbitmq-to-hybridCloud
$ cat <<EOF > local.settings.json
{
    "IsEncrypted": false,
    "Values": {
        "AzureWebJobsStorage": "UseDevelopmentStorage=true",
        "FUNCTIONS_WORKER_RUNTIME": "dotnet",
        "RabbitMQConnection": "amqp://user:PASSWORD@rabbitmq.default.svc.cluster.local:5672",
	"RabbitMQ_IPaddress": "rabbitmq",
	"RabbitMQ_DLX": "dlx.employee-queue-failure",
        "PrimaryConnection": "mongodb://mongo-0:27017,mongo-1:27017,mongo-2:27017/?replicaSet=myReplicaSet",
        "SecondaryConnection": "mongodb://myfirstcosmosdb:XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX"
    }
}
```
```
$ func kubernetes delete --name rabbitmq-to-hybridcloud --registry 192.168.1.5:5000 --max-replicas 16 --polling-interval 5 --cooldown-period 30
```

# 9-6. Result
You can find 10000 of 20000 messages are stored in Failure Queue because of SecondaryConnection in local.settings.json is not existing.<br>
<img src="https://github.com/developer-onizuka/rabbitMQ_KEDA_Csharp/blob/main/rabbitMQ5.png" width="640"> <br>


# X. Clean up
```
$ func kubernetes delete --name rabbitmq-to-mongodb --registry 192.168.1.5:5000 --max-replicas 16 --polling-interval 5 --cooldown-period 30
$ helm uninstall rabbitmq
```


