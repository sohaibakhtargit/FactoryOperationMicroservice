using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FactoryOpsApp.Domain.Entities.FactoryOpsTenants
{
    [Table("OutboxEvents")]
    public class OutboxEvent
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        public Guid EventId { get; set; } = Guid.NewGuid();

        [Required]
        public int TenantId { get; set; }

        [Required, MaxLength(128)]
        public string AggregateType { get; set; } = ""; // e.g., "workorders"

        [Required, MaxLength(128)]
        public string AggregateId { get; set; } = "";  // e.g., workorder id

        [Required, MaxLength(128)]
        public string EventType { get; set; } = "";    // e.g., "Created","Updated","Deleted","Assigned"

        [Required]
        public DateTime OccurredUtc { get; set; } = DateTime.UtcNow;

        [Required]
        public string PayloadJson { get; set; } = "{}";

        public string? HeadersJson { get; set; }

        // Dispatch state
        [Required, MaxLength(24)]
        public string Status { get; set; } = "Pending"; // Pending | Dispatched | Failed

        public int RetryCount { get; set; } = 0;

        public DateTime? NextAttemptUtc { get; set; }

        public string? LockedBy { get; set; }
        public DateTime? LockedUntilUtc { get; set; }

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? DispatchedAtUtc { get; set; }
        public string? LastError { get; set; }
    }
}
