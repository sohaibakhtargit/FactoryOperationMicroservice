using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactoryOpsApp.Application.DTOs
{
    public class NotificationDto
    {
        public int NotificationId { get; set; }
        public int TenantId { get; set; }
        public string Module { get; set; }
        public int? EntityId { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string NotificationType { get; set; }
        public int? TargetUserId { get; set; }
        public int? TargetTeamId { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? AdditionalData { get; set; }
    }
}
