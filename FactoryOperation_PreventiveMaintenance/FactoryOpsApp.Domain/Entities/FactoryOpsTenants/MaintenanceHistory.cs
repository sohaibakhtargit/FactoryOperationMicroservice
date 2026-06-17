using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FactoryOpsApp.Domain.Entities.FactoryOpsTenants
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum MaintenancePriorityEnum
    {
        High,
        Medium,
        Low
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum MaintenanceTypeEnum
    {
        Preventive,
        Corrective,
        Predictive
    }

    [Table("MaintenanceHistory")]
    public class MaintenanceHistory
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long MaintenanceId { get; set; }

        public int TenantId { get; set; }
        public int AssetId { get; set; }
        [ForeignKey("AssetId")]
        public AssetRegistry Asset { get; set; }

        public int? Technician { get; set; }
        [ForeignKey("Technician")]
        public FactoryUsers? Technicians { get; set; }

        public string? WorkOrderId { get; set; }
        [Required]
        [MaxLength(30)]
        public MaintenanceTypeEnum MaintenanceType { get; set; } 

        public string? Description { get; set; }
        public string? PartsUsed { get; set; }
        public MaintenancePriorityEnum? Priority { get; set; }
        public decimal? LaborHours { get; set; }
        public decimal? Cost { get; set; }
        public string? PerformedBy { get; set; }
        public DateTime PerformedOn { get; set; }
        public decimal? MTBF { get; set; }
        public decimal? MTTR { get; set; }

        public DateTime? FailureReportedOn { get; set; }   
        public DateTime? RepairCompletedOn { get; set; }  


        // Audit Fields
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
        public int? DeletedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}