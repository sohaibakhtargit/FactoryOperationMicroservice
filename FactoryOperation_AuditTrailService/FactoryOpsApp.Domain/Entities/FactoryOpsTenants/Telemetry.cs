using FactoryOpsApp.Domain.Entities.FactoryOpsTenants;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FactoryOpsApp.Domain.Entities.FactoryOpsTenants
{
    [Table("Telemetry")]
    public class Telemetry
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TelemetryId { get; set; }

        public int TenantId { get; set; }

        public int DeviceId { get; set; }
        [ForeignKey("DeviceId")]
        public FactoryDevice? Device { get; set; }

        [Required]
        public DateTime Timestamp { get; set; }

        [Column(TypeName = "jsonb")] 
        public string SensorDataJson { get; set; } = "{}";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string CreatedBy { get; set; } = Environment.UserName;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public string UpdatedBy { get; set; } = Environment.UserName;
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
    }
}
