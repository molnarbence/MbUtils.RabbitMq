using System;
using System.Threading.Tasks;

namespace MbUtils.RabbitMq.Producer;

public interface IMessageProducer
{
   void Produce(byte[] message);
}
