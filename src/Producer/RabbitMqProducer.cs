using RabbitMQ.Client;

namespace MbUtils.RabbitMq.Producer;

internal class RabbitMqProducer(IChannel channel, string queueName) : IMessageProducer
{
   private readonly BasicProperties _basicProperties = new() { Persistent = true };

   public async Task ProduceAsync(byte[] message)
   {
      await channel.BasicPublishAsync(exchange: string.Empty, routingKey: queueName, basicProperties: _basicProperties, mandatory: true, body: message);
   }
}
