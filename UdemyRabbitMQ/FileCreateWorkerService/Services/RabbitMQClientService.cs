using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileCreateWorkerService.Services
{
    public class RabbitMQClientService : IDisposable
    {
        //Exchange, Queue, Binding vs burada yapılacak yani channel oluşturulup kapatma servisi
        private readonly ConnectionFactory _connectionFactory;
        private IConnection _connection;
        private IModel _channel;
        //public static string ExchangeName = "ExcelDirectExchange"; //sabit exchange ismi, direct exchange kullanılacak
        //public static string RoutingExcel = "excel-route-file"; //route ismi
        public static string QueueName = "queue-excel-file"; //queue ismi

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

            //gerek yok zaten excel projesindeki aynı RQCL'de entegre ediyorum, burada sadece bağlantı kurup channel dönsün yeter
            //_channel.ExchangeDeclare(ExchangeName, type: "direct", true, false);
            //_channel.QueueDeclare(QueueName, true, false, false);
            //_channel.QueueBind(exchange: ExchangeName, queue: QueueName, routingKey: RoutingExcel);

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
