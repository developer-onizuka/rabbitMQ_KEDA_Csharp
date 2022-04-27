using System;
using RabbitMQ.Client;
using System.Text;
using System.IO;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

class sendImage_to_rabbitmqDLX
{
    public static void Main()
    {
        var rabbitmq_ipaddr = Environment.GetEnvironmentVariable("RABBITMQ_IPADDR");
        var rabbitmq_dlx = Environment.GetEnvironmentVariable("RABBITMQ_DLX");
        var rabbitmq_messageCount = Environment.GetEnvironmentVariable("RABBITMQ_MESSAGECOUNT");
	if (string.IsNullOrEmpty(rabbitmq_messageCount)) {
		rabbitmq_messageCount = "10";
	}
        var rabbitmq_messageCountInt = Int32.Parse(rabbitmq_messageCount);

	var factory = new ConnectionFactory() {
            HostName = rabbitmq_ipaddr,
            Port = 5672,
            UserName = "user",
            Password = "PASSWORD"
        };

        using(var connection = factory.CreateConnection())
        using(var channel = connection.CreateModel())
        {
	    for(int i = 1; i <= rabbitmq_messageCountInt; i++) {
		string input = "/home/vagrant/Downloads/image" + i + ".jpg";
		using (FileStream fi = File.OpenRead(input))
		{
		    string random = Guid.NewGuid().ToString("N").Substring(0, 8);
		    byte[] bs = new byte[fi.Length];
		    fi.Read(bs, 0, bs.Length);

		    EmployeeEntity emp = new EmployeeEntity
		    {
		        EmployeeID = i,
		        FirstName = random,
		        LastName = random,
		        Image = bs
		    };

		    string json = JsonConvert.SerializeObject(emp);
		    var body = Encoding.UTF8.GetBytes(json);
		    //var body = new ReadOnlyMemory<byte>(bs);
                    channel.BasicPublish(exchange: rabbitmq_dlx,
                                         routingKey: "",
                                         basicProperties: null,
                                         body: body);

                    //Console.WriteLine(body.GetType());
                    Console.WriteLine(" [x] Sent {0}", emp.EmployeeID);

                    //String output = "/home/vagrant/Downloads/rcvImage1.jpg";
                    //using (var fo = new FileStream(output, FileMode.Create))
                    //{
                    //    fo.Write(bs, 0, bs.Length);
                    //}
		}
	    }
        }

        Console.WriteLine(" Press [enter] to exit.");
        Console.ReadLine();
    }
}
