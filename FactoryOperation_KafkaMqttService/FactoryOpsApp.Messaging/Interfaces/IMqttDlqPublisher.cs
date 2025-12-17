namespace FactoryOperation_KafkaMqttService.FactoryOpsApp.Messaging.Interfaces
{
    public interface IMqttDlqPublisher
    {
        Task PublishAsync(int tenantId, string originalTopic, byte[] payload, string reason, CancellationToken ct = default);
    }
}
