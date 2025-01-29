using MbUtils.RabbitMq.Producer.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace MbUtils.RabbitMq.Producer;

internal class RabbitMqProducerFactory(
   IConnection connection) : IMessageProducerFactory
{
   private readonly Dictionary<string, RabbitMqProducer> _producers = new();

   public IMessageProducer Create(string queueName)
   {  
      if (_producers.TryGetValue(queueName, out var producer))
      {
         return producer;
      }
      
      var channel = connection.CreateModel();
      channel.QueueDeclare(queue: queueName,
         durable: true,
         exclusive: false,
         autoDelete: false,
         arguments: null);

      var basicProperties = channel.CreateBasicProperties();
      basicProperties.Persistent = true;

      var newProducer = new RabbitMqProducer(channel, basicProperties, queueName);
      _producers.Add(queueName, newProducer);
      return newProducer;
   }

   public void Dispose()
   {
      foreach (var (_, producer) in _producers)
      {
         producer.Dispose();
      }
   }
}
