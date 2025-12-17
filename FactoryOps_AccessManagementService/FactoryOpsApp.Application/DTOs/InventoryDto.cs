using FactoryOpsApp.Domain.Entities;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace FactoryOpsApp.Application.DTOs
{
    public class InventoryDto
    {
        public int ItemId { get; set; }
        public int TenantId { get; set; }
        public string ItemCode { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public string Manufacturer { get; set; } = string.Empty;
        public int? LocationId { get; set; }
        public string? Location { get; set; }
        public InventoryCategoryEnum? Category { get; set; }
        public InventoryStatusEnum Status { get; set; } = InventoryStatusEnum.Active;
        public int QuantityAvailable { get; set; }
        public int ReorderLevel { get; set; }

        public int MaxStockLevel { get; set; } = 100;
        public int ReservedQuantity { get; set; } = 0;

        [NotMapped]
        public int AvailableQuantity => QuantityAvailable - ReservedQuantity;

        public decimal UnitPrice { get; set; }
        public decimal? MonthlyConsumption { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public int UpdatedBy { get; set; }
    }
}