using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactoryOpsApp.Domain.Entities.MasterTenantsAdmin
{
    [Table("Exception_Logs")]
    public class ExceptionLogs
    {
        [Key]
        [Column("Log_Id")]
        public int LogId { get; set; }

        [Column("Log_Timestamp")]
        public DateTime LogTimestamp { get; set; } = DateTime.UtcNow;

        [Column("SourceModule")]
        [Required]
        [MaxLength(50)]
        public string SourceModule { get; set; }

        [Column("ApiName")]
        [MaxLength(100)]
        public string? ApiName { get; set; }

        [Column("ErrorCode")]
        [MaxLength(20)]
        public string? ErrorCode { get; set; }

        [Column("ErrorMessage")]
        [Required]
        public string ErrorMessage { get; set; }

        [Column("ExceptionStackTrace", TypeName = "text")]
        public string? ExceptionStackTrace { get; set; }

        [Column("TenantId")]
        public int? TenantId { get; set; }

        [ForeignKey("TenantId")]
        public FactoryTenants? Tenant { get; set; }

        [Column("UserId")]
        public int? UserId { get; set; }

        [Column("IsActive")]
        public bool IsActive { get; set; } = true;

        [Column("IsDeleted")]
        public bool IsDeleted { get; set; } = false;
    }
}
