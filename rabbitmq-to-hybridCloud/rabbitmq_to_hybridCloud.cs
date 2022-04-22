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

        public void WriteMongoDB(string connection, EmployeeEntity entity)
        {
            MongoClient client = new MongoClient(connection);
            //MongoClient client = new MongoClient("mongodb://mongo-svc:27017");

            IMongoDatabase db = client.GetDatabase("mydb");
            collection = db.GetCollection<EmployeeEntity>("Employee");
            collection.InsertOne(entity); 
            // {"EmployeeID":1,"FirstName":"Yukichi","LastName":"Fukuzawa"}
            // {"EmployeeID":2,"FirstName":"Shoin","LastName":"Yoshida"}
        }

        [FunctionName("rabbitmq_to_hybridCloud")]
        public void Run(
          [RabbitMQTrigger("employee-queue", ConnectionStringSetting = "RabbitMQConnection")] EmployeeEntity emp,
          ILogger log)
        {
	    string connectionString;

	    if (emp.EmployeeID % 2 == 0)
	    {
	        connectionString = System.Environment.GetEnvironmentVariable("MongoDBConnection");
	    }

	    else
	    {
	        connectionString = System.Environment.GetEnvironmentVariable("CosmosDBConnection");
	    }

	    WriteMongoDB(connectionString, emp);

	}
    }
}
