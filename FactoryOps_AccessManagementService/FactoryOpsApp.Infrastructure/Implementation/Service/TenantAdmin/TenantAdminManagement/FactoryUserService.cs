using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.TenantAdminManagement;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.TenantAdminManagement;
using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Service.TenantAdmin.TenantAdminManagement
{
    public class FactoryUserService : IFactoryUserService
    {
        private readonly IFactoryUserRepository _IFactoryUserRepo;
        public FactoryUserService(IFactoryUserRepository IFactoryUserRepo)
        {
            _IFactoryUserRepo = IFactoryUserRepo;

        }
        public async Task<CommonResponseModel> AddNewUser(UserResponseDto AddUser)
        {
            return await _IFactoryUserRepo.AddNewUser(AddUser);
        }
        public async Task<CommonResponseModel> EditExistingUser(UserResponseDto AddUser)
        {
            return await _IFactoryUserRepo.EditExistingUser(AddUser);
        }
        public GetAllRecord<GetUsersListDto> GetAllUsers(int TenantId)
        {
            return _IFactoryUserRepo.GetAllUsers(TenantId);
        }
        public Task<CommonResponseModel> DeleteUser(int Id, int TenantId)
        {
            return _IFactoryUserRepo.DeleteUser(Id, TenantId);
        }
        public Task<CommonResponseModel> ForceLogout(int Id, int TenantId)
        {
            return _IFactoryUserRepo.ForceLogout(Id, TenantId);
        }
        public Task<CommonResponseModel> Suspend(SuspendUserDto dto)
        {
            return _IFactoryUserRepo.Suspend(dto);
        }

        public GetAllRecord<GetManagerUserDto> GetAllManagersByTenantAsync(int tenantId)
        {
            return _IFactoryUserRepo.GetAllManagersByTenantAsync(tenantId);
        }

        public GetAllRecord<GetManagerUserDto> GetAllUsersExceptManager(int tenantId)
        {
            return _IFactoryUserRepo.GetAllUsersExceptManager(tenantId);
        }

    }
}
