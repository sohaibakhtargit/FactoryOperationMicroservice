namespace FactoryOperation_Asset.FactoryOpsApp.Application.DTOs
{
    public class AssetBOMDto
    {
        public int TenantId { get; set; }
        public int AssetId { get; set; }

        public string PartNumber { get; set; } = string.Empty;
        public string PartName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Category { get; set; }

        public int Quantity { get; set; }
        public decimal? UnitCost { get; set; }
        public int? MinimumStockLevel { get; set; }
        public int? LeadTimeDays { get; set; }

        public string? Supplier { get; set; }
        public string? StorageLocation { get; set; }
        public string? CompatibleModels { get; set; }
        public int CreatedBy { get; set; }  
    }

    public class UpdateAssetBomDto: AssetBOMDto
    {
        public int BomPartId { get; set; }
        public int UpdatedBy { get; set; }
    }

    public class GetAssetBOMDto
    {
        public int BomPartId { get; set; }
        public int AssetId { get; set; }
        public string AssetName { get; set; }
        public string PartNumber { get; set; } = string.Empty;
        public string PartName { get; set; } = string.Empty;
        public string? Category { get; set; }
        public int Quantity { get; set; }
        public decimal? UnitCost { get; set; }
        public int? MinimumStockLevel { get; set; }
        public string? Supplier { get; set; }
        public bool IsActive { get; set; }
        public string? Description { get; set; }
    }

    public class DeleteAssetBomDto
    {
        public int TenantId { get; set; }
        public int BomPartId { get; set; }
        public int DeletedBy { get; set; }  
    }

}
