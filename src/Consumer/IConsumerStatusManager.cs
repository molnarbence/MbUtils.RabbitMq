namespace MbUtils.RabbitMq.Consumer;

public interface IConsumerStatusManager
{
   void Initialized(string hostName, string queueName);
   void StartedListening();
   void StoppedListening();
   void BrokerUnreachable(int retryAttempt);
   void IncrementMessageCount();
   ulong MessageCount { get; }
   CurrentStatusInfo CurrentStatusInfo { get; }
}
