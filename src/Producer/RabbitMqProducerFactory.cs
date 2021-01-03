using System;
using MbUtils.RabbitMq.Producer.Configuration;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace MbUtils.RabbitMq.Producer
{
   internal class RabbitMqProducerFactory : IMessageProducerFactory
   {
      private readonly IConnection _connection;

      public RabbitMqProducerFactory(IOptions<RabbitMqConfiguration> configurationOptions)
      {
         var config = configurationOptions.Value;
         var factory = new ConnectionFactory() { HostName = config.HostName };
         _connection = factory.CreateConnection();
      }

      public IMessageProducer Create(string queueName)
      {
         return new RabbitMqProducer(_connection, queueName);
      }

      public void Dispose()
      {
         _connection.Dispose();
         GC.SuppressFinalize(this);
      }
   }
}
