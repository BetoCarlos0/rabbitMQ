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
        public IActionResult InsertOrder(int qnt)
        {
            Publisher(qnt);

            return Ok();
        }

        private void Publisher(int qnt)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            var manualResetevent = new ManualResetEvent(false);

            manualResetevent.Reset();

            using var connection = factory.CreateConnection();
            var queueName = "orderQueue";

            var channel1 = CreateChannel(connection, queueName);
            BuildPublishers(qnt, channel1, queueName, "Produtor A", manualResetevent);

            //manualResetevent.WaitOne();
        }

        // -- services
        private IModel CreateChannel(IConnection connection, string queueName)
        {
            var channel = connection.CreateModel();

            channel.QueueDeclare(queue: queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

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

                        channel.BasicPublish("", queue, null, body);
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
