
using System.Text;
using MbUtils.RabbitMq.Consumer;

namespace SampleWebConsumer;

public class TestConsumer2 : IMessageConsumer
{
   private readonly ILogger<TestConsumer> _logger;

   public TestConsumer2(ILogger<TestConsumer> logger)
   {
      _logger = logger;
   }

   public Task OnMessageAsync(byte[] message)
   {
      var messageReceived = Encoding.UTF8.GetString(message);
      _logger.LogInformation("TestConsumer2, Message received: '{Message}'", messageReceived);
      return Task.CompletedTask;
   }
}
