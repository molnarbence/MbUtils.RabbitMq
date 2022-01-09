using System;

namespace MbUtils.RabbitMq.Consumer;

internal class ConsumerStatusManager : IConsumerStatusManager
{
   private ulong _messageCount = 0;
   public ulong MessageCount => _messageCount;

   private int _retryAttempt = 0;
   public int RetryAttempt => _retryAttempt;

   public CurrentStatusInfo CurrentStatusInfo 
      => new(
         _status, 
         _status == ConsumerStatus.Listening, 
         _hostName, 
         _queueName, 
         _messageCount, 
         _status == ConsumerStatus.BrokerUnreachable ? _retryAttempt : 0
         );

   private ConsumerStatus _status = ConsumerStatus.Empty;
   private string _hostName = string.Empty;
   private string _queueName = string.Empty;

   public void BrokerUnreachable(int retryAttempt)
   {
      _retryAttempt = retryAttempt;
      _status = ConsumerStatus.BrokerUnreachable;
   }

   public void IncrementMessageCount() => _messageCount++;

   public void Initialized(string hostName, string queueName)
   {
      _status = ConsumerStatus.Initialized;
      _hostName = hostName;
      _queueName = queueName;
   }

   public void StartedListening() => _status = ConsumerStatus.Listening;

   public void StoppedListening() => _status = ConsumerStatus.Stopped;
}

public enum ConsumerStatus
{
   Empty,
   Initialized,
   Listening,
   BrokerUnreachable,
   Stopped
}

public record CurrentStatusInfo(ConsumerStatus Status, bool IsReady, string HostName, string QueueName, ulong MessageCount, int RetryAttempt);