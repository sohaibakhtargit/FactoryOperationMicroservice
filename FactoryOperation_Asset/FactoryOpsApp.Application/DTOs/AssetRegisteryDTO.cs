using static FactoryOpsApp.Domain.Entities.FactoryOpsTenants.AssetRegistry;

namespace FactoryOpsApp.Application.DTOs
{
    public class AssetRegistryDto
    {
        public int TenantId { get; set; }
        public int? AssetId { get; set; }
        public string? AssetUniqueId { get; set; }
        public string AssetName { get; set; } = string.Empty;

        public int? AssetTypeId { get; set; }

        public string? Model { get; set; }
        public string? SerialNumber { get; set; }
        public string? CategoryHierarchy { get; set; }

        public int LocationId { get; set; }
        public int? AssignedTo { get; set; }
        public string? Department { get; set; }
        public string? Vendor { get; set; }
        public string? Supplier { get; set; }
        public string? Manufacturer { get; set; }
        public int? ExpectedLifespan { get; set; }
        public string? DepreciationRule { get; set; }
        public string? Power { get; set; }
        public CriticalityLevel? Criticality { get; set; }
        public string? DocumentUrl { get; set; }  
        public IFormFile? DocumentFile { get; set; } 
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
        public string? InsurancePolicyNumber { get; set; }
        public DateTime? WarrantyExpiry { get; set; }

    }

    public class GetAssetRegistryDto
    {
        public int AssetId { get; set; }
        public string AssetName { get; set; } = string.Empty;
        public int TenantId { get; set; }
        public string? AssetUniqueId { get; set; }
        public int? AssetTypeId { get; set; }
        public string? AssetTypeName { get; set; }

        public string? Model { get; set; }
        public string? SerialNumber { get; set; }
        public string? CategoryHierarchy { get; set; }

        public int LocationId { get; set; }
        public string? LocationName { get; set; }

        public string? Department { get; set; }
        public string? Vendor { get; set; }
        public string? Supplier { get; set; }
        public string? Manufacturer { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public decimal? AcquisitionCost { get; set; }
        public DateTime? WarrantyExpiry { get; set; }
        public int? ExpectedLifespan { get; set; }
        public string? DepreciationRule { get; set; }
        public string? Power { get; set; }
        public CriticalityLevel? Criticality { get; set; }
        public string? DocumentUrl { get; set; }
        public string? InsurancePolicyNumber { get; set; }
        // Audit fields
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int? UpdatedBy { get; set; }
    }


}
