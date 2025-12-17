using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactoryOpsApp.Application.DTOs
{
    public class GetBackupJobDto
    {
        public int BackupJobId { get; set; }
        public string BackupName { get; set; }
        public string TargetScope { get; set; }
        public string InitiatedBy { get; set; }
        public int RetentionDays { get; set; }
        public bool IncludeFiles { get; set; }
        public bool IncludeLogs { get; set; }
        public bool CompressBackup { get; set; }
        public string? BackupStatus { get; set; }
        public string? BackupPath { get; set; }
        public string? FileName { get; set; }
        public decimal? BackupSizeGB { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public int? TenantId { get; set; }
    }

}
