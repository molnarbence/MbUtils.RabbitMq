using System;
using System.Text;
using System.Threading.Tasks;
using MbUtils.RabbitMq.Producer;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;

namespace SampleProducer;

[Command("produce")]
[HelpOption]
public class ProduceCommand(IConsole console, ILogger<ProduceCommand> logger, IMessageProducerFactory messageProducerFactory, IReporter reporter)
{
   private readonly IConsole _console = console;
   private readonly ILogger<ProduceCommand> _logger = logger;
   private readonly IMessageProducerFactory _messageProducerFactory = messageProducerFactory;
   private readonly IReporter _reporter = reporter;

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
      _console.WriteLine($"Creating producer to queue '{queueName}'.");
      using var producer = await _messageProducerFactory.CreateAsync(queueName);
      try
      {
         var messageToSend = Encoding.UTF8.GetBytes(textMessage);

         producer.Produce(messageToSend);

         _console.WriteLine("Message sent");
      }
      catch (Exception ex)
      {
         _reporter.Error(ex.Message);
         _logger.LogError(ex, nameof(OnExecuteAsync));
         throw;
      }
   }
}
