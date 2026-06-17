using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.SuperAdmin.UserManagement;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.SuperAdmin.AuditLogs;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.Common;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.ExceptionLogger;
using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Domain.Entities.MasterTenantsAdmin;
using FactoryOpsApp.Infrastructure.DBContext;
using Microsoft.EntityFrameworkCore;
using static FactoryOps_AccessManagementService.FactoryOpsApp.Common.CommonConstant;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Repository.SuperAdmin.UserManagement
{
    public class GlobalUserRepository : IGlobalUserRepository
    {
        private readonly MasterFactoryOpsDbContext _masterDbcontext;
        private readonly TenantDbContextFactory _tenantDbContext;
        private readonly IExceptionLoggerService _exceptionLogger;
        private readonly IAuditLogService _auditLogger;
        private readonly IConfiguration _configuration;
        private readonly IFileStorageService _fileStorageService;
        public GlobalUserRepository(TenantDbContextFactory tenantDbContext,
            MasterFactoryOpsDbContext masterDbcontext,
            IExceptionLoggerService exceptionLogger,
            IAuditLogService auditLogger,
            IConfiguration configuration,
            IFileStorageService fileStorageService)
        {
            _masterDbcontext = masterDbcontext;
            _tenantDbContext = tenantDbContext;
            _exceptionLogger = exceptionLogger;
            _auditLogger = auditLogger;
            _configuration = configuration;
            _fileStorageService = fileStorageService;
        }

        public async Task<CommonResponseModel> ForceLogout(int Id)
        {
            CommonResponseModel response = new CommonResponseModel();
            int CreateUserId = 0;
            int TenantId = 0;
            try
            {
                var existingUser = await _masterDbcontext.GlobalUsers.FirstOrDefaultAsync(u => u.GlobalUserId == Id);
                if (existingUser == null)
                {
                    response.StatusCode = StatusCode.NotFound;
                    response.StatusMessage = GlobalUserStatusMessage.UserNotFound;
                    return response;
                }

                if (existingUser.ForceLogout)
                {
                    response.StatusCode = StatusCode.BadRequest;
                    response.StatusMessage = GlobalUserStatusMessage.AlreadyForceLoggedOut;
                    return response;
                }
                existingUser.ForceLogout = true;

                using var tenantDb = _tenantDbContext.GetTenantDbContext(existingUser.TenantId);

                var existingTenantUser = await tenantDb.FactoryUsers.FirstOrDefaultAsync(u => u.Email == existingUser.Email);
                if (existingTenantUser == null)
                {
                    response.StatusCode = StatusCode.NotFound;
                    response.StatusMessage = GlobalUserStatusMessage.UserNotFound;
                    return response;
                }

                if (existingTenantUser.ForceLogout)
                {
                    response.StatusCode = StatusCode.BadRequest;
                    response.StatusMessage = GlobalUserStatusMessage.AlreadyForceLoggedOut;
                    return response;
                }

                CreateUserId = existingTenantUser.UserId;
                TenantId = existingUser.TenantId;
                existingTenantUser.ForceLogout = true;

                await _auditLogger.LogAuditAsync(
                       action: "Update",
                       details: $"{GlobalUserStatusMessage.AuditForceLogout} '{existingUser.Email}'",
                       tenantId: existingUser.TenantId,
                       email: existingUser.Email,
                       eventType: "UserManagement"
                   );

                await _masterDbcontext.SaveChangesAsync();
                await tenantDb.SaveChangesAsync();
                response.StatusCode = StatusCode.Success;
                response.StatusMessage = GlobalUserStatusMessage.ForceLogoutSuccess;

            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(
                    ex,
                    sourceModule: "GlobalUserModule",
                    apiName: "force-logout-user",
                    tenantId: TenantId,
                    userId: CreateUserId
                );
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = ex.Message;
            }

            return response;
        }
        public GetAllRecord<GetAllGlobalUserDto> GetAllGlobalUsers()
        {
            GetAllRecord<GetAllGlobalUserDto> response = new();
            try
            {
                var userList = (from user in _masterDbcontext.GlobalUsers
                                where user.IsDeleted == false
                                orderby user.GlobalUserId descending
                                select new GetAllGlobalUserDto
                                {
                                    GlobalUserId = user.GlobalUserId,
                                    TenantId = user.TenantId,
                                    TenantName = user.FactoryTenants.TenantName,
                                    Email = user.Email,
                                    Status = user.Status,
                                    LastLogin = user.LastLogin,
                                    IpAddress = user.IpAddress,
                                    RoleId = user.RoleId,
                                    Roles = user.Roles,
                                    Suspend = user.Suspend,
                                    SuspendedBy = user.SuspendedBy,
                                    ForceLogout = user.ForceLogout,
                                    IsActive = user.IsActive,
                                    IsDeleted = user.IsDeleted,
                                    CreatedAt = user.CreatedAt,
                                    CreatedBy = user.CreatedBy,
                                }).ToList();
                response.StatusCode = StatusCode.Success;
                response.StatusMessage = GlobalUserStatusMessage.UserDetailsFetched;
                response.GetAllData = userList;
                return response;
            }
            catch (Exception ex)
            {
                _exceptionLogger.LogExceptionAsync(
                   ex,
                   sourceModule: "GlobalUserModule",
                   apiName: "getAll-globalUsers",
                   tenantId: null,
                   userId: null
               );
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = ex.Message;
            }
            return response;
        }
        public GetAllRecord<GetInfoOfUserDto> GetInfoOfUsers(int tenantId, string email)
        {
            GetAllRecord<GetInfoOfUserDto> response = new();
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);
                string baseUrl = _configuration["BaseUrl:Staging"];
                var globalUser = _masterDbcontext.GlobalUsers
                                                 .Where(user => user.TenantId == tenantId && user.Email == email && !user.IsDeleted)
                                                 .FirstOrDefault();

                if (globalUser == null)
                {
                    response.StatusCode = StatusCode.NotFound;
                    response.StatusMessage = GlobalUserStatusMessage.UserNotFound;
                    return response;
                }
                var factoryUser = tenantDb.FactoryUsers
                                          .Where(factory => factory.TenantId == tenantId && factory.Email == email)
                                          .FirstOrDefault();

                if (factoryUser == null)
                {
                    response.StatusCode = StatusCode.NotFound;
                    response.StatusMessage = GlobalUserStatusMessage.FactoryUserInfoNotFound;
                    return response;
                }
                var teamManagedByUser = tenantDb.FactoryTeams
                                                .Where(team => team.ManagerId == factoryUser.UserId && team.IsActive && !team.IsDeleted)
                                                .FirstOrDefault();
                var teamMemberOfUser = tenantDb.FactoryTeamMembers
                                                .Where(member => member.UserId == factoryUser.UserId && member.IsActive && !member.IsDeleted)
                                                .Select(member => member.Team)
                                                .FirstOrDefault();
                var teamLocation = teamManagedByUser?.Site.ToString() ?? "Not Available";
                string? profileImageUrl = null;
                if (!string.IsNullOrEmpty(factoryUser.ProfileLogoUrl))
                {
                    profileImageUrl = $"{baseUrl}/{factoryUser.ProfileLogoUrl.Replace("\\", "/")}";
                }
                var userInfo = new GetInfoOfUserDto
                {
                    UserId = factoryUser.UserId,
                    TenantId = factoryUser.TenantId,
                    FirstName = factoryUser.FirstName,
                    LastName = factoryUser.LastName,
                    Email = factoryUser.Email,
                    ProfileUrl = profileImageUrl,
                    Role = globalUser.Roles,
                    RoleId = globalUser.RoleId,
                    Contact = factoryUser.ContactNumber,
                    TeamId = teamManagedByUser?.TeamId ?? teamMemberOfUser?.TeamId ?? 0,
                    TeamName = teamManagedByUser?.Name ?? teamMemberOfUser?.Name ?? "No Team",
                    LocationName = teamLocation,
                    JobTitle = globalUser.Roles,
                    Bio = factoryUser.Bio,
                    CreatedAt = factoryUser.CreatedAt

                };

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = GlobalUserStatusMessage.UserDetailsFetched;
                response.GetAllData = new List<GetInfoOfUserDto> { userInfo };

                return response;
            }
            catch (Exception ex)
            {
                _exceptionLogger.LogExceptionAsync(
                    ex,
                    sourceModule: "GlobalUserModule",
                    apiName: "get-UsersInfo",
                    tenantId: tenantId,
                    userId: null
                );
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = ex.Message;
            }
            return response;
        }
        public async Task<CommonResponseModel> UpdateUserProfile(UpdateUserProfileDto dto)
        {
            var response = new CommonResponseModel();

            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(dto.TenantId);
                await using var masterTransaction = await _masterDbcontext.Database.BeginTransactionAsync();
                await using var tenantTransaction = await tenantDb.Database.BeginTransactionAsync();

                try
                {
                   
                    var existingUser = await tenantDb.FactoryUsers
                        .FirstOrDefaultAsync(u => u.UserId == dto.UserId && !u.IsDeleted);

                    if (existingUser == null)
                    {
                        response.StatusCode = StatusCode.NotFound;
                        response.StatusMessage = GlobalUserStatusMessage.UserNotFound;
                        return response;
                    }

                    var oldEmail = existingUser.Email;

                    
                    if (!string.Equals(oldEmail, dto.Email, StringComparison.OrdinalIgnoreCase))
                    {
                        bool emailExistsInFactory = await tenantDb.FactoryUsers
                            .AnyAsync(u => u.Email == dto.Email && u.UserId != dto.UserId && !u.IsDeleted);

                        if (emailExistsInFactory)
                        {
                            response.StatusCode = StatusCode.BadRequest;
                            response.StatusMessage = GlobalUserStatusMessage.EmailExistsInTenant;
                            return response;
                        }

                        bool emailExistsInGlobal = await _masterDbcontext.GlobalUsers
                            .AnyAsync(g => g.Email == dto.Email && g.TenantId == dto.TenantId && !g.IsDeleted);

                        if (emailExistsInGlobal)
                        {
                            response.StatusCode = StatusCode.BadRequest;
                            response.StatusMessage = GlobalUserStatusMessage.EmailExistsGlobally;
                            return response;
                        }
                    }

                   
                    var globalUser = await _masterDbcontext.GlobalUsers
                        .FirstOrDefaultAsync(g => g.TenantId == dto.TenantId && g.Email == oldEmail && !g.IsDeleted);
                    string? relativePath = null;
                    byte[]? imageBytes = null;
                    if (dto.ProfileImage != null)
                    {
                        relativePath = await _fileStorageService.SaveFileAsync(dto.ProfileImage, "ProfileImages");
                        imageBytes = await File.ReadAllBytesAsync(Path.Combine("wwwroot", relativePath));
                        existingUser.ProfileImage = dto.ProfileImage?.FileName ?? string.Empty;
                        existingUser.ProfileLogoUrl = relativePath;
                    }
                    existingUser.Bio = dto.Bio;
                    existingUser.ContactNumber = dto.ContactNumber;
                    existingUser.FirstName = dto.FirstName;
                    existingUser.LastName = dto.LastName;
                    existingUser.Email = dto.Email;
                    existingUser.UpdatedAt = DateTime.UtcNow;
                    existingUser.UpdatedBy = dto.UpdatedBy;

                    await tenantDb.SaveChangesAsync();

                   
                    if (globalUser != null)
                    {
                        globalUser.Email = dto.Email;
                        globalUser.UpdatedAt = DateTime.UtcNow;
                        globalUser.UpdatedBy = dto.UpdatedBy;
                        await _masterDbcontext.SaveChangesAsync();
                    }

                    
                    await tenantTransaction.CommitAsync();
                    await masterTransaction.CommitAsync();

                    await _auditLogger.LogAuditAsync(
                        "Update",
                        $"Profile for user '{existingUser.UserId}' updated with {(imageBytes != null ? "an image" : "no image")}.",
                        null,
                        null,
                        "UserProfileModule"
                    );

                    response.StatusCode = StatusCode.Success;
                    response.StatusMessage = GlobalUserStatusMessage.ProfileUpdated;
                }
                catch (Exception innerEx)
                {
                    await tenantTransaction.RollbackAsync();
                    await masterTransaction.RollbackAsync();
                    throw new Exception($"Transaction rolled back due to: {innerEx.Message}", innerEx);
                }
            }
            catch (Exception ex)
            {
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"{GlobalUserStatusMessage.ProfileUpdateFailed}: {ex.InnerException?.Message ?? ex.Message}";

                await _exceptionLogger.LogExceptionAsync(
                    ex,
                    "UserProfileModule",
                    null,
                    null
                );
            }

            return response;
        }
        public GetAllRecord<GetInfoOfUserDto> GetSuperAdminInfo(int userId)
        {
            GetAllRecord<GetInfoOfUserDto> response = new();
            try
            {
                string baseUrl = _configuration["BaseUrl:Staging"];
                var SuperAdmin = _masterDbcontext.AdminLogins
                                                 .Where(user => user.Id == userId && !user.IsDeleted)
                                                 .FirstOrDefault();

                if (SuperAdmin == null)
                {
                    response.StatusCode = StatusCode.NotFound;
                    response.StatusMessage = GlobalUserStatusMessage.UserNotFound;
                    return response;
                }
                var role = _masterDbcontext.AdminRoles
                     .FirstOrDefault(r => r.RoleId == SuperAdmin.RoleId && !r.IsDeleted);

                string roleName = role?.RoleName ?? "N/A";
                string? profileImageUrl = null;
                if (!string.IsNullOrEmpty(SuperAdmin.ProfileLogoUrl))
                {

                    profileImageUrl = $"{baseUrl}/{SuperAdmin.ProfileLogoUrl.Replace("\\", "/")}";
                }
                var userInfo = new GetInfoOfUserDto
                {
                    UserId = SuperAdmin.Id,
                    FirstName = SuperAdmin.FirstName,
                    LastName = SuperAdmin.LastName,
                    Email = SuperAdmin.Email,
                    Contact = SuperAdmin.Phone,
                    ProfileUrl = profileImageUrl,
                    RoleId = SuperAdmin.RoleId,
                    Role = roleName,
                    JobTitle = roleName,
                    Bio = SuperAdmin.Bio,
                    CreatedAt = SuperAdmin.CreatedAt
                };

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = GlobalUserStatusMessage.SuperAdminDetailsFetched;
                response.GetAllData = new List<GetInfoOfUserDto> { userInfo };

                return response;
            }
            catch (Exception ex)
            {
                _exceptionLogger.LogExceptionAsync(
                    ex,
                    sourceModule: "GlobalUserModule",
                    apiName: "get-UsersInfo",
                    userId: null
                );
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = ex.Message;
            }
            return response;
        }
        public async Task<CommonResponseModel> UpdateSuperAdminProfile(UpdateSuperAdminProfileDto dto)
        {
            var response = new CommonResponseModel();

            try
            {
                var existingUser = await _masterDbcontext.AdminLogins
                    .FirstOrDefaultAsync(u => u.Id == dto.UserId && !u.IsDeleted);

                if (existingUser == null)
                {
                    response.StatusCode = StatusCode.NotFound;
                    response.StatusMessage = GlobalUserStatusMessage.UserNotFound;
                    return response;
                }

                string? relativePath = null;
                byte[]? imageBytes = null;

                if (dto.ProfileImage != null)
                {
                    relativePath = await _fileStorageService.SaveFileAsync(dto.ProfileImage, "AdminProfileImages");
                    var fullPath = Path.Combine("wwwroot", relativePath);
                    if (File.Exists(fullPath))
                        imageBytes = await File.ReadAllBytesAsync(fullPath);
                    existingUser.ProfileImage = dto.ProfileImage?.FileName ?? string.Empty;
                    existingUser.ProfileLogoUrl = relativePath;
                }
                existingUser.FirstName = dto.FirstName;
                existingUser.LastName = dto.LastName;
                existingUser.Bio = dto.Bio;
                existingUser.Phone = dto.ContactNumber;
                existingUser.UpdatedAt = DateTime.UtcNow;
                existingUser.UpdatedBy = dto.UpdatedBy;
                await _masterDbcontext.SaveChangesAsync();
                await _auditLogger.LogAuditAsync(
                    "Update",
                    $"Profile for user '{existingUser.Id}' updated with {(imageBytes != null ? "an image" : "no image")}.",
                    null,
                    null,
                    "SuperAdminProfileModule"
                );

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = GlobalUserStatusMessage.ProfileUpdated;
            }
            catch (Exception ex)
            {
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"{GlobalUserStatusMessage.ProfileUpdateFailed}: {ex.InnerException?.Message ?? ex.Message}";

                await _exceptionLogger.LogExceptionAsync(
                    ex,
                    "SuperAdminProfileModule",
                    null,
                    null
                );
            }

            return response;
        }
        public GetAllRecord<GetInfoOfUserDto> GetTenantInfo(int tenantId)
        {
            GetAllRecord<GetInfoOfUserDto> response = new();
            try
            {

                string baseUrl = _configuration["BaseUrl:Staging"];
                var factoryTenants = _masterDbcontext.FactoryTenants
                                                 .Where(user => user.TenantId == tenantId && !user.IsDeleted)
                                                 .FirstOrDefault();

                if (factoryTenants == null)
                {
                    response.StatusCode = StatusCode.NotFound;
                    response.StatusMessage = GlobalUserStatusMessage.UserNotFound;
                    return response;
                }
                var TenantAdmin = _masterDbcontext.TenantAdminLogins
                                          .Where(tenant => tenant.TenantId == tenantId && !tenant.IsDeleted)
                                          .FirstOrDefault();

                if (TenantAdmin == null)
                {
                    response.StatusCode = StatusCode.NotFound;
                    response.StatusMessage = GlobalUserStatusMessage.FactoryUserInfoNotFound;
                    return response;
                }
                var role = _masterDbcontext.AdminRoles
                   .FirstOrDefault(r => r.RoleId == TenantAdmin.RoleId && !r.IsDeleted);

                string roleName = role?.RoleName ?? "N/A";

                string? profileImageUrl = null;
                if (!string.IsNullOrEmpty(factoryTenants.ProfileImageURL))
                {
                    profileImageUrl = $"{baseUrl}/{factoryTenants.ProfileImageURL.Replace("\\", "/")}";
                }
                var userInfo = new GetInfoOfUserDto
                {
                    TenantId = TenantAdmin.TenantId,
                    Email = TenantAdmin.Email,
                    RoleId = TenantAdmin.RoleId,
                    Role = roleName,
                    Name = factoryTenants.TenantName,
                    Contact = factoryTenants.ContactNumber,
                    Bio = factoryTenants.Bio,
                    IndustryType = factoryTenants.IndustryType,
                    JobTitle = roleName,
                    ProfileUrl = profileImageUrl,
                    CreatedAt = TenantAdmin.CreatedAt
                };

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = GlobalUserStatusMessage.TenantDetailsFetched;
                response.GetAllData = new List<GetInfoOfUserDto> { userInfo };

                return response;
            }
            catch (Exception ex)
            {
                _exceptionLogger.LogExceptionAsync(
                    ex,
                    sourceModule: "GlobalUserModule",
                    apiName: "get-tenantInfo",
                    tenantId: tenantId,
                    userId: null
                );
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = ex.Message;
            }
            return response;
        }
        public async Task<CommonResponseModel> UpdateTenantProfile(UpdateTenantProfileDto dto)
        {
            var response = new CommonResponseModel();
            try
            {
                var factoryTenantUser = await _masterDbcontext.FactoryTenants
                    .FirstOrDefaultAsync(u => u.TenantId == dto.TenantId && !u.IsDeleted);

                if (factoryTenantUser == null)
                {
                    response.StatusCode = StatusCode.NotFound;
                    response.StatusMessage = GlobalUserStatusMessage.UserNotFound;
                    return response;
                }
                var tenantAdminUser = await _masterDbcontext.TenantAdminLogins
                    .FirstOrDefaultAsync(u => u.TenantId == dto.TenantId && !u.IsDeleted);

                if (tenantAdminUser == null)
                {
                    response.StatusCode = StatusCode.NotFound;
                    response.StatusMessage = GlobalUserStatusMessage.TenantUserNotFound;
                    return response;
                }
                string? relativePath = null;
                byte[]? imageBytes = null;

                if (dto.ProfileImage != null)
                {
                    relativePath = await _fileStorageService.SaveFileAsync(dto.ProfileImage, "TenantProfileImages");
                    var fullPath = Path.Combine("wwwroot", relativePath);
                    if (File.Exists(fullPath))
                        imageBytes = await File.ReadAllBytesAsync(fullPath);
                    factoryTenantUser.ProfileImageURL = relativePath;
                    factoryTenantUser.ProfileImage = dto.ProfileImage?.FileName ?? string.Empty;
                }
                factoryTenantUser.Bio = dto.Bio;
                factoryTenantUser.ContactNumber = dto.Contact;
                factoryTenantUser.TenantName = dto.Name;
                factoryTenantUser.UpdatedAt = DateTime.UtcNow;
                factoryTenantUser.UpdatedBy = dto.UpdatedBy;

                await _masterDbcontext.SaveChangesAsync();

                
                await _auditLogger.LogAuditAsync(
                    "Update",
                    $"Profile for Tenant '{factoryTenantUser.TenantId}' updated with {(imageBytes != null ? "an image" : "no image")}.",
                    null,
                    null,
                    "UserProfileModule"
                );

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = GlobalUserStatusMessage.ProfileUpdated;
            }
            catch (Exception ex)
            {
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"{GlobalUserStatusMessage.ProfileUpdateFailed}: {ex.InnerException?.Message ?? ex.Message}";

                await _exceptionLogger.LogExceptionAsync(
                    ex,
                    "UserProfileModule",
                    null,
                    null
                );
            }

            return response;
        }

        public GetAllRecord<GetAllSuspendUserDto> GetAllSuspendUsers()
        {
            GetAllRecord<GetAllSuspendUserDto> response = new();
            try
            {
                var suspendUserList = (from user in _masterDbcontext.GlobalUsers
                                       where user.IsDeleted == false && user.Suspend == true
                                       orderby (user.UpdatedAt ?? DateTime.MinValue) descending
                                       select new GetAllSuspendUserDto
                                       {
                                           GlobalUserId = user.GlobalUserId,
                                           Email = user.Email,
                                           Suspend = user.Suspend,
                                           SuspendedBy = user.SuspendedBy,
                                           SuspensionReason = user.SuspensionReason
                                       }).ToList();
                response.StatusCode = StatusCode.Success;
                response.StatusMessage = GlobalUserStatusMessage.SuspendUsersFetched;
                response.GetAllData = suspendUserList;
                return response;
            }
            catch (Exception ex)
            {
                _exceptionLogger.LogExceptionAsync(
                   ex,
                   sourceModule: "GlobalUserModule",
                   apiName: "getAll-SuspendUsers",
                   tenantId: null,
                   userId: null
               );
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = ex.Message;
            }
            return response;
        }
        public async Task<CommonResponseModel> ToggleSuspend(SuspendGlobalUserDto dto)
        {
            var response = new CommonResponseModel();
            int CreateUserId = 0;
            int TenantId = 0;
            try
            {
                var existingUser = await _masterDbcontext.GlobalUsers
                    .FirstOrDefaultAsync(u => u.GlobalUserId == dto.GlobalUserId);

                if (existingUser == null)
                {
                    response.StatusCode = StatusCode.NotFound;
                    response.StatusMessage = GlobalUserStatusMessage.UserNotFound;
                    return response;
                }

                using var tenantDb = _tenantDbContext.GetTenantDbContext(existingUser.TenantId);
                var tenantUser = await tenantDb.FactoryUsers
                    .FirstOrDefaultAsync(u => u.Email == existingUser.Email && !u.IsDeleted);

                CreateUserId = tenantUser.UserId;
                TenantId = existingUser.TenantId;

                if (tenantUser == null)
                {
                    response.StatusCode = StatusCode.NotFound;
                    response.StatusMessage = GlobalUserStatusMessage.TenantNotFound;
                    return response;
                }

                bool wasSuspended = existingUser.Suspend;
                existingUser.Suspend = !wasSuspended;
                existingUser.Status = existingUser.Suspend ? UserStatus.Suspended : UserStatus.Active;
                existingUser.UpdatedAt = DateTime.UtcNow;

                if (existingUser.Suspend)
                {
                    existingUser.SuspensionReason = dto.SuspensionReason;
                    existingUser.SuspendedBy = dto.SuspendedBy;
                }
                else
                {
                    existingUser.SuspensionReason = null;
                    existingUser.SuspendedBy = null;
                    existingUser.ForceLogout = false;
                }

                tenantUser.Suspend = existingUser.Suspend;
                tenantUser.Status = !existingUser.Suspend;
                tenantUser.UpdatedAt = DateTime.UtcNow;

                if (!tenantUser.Suspend)
                {
                    tenantUser.ForceLogout = false;
                }

                var auditAction = existingUser.Suspend ? "Suspend" : "Unsuspend";

                await _auditLogger.LogAuditAsync(
                    action: auditAction,
                    details: $"User {auditAction.ToLower()}ed: {existingUser.Email}",
                    tenantId: existingUser.TenantId,
                    email: existingUser.Email,
                    eventType: "UserStatusChange"
                );

                await using var transaction = await _masterDbcontext.Database.BeginTransactionAsync();
                try
                {
                    await _masterDbcontext.SaveChangesAsync();
                    await tenantDb.SaveChangesAsync();
                    await transaction.CommitAsync();

                    response.StatusCode = "200";
                    response.StatusMessage = $"User {(existingUser.Suspend ? "suspended" : "unsuspended")} successfully";
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    response.StatusCode = StatusCode.Error;
                    response.StatusMessage = $"Toggle failed: {ex.Message}";
                }

                return response;
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(
                    ex,
                    sourceModule: "GlobalUserModule",
                    apiName: "ToggleSuspend",
                    tenantId: TenantId,
                    userId: CreateUserId
                );
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = ex.Message;
                return response;
            }
        }

    }
}
