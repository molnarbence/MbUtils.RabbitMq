using MbUtils.RabbitMq.Consumer;
using MbUtils.RabbitMq.Consumer.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.Hosting;

public static class ConsumerHostedServiceExtensions
{
   public static IServiceCollection AddRabbitMqConsumer<TConsumer>(this IServiceCollection services, IConfigurationSection consumerConfigurationSection) where TConsumer : class, IMessageConsumer
      => services
            .Configure<RabbitMqConfiguration>(consumerConfigurationSection)
            .AddScoped<IMessageConsumer, TConsumer>()
            .AddSingleton<IConsumerStatusManager, ConsumerStatusManager>()
            .AddHostedService<ConsumerHostedService>();
}
