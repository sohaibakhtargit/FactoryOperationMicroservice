using FactoryOpsApp.Domain.Entities.FactoryOpsTenants;
using System;
using System.ComponentModel.DataAnnotations;

namespace FactoryOpsApp.Application.DTOs
{
    public class CreateReorderRuleDto
    {
        [Required]
        public int TenantId { get; set; }
        [Required]
        public int InventoryId { get; set; }
        [Required]
        public int MinThreshold { get; set; }
        [Required]
        public int ReorderQuantity { get; set; }
        [Required]
        public int SupplierManagementId { get; set; }
   
        [Range(0, 365)]
        public int LeadTimeDays { get; set; } = 0;

        [Range(0, double.MaxValue)]
        public decimal LastPrice { get; set; } = 0;
        [Required]
        public ReorderPriority Priority { get; set; } = ReorderPriority.Medium;
        [Required]
        public bool AutoGenerateOrders { get; set; }
        public int CreatedBy { get; set; }
    }

    public class UpdateReorderRuleDto
    {
        [Required]
        public int TenantId { get; set; }
        [Required]
        public int ReorderRuleId { get; set; }
        public int? MinThreshold { get; set; }
        public int? ReorderQuantity { get; set; }
        public int? SupplierManagementId { get; set; }
        [Range(0, 365)]
        public int? LeadTimeDays { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? LastPrice { get; set; }
        public ReorderPriority Priority { get; set; } = ReorderPriority.Medium;
        public bool? AutoGenerateOrders { get; set; }
        public bool? IsActive { get; set; }
        public int UpdatedBy { get; set; }
    }

    public class ReorderRuleResponseDto
    {
        public int ReorderRuleId { get; set; }
        public int TenantId { get; set; }
        public int InventoryId { get; set; }
        public string? PartCode { get; set; }
        public string? PartName { get; set; }
        public int MinThreshold { get; set; }
        public int ReorderQuantity { get; set; }
        public int SupplierManagementId { get; set; }
        public string? SupplierName { get; set; }
        public ReorderPriority Priority { get; set; } = ReorderPriority.Medium;
        public bool AutoGenerateOrders { get; set; }
        public bool IsActive { get; set; }
        public int LeadTimeDays { get; set; }
        public decimal? LastPrice { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class PurchaseRequisitionResponseDto
    {
        public int PurchaseRequisitionId { get; set; }
        public string RequisitionId { get; set; } = string.Empty;
        public string? PartCode { get; set; }
        public string? PartName { get; set; }
        public int Quantity { get; set; }
        public string? SupplierName { get; set; }
        public decimal EstimatedCost { get; set; }
        public ReorderPriority Priority { get; set; } = ReorderPriority.Medium;
        public DateOnly GeneratedDate { get; set; }
        public DateOnly ExpectedDeliveryDate { get; set; }
        public RequisitionStatus Status { get; set; } = RequisitionStatus.Pending;
    }

    public class AutomatedReplenishmentDashboardDto
    {
        public int ActiveRules { get; set; }
        public int PendingOrders { get; set; }
        public int ThisMonthOrders { get; set; }
        public decimal CostSavings { get; set; }
        public List<ReorderRuleResponseDto> ReorderRules { get; set; } = new();
        public List<PurchaseRequisitionResponseDto> PurchaseRequisitions { get; set; } = new();
    }
    public class PurchaseRequest
    {
        public int TenantId { get; set; }
        public int PurchaseRequisitionId { get; set; }
        public string RequisitionId { get; set; } = string.Empty;
        public int InventoryId { get; set; }
        public int ReorderRuleId { get; set; }
        public int Quantity { get; set; }
        public int SupplierManagementId { get; set; }
        public decimal EstimatedCost { get; set; }
        public ReorderPriority Priority { get; set; } = ReorderPriority.Medium;
        public DateOnly GeneratedDate { get; set; }
        public DateOnly ExpectedDeliveryDate { get; set; } 
        public RequisitionStatus Status { get; set; } = RequisitionStatus.Pending;
        public ManagerAprovalStatus ManagerAprovalStatus { get; set; } = ManagerAprovalStatus.Pending;
        public SupplierAcceptanceStatus SupplierAcceptanceStatus { get; set; } = SupplierAcceptanceStatus.Pending;
        public string? SupplierComment { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public int? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public int? UpdatedBy { get; set; }
        public int? DeletedBy { get; set; }
        public DateTime? DeletedDatet { get; set; } 
        public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;
    }
}