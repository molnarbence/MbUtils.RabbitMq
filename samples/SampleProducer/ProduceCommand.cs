using System;
using System.Collections.Generic;
using System.Linq;
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
      public int OnExecute(IMessageProducerFactory messageProducerFactory, IReporter reporter, IConsole console, ILogger<ProduceCommand> logger)
      {
         console.WriteLine("Creating producer to queue 'test'.");
         using var producer = messageProducerFactory.Create("test");
         try
         {
            var messageToSend = Encoding.UTF8.GetBytes("Hello world!");

            producer.Produce(messageToSend);

            console.WriteLine("Message sent");
            return 0;
         }
         catch (Exception ex)
         {
            reporter.Error(ex.Message);
            logger.LogError(ex, nameof(OnExecute));
            return 1;
         }
      }
   }
}
