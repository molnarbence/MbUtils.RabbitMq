﻿using System;
using System.Threading.Tasks;
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
   private readonly ILogger<RabbitMqProducerFactory> _logger = logger;
   private IConnection _connection;

   public async Task<IMessageProducer> CreateAsync(string queueName)
   {
      _connection ??= await CreateConnectionAsync();
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
