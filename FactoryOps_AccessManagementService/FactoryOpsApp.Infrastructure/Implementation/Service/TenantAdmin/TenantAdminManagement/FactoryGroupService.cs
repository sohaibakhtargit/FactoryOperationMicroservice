using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.TenantAdminManagement;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.TenantAdminManagement;
using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Service.TenantAdmin.TenantAdminManagement
{
    public class FactoryGroupService : IFactoryGroupService
    {
        private readonly IFactoryGroupRepository _repository;

        public FactoryGroupService(IFactoryGroupRepository repository)
        {
            _repository = repository;
        }

        public Task<CommonResponseModel> AddGroupAsync(FactoryGroupDto dto) => _repository.AddGroupAsync(dto);
        public Task<CommonResponseModel> UpdateGroupAsync(FactoryGroupDto dto) => _repository.UpdateGroupAsync(dto);
        public Task<CommonResponseModel> DeleteGroupAsync(int tenantId, int groupId) => _repository.DeleteGroupAsync(tenantId, groupId);
        public Task<GetAllRecord<FactoryGroupGetDto>> GetAllGroupsAsync(int tenantId) => _repository.GetAllGroupsAsync(tenantId);
        public Task<GetSpecificRecord<FactoryGroupGetDto>> GetGroupByIdAsync(int tenantId, int groupId) => _repository.GetGroupByIdAsync(tenantId, groupId);
    }
}
