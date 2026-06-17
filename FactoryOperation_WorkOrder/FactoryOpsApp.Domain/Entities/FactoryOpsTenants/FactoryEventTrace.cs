using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FactoryOperation_WorkOrder.FactoryOpsApp.Domain.Entities.FactoryOpsTenants
{
    [Table("FactoryEventTraces")]
    public class FactoryEventTrace
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("TraceId")]
        public int TraceId { get; set; }

        [Column("CorrelationId")]
        [MaxLength(120)]
        public string? CorrelationId { get; set; }

        [Column("CausationId")]
        [MaxLength(120)]
        public string? CausationId { get; set; }

        [Column("TenantId")]
        public int? TenantId { get; set; }

        [Column("Service")]
        [Required]
        [MaxLength(120)]
        public string Service { get; set; } = null!;

        [Column("Stage")]
        [Required]
        [MaxLength(120)]
        public string Stage { get; set; } = null!;

        [Column("EventType")]
        [MaxLength(120)]
        public string? EventType { get; set; }

        [Column("Topic")]
        [MaxLength(255)]
        public string? Topic { get; set; }

        [Column("Partition")]
        public int? Partition { get; set; }

        [Column("Offset")]
        public long? Offset { get; set; }

        [Column("Status")]
        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = null!;

        [Column("Message")]
        public string? Message { get; set; }

        [Column("Error")]
        public string? Error { get; set; }

        [Column("IpAddress")]
        [MaxLength(50)]
        public string? IpAddress { get; set; }

        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
    }
}
