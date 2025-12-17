using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FactoryOpsApp.Domain.Entities.MasterTenantsAdmin
{
    public class TenantIsolation
    {
        [Key]
        public int TenantId { get; set; } // FK to FactoryTenants
        public string? TenantName { get; set; }  

        [Required]
        [MaxLength(20)]
        public string DataEncryption { get; set; } = "Disabled"; // "Enabled" or "Disabled"

        [MaxLength(255)]
        public string? EncryptionKeyId { get; set; }

        public bool CustomBranding { get; set; } = false;

        [MaxLength(512)]
        public string? LogoUrl { get; set; }

        [MaxLength(255)]
        public string? ColorScheme { get; set; }

        [MaxLength(255)]
        public string? DataPartitionId { get; set; }

        // Navigation (optional)
        [ForeignKey("TenantId")]
        public FactoryTenants? FactoryTenant { get; set; }
    }
}
