using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactoryOpsApp.Domain.Entities.FactoryOpsTenants
{
    [Table("DeviceConfiguration")]
    public class DeviceConfiguration
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ConfigId { get; set; }

        public int TenantId { get; set; }

        [ForeignKey("DeviceId")]
        public int DeviceId { get; set; }
        public virtual FactoryDevice? Device { get; set; }

        [Required]
        public int SamplingRate { get; set; }

        [MaxLength(20)]
        public string FirmwareVersion { get; set; }

        [MaxLength(20)]
        public string DataFormat { get; set; } = "JSON";

        [MaxLength(20)]
        public string Protocol { get; set; } = "MQTT";

        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string CreatedBy { get; set; } = Environment.UserName;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public string UpdatedBy { get; set; } = Environment.UserName;
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
    }
}

