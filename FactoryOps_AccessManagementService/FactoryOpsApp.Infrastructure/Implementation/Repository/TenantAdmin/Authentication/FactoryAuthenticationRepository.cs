using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.Authentication;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.SuperAdmin.AuditLogs;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.Common;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.ExceptionLogger;
using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOps_AccessManagementService.FactoryOpsApp.Application.DTOs;
using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.Security;
using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Infrastructure.DBContext;
using FactoryOpsApp.Infrastructure.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using static FactoryOps_AccessManagementService.FactoryOpsApp.Common.CommonConstant;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Repository.TenantAdmin.Authentication
{
    public class FactoryAuthenticationRepository : IFactoryAuthenticationRepository
    {

        private readonly MasterFactoryOpsDbContext _masterDbcontext;
        private readonly TenantDbContextFactory _tenantDbContext;
        private readonly IConfiguration _config;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IExceptionLoggerService _exceptionLogger;
        private readonly IAuditLogService _auditLogger;
        private readonly IEmailService _iEmailService;
        private readonly SmtpSettings _settings;
        private readonly IPasswordHasher _hasher;
        public FactoryAuthenticationRepository(
            IConfiguration config,
            MasterFactoryOpsDbContext masterDbcontext,
            IHttpContextAccessor httpContextAccessor,
            TenantDbContextFactory tenantDbContext,
            IExceptionLoggerService exceptionLogger,
            IAuditLogService auditLogger,
            IEmailService iEmailService,
            IOptions<SmtpSettings> settings,
            IPasswordHasher hasher)
        {
            _config = config;
            _masterDbcontext = masterDbcontext;
            _httpContextAccessor = httpContextAccessor;
            _tenantDbContext = tenantDbContext;
            _exceptionLogger = exceptionLogger;
            _auditLogger = auditLogger;
            _iEmailService = iEmailService;
            _settings = settings.Value;
            _hasher = hasher;
        }

        #region Authenticate

        public async Task<ResponseToken> UnifiedAuthenticate(LoginDto login)
        {
            ResponseToken response = new ResponseToken();

            // == Super Admin ==
            var superAdmin = _masterDbcontext.AdminLogins.FirstOrDefault(l =>
                l.Email == login.Email &&
                l.IsActive && !l.IsDeleted);

            if (superAdmin != null && _hasher.Verify(login.Password!, superAdmin.PasswordHash))
            {
                var token = GenerateJWTTokenForSuperAdmin(superAdmin.Id);

                await _auditLogger.LogAuditAsync("Login", "SuperAdmin-login", null, login.Email, "Login");
                await _masterDbcontext.SaveChangesAsync();

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = AuthenticationStatusMessage.Success;
                response.Token = token;
                return response;
            }

            // == Tenant Admin ==
            var tenantAdmin = _masterDbcontext.TenantAdminLogins.FirstOrDefault(l =>
                l.Email == login.Email &&
                l.IsActive && !l.IsDeleted);

            if (tenantAdmin != null && _hasher.Verify(login.Password!, tenantAdmin.PasswordHash))
            {
                if (tenantAdmin.Suspend)
                {
                    response.StatusCode = StatusCode.Forbidden;
                    response.StatusMessage = AuthenticationStatusMessage.AccountSuspended;
                    return response;
                }

                var token = GenerateJWTTokenForTenantAdmin(tenantAdmin.Id, tenantAdmin.TenantId);

                await _auditLogger.LogAuditAsync("Login", "TenantAdmin-login", tenantAdmin.TenantId, login.Email, "Login");

                tenantAdmin.ForceLogout = false;

                var factoryTenant = _masterDbcontext.FactoryTenants.FirstOrDefault(t => t.TenantId == tenantAdmin.TenantId);
                if (factoryTenant != null)
                {
                    factoryTenant.ForceLogout = false;
                    factoryTenant.LastActiveDate = DateTime.UtcNow;
                }

                await _masterDbcontext.SaveChangesAsync();

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = AuthenticationStatusMessage.Success;
                response.Token = token;
                return response;
            }

            // == Global User ==
            var globalUser = _masterDbcontext.GlobalUsers.FirstOrDefault(l =>
                l.Email == login.Email &&
                l.IsActive && !l.IsDeleted);

            if (globalUser != null && _hasher.Verify(login.Password!, globalUser.Password))
            {
                if (globalUser.Suspend)
                {
                    response.StatusCode = StatusCode.Forbidden;
                    response.StatusMessage = AuthenticationStatusMessage.AccountSuspended;
                    return response;
                }

                using var tenantDb = _tenantDbContext.GetTenantDbContext(globalUser.TenantId);
                var factoryUser = tenantDb.FactoryUsers.FirstOrDefault(l => l.Email == login.Email && l.IsActive && !l.IsDeleted);

                if (factoryUser != null)
                {
                    // ===== MFA CHECK =====
                    if (factoryUser.MFAEnabled)
                    {
                        var otp = new Random().Next(100000, 999999).ToString();

                        factoryUser.OTPCode = otp;
                        factoryUser.OTPExpiry = DateTime.UtcNow.AddMinutes(5);

                        globalUser.LastLogin = DateTime.UtcNow;

                        await tenantDb.SaveChangesAsync();
                        await _masterDbcontext.SaveChangesAsync();

                        // Send OTP Email/SMS                       
                        await _iEmailService.SendEmailAsync(new EmailDTO
                        {
                            From = _settings.From ?? "no-reply@factoryops.com",
                            To = factoryUser.Email, // "factory.operation@yopmail.com",
                            Subject = "Your OTP Code",
                            Body = GetOtpEmailBody(factoryUser.FirstName, otp)
                        });

                        response.StatusCode = StatusCode.Success;
                        response.StatusMessage = "MFA required";
                        response.RequiresMFA = true;
                        response.UserId = factoryUser.UserId;
                        response.TenantId = globalUser.TenantId;

                        return response;
                    }

                    var token = GenerateJWTToken(factoryUser.UserId, globalUser.TenantId);

                    globalUser.LastLogin = DateTime.UtcNow;
                    globalUser.ForceLogout = false;
                    factoryUser.LastLogin = DateTime.UtcNow;
                    factoryUser.ForceLogout = false;

                    await _auditLogger.LogAuditAsync("Login", "User-login", globalUser.TenantId, login.Email, "Login");

                    await _masterDbcontext.SaveChangesAsync();
                    await tenantDb.SaveChangesAsync();

                    response.StatusCode = StatusCode.Success;
                    response.StatusMessage = AuthenticationStatusMessage.Success;
                    response.Token = token;
                    return response;
                }
            }

            response.StatusCode = StatusCode.BadRequest;
            response.StatusMessage = AuthenticationStatusMessage.InvalidCredentials;
            return response;
        }

        public async Task<ResponseToken> VerifyOtp(VerifyOTPDto verifyotp)
        {
            using var tenantDb = _tenantDbContext.GetTenantDbContext(verifyotp.TenantId);

            var user = await tenantDb.FactoryUsers
                .FirstOrDefaultAsync(x =>
                    x.UserId == verifyotp.UserId &&
                    x.OTPCode == verifyotp.Otp &&
                    x.OTPExpiry > DateTime.UtcNow);

            if (user == null)
                return new ResponseToken
                {
                    StatusCode = StatusCode.BadRequest,
                    StatusMessage = "Invalid or expired OTP"
                };

            var token = GenerateJWTToken(user.UserId, verifyotp.TenantId);

            user.OTPCode = null;
            user.OTPExpiry = null;
            user.LastLogin = DateTime.UtcNow;

            await tenantDb.SaveChangesAsync();

            return new ResponseToken
            {
                StatusCode = StatusCode.Success,
                StatusMessage = AuthenticationStatusMessage.Success,
                Token = token
            };
        }

        #endregion

        #region JWT Token Generation

        private string GenerateJWTToken(int userId, int tenantId)
        {
            using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

            var objUser = (from user in tenantDb.FactoryUsers
                           join userRole in tenantDb.FactoryUserRoles on user.UserId equals userRole.UserId
                           join role in tenantDb.FactoryRoles on userRole.RoleId equals role.RoleId
                           where user.UserId == userId
                                 && !user.IsDeleted
                                 && !userRole.IsDeleted
                                 && !role.IsDeleted
                           select new
                           {
                               user.UserId,
                               user.Email,
                               user.Username,
                               TenantId = tenantId,
                               role.RoleId,
                               role.RoleName
                           }).FirstOrDefault();

            if (objUser == null)
                throw new UnauthorizedAccessException("User not found or role not assigned");

            //-----------------------------
            // Load JWT Config Safely
            //-----------------------------
            var jwtKey = _config["JwtSettings:Key"];
            var issuer = _config["JwtSettings:Issuer"];
            var audience = _config["JwtSettings:Audience"];

            if (string.IsNullOrWhiteSpace(jwtKey))
                throw new Exception("JWT Key not configured");

            //-----------------------------
            // JWT Claims (SMALL + SAFE)
            //-----------------------------
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, objUser.UserId.ToString()),
        new Claim("UserId", objUser.UserId.ToString()),
        new Claim("TenantId", objUser.TenantId.ToString()),
        new Claim("RoleId", objUser.RoleId.ToString()),

        new Claim(JwtRegisteredClaimNames.Sub, objUser.UserId.ToString()),
        new Claim(JwtRegisteredClaimNames.Email, objUser.Email ?? string.Empty),

        new Claim(ClaimTypes.Role, objUser.RoleName.Replace(" ", "").Trim()),

        // Tracking / Security
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new Claim(JwtRegisteredClaimNames.Iat,
            DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
            ClaimValueTypes.Integer64)
    };

            //-----------------------------
            // Token Generation
            //-----------------------------
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var expiryMinutes = int.TryParse(_config["JwtSettings:Expires"], out var minutes)
                    ? minutes
                    : 120;

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


        private string GenerateJWTTokenForSuperAdmin(int adminId)
        {
            var objUser = (from ad in _masterDbcontext.AdminLogins
                           join rl in _masterDbcontext.AdminRoles on ad.RoleId equals rl.RoleId
                           where ad.Id == adminId
                           select new { ad.Id, ad.Email, rl.RoleName }).FirstOrDefault();

            if (objUser == null) throw new Exception("User not found");

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, objUser.Id.ToString()),
                new Claim("UserId", objUser.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Sub, objUser.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, objUser.Email),
                new Claim(ClaimTypes.Role, objUser.RoleName)
            };

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JwtSettings:Key"]!));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var expiryMinutes = int.TryParse(_config["JwtSettings:Expires"], out var minutes)
                    ? minutes
                    : 120;
            var token = new JwtSecurityToken(
                issuer: _config["JwtSettings:Issuer"],
                audience: _config["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateJWTTokenForTenantAdmin(int adminId, int tenantId)
        {
            var objUser = (from ad in _masterDbcontext.TenantAdminLogins
                           join rl in _masterDbcontext.AdminRoles on ad.RoleId equals rl.RoleId
                           where ad.Id == adminId && ad.TenantId == tenantId
                           select new { ad.Id, ad.TenantId, ad.Email, rl.RoleId, rl.RoleName }).FirstOrDefault();

            if (objUser == null)
                throw new Exception($"Admin with ID {adminId} not found in tenant {tenantId}");

            var modulePermissions = _masterDbcontext.ModuleMasterMapping
                .Where(mm => mm.TenantId == tenantId && mm.IsActive && !mm.IsDeleted)
                .Join(_masterDbcontext.ModuleMaster,
                      mm => mm.ModuleId,
                      m => m.ModuleId,
                      (mm, m) => new { m.ModuleId, m.ModuleName })
                .ToList();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, objUser.Id.ToString()),
                new Claim("AdminId", objUser.Id.ToString()),
                new Claim("TenantId", objUser.TenantId.ToString()),
                new Claim("RoleId", objUser.RoleId.ToString()),
                new Claim(JwtRegisteredClaimNames.Sub, objUser.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, objUser.Email),
                new Claim(ClaimTypes.Role, objUser.RoleName.Replace(" ", "").Trim())
            };

            foreach (var module in modulePermissions)
                claims.Add(new Claim("ModuleAccess", module.ModuleName));

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JwtSettings:Key"]!));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var expiryMinutes = int.TryParse(_config["JwtSettings:Expires"], out var minutes)
                                ? minutes
                                : 120;

            var token = new JwtSecurityToken(
                issuer: _config["JwtSettings:Issuer"],
                audience: _config["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        #endregion


        public async Task<ResponseOTPModel> ForgetPassword(ForgetPasswordDTO dto)
        {
            // == Super Admin ==
            var superAdmin = await _masterDbcontext.AdminLogins
                .FirstOrDefaultAsync(x =>
                    x.Email == dto.email &&
                    x.IsActive &&
                    !x.IsDeleted);

            if (superAdmin != null)
            {
                var otp = await GenerateAndSendOtp(dto.email, null, true, "SuperAdmin");

                return new ResponseOTPModel
                {
                    StatusCode = StatusCode.Success,
                    StatusMessage = "OTP sent",
                    OtpCode = otp
                };
            }

            // == Tenant Admin ==
            var tenantAdmin = await _masterDbcontext.TenantAdminLogins
                .FirstOrDefaultAsync(x =>
                    x.Email == dto.email &&
                    x.IsActive &&
                    !x.IsDeleted);

            if (tenantAdmin != null)
            {
                if (tenantAdmin.Suspend)
                {
                    return new ResponseOTPModel
                    {
                        StatusCode = StatusCode.Forbidden,
                        StatusMessage = "Account suspended"
                    };
                }

                var otp = await GenerateAndSendOtp(dto.email, tenantAdmin.TenantId, true, "TenantAdmin");

                return new ResponseOTPModel
                {
                    StatusCode = StatusCode.Success,
                    StatusMessage = "OTP sent",
                    OtpCode = otp
                };
            }

            // == Global User ==
            var globalUser = await _masterDbcontext.GlobalUsers
                .FirstOrDefaultAsync(x =>
                    x.Email == dto.email &&
                    x.IsActive &&
                    !x.IsDeleted);

            if (globalUser != null)
            {
                if (globalUser.Suspend)
                {
                    return new ResponseOTPModel
                    {
                        StatusCode = StatusCode.Forbidden,
                        StatusMessage = "Account suspended"
                    };
                }

                var otp = await GenerateAndSendOtp(dto.email, globalUser.TenantId, true, "GlobalUser");

                return new ResponseOTPModel
                {
                    StatusCode = StatusCode.Success,
                    StatusMessage = "OTP sent",
                    OtpCode = otp
                };
            }

            return new ResponseOTPModel
            {
                StatusCode = StatusCode.NotFound,
                StatusMessage = "Email not found"
            };
        }

        public async Task<CommonResponseModel> ResetPassword(ResetPasswordDTO dto)
        {
            var now = DateTime.UtcNow;
            var hashedPassword = _hasher.Hash(dto.NewPassword);

            // == Super Admin ==
            var superAdmin = await _masterDbcontext.AdminLogins
                .FirstOrDefaultAsync(x =>
                    x.Email == dto.Email &&
                    x.IsActive &&
                    !x.IsDeleted);

            if (superAdmin != null)
            {
                return await HandlePasswordReset(
                    dto,
                    superAdmin.PasswordResetRequested,
                    superAdmin.OTPCode,
                    async () =>
                    {
                        superAdmin.PasswordHash = hashedPassword;
                        superAdmin.PasswordResetRequested = false;
                        superAdmin.UpdatedAt = now;

                        await _masterDbcontext.SaveChangesAsync();
                    });
            }

            // == Tenant Admin ==
            var tenantAdmin = await _masterDbcontext.TenantAdminLogins
                .FirstOrDefaultAsync(x =>
                    x.Email == dto.Email &&
                    x.IsActive &&
                    !x.IsDeleted);

            if (tenantAdmin != null)
            {
                return await HandlePasswordReset(
                    dto,
                    tenantAdmin.PasswordResetRequested,
                    tenantAdmin.OTPCode,
                    async () =>
                    {
                        tenantAdmin.PasswordHash = hashedPassword;
                        tenantAdmin.PasswordResetRequested = false;
                        tenantAdmin.UpdatedAt = now;

                        await _masterDbcontext.SaveChangesAsync();
                    });
            }

            // == Global User ==
            var globalUser = await _masterDbcontext.GlobalUsers
                .FirstOrDefaultAsync(x =>
                    x.Email == dto.Email &&
                    x.IsActive &&
                    !x.IsDeleted);

            if (globalUser != null)
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(globalUser.TenantId);

                var user = await tenantDb.FactoryUsers
                    .FirstOrDefaultAsync(x =>
                        x.Email == dto.Email &&
                        x.IsActive &&
                        !x.IsDeleted);

                if (user == null)
                {
                    return new CommonResponseModel
                    {
                        StatusCode = StatusCode.NotFound,
                        StatusMessage = "User not found"
                    };
                }

                return await HandlePasswordReset(
                    dto,
                    user.PasswordResetRequested,
                    user.OTPCode,
                    async () =>
                    {
                        user.PasswordHash = hashedPassword;
                        user.PasswordResetRequested = false;
                        user.UpdatedAt = now;

                        globalUser.Password = hashedPassword;

                        await tenantDb.SaveChangesAsync();
                        await _masterDbcontext.SaveChangesAsync();
                    });
            }

            return new CommonResponseModel
            {
                StatusCode = StatusCode.NotFound,
                StatusMessage = "User not found"
            };
        }

        private async Task<CommonResponseModel> HandlePasswordReset(
            ResetPasswordDTO dto,
            bool passwordResetRequested,
            string? storedOtp,
            Func<Task> updatePasswordAction)
        {
            if (!passwordResetRequested)
            {
                return new CommonResponseModel
                {
                    StatusCode = StatusCode.BadRequest,
                    StatusMessage = "OTP not exist"
                };
            }

            if (storedOtp != dto.OTP)
            {
                return new CommonResponseModel
                {
                    StatusCode = StatusCode.BadRequest,
                    StatusMessage = "OTP not matched"
                };
            }

            await updatePasswordAction();

            return new CommonResponseModel
            {
                StatusCode = StatusCode.Success,
                StatusMessage = "Password reset successful"
            };
        }


        #region SwitchTenant

        public async Task<ResponseToken> SwitchTenantAsync(int tenantId)
        {
            ResponseToken response = new ResponseToken();

            var currentUserId = _httpContextAccessor.HttpContext!.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                 ?? _httpContextAccessor.HttpContext.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

            var isSuperAdmin = _masterDbcontext.AdminLogins.Any(a => a.Id == Convert.ToInt32(currentUserId) && a.IsActive && !a.IsDeleted);

            if (!isSuperAdmin)
            {
                response.StatusCode = StatusCode.Forbidden;
                response.StatusMessage = AuthenticationStatusMessage.ForbiddenSwitchTenant;
                return response;
            }

            var tenant = _masterDbcontext.FactoryTenants.FirstOrDefault(t => t.TenantId == tenantId && t.IsActive && !t.IsDeleted);
            if (tenant == null)
            {
                response.StatusCode = StatusCode.NotFound;
                response.StatusMessage = AuthenticationStatusMessage.TenantNotFound;
                return response;
            }

            var tenantAdmin = _masterDbcontext.TenantAdminLogins.FirstOrDefault(t => t.TenantId == tenantId && t.IsActive && !t.IsDeleted);
            if (tenantAdmin == null)
            {
                response.StatusCode = StatusCode.NotFound;
                response.StatusMessage = AuthenticationStatusMessage.NoTenantAdminFound;
                return response;
            }

            var token = GenerateJWTTokenForTenantAdmin(tenantAdmin.Id, tenantId);

            await _auditLogger.LogAuditAsync("login", $"SuperAdmin switched to Tenant {tenantId}", tenantId, tenantAdmin.Email, "SwitchTenant");
            await _masterDbcontext.SaveChangesAsync();

            response.StatusCode = StatusCode.Success;
            response.StatusMessage = AuthenticationStatusMessage.Success;
            response.Token = token;
            return response;
        }

        #endregion

        private string GetOtpEmailBody(string name, string otp)
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(),
                "FactoryOpsApp.Application", "Templates", "OtpEmail.html");

            var template = File.ReadAllText(filePath);

            template = template.Replace("{{name}}", name)
                               .Replace("{{otp}}", otp);

            return template;
        }

        private async Task<string> GenerateAndSendOtp(string email, int? tenantId, bool isPasswordReset, string userType)
        {
            var otp = new Random().Next(100000, 999999).ToString();
            var expiry = DateTime.UtcNow.AddMinutes(5);

            switch (userType)
            {
                case "SuperAdmin":
                    {
                        var superAdmin = await _masterDbcontext.AdminLogins
                            .FirstOrDefaultAsync(x =>
                                x.Email == email &&
                                x.IsActive &&
                                !x.IsDeleted);

                        if (superAdmin == null)
                            throw new Exception("Super Admin not found");

                        superAdmin.OTPCode = otp;
                        superAdmin.OTPExpiry = expiry;
                        superAdmin.PasswordResetRequested = isPasswordReset;

                        break;
                    }

                case "TenantAdmin":
                    {
                        var tenantAdmin = await _masterDbcontext.TenantAdminLogins
                            .FirstOrDefaultAsync(x =>
                                x.Email == email &&
                                x.IsActive &&
                                !x.IsDeleted);

                        if (tenantAdmin == null)
                            throw new Exception("Tenant Admin not found");

                        tenantAdmin.OTPCode = otp;
                        tenantAdmin.OTPExpiry = expiry;
                        tenantAdmin.PasswordResetRequested = isPasswordReset;

                        break;
                    }

                case "GlobalUser":
                    {
                        if (!tenantId.HasValue)
                            throw new Exception("TenantId required for Global User");

                        using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId.Value);

                        var user = await tenantDb.FactoryUsers
                            .FirstOrDefaultAsync(x =>
                                x.Email == email &&
                                x.IsActive &&
                                !x.IsDeleted);

                        if (user == null)
                            throw new Exception("Factory User not found");

                        user.OTPCode = otp;
                        user.OTPExpiry = expiry;
                        user.PasswordResetRequested = isPasswordReset;

                        await tenantDb.SaveChangesAsync();
                        goto SendEmail;
                    }

                default:
                    throw new Exception("Invalid user type");
            }

            await _masterDbcontext.SaveChangesAsync();

        SendEmail:

            await _iEmailService.SendEmailAsync(new EmailDTO
            {
                To = email,
                Subject = isPasswordReset ? "Password Reset OTP" : "Login OTP",
                Body = $"Your OTP is {otp}"
            });

            return otp;
        }
    }
}
