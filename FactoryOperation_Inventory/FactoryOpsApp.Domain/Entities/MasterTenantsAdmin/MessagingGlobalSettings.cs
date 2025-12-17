using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactoryOpsApp.Domain.Entities.MasterTenantsAdmin
{
    public class MessagingGlobalSettings
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int GlobalSettingId { get; set; }
        public string? BootstrapServers { get; set; }
        public string? Topic { get; set; }
        public bool? UsePerTenantTopic { get; set; }
        public string? DlqTopic { get; set; }
        public bool? EnableSSL { get; set; }
        public string? SecurityJson { get; set; }
        public string? ProducerConfigJson { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
        public int? DeletedBy { get; set; }
    }
}
