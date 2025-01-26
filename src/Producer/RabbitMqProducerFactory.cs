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
   private readonly Dictionary<string, RabbitMqProducer> _producers = new();
   private IConnection? _connection;
   

   public async Task<IMessageProducer> CreateAsync(string queueName)
   {
      _connection ??= await CreateConnectionAsync();
      
      if (_producers.TryGetValue(queueName, out var producer))
      {
         return producer;
      }

      var newProducer = new RabbitMqProducer(_connection, queueName);
      _producers.Add(queueName, newProducer);
      return newProducer;
   }

   public void Dispose()
   {
      foreach (var (_, producer) in _producers)
      {
         producer.Dispose();
      }
      
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
}
