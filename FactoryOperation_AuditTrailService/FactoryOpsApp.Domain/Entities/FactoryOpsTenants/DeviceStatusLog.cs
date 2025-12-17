using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FactoryOpsApp.Domain.Entities.FactoryOpsTenants
{
    [Table("DeviceStatusLogs")]
    public class DeviceStatusLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int LogId { get; set; }

        public int TenantId { get; set; }

        [ForeignKey("DeviceId")]
        public int DeviceId { get; set; }
        public virtual FactoryDevice? Device { get; set; }

        [Required]
        [MaxLength(50)]
        public string EventType { get; set; }

        [Required]
        public DateTime EventTime { get; set; }

        public string Remarks { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string CreatedBy { get; set; } = Environment.UserName;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public string UpdatedBy { get; set; } = Environment.UserName;
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
    }
}
