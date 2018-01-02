using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MyLib;
using RabbitMQ.Client;

namespace Startup
{
    class Program
    {
        private static CancellationTokenSource _cancellationTokenSource;

        static void Main(string[] args)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = _cancellationTokenSource.Token;

            Task.Run(() =>
            {
                Send();
            }, cancellationToken);

            /* RUN AS A DEDICATED THREAD */
            //Task.Factory.StartNew(() =>
            //{
            //    var executor = new JobExecuter();
            //    executor.DoWork(cancellationToken);
            //}, TaskCreationOptions.LongRunning);

            /* RUN ON A THREAD POOL THREAD */
            Task.Run(() =>
            {
                var executor = new JobExecuter();
                executor.DoWork(cancellationToken);
            }, cancellationToken);


            Console.WriteLine("In Main, Done, Press enter to stop sending messages");
            Console.ReadLine();
            _cancellationTokenSource.Cancel();
            Console.WriteLine("After cancellation, press any key to terminate");

            Console.ReadLine();
        }

        private static void Send()
        {
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
                while (!_cancellationTokenSource.IsCancellationRequested)
                {
                    string message = $"Hello RabbitMQ! {DateTime.UtcNow}";
                    var body = Encoding.UTF8.GetBytes(message);

                    channel.BasicPublish(
                        exchange: "",
                        routingKey: "hello",
                        basicProperties: null,
                        body: body);
                    Console.WriteLine("Message sent");
                    Console.WriteLine("--------------");
                    Task.Delay(TimeSpan.FromSeconds(2)).Wait();
                }
            }
        }
    }
}
