using System;

namespace MbUtils.RabbitMq.Producer;

public interface IMessageProducer : IDisposable
{
   void Produce(byte[] message);
}
