namespace FactoryOperation_NotificationService.FactoryOpsApp.Application.DTOs
{
    public class EmailEventDto
    {
        public string TemplateName { get; set; }
        public dynamic Payload { get; set; }
    }
}
