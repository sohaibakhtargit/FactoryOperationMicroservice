using FactoryOpsApp.Domain.Entities.FactoryOpsTenants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FactoryOperation_WorkOrder.FactoryOpsApp.Domain.Entities.FactoryOpsTenants
{
    public class AssetBulkImport
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BulkImportId { get; set; }
        public int TenantId { get; set; }
        [Required]
        public string FileName { get; set; } = string.Empty;
        [Required]
        public string FilePath { get; set; } = string.Empty;
        public int? UploadedBy { get; set; }
        public string? Status { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
        public int? TotalRecords { get; set; }
        public int? SuccessCount { get; set; }
        public int? FailureCount { get; set; }
        public ICollection<AssetRegistry>? AssetRegistry { get; set; }
    }
}
