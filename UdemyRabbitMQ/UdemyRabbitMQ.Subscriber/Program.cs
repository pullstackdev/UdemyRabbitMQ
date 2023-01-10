using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.IO;
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

            //#region Fanout Exchange
            ////channel.ExchangeDeclare("logs-fanout", durable: true, type: ExchangeType.Fanout); //zaten producerda var burada gerek yok
            //var randomQueueName = channel.QueueDeclare().QueueName;//random queue ismi ile kuyruk oluşturuldu, exchange bind edilecek
            ////channel.QueueDeclare("log-database-save-queue", true, false, false); //bunu eklersek buraya artık kuyruk kalıcı olur Consumer down olsa bile
            //channel.QueueBind(randomQueueName, "logs-fanout", "", null); //C giderse queue da gider, ama declare etseydik silinmezdi
            //#endregion
            //#region Direct Exchange
            //var queueName = "direct-queue-Critical";
            //#endregion

            channel.BasicQos(0, 5, false); //tek seferde 1 subs'a 5 mesaj iletir, true olursa tüm subs'lara totalde 5 gönderir tek seferde, bölüştürür

            //okuma yapılacak
            var consumer = new EventingBasicConsumer(channel); //kanalı dinle
                                                               ////bu kanal üzerinden hangi kuyruğu dinleyecek
                                                               //channel.BasicConsume("hello-queue", false, consumer); //true:mesaj geldiğinden doğruda yanlışta işlense kuyruktan sil durumu. false olursa silme ben işlenince haber veririm

            //#region Topic Exchange
            //var queueName = channel.QueueDeclare().QueueName;
            //var routeKey = "*.Error.*"; //routeKey'i ortasında Error olan mesajları dinle/işle sadece
            //channel.QueueBind(queueName, "logs-topic", routeKey);
            //channel.BasicConsume(queueName, false, consumer); //direct exchange
            //#endregion

            #region Header Exchange
            var queueName = channel.QueueDeclare().QueueName;
            Dictionary<string, object> headers = new Dictionary<string, object>();
            headers.Add("format", "pdf");
            headers.Add("shape", "a4");
            headers.Add("x-match", "all");
            channel.QueueBind(queueName, "logs-header", string.Empty, headers); //route yok, header var. gönderilen mesajın headerındaki değerler aynı ise dinler
            channel.BasicConsume(queueName, false, consumer); //direct exchange
            #endregion

            #region Exchange
            //channel.BasicConsume(randomQueueName, false, consumer); //fanout - artık randomqueuename'i tüketecek
            channel.BasicConsume(queueName, false, consumer); //direct exchange
            #endregion

            consumer.Received += (object sender, BasicDeliverEventArgs e) =>
            {
                var message = Encoding.UTF8.GetString(e.Body.ToArray());//byte olarak gelen mesajı aldı çevirdi

                Thread.Sleep(1500);

                Console.WriteLine("Gelen Message: " + message);

                //bazen logları txt'ye eklemek gerekebilir
                //File.AppendAllText("log-critical.txt", message + "\n");

                //mesajın işlendiğini rq'ya iletir haber verir ki gerekirse silebilsin
                channel.BasicAck(e.DeliveryTag, false); //işlenmeyen olsaydı UI daki Unack sayısını artardı
            };

            Console.ReadLine();
        }
    }
}
