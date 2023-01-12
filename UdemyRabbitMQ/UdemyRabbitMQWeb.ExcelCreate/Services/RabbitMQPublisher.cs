using RabbitMQ.Client;
using Shared;
using System.Text;
using System.Text.Json;

namespace UdemyRabbitMQWeb.ExcelCreate.Services
{
    public class RabbitMQPublisher
    {
        //publish etme yeri

        private readonly RabbitMQClientService _rabbitMQClientService;

        public RabbitMQPublisher(RabbitMQClientService rabbitMQClientService)
        {
            _rabbitMQClientService = rabbitMQClientService;
        }

        public void Publish(CreateExcelMessage createExcelMessage)
        {
            var channel = _rabbitMQClientService.Connect(); //channel'i aldık
            var bodyString = JsonSerializer.Serialize(createExcelMessage); //mesaj datası burada
            var bodyByte = Encoding.UTF8.GetBytes(bodyString);

            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;

            channel.BasicPublish(RabbitMQClientService.ExchangeName, RabbitMQClientService.RoutingExcel, properties, bodyByte);

        }
    }
}
