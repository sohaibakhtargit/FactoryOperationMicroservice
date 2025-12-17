using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.SuperAdmin.RestoreBackup
{
    public interface IBackupRepository
    {
        Task<CommonResponseModel> CreateTenantBackupAsync(BackupRequestDto request);
        Task<CommonResponseModel> CreateBackupAsync(BackupRequestDto request);
        Task<CommonResponseModel> RestoreTenantBackupAsync(RestoreBackupRequestDto request);
        Task<GetAllRecord<GetBackupJobDto>> GetAllBackupJobsAsync();
        Task<BackupStatisticsDto> GetBackupStatisticsAsync();
        Task<CommonResponseModel> DeleteBackupJobAsync(int backupJobId);

    }
}
