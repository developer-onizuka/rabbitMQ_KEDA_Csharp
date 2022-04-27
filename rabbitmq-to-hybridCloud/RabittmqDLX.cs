using System;
using RabbitMQ.Client;
using System.Text;
using Newtonsoft.Json;

public class RabbitmqDLX
{
    public string rabbitmq_ipaddr; // "RABBITMQ_IPADDR"
    public string rabbitmq_dlx; // "RABBITMQ_DLX"

    public void SendMessages(EmployeeEntity entity)
    {
	var factory = new ConnectionFactory() {
            HostName = this.rabbitmq_ipaddr,
            Port = 5672,
            UserName = "user",
            Password = "PASSWORD"
        };

        using(var connection = factory.CreateConnection())
        using(var channel = connection.CreateModel())
        {
	    string json = JsonConvert.SerializeObject(entity);

            var body = Encoding.UTF8.GetBytes(json);

            channel.BasicPublish(exchange: this.rabbitmq_dlx,
                                 routingKey: "",
                                 basicProperties: null,
                                 body: body);
        }
    }
}
