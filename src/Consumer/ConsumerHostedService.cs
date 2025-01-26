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

   private IConnection? _connection;
   private IChannel? _channel;

   public ConsumerHostedService(
      IOptions<RabbitMqConfiguration<TConsumer>> configurationOptions,
      ILogger<ConsumerHostedService<TConsumer>> logger,
      IServiceProvider serviceProvider,
      IConsumerStatusManager consumerStatus)
   {
      ArgumentNullException.ThrowIfNull(configurationOptions);

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
         AutomaticRecoveryEnabled = true
      };

      _logger.LogInformation("Creating connection to host [{HostName}]", _configuration.HostName);
      _connection = await CreateConnectionWithRetryAsync(connectionFactory, cancellationToken);
      _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

      _logger.LogInformation("Declaring queue [{QueueName}]", _configuration.QueueName);
      await _channel.QueueDeclareAsync(queue: _configuration.QueueName,
                           durable: true,
                           exclusive: false,
                           autoDelete: false,
                           arguments: null, cancellationToken: cancellationToken);
      await _channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false, cancellationToken: cancellationToken);
      _logger.LogInformation("Queue [{QueueName}] is waiting for messages on host [{HostName}]", _configuration.QueueName, _configuration.HostName);
      _consumerStatus.StartedListening();

      await base.StartAsync(cancellationToken);
   }

   protected override async Task ExecuteAsync(CancellationToken stoppingToken)
   {
      if(_channel is null) throw new InvalidOperationException("Channel is not initialized.");
      stoppingToken.ThrowIfCancellationRequested();

      var consumer = new AsyncEventingBasicConsumer(_channel);

      consumer.ReceivedAsync += async (_, ea) =>
      {
         try
         {
            await ConsumeAsync(ea.Body.ToArray());

            await _channel.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
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

      await _channel.BasicConsumeAsync(queue: _configuration.QueueName, autoAck: false, consumer: consumer, cancellationToken: stoppingToken);

      await Task.CompletedTask;
   }

   public override async Task StopAsync(CancellationToken cancellationToken)
   {
      await base.StopAsync(cancellationToken);

      if (_connection is not null)
      {
         _logger.LogInformation("Closing RabbitMQ connection.");
         await _connection.CloseAsync(cancellationToken: cancellationToken);
         _logger.LogInformation("RabbitMQ connection is closed.");
      }
      
      _consumerStatus.StoppedListening();
   }

   private async Task<IConnection> CreateConnectionWithRetryAsync(ConnectionFactory connectionFactory, CancellationToken cancellationToken = default)
   {
      var ret = default(IConnection);
      var retryAttempts = 0;
      while (ret == null)
      {
         try
         {
            ret = await connectionFactory.CreateConnectionAsync(cancellationToken);
         }
         catch (BrokerUnreachableException)
         {
            retryAttempts++;
            _logger.LogError("Broker was unreachable. Retrying in 5 seconds. [Retry attempts: {RetryAttempts}]", retryAttempts);
            _consumerStatus.BrokerUnreachable(retryAttempts);               
         }
         if (ret == null) await Task.Delay(5000, cancellationToken);
      }

      return ret;
   }

   private async Task ConsumeAsync(byte[] message)
   {
      _consumerStatus.IncrementMessageCount();
      using var scope = _serviceProvider.CreateScope();
      var messageConsumer = scope.ServiceProvider.GetRequiredService<TConsumer>();
      await messageConsumer.OnMessageAsync(message);
   }
}
