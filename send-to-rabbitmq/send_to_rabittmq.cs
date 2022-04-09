using System;
using RabbitMQ.Client;
using System.Text;

class send_to_rabbitmq
{
    public static void Main()
    {
        var rabbitmq_ipaddr = Environment.GetEnvironmentVariable("RABBITMQ_IPADDR");
        var rabbitmq_queue = Environment.GetEnvironmentVariable("RABBITMQ_QUEUE");
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
            channel.QueueDeclare(queue: rabbitmq_queue,
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

	    for(int i = 0; i < rabbitmq_messageCountInt; i++) {
                string message = "{\"EmployeeID\":" + i + ",\"FirstName\":\"xxxxx\",\"LastName\":\"xxxxx\"}";
                var body = Encoding.UTF8.GetBytes(message);

                channel.BasicPublish(exchange: "",
                                     routingKey: rabbitmq_queue,
                                     basicProperties: null,
                                     body: body);
                Console.WriteLine(" [x] Sent {0}", message);
	    }
        }

        Console.WriteLine(" Press [enter] to exit.");
        Console.ReadLine();
    }
}
