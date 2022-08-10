using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using WebApiRabbitMQ.Domain;

namespace WebApiRabbitMQ.Controllers
{
    [Route("api/orders")]
    [ApiController]
    public class OrderController : ControllerBase
    {

        [HttpPost("{qnt}")]
        public async Task<IActionResult> InsertOrder(int qnt)
        {
            await Publisher(qnt);

            return Ok();
        }

        private Task Publisher(int qnt)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            var manualResetevent = new ManualResetEvent(false);

            manualResetevent.Reset();

            using var connection = factory.CreateConnection();
            var queueName = "orderQueue";

            var channel1 = SetupChannel(connection);
            BuildPublishers(qnt, channel1, queueName, "Produtor A", manualResetevent);

            //manualResetevent.WaitOne();
            return Task.CompletedTask;
        }

        // -- services
        private IModel SetupChannel(IConnection connection)
        {
            var channel = connection.CreateModel();

            channel.QueueDeclare(queue: "order", durable: false, exclusive: false, autoDelete: false, arguments: null);
            channel.QueueDeclare(queue: "logs", durable: false, exclusive: false, autoDelete: false, arguments: null);
            channel.QueueDeclare(queue: "finance_order", durable: false, exclusive: false, autoDelete: false, arguments: null);

            channel.ExchangeDeclare("order", "fanout");

            channel.QueueBind("order", "order", "");
            channel.QueueBind("logs", "order", "");
            channel.QueueBind("finance_order", "order", "");

            return channel;
        }

        private void BuildPublishers(int qnt, IModel channel, string queue, string publisherName, ManualResetEvent manualResetEvent)
        {
            Task.Run(() =>
            {
                int count = 0;

                try
                {
                    for (int i = 0; i < qnt; i++)
                    {
                        string message = $"OrderNumber: {count++} de {publisherName}";
                        var body = Encoding.UTF8.GetBytes(message);

                        channel.BasicPublish("order", "", null, body);
                    }
                }
                catch (Exception ex)
                {
                    manualResetEvent.Set();
                    //return BadRequest(ex);
                }
            });
        }
    }
}
