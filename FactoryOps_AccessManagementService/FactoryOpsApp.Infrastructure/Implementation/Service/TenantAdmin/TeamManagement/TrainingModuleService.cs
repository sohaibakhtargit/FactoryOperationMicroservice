using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.TeamManagement;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.TeamManagement;
using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Service.TenantAdmin.TeamManagement
{
    public class TrainingModuleService : ITrainingModuleService
    {
        private readonly ITrainingModuleRepository _trainingModulerepository;

        public TrainingModuleService(ITrainingModuleRepository trainingModuleRepository)
        {
            _trainingModulerepository = trainingModuleRepository;
        }

        public Task<CommonResponseModel> AddTrainingModuleAsync(AddTrainingModuleDto dto)
            => _trainingModulerepository.AddTrainingModuleAsync(dto);

        public Task<CommonResponseModel> UpdateTrainingModuleAsync(AddTrainingModuleDto dto)
            => _trainingModulerepository.UpdateTrainingModuleAsync(dto);

        public Task<CommonResponseModel> DeleteTrainingModuleAsync(int id, int tenantId)
            => _trainingModulerepository.DeleteTrainingModuleAsync(id, tenantId);

        public Task<GetAllRecord<GetTrainingModuleDto>> GetAllTrainingModuleAsync(int tenantId)
        => _trainingModulerepository.GetAllTrainingModuleAsync(tenantId);
    }
}
