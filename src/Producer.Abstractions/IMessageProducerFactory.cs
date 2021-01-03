using System;

namespace MbUtils.RabbitMq.Producer
{
   public interface IMessageProducerFactory : IDisposable
   {
      IMessageProducer Create(string queueName);
   }
}
