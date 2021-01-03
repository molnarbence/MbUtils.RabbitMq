using System;
using System.Threading;
using System.Threading.Tasks;
using MbUtils.RabbitMq.Consumer.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace MbUtils.RabbitMq.Consumer
{
   internal class ConsumerHostedService : BackgroundService
   {
      private readonly RabbitMqConfiguration _configuration;
      private readonly ILogger<ConsumerHostedService> _logger;
      private readonly IServiceProvider _serviceProvider;

      private IConnection _connection;
      private IModel _channel;

      public ConsumerHostedService(IOptions<RabbitMqConfiguration> configurationOptions, ILogger<ConsumerHostedService> logger, IServiceProvider serviceProvider)
      {
         if (configurationOptions is null)
         {
            throw new ArgumentNullException(nameof(configurationOptions));
         }

         _configuration = configurationOptions.Value;
         _logger = logger;
         _serviceProvider = serviceProvider;
      }

      public override Task StartAsync(CancellationToken cancellationToken)
      {
         var connectionFactory = new ConnectionFactory 
         { 
            HostName = _configuration.HostName, 
            DispatchConsumersAsync = true, 
            AutomaticRecoveryEnabled = true 
         };

         _connection = CreateConnectionWithRetry(connectionFactory);
         _channel = _connection.CreateModel();
         _channel.QueueDeclare(queue: _configuration.QueueName,
                              durable: true,
                              exclusive: false,
                              autoDelete: false,
                              arguments: null);
         _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
         _logger.LogInformation("Queue [{0}] is waiting for messages on host [{1}]", _configuration.QueueName, _configuration.HostName);

         return base.StartAsync(cancellationToken);
      }

      protected override async Task ExecuteAsync(CancellationToken stoppingToken)
      {
         stoppingToken.ThrowIfCancellationRequested();

         var consumer = new AsyncEventingBasicConsumer(_channel);

         consumer.Received += async (bc, ea) =>
         {
            try
            {
               await ConsumeAsync(ea.Body.ToArray());

               _channel.BasicAck(ea.DeliveryTag, false);
            }
            catch (AlreadyClosedException)
            {
               _logger.LogInformation("RabbitMQ is closed!");
            }
            catch (Exception e)
            {
               _logger.LogError(default, e, e.Message);
            }
         };

         _channel.BasicConsume(queue: _configuration.QueueName, autoAck: false, consumer: consumer);

         await Task.CompletedTask;
      }

      public override async Task StopAsync(CancellationToken cancellationToken)
      {
         await base.StopAsync(cancellationToken);
         _connection.Close();
         _logger.LogInformation("RabbitMQ connection is closed.");
      }

      private IConnection CreateConnectionWithRetry(ConnectionFactory connectionFactory)
      {
         var ret = default(IConnection);
         var retryAttempts = 0;
         while (ret == null)
         {
            try
            {
               ret = connectionFactory.CreateConnection();
            }
            catch (BrokerUnreachableException)
            {
               retryAttempts++;
               _logger.LogError("Broker was unreachable. Retrying in 5 seconds. [Retry attempts: {0}]", retryAttempts);
               Thread.Sleep(5000);
            }
         }

         return ret;
      }

      private Task ConsumeAsync(byte[] message)
      {
         using var scope = _serviceProvider.CreateScope();
         var messageConsumer = scope.ServiceProvider.GetService<IMessageConsumer>();
         return messageConsumer.OnMessageAsync(message);
      }
   }
}
