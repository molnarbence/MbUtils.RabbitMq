using System.Threading.Tasks;
using MbUtils.RabbitMq.Consumer;
using Microsoft.Extensions.Hosting;

namespace SampleConsumer
{
   class Program
   {
      static async Task Main(string[] args)
      {
         var hostBuilder = new ConsumerHostBuilder(args).AddRabbitMqConsumer<TestConsumer>();
         await hostBuilder.RunConsoleAsync();
      }
   }
}
