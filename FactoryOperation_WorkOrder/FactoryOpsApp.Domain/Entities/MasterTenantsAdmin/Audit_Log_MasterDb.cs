using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactoryOpsApp.Domain.Entities.MasterTenantsAdmin
{
    public class Audit_Log_MasterDb
    {
        [Key]
        public int AuditLogID { get; set; }
        [Required]
        [MaxLength(100)]
        public string EventType { get; set; }
        public int UserId { get; set; }

        [MaxLength(150)]
        public string? Email { get; set; }
        public string? Roles { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public string? UserName { get; set; }
        public string? Ipaddress { get; set; }
        public string? Action { get; set; }
        public string? Details { get; set; }
        public int? TenantId { get; set; }
    }
}
