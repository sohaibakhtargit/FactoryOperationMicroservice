namespace FactoryOpsApp.Application.DTOs
{
    public class MessagingGlobalSettingsDto
    {
        public int GlobalSettingId { get; set; }
        public string? BootstrapServers { get; set; }
        public string? Topic { get; set; }
        public bool? UsePerTenantTopic { get; set; }
        public string? DlqTopic { get; set; }
        public bool? EnableSSL { get; set; }
    }
}