namespace FactoryOperation_KafkaMqttService.FactoryOpsApp.Shared.Interfaces
{
    public interface IKafkaConsumerService : IDisposable
    {
        Task StartConsumingAsync(string topic, Func<Confluent.Kafka.ConsumeResult<string, byte[]>, Task> handler, CancellationToken ct);
        Task StopAsync();
    }
}
