namespace FactoryOps_AccessManagementService.FactoryOpsApp.Application.DTOs
{
    public class BackupSettings
    {
        public string BackupDirectory { get; set; } = string.Empty;
        public string PgDumpPath { get; set; } = string.Empty;
        public string PgRestorePath { get; set; } = string.Empty;
        public string PostgresUser { get; set; } = string.Empty;
        public string PostgresPassword { get; set; } = string.Empty;
        public string PostgresHost { get; set; } = string.Empty;
        public string PostgresPort { get; set; } = string.Empty;
    }
}
