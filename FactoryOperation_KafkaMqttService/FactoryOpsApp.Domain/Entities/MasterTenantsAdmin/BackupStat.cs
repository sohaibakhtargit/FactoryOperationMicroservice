using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactoryOpsApp.Domain.Entities.MasterTenantsAdmin
{
    public class BackupStat
    {
        [Key]
        public int BackupStatId { get; set; }
        public int TotalBackups { get; set; } = 0;
        public int TenantsCovered { get; set; } = 0;
        public DateTime? LastSuccessTime { get; set; }
        public DateTime? NextScheduleTime { get; set; }
        public decimal AverageSizeGB { get; set; } = 0.0m;
        public decimal TotalUsedGB { get; set; } = 0.0m;
        public decimal StorageLimitGB { get; set; } = 20.0m;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; } = false;
        public bool IsActive { get; set; } = true;
    }
}
