using System;
using System.Text;
using System.Threading.Tasks;
using MbUtils.RabbitMq.Producer;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;

namespace SampleProducer
{
   [Command("produce")]
   [HelpOption]
   public class ProduceCommand
   {
      private readonly IConsole _console;
      private readonly ILogger<ProduceCommand> _logger;
      private readonly IMessageProducerFactory _messageProducerFactory;
      private readonly IReporter _reporter;

      public ProduceCommand(IConsole console, ILogger<ProduceCommand> logger, IMessageProducerFactory messageProducerFactory, IReporter reporter)
      {
         _console = console;
         _logger = logger;
         _messageProducerFactory = messageProducerFactory;
         _reporter = reporter;
      }
      public async Task<int> OnExecuteAsync()
      {
         try
         {
            await SendTestMessageAsync("test");
            await SendTestMessageAsync("test2");
            return 0;
         }
         catch (Exception)
         {
            return 1;
         }
      }

      private async Task SendTestMessageAsync(string queueName)
      {
         _console.WriteLine($"Creating producer to queue '{queueName}'.");
         using var producer = await _messageProducerFactory.CreateAsync(queueName);
         try
         {
            var messageToSend = Encoding.UTF8.GetBytes("Hello world!");

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
}
