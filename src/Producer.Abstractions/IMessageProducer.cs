using System;
using System.Threading.Tasks;

namespace MbUtils.RabbitMq.Producer;

public interface IMessageProducer
{
   Task ProduceAsync(byte[] message);
}
