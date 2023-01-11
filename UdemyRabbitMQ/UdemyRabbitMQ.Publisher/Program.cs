using RabbitMQ.Client;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace UdemyRabbitMQ.Publisher
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //rabbitmq'ye bağlanma ayarı
            var factory = new ConnectionFactory();
            factory.Uri = new Uri("amqps://qaqduwwf:w15QbvWWXVANGQQgNrLkOD3KhZ7BTxp4@woodpecker.rmq.cloudamqp.com/qaqduwwf");//amqp url'i
            //bağlantı sağlamak için
            using var connection = factory.CreateConnection();
            //rabbitmq'ya kanal üzerinden bağlanılır, kanal oluştur
            var channel = connection.CreateModel();
            //kanal üzerinden kuyruk oluşacak, şuan P mesajı direk queue'ya atıyor ama exchange'de böyle olmayacak buna gerek kalmayacak
            //channel.QueueDeclare("hello-queue", true, false, false);//true:ram değil fiziksel tut restart olsa bile kalsın. false:bu kuyruğa başka kanallardanda ulaşılabilsin.

            //#region Fanout Exchange
            //channel.ExchangeDeclare("logs-fanout", durable: true, type: ExchangeType.Fanout);
            //#endregion

            //#region Direct Exchange
            //channel.ExchangeDeclare("logs-direct", durable: true, type: ExchangeType.Direct); //exchange oluştu

            //Enum.GetNames(typeof(LogNames)).ToList().ForEach(x =>
            //{
            //    //mesajlar route'lanacak directive exchange için
            //    var routeKey = $"route-{x}"; //4 farklı route oluştu
            //    var queueName = $"direct-queue-{x}"; //kuyruk ismi
            //    channel.QueueDeclare(queueName, true, false, false);//queue oluştu
            //    channel.QueueBind(queueName, "logs-direct", routeKey, null); //queue'yu exchange'e route ile birlikte bind ettik
            //});
            //#endregion

            //#region Topic Exchange
            //channel.ExchangeDeclare("logs-topic", durable: true, type: ExchangeType.Topic); //exchange oluştu
            //#endregion

            #region Header Exchange
            channel.ExchangeDeclare("logs-header", durable: true, type: ExchangeType.Headers); //exchange oluştu

            Dictionary<string, object> headers = new Dictionary<string, object>();
            headers.Add("format", "pdf");
            headers.Add("shape", "a4");

            var properties = channel.CreateBasicProperties();
            properties.Headers = headers; //header hazırlandı

            properties.Persistent = true; //mesajların kalıcılığı

            #region complex type
            var product = new Product { Id = 1, Name = "Kalem", Price = 100, Stock = 10 };
            var productJsonString = JsonSerializer.Serialize(product); //class serialize edildi artık bu ifade byte edilecek
            // Encoding.UTF8.GetBytes(productJsonString) ile gönderilir
            #endregion



            channel.BasicPublish("logs-header", string.Empty, properties, Encoding.UTF8.GetBytes(productJsonString)); //artık routeKey boş çünkü onunla değil headerle gidiyor
            Console.WriteLine("Mesaj gönderildi");
            #endregion

            //#region WORK QUEUE
            //Random rnd = new Random(); //for topic exchange
            //Enumerable.Range(1, 50).ToList().ForEach(x =>
            //{
            //    LogNames log = (LogNames)new Random().Next(1, 5);
            //    //mesajlar route'landı, topic exchange için

            //    LogNames log1 = (LogNames)rnd.Next(1, 5);
            //    LogNames log2 = (LogNames)rnd.Next(1, 5);
            //    LogNames log3 = (LogNames)rnd.Next(1, 5);
            //    var routeKey = $"{log1}.{log2}.{log3}";
            //    string message = $"Log-Type: {log1}-{log2}-{log3}";
            //    var messageBody = Encoding.UTF8.GetBytes(message);
            //    //exchange olmadığı zaman string.Empty
            //    //channel.BasicPublish(string.Empty, "hello-queue", null, messageBody);

            //    //mesajlar route'lanacak directive exchange için
            //    //var routeKey = $"route-{log}";

            //    //exchange olursa
            //    //channel.BasicPublish("logs-fanout", "", null, messageBody);
            //    channel.BasicPublish("logs-topic", routeKey, null, messageBody);
            //    Console.WriteLine($"Message Gönderildi: {message}");
            //});
            //#endregion

            ////mesajı ayarla
            //string message = "hello world";
            ////rabbitmq byte türünden herşeyi mesaj olarak alabilir, ayarlaması yapılacak
            //var messageBody = Encoding.UTF8.GetBytes(message);
            ////kanal üzerinden kuyruğa mesaj pushlanacak
            //channel.BasicPublish(string.Empty, "hello-queue", null, messageBody);
            //Console.WriteLine("Message Gönderildi");
            //Console.ReadLine();
        }
    }

    //direct exchange
    public enum LogNames
    {
        Critical = 1,
        Error = 2,
        Warning = 3,
        Info = 4
    }
}
