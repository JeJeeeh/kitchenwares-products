using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Kitchenwares_Products.Services;

public interface IRabbitMqService
{
    void StartConsuming();
    void SendMessage<T>(T message);
}

public class RabbitMqService : IRabbitMqService
{
    public void StartConsuming()
    {
        var factory = new ConnectionFactory
        {
            HostName = Environment.GetEnvironmentVariable("RABBITMQ__HOST") ?? "localhost",
            Port = 5672,
            UserName = Environment.GetEnvironmentVariable("RABBITMQ__USER") ?? "guest",
            Password = Environment.GetEnvironmentVariable("RABBITMQ__PASS") ?? "guest"
        };
        var connection = factory.CreateConnection();
        var channel = connection.CreateModel();

        channel.QueueDeclare(queue: "images",
            exclusive: false,
            durable: false,
            arguments: null);

        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += async (model, ea) =>
        {

            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            var productId = message.Trim('"');
            // await imageService.DeleteMultiple(productId);
        };

        channel.BasicConsume(queue: "images",
            autoAck: true,
            consumer: consumer);
    }
    
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