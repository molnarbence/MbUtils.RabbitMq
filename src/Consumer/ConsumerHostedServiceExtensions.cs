using MbUtils.RabbitMq.Consumer;
using MbUtils.RabbitMq.Consumer.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.Hosting;

public static class ConsumerHostedServiceExtensions
{
   public static IHostApplicationBuilder AddRabbitMqConsumer<TConsumer>(this IHostApplicationBuilder builder, IConfiguration consumerConfiguration, string connectionName) where TConsumer : class, IMessageConsumer
   {
      builder.AddRabbitMQClient(connectionName, configureConnectionFactory: static factory =>
      {
         factory.DispatchConsumersAsync = true;
         factory.AutomaticRecoveryEnabled = true;
      });
      builder.Services.AddOptions<RabbitMqConfiguration<TConsumer>>()
         .Bind(consumerConfiguration);
      
      builder.Services
         .AddScoped<TConsumer>()
         .AddHostedService<ConsumerHostedService<TConsumer>>();

      return builder;
   }
}
