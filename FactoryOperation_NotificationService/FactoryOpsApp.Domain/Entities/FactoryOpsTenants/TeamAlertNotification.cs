using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactoryOpsApp.Domain.Entities.FactoryOpsTenants
{
    [Table("TeamAlertNotifications")]
    public class TeamAlertNotification
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TeamAlertNotificationId { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Message { get; set; } = string.Empty;
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
        public bool IsDeleted { get; set; } = false;
        public int TenantId { get; set; }
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
        public int? DeletedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("SentToUserId")]
        public virtual FactoryUsers? SentToUser { get; set; }

        [ForeignKey("SentToTeamId")]
        public virtual FactoryTeam? SentToTeam { get; set; }
    }

    public enum NotificationType
    {
        AlertRule,
        SystemNotification,
        TaskReminder,
        AuditReminder,
        SafetyAlert,
        TeamAchievement,
        SystemMaintenance
    }

    public enum TriggerType
    {
        OverdueTask,
        UpcomingDeadline,
        SafetyIncident,
        TeamAchievement,
        EquipmentFailure,
        WeeklySummary
    }

    public enum Priority
    {
        None,
        Low,
        Medium,
        High,
        Critical
    }
}

