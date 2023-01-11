using RabbitMQ.Client; //for basicpublish
using System.Text; //for Encoding
using System.Text.Json; //for JsonSerializer

namespace UdemyRabbitMQWeb.Watermark.Services
{
    public class RabbitMQPublisher
    {
        //publish etme yeri

        private readonly RabbitMQClientService _rabbitMQClientService;

        public RabbitMQPublisher(RabbitMQClientService rabbitMQClientService)
        {
            _rabbitMQClientService = rabbitMQClientService;
        }

        public void Publish(ProductImageCreatedEvent productImageCreatedEvent) 
        {
            var channel = _rabbitMQClientService.Connect(); //channel'i aldık
            var bodyString = JsonSerializer.Serialize(productImageCreatedEvent); //mesaj datası burada
            var bodyByte = Encoding.UTF8.GetBytes(bodyString);

            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;

            channel.BasicPublish(RabbitMQClientService.ExchangeName, RabbitMQClientService.RoutingWatermark, properties, bodyByte);

        }
    }
}
