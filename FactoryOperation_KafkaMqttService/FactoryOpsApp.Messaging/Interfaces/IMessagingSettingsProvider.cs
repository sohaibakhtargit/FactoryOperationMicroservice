using FactoryOperation_KafkaMqttService.FactoryOpsApp.Messaging.Models;

namespace FactoryOperation_KafkaMqttService.FactoryOpsApp.Messaging.Interfaces
{
    public interface IMessagingSettingsProvider
    {
        MessagingEffectiveSettings GetEffectiveSettings(int? tenantId = null);
        Task ReloadAsync(int? tenantId = null, CancellationToken ct = default);
        event EventHandler<int?>? SettingsChanged; // null => global change
    }
}
