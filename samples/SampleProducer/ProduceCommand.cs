using System.Text;
using MbUtils.RabbitMq.Producer;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;

namespace SampleProducer;

[Command("produce")]
[HelpOption]
public class ProduceCommand(IConsole console, ILogger<ProduceCommand> logger, IMessageProducerFactory messageProducerFactory, IReporter reporter)
{
   public async Task<int> OnExecuteAsync()
   {
      try
      {
         await SendTestMessageAsync("web", "message for web");
         await SendTestMessageAsync("web2", "message for web2");
         return 0;
      }
      catch (Exception)
      {
         return 1;
      }
   }

   private async Task SendTestMessageAsync(string queueName, string textMessage)
   {
      console.WriteLine($"Creating producer to queue '{queueName}'.");
      var producer = await messageProducerFactory.CreateAsync(queueName);
      try
      {
         var messageToSend = Encoding.UTF8.GetBytes(textMessage);

         await producer.ProduceAsync(messageToSend);

         console.WriteLine("Message sent");
      }
      catch (Exception ex)
      {
         reporter.Error(ex.Message);
         logger.LogError(ex, nameof(OnExecuteAsync));
         throw;
      }
   }
}
