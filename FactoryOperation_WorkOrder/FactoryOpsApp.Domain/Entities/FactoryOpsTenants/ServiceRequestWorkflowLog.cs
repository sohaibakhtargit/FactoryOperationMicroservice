using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using FactoryOpsApp.Domain.Entities.FactoryOpsTenants;

namespace FactoryOperation_WorkOrder.FactoryOpsApp.Domain.Entities.FactoryOpsTenants
{
    public class ServiceRequestWorkflowLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int LogId { get; set; }

        public int TenantId { get; set; }

        public int ServiceRequestId { get; set; }

        [ForeignKey(nameof(ServiceRequestId))]
        public virtual ServiceRequest ServiceRequest { get; set; }

        public string? ActionType { get; set; }

        public int? PerformedBy { get; set; }

        public DateTime PerformedAt { get; set; } = DateTime.UtcNow;

        public string? Notes { get; set; }
    }
}
