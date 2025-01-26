using RabbitMQ.Client;

namespace MbUtils.RabbitMq.Producer;

internal class RabbitMqProducer(IConnection connection, string queueName) : IMessageProducer, IDisposable, IAsyncDisposable
{
   private readonly BasicProperties _basicProperties = new() { Persistent = true };

   private IChannel? _channel;

   public async Task ProduceAsync(byte[] message)
   {
      var channel = await GetChannelAsync();
      
      await channel.BasicPublishAsync(exchange: string.Empty, routingKey: queueName, basicProperties: _basicProperties, mandatory: true, body: message);
   }
   
   private async Task<IChannel> GetChannelAsync()
   {
      if (_channel is null)
      {
         _channel = await connection.CreateChannelAsync();
         await _channel.QueueDeclareAsync(queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);
      }

      if (_channel.IsClosed)
      {
         _channel = await connection.CreateChannelAsync();
      }
      
      return _channel;
   }

   public void Dispose()
   {
      _channel?.Dispose();
   }

   public async ValueTask DisposeAsync()
   {
      if (_channel != null)
      {
         await _channel.DisposeAsync();
      }
   }
}
