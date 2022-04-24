using System;
using RabbitMQ.Client;
using System.Text;

class send_to_rabbitmqDLX
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
	    for(int i = 0; i < rabbitmq_messageCountInt; i++) {
		string random = Guid.NewGuid().ToString("N").Substring(0, 8);
                string message = "{\"EmployeeID\":" + i + ",\"FirstName\":\"" + random + "\",\"LastName\":\"" + random + "\"}";
                var body = Encoding.UTF8.GetBytes(message);

                channel.BasicPublish(exchange: rabbitmq_dlx,
                                     routingKey: "",
                                     basicProperties: null,
                                     body: body);
                Console.WriteLine(" [x] Sent {0}", message);
	    }
        }

        Console.WriteLine(" Press [enter] to exit.");
        Console.ReadLine();
    }
}
