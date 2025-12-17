using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.SuperAdmin.UserManagement
{
    public interface IGlobalUserRepository
    {
        public GetAllRecord<GetAllGlobalUserDto> GetAllGlobalUsers();
        public GetAllRecord<GetAllSuspendUserDto> GetAllSuspendUsers();
        public Task<CommonResponseModel> ForceLogout(int Id);
        public Task<CommonResponseModel> ToggleSuspend(SuspendGlobalUserDto dto);
        public GetAllRecord<GetInfoOfUserDto> GetInfoOfUsers(int tenantId, string Email);
        public Task<CommonResponseModel> UpdateUserProfile(UpdateUserProfileDto dto);
        public GetAllRecord<GetInfoOfUserDto> GetSuperAdminInfo(int userId);
        public Task<CommonResponseModel> UpdateSuperAdminProfile(UpdateSuperAdminProfileDto dto);
        public GetAllRecord<GetInfoOfUserDto> GetTenantInfo(int tenantId);
        public Task<CommonResponseModel> UpdateTenantProfile(UpdateTenantProfileDto dto);



    }
}
