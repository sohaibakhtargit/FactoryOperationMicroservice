using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactoryOpsApp.Application.DTOs
{
    public class BackupStatisticsDto
    {
        public double TotalStorageUsedGB { get; set; }
        public double StorageLimitGB { get; set; } = 20.0; 
        public int TotalBackups { get; set; }
        public int TenantsCovered { get; set; }
        public string LastSuccessfulBackup { get; set; } = "--";
        public string NextScheduledBackup { get; set; } = "--";
        public decimal? AverageBackupSizeGB { get; set; }
    }

}
