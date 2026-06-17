using FactoryOpsApp.Application.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactoryOpsApp.Application.DTOs
{
    public class InventoryCostIntegrationDto
    {
        public int WorkOrderPartId { get; set; }
        public int TenantId { get; set; }

        public int WorkOrderId { get; set; }
        public string? WorkOrderName { get; set; }

        public int InventoryId { get; set; }
        public string? PartName { get; set; }

        public int Quantity { get; set; }
        public decimal UnitCost { get; set; }
        //public decimal TotalCost { get; set; }
        public bool IsActive { get; set; }
        public decimal? TotalInventoryValue { get; set; }
    }
    public class InventoryCostSummaryResponse
    {
        public GetAllRecord<InventoryCostIntegrationDto> Response { get; set; } = new();
        public decimal TotalInventoryValue { get; set; }
    }
    public class InventoryItemInfoDto
    {
        public int TenantId { get; set; }
        public int InventoryId { get; set; }
        public string? Catagory {  get; set; }
        public int Stock { get; set; }
        public string? PartName { get; set; }
        public string? PartNumber {  get; set; } 
        public decimal UnitCost { get; set; }
        public string? Status { get; set; }
        public decimal TotalCost {  get; set; }
    }
    public class WorkOrderCostIntegrationDto
    {
        public int TenantId { get; set; }
        public string? WorkOredrNumber { get; set; }
        public string? Title { get; set; }
        public string? Status { get; set; }
        public int? AssignedToUserId { get; set; }
        public string? AssigedToUserName { get; set; }
        public int? AssignedToTeamId { get; set; }
        public string? AssigedToTeam { get; set; }
        public decimal? PartCost { get; set; }
        public decimal? TotalCost { get; set; }
        public decimal? LabourCost {  get; set; }
    }
    public class CreateInventoryCostIntegrationDto
    {
        public int TenantId { get; set; }
        public int WorkOrderPartId { get; set; }
        public int WorkOrderId { get; set; }
        public int InventoryId { get; set; }
        public string? WorkOrderName { get; set; }
        public string? PartName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitCost { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public int? CreatedBy { get; set; }
        [Column(TypeName = "timestamp without time zone")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int? UpdatedBy { get; set; }
        [Column(TypeName = "timestamp without time zone")]
        public DateTime UpdatedAt { get;set; } = DateTime.UtcNow;

    }

    public class CostReportDto
    {
        public CostDetailsDto? CostAnalysis { get; set; }
        public CostPerCategoryDto? CostPerCategory { get; set; }
    }

    public class CostDetailsDto
    {
        public decimal TotalLaborCosts { get; set; }
        public decimal TotalPartsCosts { get; set; }
        public decimal TotalCosts { get; set; }
    }

    public class CostPerCategoryDto
    {
        public decimal PreventiveMaintenance { get; set; }
        public decimal EmergencyRepairs { get; set; }
        public decimal RoutineMaintenance { get; set; }
    }

    public class WorkOrderPartUsageDto
    {
        public int TenantId { get; set; }
        public int WorkOrderId { get; set; }
        public string? WorkOrderNumber { get; set; }
        public string? WorkOrderTitle { get; set; }

        public int? ToolId { get; set; }
        public string? PartName { get; set; }
        public string? PartNumber { get; set; }

        public int QuantityUsed { get; set; }
        public decimal UnitCost { get; set; }
        public decimal PartCost { get; set; }

        public decimal LaborCost { get; set; }
        public decimal TotalCost { get; set; }
    }

    //

    public class WorkOrderDashboardDto
    {
        public List<WorkOrderTypeSummaryDto> WorkOrdersByType { get; set; } = new();
        public List<WorkOrderPrioritySummaryDto> WorkOrdersByPriority { get; set; } = new();
    }

    public class WorkOrderTypeSummaryDto
    {
        public string? Type { get; set; }
        public int Count { get; set; }
        public decimal Percentage { get; set; }
    }

    public class WorkOrderPrioritySummaryDto
    {
        public string? Priority { get; set; }
        public int Count { get; set; }
        public decimal Percentage { get; set; }
    }
}


