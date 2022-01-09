using System.Text;
using System.Threading.Tasks;
using MbUtils.RabbitMq.Consumer;
using Microsoft.Extensions.Logging;

namespace SampleConsumer
{
   public class TestConsumer : IMessageConsumer
   {
      private readonly ILogger<TestConsumer> _logger;

      public TestConsumer(ILogger<TestConsumer> logger)
      {
         _logger = logger;
      }

      public Task OnMessageAsync(byte[] message)
      {
         var messageReceived = Encoding.UTF8.GetString(message);
         _logger.LogInformation("Message received: '{Message}'", messageReceived);
         return Task.CompletedTask;
      }
   }
}
