using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace FactoryOpsApp.Domain.Entities.FactoryOpsTenants
{
    public class MaintenanceSchedule
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ScheduleId { get; set; }
        public int TenantId { get; set; }

        [Required]
        [MaxLength(150)]
        public string ScheduleName { get; set; } = string.Empty;

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ScheduleType ScheduleType { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public FrequencyType Frequency { get; set; }

        public int FrequencyValue { get; set; } = 1;

        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        [MaxLength(200)]
        public string? LocationFilter { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public WorkOrderStatus Status { get; set; } = WorkOrderStatus.Active;

        public DateTime? NextDueDate { get; set; }

        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
        public int? DeletedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public int? AssetId { get; set; }
        [ForeignKey("AssetId")]
        public AssetRegistry? Asset { get; set; }

        public int? PrimarySubTaskId { get; set; }

        [ForeignKey("PrimarySubTaskId")]
        public WorkOrderSubTask? PrimarySubTask { get; set; }

        public string? SelectedSubTaskIds { get; set; }

        [MaxLength(200)]
        public string? ImageUrl { get; set; }

        [MaxLength(200)]
        public string? Image { get; set; }

        public int? WorkOrderId { get; set; }

        [MaxLength(100)]
        public string? WorkOrderNumber { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public InventoryCategoryEnum? Category { get; set; }
        // Navigation
        public ICollection<WorkOrder> WorkOrders { get; set; } = new List<WorkOrder>();
        public ICollection<MaintenanceScheduleOccurrence> Occurrences { get; set; } = new List<MaintenanceScheduleOccurrence>();
    }

    public enum ScheduleType
    {
        TimeBased,
        ConditionBased,
        Usagebased

    }

    public enum FrequencyType
    {
        Daily,
        Weekly,
        Monthly,
        Quarterly,
        Yearly
    }

    public enum ScheduleStatus
    {
        Active,
        Inactive,
        Completed
    }
}