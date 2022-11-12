using System;
using System.Threading.Tasks;
using MbUtils.RabbitMq.Producer.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace MbUtils.RabbitMq.Producer;

internal class RabbitMqProducerFactory : IMessageProducerFactory
{
   private readonly ConnectionFactory _connectionFactory;
   private readonly ILogger<RabbitMqProducerFactory> _logger;
   private IConnection _connection;

   public RabbitMqProducerFactory(IOptions<RabbitMqConfiguration> configurationOptions, ILogger<RabbitMqProducerFactory> logger)
   {
      var config = configurationOptions.Value;
      _connectionFactory = new ConnectionFactory() { HostName = config.HostName };
      _logger = logger;
   }

   public async Task<IMessageProducer> CreateAsync(string queueName)
   {
      if (_connection == null)
         _connection = await CreateConnectionAsync();
      return new RabbitMqProducer(_connection, queueName);
   }

   public void Dispose()
   {
      _connection.Dispose();
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
            _logger.LogInformation("Connecting to RabbitMQ host '{HostName}'", _connectionFactory.HostName);
            connection = _connectionFactory.CreateConnection();
         }
         catch (Exception)
         {
         }
         if(connection == null)
         {
            retryAttempts++;
            _logger.LogError("Unable to connect to host '{HostName}'. Retrying in 3 seconds. [Retry attempts: {RetryAttempts}]", _connectionFactory.HostName, retryAttempts);
            await Task.Delay(3000);
         }
      }
      _logger.LogInformation("Connected to RabbitMQ host '{HostName}'", _connectionFactory.HostName);
      return connection;
   }
}
