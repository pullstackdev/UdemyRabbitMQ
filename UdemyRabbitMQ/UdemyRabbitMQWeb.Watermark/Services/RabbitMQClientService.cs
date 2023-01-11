using Microsoft.Extensions.Logging;
using RabbitMQ.Client; //for ConnectionFactory
using System;

namespace UdemyRabbitMQWeb.Watermark.Services
{
    public class RabbitMQClientService : IDisposable
    {
        //Exchange, Queue, Binding vs burada yapılacak yani channel oluşturulup kapatma servisi
        private readonly ConnectionFactory _connectionFactory;
        private IConnection _connection;
        private IModel _channel;
        public static string ExchangeName = "ImageDirectExchange"; //sabit exchange ismi, direct exchange kullanılacak
        public static string RoutingWatermark = "watermark-route-image"; //route ismi
        public static string QueueName = "queue-watermark-image"; //queue ismi

        private readonly ILogger<RabbitMQClientService> _logger; //Category'si burası olsun

        public RabbitMQClientService(ConnectionFactory connectionFactory, ILogger<RabbitMQClientService> logger)
        {
            _connectionFactory = connectionFactory;
            _logger = logger;
        }

        public IModel Connect()
        {
            _connection = _connectionFactory.CreateConnection();
            if (_channel is { IsOpen: true })
            {
                return _channel;
            }
            _channel = _connection.CreateModel();
            _channel.ExchangeDeclare(ExchangeName, type: "direct", true, false);
            _channel.QueueDeclare(QueueName, true, false, false);
            _channel.QueueBind(exchange: ExchangeName, queue: QueueName, routingKey: RoutingWatermark);

            _logger.LogInformation("RabbitMQ Connected");

            return _channel; //bu kanal üzerinden mesajlar gönderilecek
        }

        public void Dispose()
        {
            _channel?.Close();
            _channel?.Dispose();

            _connection?.Close();
            _connection?.Dispose();

            _logger.LogInformation("RabbitMQ Disconnected");
        }
    }
}
