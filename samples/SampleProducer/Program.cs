using System;
using System.Threading.Tasks;
using MbUtils.Extensions.CommandLineUtils;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;

namespace SampleProducer;

[Command("sample-producer")]
[Subcommand(typeof(ProduceCommand))]
public class Program
{
   static Task<int> Main(string[] args)
   {
      var wrapper = new CommandLineApplicationWrapper<Program>(args);

      wrapper.HostBuilder.ConfigureServices((hostBuilderContext, services) => services
         .AddSingleton<IReporter, ConsoleReporter>()
         .AddRabbitMqMessageProducer(hostBuilderContext.Configuration)
         );

      return wrapper.ExecuteAsync();
   }
}
