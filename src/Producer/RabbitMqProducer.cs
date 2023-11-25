using System;
using RabbitMQ.Client;

namespace MbUtils.RabbitMq.Producer;

internal class RabbitMqProducer : IMessageProducer
{
   private readonly IModel _channel;
   private readonly string _queueName;
   private readonly IBasicProperties _basicProperties;

   public RabbitMqProducer(IConnection connection, string queueName)
   {
      _channel = connection.CreateModel();
      _channel.QueueDeclare(queue: queueName,
                           durable: true,
                           exclusive: false,
                           autoDelete: false,
                           arguments: null);

      _queueName = queueName;

      _basicProperties = _channel.CreateBasicProperties();
      _basicProperties.Persistent = true;
   }
   public void Dispose()
   {
      _channel.Dispose();
      GC.SuppressFinalize(this);
   }

   public void Produce(byte[] message)
   {
      _channel.BasicPublish(exchange: string.Empty,
                           routingKey: _queueName,
                           basicProperties: _basicProperties,
                           body: message);
   }
}
