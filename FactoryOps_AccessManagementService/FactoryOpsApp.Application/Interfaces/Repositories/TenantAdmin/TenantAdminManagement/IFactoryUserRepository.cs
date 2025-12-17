using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.TenantAdminManagement
{
    public interface IFactoryUserRepository
    {

        public Task<CommonResponseModel> AddNewUser(UserResponseDto AddUser);
        public Task<CommonResponseModel> EditExistingUser(UserResponseDto AddUser);
        public GetAllRecord<GetUsersListDto> GetAllUsers(int TenantId);
        public Task<CommonResponseModel> DeleteUser(int Id, int TenantId);
        public Task<CommonResponseModel> ForceLogout(int Id, int TenantId);
        public Task<CommonResponseModel> Suspend(SuspendUserDto dto);
        public GetAllRecord<GetManagerUserDto> GetAllManagersByTenantAsync(int tenantId);
        public GetAllRecord<GetManagerUserDto> GetAllUsersExceptManager(int tenantId);


    }
}
