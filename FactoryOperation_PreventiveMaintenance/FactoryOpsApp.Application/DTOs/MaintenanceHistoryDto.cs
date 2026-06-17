using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FactoryOpsApp.Domain.Entities.FactoryOpsTenants;
using System;
using System.ComponentModel.DataAnnotations;

namespace FactoryOpsApp.Application.DTOs
{
    public class MaintenanceHistoryDto
    {
        [Required]
        public int AssetId { get; set; }

        public string? WorkOrderId { get; set; }

        [Required]
        public MaintenanceTypeEnum MaintenanceType { get; set; }

        public int? TechnicianId { get; set; }
        public string? Description { get; set; }
        public string? PartsUsed { get; set; }
        public MaintenancePriorityEnum? Priority { get; set; }
        public decimal? LaborHours { get; set; }
        public decimal? Cost { get; set; }
        public string? PerformedBy { get; set; }
        public DateTime PerformedOn { get; set; }
        public int TenantId { get; set; }
        public int? CreatedBy { get; set; }
    }

    public class UpdateMaintenanceHistoryDto : MaintenanceHistoryDto
    {
        public long MaintenanceId { get; set; }
        public int? UpdatedBy { get; set; }
    }

    public class GetMaintenanceHistoryDto
    {
        public long MaintenanceId { get; set; }
        public int AssetId { get; set; }
        public int TenantId { get; set; }
        public string AssetName { get; set; } = string.Empty;
        public int? TechnicianId { get; set; }
        public string? Technician { get; set; }
        public string? WorkOrderId { get; set; }
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
        public string Status { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string PerformedOnFormatted { get; set; } = string.Empty;
    }

    public class MaintenanceMetricsDto
    {
        public int TenantId { get; set; }
        public decimal AvgMTBF { get; set; }
        public decimal AvgMTTR { get; set; }
        public decimal TotalCost { get; set; }
        public decimal Efficiency { get; set; }
        public int TotalMaintenanceCount { get; set; }
        public int CompletedCount { get; set; }
        public int InProgressCount { get; set; }
    }
}