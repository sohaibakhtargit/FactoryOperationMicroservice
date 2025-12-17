namespace FactoryOpsApp.Application.DTOs
{
    public class BackupRequestDto
    {
        public int TenantId { get; set; }
        public string BackupName { get; set; } = null!;
        public string TargetScope { get; set; } = null!;
        public string InitiatedBy { get; set; } = null!;
        public int RetentionDays { get; set; } = 7;
        public bool IncludeFiles { get; set; } = false;
        public bool IncludeLogs { get; set; } = false;
        public bool CompressBackup { get; set; } = false;

    }
}
