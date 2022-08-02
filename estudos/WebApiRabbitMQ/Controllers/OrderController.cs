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
        private readonly ILogger<OrderController> _looger;

        public OrderController(ILogger<OrderController> looger)
        {
            _looger = looger;
        }

        [HttpGet]
        public IActionResult InsertOrder()
        {
            Publisher();

            return Ok();
        }

        private void Publisher()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            {
                var queueName = "order";

                var channel1 = CreateChannel(connection);
                var channel2 = CreateChannel(connection);

                BuildPublishers(channel1, queueName, "Produtor A");
                BuildPublishers(channel2, queueName, "Produtor B");
            }
        }

        // -- services
        private IModel CreateChannel(IConnection connection)
        {
            var channel = connection.CreateModel();

            return channel;
        }

        private void BuildPublishers(IModel channel, string queue, string publisherName)
        {
            Task.Run(() =>
            {
                int count = 0;

                channel.QueueDeclare(queue: "orderQueue",
                                         durable: false,
                                         exclusive: false,
                                         autoDelete: false,
                                         arguments: null);

                while (true)
                {
                    string message = $"OrderNumber: {count++}";
                    var body = Encoding.UTF8.GetBytes(message);

                    channel.BasicPublish("", queue, null, body);

                    Thread.Sleep(1000);
                }
            });
        }
    }
}
