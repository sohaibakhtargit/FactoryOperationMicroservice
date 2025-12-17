using FactoryOpsApp.Domain.Entities.FactoryOpsTenants;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactoryOpsApp.Application.DTOs
{
    public class CreateTeamAlertNotificationDto
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Message { get; set; } = string.Empty;

        [Required]
        public NotificationType? NotificationType { get; set; }
        public Priority? Priority { get; set; }
        public TriggerType? TriggerType { get; set; }
        public int? ThresholdMinutes { get; set; }
        public string? ConditionValue { get; set; }
        public string? ConditionUnit { get; set; }
        public string? NotificationChannels { get; set; } = "InApp";
        public bool IsAlertRule { get; set; } = false;
        public string? TargetType { get; set; }
        public int? TargetId { get; set; }
        public int? SentToUserId { get; set; }
        public int? SentToTeamId { get; set; }
        public bool SentToAll { get; set; } = false;

        [Required]
        public int TenantId { get; set; }

        [Required]
        public int CreatedBy { get; set; }
    }

    public class UpdateTeamAlertNotificationDto
    {
        [Required]
        public int TeamAlertNotificationId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Message { get; set; } = string.Empty;

        [Required]
        public NotificationType? NotificationType { get; set; }
        public Priority? Priority { get; set; }
        public TriggerType? TriggerType { get; set; }
        public int? ThresholdMinutes { get; set; }
        public string? ConditionValue { get; set; }
        public string? ConditionUnit { get; set; }
        public string? NotificationChannels { get; set; } = "InApp";
        public bool IsAlertRule { get; set; } = false;
        public string? TargetType { get; set; }
        public int? TargetId { get; set; }
        public bool IsRead { get; set; } = false;
        public int? SentToUserId { get; set; }
        public int? SentToTeamId { get; set; }
        public bool SentToAll { get; set; } = false;
        public bool IsActive { get; set; } = true;

        [Required]
        public int TenantId { get; set; }

        [Required]
        public int UpdatedBy { get; set; }
    }

    public class GetTeamAlertNotificationDto
    {
        public int TeamAlertNotificationId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public NotificationType? NotificationType { get; set; }
        public Priority? Priority { get; set; }
        public TriggerType? TriggerType { get; set; }
        public int? ThresholdMinutes { get; set; }
        public string? ConditionValue { get; set; }
        public string? ConditionUnit { get; set; }
        public string? NotificationChannels { get; set; }
        public bool IsAlertRule { get; set; }
        public string? TargetType { get; set; }
        public int? TargetId { get; set; }
        public bool IsRead { get; set; }
        public int? SentToUserId { get; set; }
        public int? SentToTeamId { get; set; }
        public bool SentToAll { get; set; }
        public bool IsActive { get; set; }
        public int TenantId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? SentToUserName { get; set; }
        public string? SentToTeamName { get; set; }
    }
}
