using System;
using System.Collections.Generic;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace rabbitmq_to_mongodb
{
    public class EmployeeEntity
    {
        public ObjectId Id { get; set; }
        public int EmployeeID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    public class rabbitmq_to_mongodb
    {
        private IMongoCollection<EmployeeEntity> collection;
        MongoClient client = new MongoClient("mongodb://mongo-svc:27017");

        [FunctionName("rabbitmq_to_mongodb")]
        public void Run(
          [RabbitMQTrigger("employee-queue", ConnectionStringSetting = "RabbitMQConnection")] EmployeeEntity emp,
          ILogger log)
        {
            IMongoDatabase db = client.GetDatabase("mydb");
            collection = db.GetCollection<EmployeeEntity>("Employee");
            collection.InsertOne(emp); 
	    // {"EmployeeID":1,"FirstName":"Yukichi","LastName":"Fukuzawa"}
	    // {"EmployeeID":2,"FirstName":"Shoin","LastName":"Yoshida"}
        }
    }
}
