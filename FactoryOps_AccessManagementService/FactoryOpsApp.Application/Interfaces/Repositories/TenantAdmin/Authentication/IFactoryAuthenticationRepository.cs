using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.Authentication
{
    public interface IFactoryAuthenticationRepository
    {
        public Task<ResponseToken> UnifiedAuthenticate(LoginDto login);
        public Task<ResponseToken> SwitchTenantAsync(int tenantId);

        public Task<CommonResponseModel> CheckEmailExistence(ForgetPasswordDTO dto);
    }
}
