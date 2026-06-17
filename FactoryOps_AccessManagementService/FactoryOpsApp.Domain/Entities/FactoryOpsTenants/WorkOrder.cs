using FactoryOps_AccessManagementService.FactoryOpsApp.Domain.Entities.FactoryOpsTenants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace FactoryOpsApp.Domain.Entities.FactoryOpsTenants
{
    public class WorkOrder
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int WorkOrderId { get; set; }
        public int TenantId { get; set; }

        [Required]
        [MaxLength(20)]
        public string WorkOrderNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public int? LocationId { get; set; }
        [ForeignKey("LocationId")]
        public Location? Location { get; set; }

        public WorkOrderStatus? Status { get; set; } = WorkOrderStatus.Open;

        public PriorityLevel? Priority { get; set; } = PriorityLevel.Medium;

        public int? AssignedToUserId { get; set; }
        [ForeignKey("AssignedToUserId")]
        public FactoryUsers? AssignedToUser { get; set; }
        public int? AssignedToTeamId { get; set; }
        [ForeignKey("AssignedToTeamId")]
        public FactoryTeam? AssignedToTeam { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? ScheduleDate { get; set; }
        public int? EstimatedDurationMinutes { get; set; }

        [MaxLength(1000)]
        public string? Instructions { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public WorkOrderTypeEnum? WorkOrderType { get; set; } = WorkOrderTypeEnum.Preventive;

        public string? FileType { get; set; }

        public decimal? LaborCost { get; set; } = 0;
        public decimal? PartCost { get; set; } = 0;
        public decimal? TotalCost { get; set; } = 0;
        public int? BulkImportId { get; set; }

        [ForeignKey("BulkImportId")]
        public WorkOrderBulkImport? BulkImport { get; set; }
        public int? MaintenanceScheduleId { get; set; }
        [ForeignKey("MaintenanceScheduleId")]
        public MaintenanceSchedule? MaintenanceSchedule { get; set; }

        public int? AssetId { get; set; }
        [ForeignKey("AssetId")]
        public AssetRegistry? Asset { get; set; }
        public int? ServiceRequestId { get; set; }
        [ForeignKey("ServiceRequestId")]
        public ServiceRequest? ServiceRequest { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
        public int? DeletedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<MaintenanceTask>? Tasks { get; set; }
        public ICollection<WorkOrderSubTask>? WorkOrderSubTasks { get; set; }
        public ICollection<WorkOrderRequiredTool>? RequiredTools { get; set; }
        public ICollection<WorkOrderProgressUpdates>? WorkOrderProgressUpdates { get; set; }

        public UpdateTypeEnum? UpdateType { get; set; }
        public Decimal? ProgressPercentage { get; set; }
        public string? LastUpdateMessage { get; set; }
        public string? WorkOrderMedia { get; set; }
        public string? WorkOrderMediaPath { get; set; }
        public string? WorkOrderProgressMedia { get; set; }
        public string? WorkOrderProgressMediaPath { get; set; }
        [MaxLength(1000)]
        public string? CompletionNotes { get; set; }
        public string? Comments { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? PauseTime { get; set; }
        public bool IsStarted { get; set; } = false;
        public bool IsPaused { get; set; } = false;
        public bool IsReminderSent { get; set; } = false;
        public string? TotalTime { get; set; }
        public Decimal? WorkOrderProgressStatus { get; set; }
        public DateTime? CompletedAt { get; set; }
        public bool IsApprovalReminderSent { get; set; }


    }
}
public enum UpdateTypeEnum
{
    ProgressUpdate,
    StatusChange,
    Comment,
    Attachment
}

public enum WorkOrderStatus
{
    Started,
    Pending,
    Assigned,
    InProgress,
    Completed,
    Cancelled,
    Overdue,
    Active,
    Inactive,
    Open,
    OnHold,
    Closed,
    ReOpened
}

public enum WorkOrderTypeEnum
{
    Preventive,
    Corrective,
    Maintenance,
    Repair,
    Inspection,
    Cleaning,
    Other
}

public enum PriorityLevel
{
    Low,
    Medium,
    High,
    Critical
}
