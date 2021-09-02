using MbUtils.RabbitMq.Producer;
using MbUtils.RabbitMq.Producer.Configuration;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection
{
   public static class MessageProducerServiceExtensions
   {
      public static IServiceCollection AddRabbitMqMessageProducer(this IServiceCollection services, IConfiguration configuration)
      {
         return services
            .AddSingleton<IMessageProducerFactory, RabbitMqProducerFactory>()
            .Configure<RabbitMqConfiguration>(configuration.GetSection("RabbitMq"));
      }
   }
}
