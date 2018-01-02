using System;
using System.Text;
using System.Threading;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MyLib
{
    public class JobExecuter
    {
        public void DoWork(CancellationToken cancellationToken)
        {
            Console.WriteLine($"Working, ThreadPool: {Thread.CurrentThread.IsThreadPoolThread}, Thread ID: {Thread.CurrentThread.ManagedThreadId}");
            var factory = new ConnectionFactory
            {
                HostName = "localhost",
                UserName = "admin",
                Password = "admin"
            };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(
                    queue: "hello",
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                // EventingBasicConsumer fires the handler subscribed to consumer.Received event
                // As long as the thread containing this code is running
                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body;
                    var message = Encoding.UTF8.GetString(body);
                    Console.WriteLine($" [x] Received {message}");
                };
                channel.BasicConsume(queue: "hello", noAck: true, consumer: consumer);

                cancellationToken.WaitHandle.WaitOne();
                Console.WriteLine($"Exiting consumer, ThreadPool: {Thread.CurrentThread.IsThreadPoolThread}, Thread ID: {Thread.CurrentThread.ManagedThreadId}");
            }
        }
    }
}
