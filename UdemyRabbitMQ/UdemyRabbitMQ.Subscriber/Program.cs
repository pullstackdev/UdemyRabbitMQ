using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading;

namespace UdemyRabbitMQ.Subscriber
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
            //kanal üzerinden kuyruk oluşacak
            //kalabilirde ama publisherda var gerekte kalmadı
            //channel.QueueDeclare("hello-queue", true, false, false);//true:ram değil fiziksel tut restart

            channel.BasicQos(0, 5, false); //tek seferde 1 subs'a 5 mesaj iletir, true olursa tüm subs'lara totalde 5 gönderir tek seferde, bölüştürür

            //okuma yapılacak
            var consumer = new EventingBasicConsumer(channel); //kanalı dinle
            //bu kanal üzerinden hangi kuyruğu dinleyecek
            channel.BasicConsume("hello-queue", false, consumer); //true:mesaj geldiğinden doğruda yanlışta işlense kuyruktan sil durumu. false olursa silme ben işlenince haber veririm

            consumer.Received += (object sender, BasicDeliverEventArgs e) =>
            {
                var message = Encoding.UTF8.GetString(e.Body.ToArray());//byte olarak gelen mesajı aldı çevirdi

                Thread.Sleep(1500);

                Console.WriteLine("Gelen Message: " + message);

                //mesajın işlendiğini rq'ya iletir haber verir ki gerekirse silebilsin
                channel.BasicAck(e.DeliveryTag, false);
            };

            Console.ReadLine();
        }
    }
}
