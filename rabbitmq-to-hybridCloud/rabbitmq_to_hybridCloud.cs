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

        public void WriteMongoDB(string connection_1st, string connection_2nd, EmployeeEntity entity)
        {
            MongoClient client;

	    try
	    {
                client = new MongoClient(connection_1st);
	    }
	    catch (Exception e)
	    {
                client = new MongoClient(connection_2nd);
	    }

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
	    string connectionString_1st;
	    string connectionString_2nd;

	    if (emp.EmployeeID % 2 == 0)
	    {
	        connectionString_1st = System.Environment.GetEnvironmentVariable("MongoDBConnection");
	        connectionString_2nd = System.Environment.GetEnvironmentVariable("CosmosDBConnection");
	    }

	    else
	    {
	        connectionString_1st = System.Environment.GetEnvironmentVariable("CosmosDBConnection");
	        connectionString_2nd = System.Environment.GetEnvironmentVariable("MongoDBConnection");
	    }

	    WriteMongoDB(connectionString_1st, connectionString_2nd, emp);
	}
    }
}
