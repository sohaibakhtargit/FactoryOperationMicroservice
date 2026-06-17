using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactoryOpsApp.Application.DTOs
{
    public class CreateAnnouncementDto
    {
        public string Title { get; set; }
        public string Type { get; set; } 
        public string Message { get; set; }
        public List<string> Channels { get; set; } 
        public List<int> TenantIds { get; set; } 
        public bool SendImmediately { get; set; }
        public DateTime? ScheduledTime { get; set; }
        public int? TemplateId { get; set; }
        public bool IsDraft { get; set; } = false;
        public Dictionary<string, string> TemplatePlaceholders { get; set; }
    }

    public class AnnouncementResponseDto
    {
        public int AnnouncementId { get; set; }
        public string Title { get; set; }
        public string Type { get; set; }
        public string Message { get; set; }
        public DateTime? ScheduledTime { get; set; }
        public string Status { get; set; }
        public List<string> Channels { get; set; }
        public List<TenantDto> Tenants { get; set; } = new();
    }

    public class TenantDto
    {
        public int TenantId { get; set; }
        public string TenantName { get; set; }
    }

    public class UpdateAnnouncementDto
    {
        public int AnnouncementId { get; set; }
        public string Title { get; set; }
        public string Type { get; set; }
        public string Message { get; set; }
        public bool SendImmediately { get; set; }
        public DateTime? ScheduledTime { get; set; }
        public List<string> Channels { get; set; }
        public List<int> TenantIds { get; set; }
        public bool IsDraft { get; set; } = false;
        public int? TemplateId { get; set; }
        public Dictionary<string, string> TemplatePlaceholders { get; set; }
    }

}
