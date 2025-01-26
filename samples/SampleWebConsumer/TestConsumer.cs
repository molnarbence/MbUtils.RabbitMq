using System.Text;
using MbUtils.RabbitMq.Consumer;

namespace SampleWebConsumer;

public class TestConsumer(ILogger<TestConsumer> logger) : IMessageConsumer
{
   public Task OnMessageAsync(byte[] message)
   {
      var messageReceived = Encoding.UTF8.GetString(message);
      logger.LogInformation("Message received: '{Message}'", messageReceived);
      return Task.CompletedTask;
   }
}
