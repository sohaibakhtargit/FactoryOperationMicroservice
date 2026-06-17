namespace FactoryOperation_WorkOrder.FactoryOpsApp.Application.Models
{
    public class NotificationUsersResult
    {
        public int? Outgoing { get; set; }
        public List<int?> Incoming { get; set; } = new();
        public List<int?> AllUsers { get; set; } = new();
        public List<string> Emails { get; set; } = new();
    }
}
