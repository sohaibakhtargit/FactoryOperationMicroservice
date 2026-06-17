using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.Authentication;
using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOps_AccessManagementService.FactoryOpsApp.Application.DTOs;
using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.Security;
using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Infrastructure.DBContext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Runtime;

namespace FactoryOps_AccessManagementService.Controllers.AuthenticateController
{  /// <summary>
   /// Authentication API
   /// Manages user authentication, tenant switching, and password recovery
   /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticateController : ControllerBase
    {
        private readonly IFactoryAuthenticationService _iAuthService;

        private readonly MasterFactoryOpsDbContext _masterDbcontext;
        private readonly TenantDbContextFactory _tenantDbContext;
        private readonly IPasswordHasher _hasher;
        public AuthenticateController(IFactoryAuthenticationService iAuthService,
            MasterFactoryOpsDbContext masterDbcontext,
            TenantDbContextFactory tenantDbContext,
            IPasswordHasher hasher)
        {
            _iAuthService = iAuthService;
            _masterDbcontext = masterDbcontext;
            _tenantDbContext = tenantDbContext;
            _hasher = hasher;
        }

        /// <summary>
        /// User login
        /// Authenticates user credentials and returns access token
        /// </summary>
        /// <param name="login">User login credentials</param>
        /// <returns>Authentication token and user information</returns>
        /// <response code="200">Successfully authenticated</response>
        /// <response code="400">Invalid login data provided</response>
        /// <response code="401">Authentication failed</response>
        [AllowAnonymous]
        [HttpPost("authenticate")]
        public async Task<IActionResult> Login([FromBody] LoginDto login)
        {
            if (string.IsNullOrEmpty(login.Email) || string.IsNullOrEmpty(login.Password))
            {
                return BadRequest(new ResponseToken
                {
                    StatusCode = "400",
                    StatusMessage = "Email and Password are required."
                });
            }

            var result = await _iAuthService.UnifiedAuthenticate(login);

            if (result.StatusCode == "200")
            {
                return Ok(result);
            }
            else
            {
                return Unauthorized(result);
            }
        }

        [AllowAnonymous]
        [HttpPost("verifyOtp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOTPDto verifyotp)
        {
            var result = await _iAuthService.VerifyOtp(verifyotp);
            return Ok(result);
        }

        /// <summary>
        /// Switch tenant
        /// Allows super admin to switch between different tenant contexts
        /// </summary>
        /// <param name="request">Tenant switching request</param>
        /// <returns>Tenant switch confirmation</returns>
        /// <response code="200">Successfully switched tenant</response>
        [Authorize(Roles = "SuperAdmin")]
        [HttpPost("switch-to-tenant")]
        public async Task<IActionResult> SwitchTenant([FromBody] SwitchTenantRequestDto request)
        {
            var response = await _iAuthService.SwitchTenantAsync(request.TenantId);
            return Ok(response);
        }

        /// <summary>
        /// Forget password
        /// Initiates password recovery process for user account
        /// </summary>
        /// <param name="dto">Password recovery request data</param>
        /// <returns>Password recovery initiation result</returns>
        /// <response code="200">Password recovery process initiated</response>
    
        [AllowAnonymous]
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgetPasswordDTO dto)
        {
            if (string.IsNullOrEmpty(dto.email))
            {
                return BadRequest(new CommonResponseModel
                {
                    StatusCode = "400",
                    StatusMessage = "Email is required"
                });
            }

            var result = await _iAuthService.ForgetPassword(dto);
            return Ok(result);
        }

        //[AllowAnonymous]
        //[HttpPost("verify-otp-email")]
        //public async Task<IActionResult> VerifyOtpByEmail([FromBody] VerifyOtpByEmailDto dto)
        //{
        //    var result = await _iAuthService.VerifyOtpByEmail(dto);
        //    return Ok(result);
        //}

        [AllowAnonymous]
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDTO dto)
        {
            var result = await _iAuthService.ResetPassword(dto);
            return Ok(result);
        }

        [HttpPost("migrate-passwords")]
        public async Task<IActionResult> MigratePasswords()
        {
            int updatedCount = 0;

            // 1. SUPER ADMIN
            var superAdmins = _masterDbcontext.AdminLogins
                .Where(x => x.IsActive && !x.IsDeleted)
                .ToList();

            foreach (var admin in superAdmins)
            {
                if (!IsHashed(admin.PasswordHash))
                {
                    admin.PasswordHash = _hasher.Hash(admin.PasswordHash);
                    updatedCount++;
                }
            }

            // 2. TENANT ADMINS
            var tenantAdmins = _masterDbcontext.TenantAdminLogins
                .Where(x => x.IsActive && !x.IsDeleted)
                .ToList();

            foreach (var admin in tenantAdmins)
            {
                if (!IsHashed(admin.PasswordHash))
                {
                    admin.PasswordHash = _hasher.Hash(admin.PasswordHash);
                    updatedCount++;
                }
            }

            // 3. GLOBAL USERS
            var globalUsers = _masterDbcontext.GlobalUsers
                .Where(x => x.IsActive && !x.IsDeleted)
                .ToList();

            foreach (var user in globalUsers)
            {
                if (!IsHashed(user.Password))
                {
                    user.Password = _hasher.Hash(user.Password);
                    updatedCount++;
                }
            }

            await _masterDbcontext.SaveChangesAsync();

            // 4. TENANT USERS (MULTI-TENANT LOOP)
            var tenants = _masterDbcontext.FactoryTenants
          .Where(l => l.IsActive && !l.IsDeleted)
          .ToList();


            foreach (var tenant in tenants)
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenant.TenantId);

                var users = tenantDb.FactoryUsers
                    .Where(x => x.IsActive && !x.IsDeleted)
                    .ToList();

                foreach (var user in users)
                {
                    if (!IsHashed(user.PasswordHash))
                    {
                        user.PasswordHash = _hasher.Hash(user.PasswordHash);
                        updatedCount++;
                    }
                }

                await tenantDb.SaveChangesAsync();
            }

            return Ok(new
            {
                message = "Password migration completed",
                totalUpdated = updatedCount
            });
        }

        // SIMPLE CHECK (important to avoid double hashing)
        private bool IsHashed(string password)
        {
            if (string.IsNullOrEmpty(password)) return true;

            // Example: BCrypt starts with $2a / $2b
            return password.StartsWith("$2");
        }


        [HttpPost("force-reset-all-passwords")]
        public async Task<IActionResult> ForceResetAllPasswords()
        {
            int updatedCount = 0;
            string tempPassword = "Temp@123";
            string hashedPassword = _hasher.Hash(tempPassword);

            // 1. SUPER ADMINS
            var superAdmins = await _masterDbcontext.AdminLogins
                .Where(x => x.IsActive && !x.IsDeleted)
                .ToListAsync();

            foreach (var admin in superAdmins)
            {
                admin.PasswordHash = hashedPassword;
                updatedCount++;
            }

            // 2. TENANT ADMINS
            var tenantAdmins = await _masterDbcontext.TenantAdminLogins
                .Where(x => x.IsActive && !x.IsDeleted)
                .ToListAsync();

            foreach (var admin in tenantAdmins)
            {
                admin.PasswordHash = hashedPassword;
                updatedCount++;
            }

            // 3. GLOBAL USERS
            var globalUsers = await _masterDbcontext.GlobalUsers
                .Where(x => x.IsActive && !x.IsDeleted)
                .ToListAsync();

            foreach (var user in globalUsers)
            {
                user.Password = hashedPassword;
                user.ForceLogout = true; // logout all
                updatedCount++;
            }

            await _masterDbcontext.SaveChangesAsync();

            // 4. TENANT USERS (MULTI-TENANT)
            var tenants = await _masterDbcontext.FactoryTenants
                .Where(x => x.IsActive && !x.IsDeleted)
                .ToListAsync();

            foreach (var tenant in tenants)
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenant.TenantId);

                var users = await tenantDb.FactoryUsers
                    .Where(x => x.IsActive && !x.IsDeleted)
                    .ToListAsync();

                foreach (var user in users)
                {
                    user.PasswordHash = hashedPassword;
                    user.ForceLogout = true;
                    updatedCount++;
                }

                await tenantDb.SaveChangesAsync();
            }

            return Ok(new
            {
                message = "All passwords reset to Temp@123. Users must change password.",
                totalUpdated = updatedCount
            });
        }


        [HttpPost("reset-specific-user-password")]
        public async Task<IActionResult> ResetSpecificUserPassword(string email, string password)
        {
            int updatedCount = 0;

            var hashedPassword = _hasher.Hash(password);

            // 1. SUPER ADMIN
            var superAdmin = await _masterDbcontext.AdminLogins
                .FirstOrDefaultAsync(x => x.Email == email && x.IsActive && !x.IsDeleted);

            if (superAdmin != null)
            {
                superAdmin.PasswordHash = hashedPassword;
                updatedCount++;
            }

            // 2. TENANT ADMIN
            var tenantAdmin = await _masterDbcontext.TenantAdminLogins
                .FirstOrDefaultAsync(x => x.Email == email && x.IsActive && !x.IsDeleted);

            if (tenantAdmin != null)
            {
                tenantAdmin.PasswordHash = hashedPassword;
                updatedCount++;
            }

            // 3. GLOBAL USER
            var globalUser = await _masterDbcontext.GlobalUsers
                .FirstOrDefaultAsync(x => x.Email == email && x.IsActive && !x.IsDeleted);

            if (globalUser != null)
            {
                globalUser.Password = hashedPassword;
                globalUser.ForceLogout = false;
                updatedCount++;

                using var tenantDb = _tenantDbContext.GetTenantDbContext(globalUser.TenantId);

                var factoryUser = await tenantDb.FactoryUsers
                    .FirstOrDefaultAsync(x => x.Email == email && x.IsActive && !x.IsDeleted);

                if (factoryUser != null)
                {
                    factoryUser.PasswordHash = hashedPassword;
                    factoryUser.ForceLogout = false;
                    updatedCount++;

                    await tenantDb.SaveChangesAsync();
                }
            }

            await _masterDbcontext.SaveChangesAsync();

            return Ok(new
            {
                message = $"Password reset successful for {email}",
                totalUpdated = updatedCount
            });
        }
    }


}