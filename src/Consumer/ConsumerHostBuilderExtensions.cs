using MbUtils.RabbitMq.Consumer;
using MbUtils.RabbitMq.Consumer.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.Hosting;

public static class ConsumerHostBuilderExtensions
{
   public static IHostBuilder AddRabbitMqConsumer<TConsumer>(this IHostBuilder hostBuilder) where TConsumer : class, IMessageConsumer
    => hostBuilder.ConfigureServices((context, services) => services.AddRabbitMqConsumer<TConsumer>(context.Configuration));
   

   public static IServiceCollection AddRabbitMqConsumer<TConsumer>(this IServiceCollection services, IConfiguration configuration) where TConsumer : class, IMessageConsumer
      => services
            .Configure<RabbitMqConfiguration>(configuration.GetSection("RabbitMq"))
            .AddScoped<IMessageConsumer, TConsumer>()
            .AddSingleton<IConsumerStatusManager, ConsumerStatusManager>()
            .AddHostedService<ConsumerHostedService>();
}
