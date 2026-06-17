using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace FactoryOpsApp.Domain.Entities
{
    [Table("MasterNotification")]
    public class MasterNotification
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int NotificationId { get; set; }

        [Required]
        public int TenantId { get; set; }

        [Required]
        [MaxLength(100)]
        public string? Module { get; set; } = string.Empty;

        public int? EntityId { get; set; }
        public int? ServiceRequestId { get; set; }
        public string? ServiceRequestNumber { get; set; } = string.Empty;


        [Required]
        [MaxLength(255)]
        public string? Title { get; set; } = string.Empty;

        [Required]
        public string? Message { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string NotificationType { get; set; } = string.Empty;
        public string? WorkOrderType { get; set; }
        public string? WorkOrderNumber { get; set; } = string.Empty;
        public string? EventType { get; set; } = string.Empty;
        public string? ItemCode { get; set; } = string.Empty;
        public int? TargetUserId { get; set; }

        public int? TargetTeamId { get; set; }

        public bool IsRead { get; set; } = false;
        public bool IsReadByTenant { get; set; } = false;

        public int? CreatedByUserId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public JsonDocument? AdditionalData { get; set; }
        public int? OutgoingNotification { get; set; }

    }
}
