using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.Authentication;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.SuperAdmin.AuditLogs;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.ExceptionLogger;
using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Infrastructure.DBContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
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

        public FactoryAuthenticationRepository(
            IConfiguration config,
            MasterFactoryOpsDbContext masterDbcontext,
            IHttpContextAccessor httpContextAccessor,
            TenantDbContextFactory tenantDbContext,
            IExceptionLoggerService exceptionLogger,
            IAuditLogService auditLogger)
        {
            _config = config;
            _masterDbcontext = masterDbcontext;
            _httpContextAccessor = httpContextAccessor;
            _tenantDbContext = tenantDbContext;
            _exceptionLogger = exceptionLogger;
            _auditLogger = auditLogger;
        }

        #region Authenticate

        public async Task<ResponseToken> UnifiedAuthenticate(LoginDto login)
        {
            ResponseToken response = new ResponseToken();

            // == Super Admin ==
            var superAdmin = _masterDbcontext.AdminLogins.FirstOrDefault(l =>
                l.Email == login.Email &&
                l.PasswordHash == login.Password &&
                l.IsActive && !l.IsDeleted);

            if (superAdmin != null)
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
                l.PasswordHash == login.Password &&
                l.IsActive && !l.IsDeleted);

            if (tenantAdmin != null)
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
                l.Password == login.Password &&
                l.IsActive && !l.IsDeleted);

            if (globalUser != null)
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

            if (objUser == null) throw new Exception("User not found");

            //-----------------------------
            // Fetch Permissions with SubModules
            //-----------------------------
            var permissionList = tenantDb.FactoryRolePermissions
                .Where(rp => rp.RoleId == objUser.RoleId && rp.IsActive && !rp.IsDeleted)
                .Join(tenantDb.FactoryPermissions,
                      rp => rp.PermissionId,
                      p => p.PermissionId,
                      (rp, p) => new
                      {
                          p.PermissionId,
                          p.Name,
                          SubPermissions = p.SubPermissions
                               .Where(s => !s.IsDeleted)
                               .Select(s => s.Name)
                               .ToList()
                      })
                .ToList();


            //-----------------------------
            // JWT Claims
            //-----------------------------
            var claims = new List<Claim>
    {
        new Claim("UserId", objUser.UserId.ToString()),
        new Claim("TenantId", objUser.TenantId.ToString()),
        new Claim("RoleId", objUser.RoleId.ToString()),
        new Claim(JwtRegisteredClaimNames.Sub, objUser.UserId.ToString()),
        new Claim(JwtRegisteredClaimNames.Email, objUser.Email),
        new Claim(ClaimTypes.Role, objUser.RoleName.Replace(" ", "").Trim())
    };

            //-----------------------------
            // Add individual flat permissions (parent permission only)
            //-----------------------------
            foreach (var p in permissionList)
                claims.Add(new Claim("Permission", p.Name));


            //-----------------------------
            // ADD HIERARCHY CLAIM
            //-----------------------------
            var hierarchy = permissionList
                .Select(p => new
                {
                    permissionName = p.Name,
                    subPermissions = p.SubPermissions
                })
                .ToList();

            claims.Add(new Claim("PermissionHierarchy", JsonConvert.SerializeObject(hierarchy)));


            //-----------------------------
            // Token Generation
            //-----------------------------
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JwtSettings:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["JwtSettings:Issuer"],
                audience: _config["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(120),
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
                new Claim("UserId", objUser.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Sub, objUser.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, objUser.Email),
                new Claim(ClaimTypes.Role, objUser.RoleName)
            };

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JwtSettings:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["JwtSettings:Issuer"],
                audience: _config["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(120),
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
                new Claim("AdminId", objUser.Id.ToString()),
                new Claim("TenantId", objUser.TenantId.ToString()),
                new Claim("RoleId", objUser.RoleId.ToString()),
                new Claim(JwtRegisteredClaimNames.Sub, objUser.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, objUser.Email),
                new Claim(ClaimTypes.Role, objUser.RoleName.Replace(" ", "").Trim())
            };

            foreach (var module in modulePermissions)
                claims.Add(new Claim("ModuleAccess", module.ModuleName));

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JwtSettings:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["JwtSettings:Issuer"],
                audience: _config["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(120),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        #endregion

        #region CheckEmailExistence

        public async Task<CommonResponseModel> CheckEmailExistence(ForgetPasswordDTO dto)
        {
            var response = new CommonResponseModel();
            var timestamp = DateTime.UtcNow;

            // SuperAdmin
            var superAdmin = await _masterDbcontext.AdminLogins.FirstOrDefaultAsync(l => l.Email == dto.email && l.IsActive && !l.IsDeleted);
            if (superAdmin != null)
                return await ResetPassword(superAdmin, dto.newPassword, null, "SuperAdmin password reset", AuthenticationStatusMessage.PasswordUpdatedSuperAdmin);

            // TenantAdmin
            var tenantAdmin = await _masterDbcontext.TenantAdminLogins.FirstOrDefaultAsync(l => l.Email == dto.email && l.IsActive && !l.IsDeleted);
            if (tenantAdmin != null)
                return await ResetPassword(tenantAdmin, dto.newPassword, tenantAdmin.TenantId, "TenantAdmin password reset", AuthenticationStatusMessage.PasswordUpdatedTenantAdmin);

            // GlobalUser
            var globalUser = await _masterDbcontext.GlobalUsers.FirstOrDefaultAsync(l => l.Email == dto.email && l.IsActive && !l.IsDeleted && !l.Suspend);
            if (globalUser != null)
            {
                using var masterTransaction = await _masterDbcontext.Database.BeginTransactionAsync();
                using var tenantDb = _tenantDbContext.GetTenantDbContext(globalUser.TenantId);
                await using var tenantTransaction = await tenantDb.Database.BeginTransactionAsync();

                try
                {
                    globalUser.Password = dto.newPassword;
                    globalUser.UpdatedAt = timestamp;

                    var factoryUser = await tenantDb.FactoryUsers.FirstOrDefaultAsync(u => u.Email == dto.email);
                    if (factoryUser != null)
                    {
                        factoryUser.PasswordHash = dto.newPassword;
                        factoryUser.UpdatedAt = timestamp;
                    }

                    await _auditLogger.LogAuditAsync("PasswordReset", "GlobalUser password reset", globalUser.TenantId, dto.email, "Security");

                    await _masterDbcontext.SaveChangesAsync();
                    await tenantDb.SaveChangesAsync();

                    await masterTransaction.CommitAsync();
                    await tenantTransaction.CommitAsync();

                    response.StatusCode = StatusCode.Success;
                    response.StatusMessage = AuthenticationStatusMessage.PasswordResetSuccess;
                }
                catch (Exception ex)
                {
                    await _exceptionLogger.LogExceptionAsync(ex, "AuthModule", "Forget-Password", globalUser.TenantId, null);
                    await masterTransaction.RollbackAsync();
                    await tenantTransaction.RollbackAsync();

                    response.StatusCode = StatusCode.Error;
                    response.StatusMessage = AuthenticationStatusMessage.PasswordResetFailed + " " + ex.Message;
                }
                return response;
            }

            response.StatusCode = StatusCode.NotFound;
            response.StatusMessage = AuthenticationStatusMessage.EmailNotFound;
            return response;
        }

        private async Task<CommonResponseModel> ResetPassword(dynamic user, string newPassword, int? tenantId, string auditDetails, string successMessage)
        {
            var response = new CommonResponseModel();
            var timestamp = DateTime.UtcNow;

            try
            {
                using var transaction = await _masterDbcontext.Database.BeginTransactionAsync();

                user.PasswordHash = newPassword;
                user.UpdatedAt = timestamp;

                await _auditLogger.LogAuditAsync("PasswordReset", auditDetails, tenantId, user.Email, "Security");

                await _masterDbcontext.SaveChangesAsync();
                await transaction.CommitAsync();

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = successMessage;
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "AuthModule", "Forget-Password", tenantId, null);
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = AuthenticationStatusMessage.PasswordResetFailed + " " + ex.Message;
            }

            return response;
        }

        #endregion

        #region SwitchTenant

        public async Task<ResponseToken> SwitchTenantAsync(int tenantId)
        {
            ResponseToken response = new ResponseToken();

            var currentUserId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
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
    }
}
