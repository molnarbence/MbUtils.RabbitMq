using System;
using System.Threading.Tasks;

namespace MbUtils.RabbitMq.Producer;

public interface IMessageProducerFactory : IDisposable
{
   Task<IMessageProducer> CreateAsync(string queueName);
}
