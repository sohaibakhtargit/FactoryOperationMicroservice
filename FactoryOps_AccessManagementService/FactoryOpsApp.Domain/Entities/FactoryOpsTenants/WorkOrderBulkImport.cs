using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using FactoryOpsApp.Domain.Entities.FactoryOpsTenants;

namespace FactoryOps_AccessManagementService.FactoryOpsApp.Domain.Entities.FactoryOpsTenants
{
    public class WorkOrderBulkImport
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BulkImportId { get; set; }
        public int TenantId { get; set; }
        [Required]
        public string FileName { get; set; } = string.Empty;
        [Required]
        public string FilePath { get; set; } = string.Empty;
        public int UploadedBy { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
        public int? TotalRecords { get; set; }
        public int? SuccessCount { get; set; }
        public int? FailureCount { get; set; }
        public ICollection<WorkOrder>? WorkOrders { get; set; }
    }
}
