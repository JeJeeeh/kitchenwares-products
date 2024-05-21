using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace Kitchenwares_Products.Services;

public interface IRabbitMqService
{
    void SendMessage<T>(T message);
}

public class RabbitMqService: IRabbitMqService
{
    public void SendMessage<T>(T message)
    {
        var factory = new ConnectionFactory
        {
            HostName = Environment.GetEnvironmentVariable("RABBITMQ__HOST") ?? "localhost",
            Port = 5672,
            UserName = Environment.GetEnvironmentVariable("RABBITMQ__USER") ?? "guest",
            Password = Environment.GetEnvironmentVariable("RABBITMQ__PASS") ?? "guest"
        };
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.QueueDeclare("products", exclusive: false);

        var json = JsonConvert.SerializeObject(message);
        var body = Encoding.UTF8.GetBytes(json);
        
        channel.BasicPublish(exchange: "", routingKey: "products", body: body);
    }
}