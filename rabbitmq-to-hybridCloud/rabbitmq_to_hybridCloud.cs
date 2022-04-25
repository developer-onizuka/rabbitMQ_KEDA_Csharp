using System;
using System.Collections.Generic;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace rabbitmq_to_hybridCloud
{
    public class rabbitmq_to_hybridCloud
    {
        private IMongoCollection<EmployeeEntity> collection;
        private MongoClient client;

	public void InsertEntity(EmployeeEntity entity)
	{
            IMongoDatabase db = client.GetDatabase("mydb");
            collection = db.GetCollection<EmployeeEntity>("Employee");
            collection.InsertOne(entity); 
            // {"EmployeeID":1,"FirstName":"Yukichi","LastName":"Fukuzawa"}
            // {"EmployeeID":2,"FirstName":"Shoin","LastName":"Yoshida"}
	}

        public void WriteMongoDB(string primary, string secondary, EmployeeEntity entity)
        {
	    try
	    {
                client = new MongoClient(primary);
 	        InsertEntity(entity);
	    }
	    catch (Exception e)
	    {
		//If you would like to write messages to Secondary DB, remove comments of 2 lines below:
                //client = new MongoClient(secondary);
		//InsertEntity(entity);

		//The logic to leave messages on dead letter queue thru DLX, instead of DB write.
		RabbitmqDLX dl = new RabbitmqDLX();
		dl.rabbitmq_ipaddr = System.Environment.GetEnvironmentVariable("RabbitMQ_IPaddress");
                dl.rabbitmq_dlx = System.Environment.GetEnvironmentVariable("RabbitMQ_DLX");
		dl.SendMessages(entity);
	    }
        }

        [FunctionName("rabbitmq_to_hybridCloud")]
        public void Run(
          [RabbitMQTrigger("employee-queue", ConnectionStringSetting = "RabbitMQConnection")] EmployeeEntity emp,
          ILogger log)
        {
	    string primaryConnection;
	    string secondaryConnection;

	    if (emp.EmployeeID % 2 == 0)
	    {
	        primaryConnection = System.Environment.GetEnvironmentVariable("PrimaryConnection");
	        secondaryConnection = System.Environment.GetEnvironmentVariable("SecondaryConnection");
	    }

	    else
	    {
	        secondaryConnection = System.Environment.GetEnvironmentVariable("PrimaryConnection");
	        primaryConnection = System.Environment.GetEnvironmentVariable("SecondaryConnection");
	    }

	    WriteMongoDB(primaryConnection, secondaryConnection, emp);
	}
    }
}
