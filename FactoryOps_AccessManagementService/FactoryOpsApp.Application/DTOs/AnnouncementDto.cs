namespace FactoryOpsApp.Application.DTOs.SuperAdmin
{
    public class AddAnnouncementDto
    {
        public string Title { get; set; } = string.Empty;
        public string Type { get; set; } = "info"; 
        public string Message { get; set; } = string.Empty;
        public DateTime? ScheduledTime { get; set; }
        public string? Status { get; set; } = "draft"; 
        public int CreatedBy { get; set; }

        public List<string> Channels { get; set; } = new();    
        public List<int> TenantIds { get; set; } = new();      
    }

    public class GetAnnouncementDto
    {
        public int AnnouncementId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime? ScheduledTime { get; set; }
        public string Status { get; set; } = string.Empty;


        public List<string> Channels { get; set; } = new();
        public List<int> TenantIds { get; set; } = new();
    }

    public class AddAnnouncementChannelDto
    {
        public int AnnouncementId { get; set; }
        public string ChannelType { get; set; } = string.Empty;
    }

    public class AddAnnouncementTenantDto
    {
        public int AnnouncementId { get; set; }
        public int TenantId { get; set; }
    }
}
