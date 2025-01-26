
using System.Text;
using MbUtils.RabbitMq.Consumer;

namespace SampleWebConsumer;

public class TestConsumer2(ILogger<TestConsumer2> logger) : IMessageConsumer
{
   public Task OnMessageAsync(byte[] message)
   {
      var messageReceived = Encoding.UTF8.GetString(message);
      logger.LogInformation("TestConsumer2, Message received: '{Message}'", messageReceived);
      return Task.CompletedTask;
   }
}
