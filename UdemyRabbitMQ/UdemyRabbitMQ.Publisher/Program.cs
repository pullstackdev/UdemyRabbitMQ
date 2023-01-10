using RabbitMQ.Client;
using System;
using System.Linq;
using System.Text;

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
            channel.QueueDeclare("hello-queue", true, false, false);//true:ram değil fiziksel tut restart olsa bile kalsın. false:bu kuyruğa başka kanallardanda ulaşılabilsin.

            #region WORK QUEUE
            Enumerable.Range(1, 50).ToList().ForEach(x =>
            {
                string message = $"Message {x}";
                var messageBody = Encoding.UTF8.GetBytes(message);
                channel.BasicPublish(string.Empty, "hello-queue", null, messageBody);
                Console.WriteLine($"Message Gönderildi: {message}");
            });
            #endregion

            //mesajı ayarla
            string message = "hello world";
            //rabbitmq byte türünden herşeyi mesaj olarak alabilir, ayarlaması yapılacak
            var messageBody = Encoding.UTF8.GetBytes(message);
            //kanal üzerinden kuyruğa mesaj pushlanacak
            channel.BasicPublish(string.Empty, "hello-queue", null, messageBody);
            Console.WriteLine("Message Gönderildi");
            Console.ReadLine();
        }
    }
}
