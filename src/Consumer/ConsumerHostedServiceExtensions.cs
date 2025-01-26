using MbUtils.RabbitMq.Consumer;
using MbUtils.RabbitMq.Consumer.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.Hosting;

public static class ConsumerHostedServiceExtensions
{
   public static IServiceCollection AddRabbitMqConsumer<TConsumer>(this IServiceCollection services, IConfiguration consumerConfiguration) where TConsumer : class, IMessageConsumer
   {
      services.AddOptions<RabbitMqConfiguration<TConsumer>>()
         .Bind(consumerConfiguration);
      
      services
         .AddScoped<TConsumer>()
         .AddSingleton<IConsumerStatusManager, ConsumerStatusManager>()
         .AddHostedService<ConsumerHostedService<TConsumer>>();

      return services;
   }
}
