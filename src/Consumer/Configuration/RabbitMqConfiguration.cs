namespace MbUtils.RabbitMq.Consumer.Configuration;

public class RabbitMqConfiguration<TConsumer> where TConsumer : IMessageConsumer
{
   public string HostName { get; init; } = string.Empty;
   public string QueueName { get; init; } = string.Empty;
}
