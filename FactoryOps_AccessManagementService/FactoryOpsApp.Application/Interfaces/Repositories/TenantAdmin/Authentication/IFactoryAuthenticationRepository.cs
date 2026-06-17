using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOps_AccessManagementService.FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Application.DTOs;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.Authentication
{
    public interface IFactoryAuthenticationRepository
    {
        public Task<ResponseToken> UnifiedAuthenticate(LoginDto login);
        public Task<ResponseToken> VerifyOtp(VerifyOTPDto verifyotp);
        public Task<ResponseToken> SwitchTenantAsync(int tenantId);
        //public Task<CommonResponseModel> CheckEmailExistence(ForgetPasswordDTO dto);
        public Task<CommonResponseModel> ResetPassword(ResetPasswordDTO dto);
        //public Task<CommonResponseModel> VerifyOtpByEmail(VerifyOtpByEmailDto dto);
        public Task<ResponseOTPModel> ForgetPassword(ForgetPasswordDTO dto);
    }
}
