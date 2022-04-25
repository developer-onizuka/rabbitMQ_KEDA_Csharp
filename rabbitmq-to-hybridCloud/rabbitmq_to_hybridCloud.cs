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
    public class EmployeeEntity
    {
	[BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public int EmployeeID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

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
                client = new MongoClient(secondary);
		InsertEntity(entity);
	    }
        }

        [FunctionName("rabbitmq_to_hybridCloud")]
        public void Run(
          [RabbitMQTrigger("employee-queue", ConnectionStringSetting = "RabbitMQConnection")] EmployeeEntity emp,
          ILogger log)
        {
	    string connMongoDB;
	    string connCosmosDB;

	    if (emp.EmployeeID % 2 == 0)
	    {
	        connMongoDB = System.Environment.GetEnvironmentVariable("PrimaryConnection");
	        connCosmosDB = System.Environment.GetEnvironmentVariable("SecondaryConnection");
	    }

	    else
	    {
	        connCosmosDB = System.Environment.GetEnvironmentVariable("PrimaryConnection");
	        connMongoDB = System.Environment.GetEnvironmentVariable("SecondaryConnection");
	    }

	    WriteMongoDB(connMongoDB, connCosmosDB, emp);
	}
    }
}
