using MbUtils.RabbitMq.Consumer;
using MbUtils.RabbitMq.Consumer.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.Hosting
{
   public static class ConsumerHostBuilderExtensions
   {
      public static IHostBuilder AddRabbitMqConsumer<TConsumer>(this IHostBuilder hostBuilder) where TConsumer : class, IMessageConsumer
      {
         return hostBuilder.ConfigureServices((context, services) =>
         {
            services
               .Configure<RabbitMqConfiguration>(context.Configuration.GetSection("RabbitMq"))
               .AddScoped<IMessageConsumer, TConsumer>()
               .AddHostedService<ConsumerHostedService>();
         });
      }
   }
}
