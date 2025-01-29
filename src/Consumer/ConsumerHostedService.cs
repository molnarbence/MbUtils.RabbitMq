using MbUtils.RabbitMq.Consumer.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace MbUtils.RabbitMq.Consumer;

internal class ConsumerHostedService<TConsumer>(
   IOptions<RabbitMqConfiguration<TConsumer>> configurationOptions,
   ILogger<ConsumerHostedService<TConsumer>> logger,
   IServiceProvider serviceProvider,
   IConnection connection)
   : BackgroundService
   where TConsumer : IMessageConsumer
{
   private IModel? _channel;

   public override async Task StartAsync(CancellationToken cancellationToken)
   {
      _channel = connection.CreateModel();

      logger.LogInformation("Declaring queue [{QueueName}]", configurationOptions.Value.QueueName);
      _channel.QueueDeclare(queue: configurationOptions.Value.QueueName,
         durable: true,
         exclusive: false,
         autoDelete: false,
         arguments: null);
      _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
      logger.LogInformation("Queue [{QueueName}] is waiting for messages", configurationOptions.Value.QueueName);
      
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

            _channel?.BasicAck(ea.DeliveryTag, false);
         }
         catch (AlreadyClosedException)
         {
            logger.LogInformation("RabbitMQ is closed!");
         }
         catch (Exception e)
         {
            logger.LogError(default, e, "Exception message: {ExceptionMessage}", e.Message);
         }
      };
      
      _channel.BasicConsume(queue: configurationOptions.Value.QueueName, autoAck: false, consumer: consumer);

      await Task.CompletedTask;
   }

   public override async Task StopAsync(CancellationToken cancellationToken)
   {
      await base.StopAsync(cancellationToken);

      logger.LogInformation("Closing RabbitMQ connection.");
      connection.Close();
      logger.LogInformation("RabbitMQ connection is closed.");
   }

   private async Task ConsumeAsync(byte[] message)
   {
      using var scope = serviceProvider.CreateScope();
      var messageConsumer = scope.ServiceProvider.GetRequiredService<TConsumer>();
      await messageConsumer.OnMessageAsync(message);
   }
}
