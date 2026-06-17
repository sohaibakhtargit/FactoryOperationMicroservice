using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.SuperAdmin.RestoreBackup;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.SuperAdmin.AuditLogs;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.ExceptionLogger;
using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOps_AccessManagementService.FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Domain.Entities.MasterTenantsAdmin;
using FactoryOpsApp.Infrastructure.DBContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Text;
using static FactoryOps_AccessManagementService.FactoryOpsApp.Common.CommonConstant;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Repository.SuperAdmin.RestoreBackup
{
    public class BackupRepository : IBackupRepository
    {
        private readonly MasterFactoryOpsDbContext _masterDbContext;
        private readonly IExceptionLoggerService _exceptionLogger;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;
        private readonly IAuditLogService _auditLogger;
        private readonly BackupSettings _backupSettings;
        public BackupRepository(MasterFactoryOpsDbContext masterDbContext,
            IExceptionLoggerService exceptionLogger,
            IConfiguration configuration,
            IWebHostEnvironment env,
            IAuditLogService auditLogger,
            IOptions<BackupSettings> backupSettings)
        {
            _masterDbContext = masterDbContext;
            _exceptionLogger = exceptionLogger;
            _configuration = configuration;
            _env = env;
            _auditLogger = auditLogger;
            _backupSettings = backupSettings.Value;
        }

        public async Task<CommonResponseModel> CreateTenantBackupAsync(BackupRequestDto request)
        {
            var response = new CommonResponseModel();

            try
            {
                var tenantDb = await _masterDbContext.TenantMasterMapping
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.TenantId == request.TenantId && !t.IsDeleted && t.IsActive);

                if (tenantDb == null)
                    throw new Exception("Tenant DB not found");

                string databaseName = tenantDb.DbName;

                string user = _backupSettings.PostgresUser;
                string password = _backupSettings.PostgresPassword;
                string port = _backupSettings.PostgresPort;
                string host = _backupSettings.PostgresHost;
                string pgDumpPath = _backupSettings.PgDumpPath;
                string backupDirectory = Path.Combine(_env.WebRootPath, "Db_Backups");

                if (!Directory.Exists(backupDirectory))
                    Directory.CreateDirectory(backupDirectory);

                string backupFileName = $"{request.BackupName}_{DateTime.UtcNow:yyyyMMddHHmmss}.backup";
                string backupFilePath = Path.Combine(backupDirectory, backupFileName);

                var psi = new ProcessStartInfo
                {
                    FileName = pgDumpPath,
                    Arguments = $"-h {host} -p {port} -U {user} -d \"{databaseName}\" -F c -f \"{backupFilePath}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                psi.Environment["PGPASSWORD"] = password;

                using var process = Process.Start(psi);
                var error = await process!.StandardError.ReadToEndAsync();
                var output = await process.StandardOutput.ReadToEndAsync();
                process.WaitForExit();

                if (process.ExitCode != 0)
                    throw new Exception($"pg_dump failed: {error}");

                var fileSize = new FileInfo(backupFilePath).Length / (1024.0 * 1024.0);
                var fileSizeGB = (decimal)fileSize / 1024;

                var backupRecord = new BackupJob
                {
                    BackupName = request.BackupName,
                    TargetScope = request.TargetScope,
                    InitiatedBy = request.InitiatedBy,
                    RetentionDays = request.RetentionDays,
                    IncludeFiles = request.IncludeFiles,
                    IncludeLogs = request.IncludeLogs,
                    CompressBackup = request.CompressBackup,
                    BackupStatus = "Completed",
                    BackupPath = $"/backups/{backupFileName}",
                    BackupSizeGB = fileSizeGB,
                    StartedAt = DateTime.UtcNow,
                    CompletedAt = DateTime.UtcNow,
                    TenantId = request.TenantId,
                    IsDeleted = false,
                    IsActive = true
                };

                _masterDbContext.BackupJobs.Add(backupRecord);

                await _auditLogger.LogAuditAsync(
                        action: "Create",
                        details: "Create-DbBackup",
                        tenantId: request.TenantId,
                        email: "",
                        eventType: "DatabaseBackup"
                    );
                await _masterDbContext.SaveChangesAsync();

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = BackupStatusMessage.AddBackup;
            }
            catch (Exception ex)
            {

                await _exceptionLogger.LogExceptionAsync(
                    ex,
                    sourceModule: "DbBackup-Module",
                    apiName: "Create-DbBackup",
                    tenantId: request.TenantId,
                    userId: null
                );

                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"{BackupStatusMessage.AddBackupFailed}: {ex.Message}";
            }

            return response;
        }
        public async Task<CommonResponseModel> CreateBackupAsync(BackupRequestDto request)
        {
            var response = new CommonResponseModel();

            try
            {

                string databaseName = "MasterFactoryOperation";

                string user = _backupSettings.PostgresUser;
                string password = _backupSettings.PostgresPassword;
                string port = _backupSettings.PostgresPort;
                string host = _backupSettings.PostgresHost;
                string pgDumpPath = _backupSettings.PgDumpPath;
                string backupDirectory = Path.Combine(_env.WebRootPath, "Db_Backups");

                if (!Directory.Exists(backupDirectory))
                    Directory.CreateDirectory(backupDirectory);

                string backupFileName = $"{request.BackupName}_{DateTime.UtcNow:yyyyMMddHHmmss}.backup";
                string backupFilePath = Path.Combine(backupDirectory, backupFileName);

                var psi = new ProcessStartInfo
                {
                    FileName = pgDumpPath,
                    Arguments = $"-h {host} -p {port} -U {user} -d \"{databaseName}\" -F c -f \"{backupFilePath}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                psi.Environment["PGPASSWORD"] = password;

                using var process = Process.Start(psi);
                var error = await process!.StandardError.ReadToEndAsync();
                var output = await process.StandardOutput.ReadToEndAsync();
                process.WaitForExit();

                if (process.ExitCode != 0)
                    throw new Exception($"pg_dump failed: {error}");

                var fileSize = new FileInfo(backupFilePath).Length / (1024.0 * 1024.0);
                var fileSizeGB = (decimal)fileSize / 1024;

                var backupRecord = new BackupJob
                {
                    BackupName = request.BackupName,
                    TargetScope = request.TargetScope,
                    InitiatedBy = request.InitiatedBy,
                    RetentionDays = request.RetentionDays,
                    IncludeFiles = request.IncludeFiles,
                    IncludeLogs = request.IncludeLogs,
                    CompressBackup = request.CompressBackup,
                    BackupStatus = "Completed",
                    BackupPath = $"/backups/{backupFileName}",
                    BackupSizeGB = fileSizeGB,
                    StartedAt = DateTime.UtcNow,
                    CompletedAt = DateTime.UtcNow,
                    IsDeleted = false,
                    IsActive = true
                };

                _masterDbContext.BackupJobs.Add(backupRecord);

                await _masterDbContext.SaveChangesAsync();

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = BackupStatusMessage.AddBackup;
            }
            catch (Exception ex)
            {
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"{BackupStatusMessage.AddBackupFailed}: {ex.Message}";
            }

            return response;
        }
        public async Task<CommonResponseModel> DeleteBackupJobAsync(int backupJobId)
        {
            var response = new CommonResponseModel();

            try
            {
                var backupJob = await _masterDbContext.BackupJobs
                    .FirstOrDefaultAsync(x => x.BackupJobId == backupJobId && !x.IsDeleted);

                if (backupJob == null)
                {
                    response.StatusCode = StatusCode.NotFound;
                    response.StatusMessage = BackupStatusMessage.NoRecordsFound;
                    return response;
                }

                backupJob.IsDeleted = true;
                backupJob.IsActive = false;

                await _auditLogger.LogAuditAsync(
                    "Delete",
                    $"Deleted backup job: {backupJob.BackupName}",
                    backupJob.TenantId,
                    "",
                    "DeleteBackup"
                );

                await _masterDbContext.SaveChangesAsync();

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = BackupStatusMessage.BackupDeleted;
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(
                    ex,
                    sourceModule: "DbBackup-Module",
                    apiName: "Delete-DbBackup",
                    tenantId: null,
                    userId: null
                );

                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"{BackupStatusMessage.BackupDeleteFailed}: {ex.Message}";
            }

            return response;
        }
        public async Task<GetAllRecord<GetBackupJobDto>> GetAllBackupJobsAsync()
        {
            var response = new GetAllRecord<GetBackupJobDto>();

            try
            {
                string baseUrl = _configuration["BaseUrl:Staging"] ?? "https://ms.stagingsdei.com:8107";

                var jobs = await _masterDbContext.BackupJobs
                    .Where(x => !x.IsDeleted)
                    .OrderByDescending(x => x.StartedAt)
                    .ToListAsync();

                var jobDtos = jobs.Select(x => new GetBackupJobDto
                {
                    BackupJobId = x.BackupJobId,
                    BackupName = x.BackupName,
                    TargetScope = x.TargetScope,
                    InitiatedBy = x.InitiatedBy,
                    RetentionDays = x.RetentionDays,
                    IncludeFiles = x.IncludeFiles,
                    IncludeLogs = x.IncludeLogs,
                    CompressBackup = x.CompressBackup,
                    BackupStatus = x.BackupStatus,

                   
                    BackupPath = !string.IsNullOrEmpty(x.BackupPath)
                                    ? $"{baseUrl}{(x.BackupPath.StartsWith("/") ? "" : "/")}{x.BackupPath.Replace("\\", "/")}"
                                    : null,

                    FileName = !string.IsNullOrEmpty(x.BackupPath)
                            ? Path.GetFileName(x.BackupPath)
                            : null,

                    BackupSizeGB = x.BackupSizeGB,
                    StartedAt = x.StartedAt,
                    CompletedAt = x.CompletedAt,
                    TenantId = x.TenantId
                }).ToList();

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = BackupStatusMessage.BackupFetched;
                response.GetAllData = jobDtos;
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(
                    ex,
                    sourceModule: "DbBackup-Module",
                    apiName: "GetAll-DbBackup",
                    tenantId: null,
                    userId: null
                );

                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"{BackupStatusMessage.BackupFetchedFailed}:{ex.Message}";
            }

            return response;
        }
        public async Task<BackupStatisticsDto> GetBackupStatisticsAsync()
        {
            var dto = new BackupStatisticsDto();

            try
            {
                var backups = await _masterDbContext.BackupJobs
                    .Where(x => !x.IsDeleted && x.BackupStatus == "Completed")
                    .ToListAsync();

                dto.TotalBackups = backups.Count;
                dto.TotalStorageUsedGB = 12.8;
                dto.AverageBackupSizeGB = Math.Round(backups.Average(b => b.BackupSizeGB) ?? 0, 10);

                dto.TenantsCovered = backups.Select(b => b.TenantId).Distinct().Count();

                var lastBackup = backups.OrderByDescending(b => b.CompletedAt).FirstOrDefault();
                if (lastBackup != null)
                {
                    var timeAgo = DateTime.UtcNow - lastBackup.CompletedAt;

                    dto.LastSuccessfulBackup = timeAgo.HasValue
                        ? $"{(int)timeAgo.Value.TotalHours} hours ago"
                        : "N/A";
                }

                dto.NextScheduledBackup = "Tonight 2:00 AM";

                return dto;
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(
                    ex,
                    sourceModule: "DbBackup-Module",
                    apiName: "Get-BackupStatistics",
                    tenantId: null,
                    userId: null
                );
                return dto;
            }
        }
        public async Task<CommonResponseModel> RestoreTenantBackupAsync(RestoreBackupRequestDto request)
        {
            var response = new CommonResponseModel();

            try
            {
                var tenantDb = await _masterDbContext.TenantMasterMapping
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.TenantId == request.TenantId && !t.IsDeleted && t.IsActive);

                if (tenantDb == null)
                    throw new Exception("Tenant DB not found");

                string databaseName = tenantDb.DbName;

                string user = _backupSettings.PostgresUser;
                string password = _backupSettings.PostgresPassword;
                string port = _backupSettings.PostgresPort;
                string host = _backupSettings.PostgresHost;
                string pgRestorePath = _backupSettings.PgRestorePath;
                string backupDirectory = Path.Combine(_env.WebRootPath, "backups");

                string backupFilePath = Path.Combine(backupDirectory, request.BackupFileName);
                if (!File.Exists(backupFilePath))
                    throw new FileNotFoundException("Backup file not found", backupFilePath);

                var psi = new ProcessStartInfo
                {
                    FileName = pgRestorePath,
                    Arguments = $"-h {host} -p {port} -U {user} -d \"{databaseName}\" --clean --if-exists \"{backupFilePath}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                psi.Environment["PGPASSWORD"] = password;

                using var process = new Process { StartInfo = psi, EnableRaisingEvents = true };

                var outputBuilder = new StringBuilder();
                var errorBuilder = new StringBuilder();

                process.OutputDataReceived += (sender, args) =>
                {
                    if (args.Data != null)
                        outputBuilder.AppendLine(args.Data);
                };

                process.ErrorDataReceived += (sender, args) =>
                {
                    if (args.Data != null)
                        errorBuilder.AppendLine(args.Data);
                };

                var tcs = new TaskCompletionSource<bool>();

                process.Exited += (sender, args) =>
                {
                    tcs.TrySetResult(true);
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                await tcs.Task;

                string error = errorBuilder.ToString();
                string output = outputBuilder.ToString();

                if (process.ExitCode != 0 || !string.IsNullOrWhiteSpace(error) && error.Contains("ERROR", StringComparison.OrdinalIgnoreCase))
                    throw new Exception($"Restore failed: {error}");

                await _auditLogger.LogAuditAsync(
                    "Restore",
                    $"Restored .backup file {request.BackupFileName} to tenant DB {databaseName}",
                    request.TenantId,
                    request.RestoredBy,
                    "RestoreBackupFile"
                );

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = BackupStatusMessage.BackupRestored;
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(
                    ex,
                    sourceModule: "DbBackup-Module",
                    apiName: "Restore-DbBackup",
                    tenantId: request.TenantId,
                    userId: null
                );

                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"{BackupStatusMessage.BackupRestoredFailed}: {ex.Message}";
            }

            return response;
        }


    }
}
