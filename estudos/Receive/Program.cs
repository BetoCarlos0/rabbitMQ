// See https://aka.ms/new-console-template for more information
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Receive
{
    class Program{
        static void Main(string[] args){

            var factory = new ConnectionFactory() { HostName = "localhost" };
            using var connection = factory.CreateConnection();
            
            var channel = CreateChannel(connection);

            var queueName = "order";

            channel.QueueDeclare(queue: queueName,
                                    durable: false,
                                    exclusive: false,
                                    autoDelete: false,
                                    arguments: null);


            BuildWorker(channel, $"Worker A", "order");
            BuildWorker(channel, $"Worker B", "finance_order");

            Console.ReadLine();            
        }
        private static IModel CreateChannel(IConnection connection)
        {
            var channel = connection.CreateModel();

            return channel;
        }

        public static void BuildWorker(IModel channel, string workerName, string queueName)
        {
            //channel.BasicQos(0, 1, false);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);

                    Console.WriteLine($"channel: {channel.ChannelNumber} - {workerName}, {queueName}: Received {message}");

                    channel.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception)
                {
                    channel.BasicNack(ea.DeliveryTag, false, true);
                }
            };
            channel.BasicConsume(queue: queueName,
                                            autoAck: false,
                                            consumer: consumer);

        }
    }
}

