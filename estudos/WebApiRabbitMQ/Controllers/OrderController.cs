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

        [HttpGet]
        public async Task<IActionResult> InsertOrder()
        {
            await Publisher();

            return Ok();
        }

        private async Task Publisher()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            var manualResetevent = new ManualResetEvent(false);

            manualResetevent.Reset();

            using var connection = factory.CreateConnection();
            var queueName = "order";

            var channel1 = SetupChannel(connection);
            await BuildPublishers(channel1, queueName, "Produtor A", manualResetevent);

            //manualResetevent.WaitOne();
            //return Task.CompletedTask;
        }

        // -- services
        private IModel SetupChannel(IConnection connection)
        {
            var channel = connection.CreateModel();

            channel.QueueDeclare(queue: "order", durable: false, exclusive: false, autoDelete: false, arguments: null);
            channel.QueueDeclare(queue: "finance_order", durable: false, exclusive: false, autoDelete: false, arguments: null);

            channel.ExchangeDeclare("order", "direct");

            channel.QueueBind("order", "order", "order_new");
            channel.QueueBind("order", "order", "order_upd");
            channel.QueueBind("finance_order", "order", "order_new");

            return channel;
        }

        private async Task BuildPublishers(IModel channel, string queue, string publisherName, ManualResetEvent manualResetEvent)
        {
            
            int count = 0;

            try
            {
                var order = new Order(100);
                var message1 = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(order));

                channel.BasicPublish("order", "order_new", null, message1);

                order.Update("Novo nome");
                var message2 = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(order));

                channel.BasicPublish("order", "order_upd", null, message2);

                //return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                manualResetEvent.Set();
                //return Task.FromResult(ex);
            }
        }
    }
}
