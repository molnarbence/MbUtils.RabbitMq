using MbUtils.RabbitMq.Producer.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace MbUtils.RabbitMq.Producer;

internal class RabbitMqProducerFactory(
   IOptions<RabbitMqConfiguration> configurationOptions, 
   ILogger<RabbitMqProducerFactory> logger) : IMessageProducerFactory
{
   private readonly ConnectionFactory _connectionFactory = new() { HostName = configurationOptions.Value.HostName };
   private IConnection? _connection;
   private IChannel? _channel;

   public async Task<IMessageProducer> CreateAsync(string queueName)
   {
      _connection ??= await CreateConnectionAsync();
      _channel ??= await CreateChannelAsync(_connection, queueName);
      return new RabbitMqProducer(_channel, queueName);
   }

   public void Dispose()
   {
      _channel?.Dispose();
      _connection?.Dispose();
      GC.SuppressFinalize(this);
   }

   private async Task<IConnection> CreateConnectionAsync()
   {
      var retryAttempts = 0;
      var connection = default(IConnection);
      while(connection == null)
      {
         try
         {
            logger.LogInformation("Connecting to RabbitMQ host '{HostName}'", _connectionFactory.HostName);
            connection = await _connectionFactory.CreateConnectionAsync();
         }
         catch
         {
            // ignored
         }

         if (connection != null)
         {
            continue;
         }

         retryAttempts++;
         logger.LogError("Unable to connect to host '{HostName}'. Retrying in 3 seconds. [Retry attempts: {RetryAttempts}]", _connectionFactory.HostName, retryAttempts);
         await Task.Delay(3000);
      }
      logger.LogInformation("Connected to RabbitMQ host '{HostName}'", _connectionFactory.HostName);
      return connection;
   }
   
   private static async Task<IChannel> CreateChannelAsync(IConnection connection, string queueName)
   {
      var channel = await connection.CreateChannelAsync();
      await channel.QueueDeclareAsync(queue: queueName,
         durable: true,
         exclusive: false,
         autoDelete: false,
         arguments: null);

      return channel;
   }
}
