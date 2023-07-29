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

namespace MbUtils.RabbitMq.Consumer;

internal class ConsumerHostedService<TConsumer> : BackgroundService where TConsumer : IMessageConsumer
{
   private readonly RabbitMqConfiguration<TConsumer> _configuration;
   private readonly ILogger<ConsumerHostedService<TConsumer>> _logger;
   private readonly IServiceProvider _serviceProvider;
   private readonly IConsumerStatusManager _consumerStatus;

   private IConnection _connection;
   private IModel _channel;

   public ConsumerHostedService(
      IOptions<RabbitMqConfiguration<TConsumer>> configurationOptions,
      ILogger<ConsumerHostedService<TConsumer>> logger,
      IServiceProvider serviceProvider,
      IConsumerStatusManager consumerStatus)
   {
      if (configurationOptions is null)
      {
         throw new ArgumentNullException(nameof(configurationOptions));
      }

      _configuration = configurationOptions.Value;
      _logger = logger;
      _serviceProvider = serviceProvider;
      _consumerStatus = consumerStatus;
      _consumerStatus.Initialized(_configuration.HostName, _configuration.QueueName);
   }

   public override async Task StartAsync(CancellationToken cancellationToken)
   {
      var connectionFactory = new ConnectionFactory
      {
         HostName = _configuration.HostName,
         DispatchConsumersAsync = true,
         AutomaticRecoveryEnabled = true
      };

      _logger.LogInformation("Creating connection to host [{HostName}]", _configuration.HostName);
      _connection = await CreateConnectionWithRetryAsync(connectionFactory);
      _channel = _connection.CreateModel();

      _logger.LogInformation("Declaring queue [{QueueName}]", _configuration.QueueName);
      _channel.QueueDeclare(queue: _configuration.QueueName,
                           durable: true,
                           exclusive: false,
                           autoDelete: false,
                           arguments: null);
      _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
      _logger.LogInformation("Queue [{QueueName}] is waiting for messages on host [{HostName}]", _configuration.QueueName, _configuration.HostName);
      _consumerStatus.StartedListening();

      await base.StartAsync(cancellationToken);
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
            _logger.LogError(default, e, "Exception message: {ExceptionMessage}", e.Message);
         }
      };

      _channel.BasicConsume(queue: _configuration.QueueName, autoAck: false, consumer: consumer);

      await Task.CompletedTask;
   }

   public override async Task StopAsync(CancellationToken cancellationToken)
   {
      await base.StopAsync(cancellationToken);

      _logger.LogInformation("Closing RabbitMQ connection.");
      _connection.Close();
      _logger.LogInformation("RabbitMQ connection is closed.");

      _consumerStatus.StoppedListening();
   }

   private async Task<IConnection> CreateConnectionWithRetryAsync(ConnectionFactory connectionFactory)
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
            _logger.LogError("Broker was unreachable. Retrying in 5 seconds. [Retry attempts: {RetryAttempts}]", retryAttempts);
            _consumerStatus.BrokerUnreachable(retryAttempts);               
         }
         if (ret == null) await Task.Delay(5000);
      }

      return ret;
   }

   private async Task ConsumeAsync(byte[] message)
   {
      _consumerStatus.IncrementMessageCount();
      using var scope = _serviceProvider.CreateScope();
      var messageConsumer = scope.ServiceProvider.GetService<TConsumer>();
      await messageConsumer.OnMessageAsync(message);
   }
}
