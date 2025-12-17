using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.TenantAdminManagement
{
    public interface IFactoryGroupRepository
    {
        Task<GetAllRecord<FactoryGroupGetDto>> GetAllGroupsAsync(int tenantId);
        Task<GetSpecificRecord<FactoryGroupGetDto>> GetGroupByIdAsync(int tenantId, int groupId);
        Task<CommonResponseModel> AddGroupAsync(FactoryGroupDto dto);
        Task<CommonResponseModel> UpdateGroupAsync(FactoryGroupDto dto);
        Task<CommonResponseModel> DeleteGroupAsync(int tenantId, int groupId);
    }
}
