namespace MbUtils.RabbitMq.Consumer.Configuration;

public class RabbitMqConfiguration<TConsumer> where TConsumer : IMessageConsumer
{
   public string QueueName { get; init; } = string.Empty;
}
