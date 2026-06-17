using FactoryOperation_NotificationService.FactoryOpsApp.Common.Models;

namespace FactoryOperation_NotificationService.FactoryOpsApp.Application.Interfaces.Kafka_Notification
{
    public interface INotificationRepository
    {
        // Task SaveAsync(NotificationModel model);
        Task<int> SaveAsync(NotificationModel model);
    }
}
