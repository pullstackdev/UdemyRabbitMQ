using Microsoft.Extensions.Hosting; //for BackgroundService
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Drawing; //for Graphics
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using UdemyRabbitMQWeb.Watermark.Services;

namespace UdemyRabbitMQWeb.Watermark.BackgroundServices
{
    public class ImageWatermarkProcessBackgroundServices : BackgroundService
    {
        //Consumer gibi dinleyecek ve işleyecek yer burası

        private readonly RabbitMQClientService _rabbitMQClientService;
        private readonly ILogger<ImageWatermarkProcessBackgroundServices> _logger;
        private IModel _channel;
        public ImageWatermarkProcessBackgroundServices(RabbitMQClientService rabbitMQClientService, ILogger<ImageWatermarkProcessBackgroundServices> logger)
        {
            _rabbitMQClientService = rabbitMQClientService;
            _logger = logger;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _channel = _rabbitMQClientService.Connect(); //rq bağlandı
            _channel.BasicQos(0, 1, false);//kaçar kaçar alacağız mesajları

            return base.StartAsync(cancellationToken);
        }
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new AsyncEventingBasicConsumer(_channel); //event'in async hali
            _channel.BasicConsume(RabbitMQClientService.QueueName, false, consumer);
            consumer.Received += Consumer_Received;//az kod ise lambda, çok ise ayrı metod olması best practice

            return Task.CompletedTask;
        }

        private Task Consumer_Received(object sender, BasicDeliverEventArgs @event)
        {
            try
            {
                //resime watermark burada eklenecek
                var productImageCreatedEvent = JsonSerializer.Deserialize<ProductImageCreatedEvent>(Encoding.UTF8.GetString(@event.Body.ToArray()));
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\Images", productImageCreatedEvent.ImageName);

                var siteName = "www.mysite.com";
                using var img = Image.FromFile(path); //resmi aldık
                using var graphic = Graphics.FromImage(img);

                //watermark özellikleri
                var font = new Font(FontFamily.GenericSansSerif, 40, FontStyle.Bold, GraphicsUnit.Pixel);
                var textSize = graphic.MeasureString(siteName, font);
                var color = Color.FromArgb(128, 255, 255, 255);
                var brush = new SolidBrush(color);
                var position = new Point(img.Width - ((int)textSize.Width + 30), img.Height - ((int)textSize.Height + 30));

                graphic.DrawString(siteName, font, brush, position);

                img.Save("wwwroot/Images/watermarks/" + productImageCreatedEvent.ImageName);
                img.Dispose(); //bellekten sil
                graphic.Dispose(); //bellekten sil

                _channel.BasicAck(@event.DeliveryTag, false); //bu mesaj işlendi, RQ'ya bildir
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex.Message);
            }
            return Task.CompletedTask;
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            return base.StopAsync(cancellationToken);
        }
    }
}
