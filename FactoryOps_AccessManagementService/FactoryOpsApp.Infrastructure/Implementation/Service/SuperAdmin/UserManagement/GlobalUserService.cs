using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.SuperAdmin.UserManagement;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.SuperAdmin.UserManagement;
using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Service.SuperAdmin.UserManagement
{
    public class GlobalUserService : IGlobalUserService
    {
        private readonly IGlobalUserRepository _IGlobalUserRepo;
        public GlobalUserService(IGlobalUserRepository IGlobalUserRepo)
        {
            _IGlobalUserRepo = IGlobalUserRepo;

        }

        public GetAllRecord<GetAllGlobalUserDto> GetAllGlobalUsers()
        {
            return _IGlobalUserRepo.GetAllGlobalUsers();
        }

        public GetAllRecord<GetAllSuspendUserDto> GetAllSuspendUsers()
        {
            return _IGlobalUserRepo.GetAllSuspendUsers();
        }

        public Task<CommonResponseModel> ForceLogout(int Id)
        {
            return _IGlobalUserRepo.ForceLogout(Id);
        }


        public Task<CommonResponseModel> ToggleSuspend(SuspendGlobalUserDto dto)
        {
            return _IGlobalUserRepo.ToggleSuspend(dto);
        }
        public GetAllRecord<GetInfoOfUserDto> GetInfoOfUsers(int tenantId, string Email)
        {
            return _IGlobalUserRepo.GetInfoOfUsers(tenantId, Email);
        }
        public Task<CommonResponseModel> UpdateUserProfile(UpdateUserProfileDto dto)
        {
            return _IGlobalUserRepo.UpdateUserProfile(dto);
        }
        public GetAllRecord<GetInfoOfUserDto> GetSuperAdminInfo(int userId)
        {
            return _IGlobalUserRepo.GetSuperAdminInfo(userId);
        }

        public Task<CommonResponseModel> UpdateSuperAdminProfile(UpdateSuperAdminProfileDto dto)
        {
            return _IGlobalUserRepo.UpdateSuperAdminProfile(dto);
        }
        public GetAllRecord<GetInfoOfUserDto> GetTenantInfo(int tenantId)
        {
            return _IGlobalUserRepo.GetTenantInfo(tenantId);
        }
        public Task<CommonResponseModel> UpdateTenantProfile(UpdateTenantProfileDto dto)
        {
            return _IGlobalUserRepo.UpdateTenantProfile(dto);
        }

    }
}
