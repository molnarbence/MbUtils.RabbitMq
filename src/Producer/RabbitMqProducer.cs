using RabbitMQ.Client;

namespace MbUtils.RabbitMq.Producer;

internal class RabbitMqProducer(IModel channel, IBasicProperties basicProperties, string queueName) : IMessageProducer, IDisposable
{
   public void Produce(byte[] message)
   {
      channel.BasicPublish(exchange: string.Empty,
         routingKey: queueName,
         basicProperties: basicProperties,
         body: message);
   }

   public void Dispose()
   {
      channel.Dispose();
   }
}
