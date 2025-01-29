using MbUtils.RabbitMq.Producer;
using MbUtils.RabbitMq.Producer.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Microsoft.Extensions.DependencyInjection;

public static class MessageProducerServiceExtensions
{
   public static IHostApplicationBuilder AddRabbitMqMessageProducer(this IHostApplicationBuilder builder, string connectionName)
   {
      builder.AddRabbitMQClient(connectionName);

      builder.Services.AddSingleton<IMessageProducerFactory, RabbitMqProducerFactory>();

      return builder;
   }
}
