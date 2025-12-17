using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.TeamManagement
{
    public interface ITrainingModuleRepository
    {
        Task<CommonResponseModel> AddTrainingModuleAsync(AddTrainingModuleDto dto);
        Task<CommonResponseModel> UpdateTrainingModuleAsync(AddTrainingModuleDto dto);
        Task<CommonResponseModel> DeleteTrainingModuleAsync(int id, int tenantId);
        Task<GetAllRecord<GetTrainingModuleDto>> GetAllTrainingModuleAsync(int tenantId);
    }
}
