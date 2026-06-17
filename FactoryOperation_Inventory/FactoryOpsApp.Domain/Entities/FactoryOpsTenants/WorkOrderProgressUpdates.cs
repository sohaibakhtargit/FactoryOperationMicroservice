using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using FactoryOpsApp.Domain.Entities.FactoryOpsTenants;

namespace FactoryOperation_Inventory.FactoryOpsApp.Domain.Entities.FactoryOpsTenants
{
    public class WorkOrderProgressUpdates
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int WorkOrderProgressUpdateId { get; set; }
        public int TenantId { get; set; }
        public int WorkOrderId { get; set; }
        public int? AssignedToUserId { get; set; }
        [ForeignKey(nameof(WorkOrderId))]
        public virtual WorkOrder WorkOrder { get; set; }
        public UpdateTypeEnum? UpdateType { get; set; }
        public WorkOrderStatus? Status { get; set; }
        public decimal? ProgressPercentage { get; set; }
        [MaxLength(1000)]
        public string? Message { get; set; }
        public string? AttachmentName { get; set; }
        public string? AttachmentPath { get; set; }
        public string? Action { get; set; }   // Start / Pause / Resume / Complete
        public int? UpdatedBy { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public int? DeletedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
