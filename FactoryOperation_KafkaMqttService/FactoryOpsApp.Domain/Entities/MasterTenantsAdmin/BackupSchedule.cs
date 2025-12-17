using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactoryOpsApp.Domain.Entities.MasterTenantsAdmin
{
    public class BackupSchedule
    {
        [Key]
        public int BackupScheduleId { get; set; }
        public string CronExpression { get; set; } = null!;
        public string TargetScope { get; set; } = null!;
        public int RetentionDays { get; set; } = 7;
        public bool IsEnabled { get; set; } = true;
        public DateTime? LastRun { get; set; }
        public DateTime? NextRun { get; set; }
        public bool IsDeleted { get; set; } = false;
        public bool IsActive { get; set; } = true;
    }
}
