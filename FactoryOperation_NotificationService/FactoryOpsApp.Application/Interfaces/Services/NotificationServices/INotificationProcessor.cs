namespace FactoryOperation_NotificationService.FactoryOpsApp.Application.Interfaces.Kafka_Notification
{
    public interface INotificationProcessor
    {
        Task ProcessEventAsync(string topic, byte[] jsonPayload);
    }
}
