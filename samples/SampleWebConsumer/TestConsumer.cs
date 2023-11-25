using System.Text;
using MbUtils.RabbitMq.Consumer;

namespace SampleWebConsumer;

public class TestConsumer(ILogger<TestConsumer> logger) : IMessageConsumer
{
   private readonly ILogger<TestConsumer> _logger = logger;

   public Task OnMessageAsync(byte[] message)
   {
      var messageReceived = Encoding.UTF8.GetString(message);
      _logger.LogInformation("Message received: '{Message}'", messageReceived);
      return Task.CompletedTask;
   }
}
