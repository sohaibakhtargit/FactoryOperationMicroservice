using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactoryOpsApp.Domain.Entities.MasterTenantsAdmin
{
    public class BackupJob
    {
        [Key]
        public int BackupJobId { get; set; }
        public string BackupName { get; set; } = null!;
        public string TargetScope { get; set; } = null!;
        public string InitiatedBy { get; set; } = null!;
        public int RetentionDays { get; set; } = 7;
        public bool IncludeFiles { get; set; } = false;
        public bool IncludeLogs { get; set; } = false;
        public bool CompressBackup { get; set; } = false;
        public string? BackupStatus { get; set; } = "Pending";
        public string? BackupPath { get; set; }
        public decimal? BackupSizeGB { get; set; }
        public DateTime StartedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }
        public bool IsDeleted { get; set; } = false;
        public bool IsActive { get; set; } = true;
        public int? TenantId { get; set; }
    }
}
