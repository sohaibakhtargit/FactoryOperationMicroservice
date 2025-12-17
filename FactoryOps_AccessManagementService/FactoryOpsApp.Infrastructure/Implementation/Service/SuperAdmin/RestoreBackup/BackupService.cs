using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.SuperAdmin.RestoreBackup;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.SuperAdmin.RestoreBackup;
using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Service.SuperAdmin.RestoreBackup
{
    public class BackupService : IBackupService
    {
        private readonly IBackupRepository _IBackupRepo;
        public BackupService(IBackupRepository IBackupRepo)
        {
            _IBackupRepo = IBackupRepo;
        }
        public async Task<CommonResponseModel> CreateTenantBackupAsync(BackupRequestDto request)
        {
            return await _IBackupRepo.CreateTenantBackupAsync(request);
        }

        public async Task<CommonResponseModel> CreateBackupAsync(BackupRequestDto request)
        {
            return await _IBackupRepo.CreateBackupAsync(request);
        }

        public async Task<CommonResponseModel> DeleteBackupJobAsync(int backupJobId)
        {
            return await _IBackupRepo.DeleteBackupJobAsync(backupJobId);
        }


        public async Task<GetAllRecord<GetBackupJobDto>> GetAllBackupJobsAsync()
        {
            return await _IBackupRepo.GetAllBackupJobsAsync();
        }

        public async Task<BackupStatisticsDto> GetBackupStatisticsAsync()
        {
            return await _IBackupRepo.GetBackupStatisticsAsync();
        }

        public async Task<CommonResponseModel> RestoreTenantBackupAsync(RestoreBackupRequestDto request)
        {
            return await _IBackupRepo.RestoreTenantBackupAsync(request);
        }
    }
}
