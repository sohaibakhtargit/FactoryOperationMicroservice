using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactoryOpsApp.Application.DTOs
{
    public class ResourceUsageAnalyticsDto
    {
        public List<ResourceUsageDto> ResourceUsage { get; set; } = new();
        public List<ResourceCategoryUtilizationDto> CategoryUtilization { get; set; } = new();
        public int TotalResources { get; set; }
        public int TotalItems { get; set; }
        public int ItemsInUse { get; set; }
        public decimal TotalInventoryValue { get; set; }
        public double AverageUtilization { get; set; }
        public List<ResourceUsageDto> LowStockItems { get; set; } = new();
        public List<ResourceUsageDto> OutOfStockItems { get; set; } = new();
        public int LowStockCount { get; set; }
        public int OutOfStockCount { get; set; }
        public ResourceSummaryDto Summary { get; set; } = new();
    }

    public class ResourceUsageDto
    {
        public int ResourceId { get; set; }
        public string ResourceName { get; set; } = string.Empty;
        public string ResourceCode { get; set; } = string.Empty;
        public string ResourceType { get; set; } = string.Empty;
        public string Manufacturer { get; set; } = string.Empty;
        public int TotalQuantity { get; set; }
        public int InUse { get; set; }
        public int Available { get; set; }
        public double Utilization { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalValue { get; set; }
        public string Status { get; set; } = string.Empty;
        public int ReorderLevel { get; set; }
        public int MaxStockLevel { get; set; }
        public string Location { get; set; } = string.Empty;
    }

    public class ResourceCategoryUtilizationDto
    {
        public string Category { get; set; } = string.Empty;
        public double Utilization { get; set; }
        public int TotalItems { get; set; }
        public int ItemsInUse { get; set; }
        public decimal TotalValue { get; set; }
    }

    public class ResourceSummaryDto
    {
        public int OptimalUtilization { get; set; }
        public int UnderUtilized { get; set; }
        public int OverUtilized { get; set; }
    }
}
