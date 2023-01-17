using ClosedXML.Excel;//XLWorkbook datatable'� excele d�n��t�recek
using FileCreateWorkerService.Models;
using FileCreateWorkerService.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace FileCreateWorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly RabbitMQClientService _rabbitMQClientService; //2taraftada da singleton oldu�u i�in DI alabiliriz
        private readonly IServiceProvider _serviceProvider;//programcsde services.AddDbContex scoped olarak al�n�p, workerda singleton oldu�u i�in direk DI ile �a��ramay�z IServiceProvider kullan�l�r
        private IModel _channel;
        public Worker(ILogger<Worker> logger, RabbitMQClientService rabbitMQClientService, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _rabbitMQClientService = rabbitMQClientService;
            _serviceProvider = serviceProvider;
        }

        public override Task StartAsync(CancellationToken cancellationToken) //start olunca rq'ya ba�lan
        {
            _channel = _rabbitMQClientService.Connect();
            _channel.BasicQos(0, 1, false); //rq bana 1 1 g�nder

            return base.StartAsync(cancellationToken);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //consume edecek yer
            var consumer = new AsyncEventingBasicConsumer(_channel);
            _channel.BasicConsume(RabbitMQClientService.QueueName, false, consumer);

            consumer.Received += Consumer_Received;
            return Task.CompletedTask;
        }

        private async Task Consumer_Received(object sender, BasicDeliverEventArgs @event)
        {
            await Task.Delay(1000);
            var createExcelMessage = JsonSerializer.Deserialize<CreateExcelMessage>(Encoding.UTF8.GetString(@event.Body.ToArray()));

            using var ms = new MemoryStream(); //excel dosyas�n� memorystreame ataca��z

            //workbook olu�turulacak
            var wb = new XLWorkbook();
            var ds = new DataSet();//table gibi
            ds.Tables.Add(GetTable("users"));

            wb.Worksheets.Add(ds);
            wb.SaveAs(ms);

            MultipartFormDataContent multipartFormDataContent = new();
            multipartFormDataContent.Add(new ByteArrayContent(ms.ToArray()), "file", Guid.NewGuid().ToString() + ".xlsx");

            try
            {
                var baseUrl = "http://localhost:44383/api/files"; //apiye istek at�p file'� wwwroot'a kaydettirecek
                using (var httpClient = new HttpClient())
                {
                    var response = await httpClient.PostAsync($"{baseUrl}?fileId={createExcelMessage.FileId}", multipartFormDataContent);

                    if (response.IsSuccessStatusCode) //files controllerdan post success gelirse
                    {
                        _logger.LogInformation($"File (Id : {createExcelMessage.FileId}) was created by successfull");
                        _channel.BasicAck(@event.DeliveryTag, false); //rq'ya bildirki silsin, yoksa tekrar deneyecek ba�ka supplierda
                    }
                }
            }
            catch (Exception ex)
            {
                //signalr'� buraya kural�m �imdilik
            }
            
        }

        //excele d�n��t�relecek table olu�turulacak
        private DataTable GetTable(string tableName)
        {
            List<AspNetUser> aspNetUsers;
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<UdemyRabbitMQIdentityContext>();
                aspNetUsers = context.AspNetUsers.ToList();
            }

            DataTable dataTable = new DataTable { TableName = tableName };
            dataTable.Columns.Add("UserName", typeof(string));
            dataTable.Columns.Add("Email", typeof(string));

            aspNetUsers.ForEach(x =>
            {
                dataTable.Rows.Add(x.UserName, x.Email);
            });

            return dataTable;
        }
    }
}
