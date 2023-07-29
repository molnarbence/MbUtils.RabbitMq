namespace MbUtils.RabbitMq.Consumer.Configuration;

public class RabbitMqConfiguration<TConsumer> where TConsumer : IMessageConsumer
{
   public string HostName { get; set; }
   public string QueueName { get; set; }
}
