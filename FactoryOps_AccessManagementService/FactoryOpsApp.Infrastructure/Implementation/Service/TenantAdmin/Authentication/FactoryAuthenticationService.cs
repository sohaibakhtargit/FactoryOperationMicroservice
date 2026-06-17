using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.Authentication;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.Authentication;
using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOps_AccessManagementService.FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Application.DTOs;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Service.TenantAdmin.Authentication
{
    public class FactoryAuthenticationService : IFactoryAuthenticationService
    {
        private readonly IFactoryAuthenticationRepository _IFactoryAuthRepo;
        public FactoryAuthenticationService(IFactoryAuthenticationRepository IFactoryAuthRepo)
        {
            _IFactoryAuthRepo = IFactoryAuthRepo;

        }
        public async Task<ResponseToken> UnifiedAuthenticate(LoginDto login)
        {
            return await _IFactoryAuthRepo.UnifiedAuthenticate(login);
        }

        public async Task<ResponseToken> VerifyOtp(VerifyOTPDto verifyotp)
        {
            return await _IFactoryAuthRepo.VerifyOtp(verifyotp);
        }
        //public async Task<CommonResponseModel> CheckEmailExistence(ForgetPasswordDTO dto)
        //{
        //    return await _IFactoryAuthRepo.CheckEmailExistence(dto);
        //}

        public async Task<ResponseToken> SwitchTenantAsync(int tenantId)
        {
            return await _IFactoryAuthRepo.SwitchTenantAsync(tenantId);
        }
        public async Task<CommonResponseModel> ResetPassword(ResetPasswordDTO dto)
        {
            return await _IFactoryAuthRepo.ResetPassword(dto);
        }
        //public async Task<CommonResponseModel> VerifyOtpByEmail(VerifyOtpByEmailDto dto)
        //{
        //    return await _IFactoryAuthRepo.VerifyOtpByEmail(dto);
        //}
        public async Task<ResponseOTPModel> ForgetPassword(ForgetPasswordDTO dto)
        {
            return await _IFactoryAuthRepo.ForgetPassword(dto);

        }
    }
}
