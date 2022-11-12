using System.Threading.Tasks;

namespace MbUtils.RabbitMq.Consumer;

public interface IMessageConsumer
{
   Task OnMessageAsync(byte[] message);
}
