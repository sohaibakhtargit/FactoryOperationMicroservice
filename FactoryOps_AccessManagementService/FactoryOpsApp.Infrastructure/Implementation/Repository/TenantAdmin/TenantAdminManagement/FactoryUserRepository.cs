using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.TenantAdminManagement;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.SuperAdmin.AuditLogs;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.Common;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.ExceptionLogger;
using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.Security;
using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Domain.Entities.FactoryOpsTenants;
using FactoryOpsApp.Domain.Entities.MasterTenantsAdmin;
using FactoryOpsApp.Infrastructure.DBContext;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using System.Text.Json;
using System.Transactions;
using static FactoryOps_AccessManagementService.FactoryOpsApp.Common.CommonConstant;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Repository.TenantAdmin.TenantAdminManagement
{
    public class FactoryUserRepository : IFactoryUserRepository
    {
        private readonly IPasswordHasher _hasher;
        private readonly MasterFactoryOpsDbContext _masterDbcontext;
        private readonly TenantDbContextFactory _tenantDbContext;
        private readonly IEmailService _iEmailService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IExceptionLoggerService _exceptionLogger;
        private readonly IAuditLogService _auditLogger;
        private readonly IConnectionMultiplexer _redis;
        public FactoryUserRepository(MasterFactoryOpsDbContext masterDbcontext,
            IHttpContextAccessor httpContextAccessor,
            TenantDbContextFactory tenantDbContext,
            IEmailService iEmailService,
            IExceptionLoggerService exceptionLogger,
            IAuditLogService auditLogger,
            IPasswordHasher hasher,
            IConnectionMultiplexer redis
            )
        {
            _masterDbcontext = masterDbcontext;
            _httpContextAccessor = httpContextAccessor;
            _tenantDbContext = tenantDbContext;
            _iEmailService = iEmailService;
            _exceptionLogger = exceptionLogger;
            _auditLogger = auditLogger;
            _hasher = hasher;
            _redis = redis;
        }

        public async Task<CommonResponseModel> AddNewUser(UserResponseDto AddUser)
        {
            CommonResponseModel response = new CommonResponseModel();
            int? createdUserId = null;
            using var tenantDb = _tenantDbContext.GetTenantDbContext(AddUser.TenantId);
            var existingUser = await tenantDb.FactoryUsers
      .Where(u => !u.IsDeleted &&
             (u.Email == AddUser.Email ||
              u.Username == AddUser.Username ||
              u.ContactNumber == AddUser.ContactNumber))
      .ToListAsync();

            if (existingUser.Any(u => u.Email == AddUser.Email))
            {
                response.StatusCode = StatusCode.BadRequest;
                response.StatusMessage = FactoryUserStatusMessage.EmailAlreadyExists;
                return response;
            }

            if (existingUser.Any(u => u.Username == AddUser.Username))
            {
                response.StatusCode = StatusCode.BadRequest;
                response.StatusMessage = FactoryUserStatusMessage.UsernameAlreadyExists;
                return response;
            }

            if (existingUser.Any(u => u.ContactNumber == AddUser.ContactNumber))
            {
                response.StatusCode = StatusCode.BadRequest;
                response.StatusMessage = FactoryUserStatusMessage.PhoneAlreadyExists;
                return response;
            }

            int currentUserCount = await tenantDb.FactoryUsers.CountAsync(l => l.IsDeleted == false);

            var tenantConfig = await _masterDbcontext.FactoryTenants.FirstOrDefaultAsync(l => l.TenantId == AddUser.TenantId);

            if (tenantConfig == null)
            {
                response.StatusCode = StatusCode.NotFound;
                response.StatusMessage = FactoryUserStatusMessage.UserNotFound;
                return response;
            }

            if (currentUserCount >= tenantConfig.MaxUsers)
            {
                response.StatusCode = StatusCode.BadRequest;
                response.StatusMessage = FactoryUserStatusMessage.UserLimitExceeded;
                return response;
            }

            using var transaction = await tenantDb.Database.BeginTransactionAsync();
            var tempPassword = AddUser.FirstName + "@123";
            var hashedPassowrd = _hasher.Hash(tempPassword);   
            try
            {
                var user = new FactoryUsers
                {
                    TenantId = AddUser.TenantId,
                    FirstName = AddUser.FirstName,
                    LastName = AddUser.LastName,
                    Username = AddUser.Username,
                    Email = AddUser.Email,
                    //PasswordHash = AddUser.FirstName + "@123",
                    PasswordHash = hashedPassowrd,
                    ContactNumber = AddUser.ContactNumber,
                    MFAEnabled = AddUser.MFAEnabled,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = AddUser.TenantId,
                    IsActive = true,
                    IsDeleted = false
                };

                tenantDb.FactoryUsers.Add(user);
                await tenantDb.SaveChangesAsync();

                createdUserId = user.UserId;

                var role = await tenantDb.FactoryRoles
                             .AsNoTracking()
                             .FirstOrDefaultAsync(r => r.RoleId == AddUser.RoleId && !r.IsDeleted);

                if (role == null)
                {
                    response.StatusCode = StatusCode.BadRequest;
                    response.StatusMessage = FactoryUserStatusMessage.InvalidRole;
                    return response;
                }

                var userRole = new FactoryUserRoles
                {
                    UserId = user.UserId,
                    RoleId = AddUser.RoleId,
                    TenantId = AddUser.TenantId,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = AddUser.TenantId
                };
                tenantDb.FactoryUserRoles.Add(userRole);

                var rolesJson = JsonSerializer.Serialize(new[] { role.RoleName });

                var ctx = _httpContextAccessor.HttpContext;
                var globalUsers = new GlobalUsers()
                {
                    TenantId = AddUser.TenantId,
                    Email = AddUser.Email,
                    Password = hashedPassowrd,
                    Status = UserStatus.Active,
                    IpAddress = ctx?.Connection.RemoteIpAddress?.ToString()?? "N/A",
                    Roles = rolesJson,
                    RoleId = role.RoleId,
                    Suspend = false,
                    ForceLogout = false,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = AddUser.TenantId

                };
                await _auditLogger.LogAuditAsync(
                       action: "Create",
                       details: "User-Added",
                       tenantId: AddUser.TenantId,
                       email: AddUser.Email,
                       eventType: "UserManagement"
                   );

                await _masterDbcontext.GlobalUsers.AddAsync(globalUsers);
                await tenantDb.SaveChangesAsync();
                await _masterDbcontext.SaveChangesAsync();
                await transaction.CommitAsync();

                var emailDto = new EmailDTO
                {
                    From = "shoaibmaliklenovo@gmail.com",
                    To = AddUser.Email ?? "factory.operation@yopmail.com",
                    Subject = "Your New Account Credentials",
                    Body = $@"<html>
                    <body>
                        <h2>Welcome, {AddUser.FirstName}+{AddUser.LastName}!</h2>
                        <p>Your account has been successfully created.</p>
                        <p><strong>Username:</strong> {AddUser.Username}</p>
                        <p><strong>Email:</strong> {AddUser.Email}</p>
                        <p><strong>Temporary Password:</strong> {tempPassword}</p>
                        <p>Please change your password after first login.</p>
                    </body>
                    </html>"
                };

                var emailResponse = await _iEmailService.SendEmailAsync(emailDto);

                if (!emailResponse.Success)
                {
                    response.StatusCode = StatusCode.Error;
                    response.StatusMessage = FactoryUserStatusMessage.EmailFailed;
                }

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = FactoryUserStatusMessage.UserCreated;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _exceptionLogger.LogExceptionAsync(
                ex,
                sourceModule: "UserManagement",
                apiName: "add-user",
                tenantId: AddUser.TenantId,
                userId: createdUserId
            );

                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"{FactoryUserStatusMessage.UserCreateFailed}: {ex.Message}";
            }

            return response;
        }

        public async Task<CommonResponseModel> EditExistingUser(UserResponseDto EditUser)
        {
            CommonResponseModel response = new CommonResponseModel();

            using var tenantDb = _tenantDbContext.GetTenantDbContext(EditUser.TenantId);

            await using var tenantTx = await tenantDb.Database.BeginTransactionAsync();
            await using var masterTx = await _masterDbcontext.Database.BeginTransactionAsync();

            try
            {
                var existingUser = await tenantDb.FactoryUsers
                    .FirstOrDefaultAsync(u => u.UserId == EditUser.UserId && !u.IsDeleted);

                if (existingUser == null)
                {
                    response.StatusCode = StatusCode.NotFound;
                    response.StatusMessage = FactoryUserStatusMessage.UserNotFound;
                    return response;
                }

                bool emailExists = await tenantDb.FactoryUsers
                    .AnyAsync(u => u.Email == EditUser.Email &&
                                   u.UserId != EditUser.UserId &&
                                   !u.IsDeleted);

                if (emailExists)
                {
                    response.StatusCode = StatusCode.BadRequest;
                    response.StatusMessage = FactoryUserStatusMessage.EmailAlreadyExists;
                    return response;
                }

                bool usernameExists = await tenantDb.FactoryUsers
                    .AnyAsync(u => u.Username == EditUser.Username &&
                                   u.UserId != EditUser.UserId &&
                                   !u.IsDeleted);

                if (usernameExists)
                {
                    response.StatusCode = StatusCode.BadRequest;
                    response.StatusMessage = FactoryUserStatusMessage.UsernameAlreadyExists;
                    return response;
                }

                bool contactExists = await tenantDb.FactoryUsers
                    .AnyAsync(u => u.ContactNumber == EditUser.ContactNumber &&
                                   u.UserId != EditUser.UserId &&
                                   !u.IsDeleted);

                if (contactExists)
                {
                    response.StatusCode = StatusCode.BadRequest;
                    response.StatusMessage = FactoryUserStatusMessage.PhoneAlreadyExists;
                    return response;
                }

                var originalUserEmail = existingUser.Email;

                existingUser.FirstName = EditUser.FirstName;
                existingUser.LastName = EditUser.LastName;
                existingUser.Username = EditUser.Username;
                existingUser.Email = EditUser.Email;
                existingUser.ContactNumber = EditUser.ContactNumber;
                existingUser.MFAEnabled = EditUser.MFAEnabled;
                existingUser.UpdatedAt = DateTime.UtcNow;
                existingUser.UpdatedBy = EditUser.TenantId;
                existingUser.IsActive = true;
                existingUser.IsDeleted = false;

                var existingUserRole = await tenantDb.FactoryUserRoles
                    .FirstOrDefaultAsync(r => r.UserId == EditUser.UserId && r.IsActive && !r.IsDeleted);

                if (existingUserRole != null)
                {
                    if (existingUserRole.RoleId != EditUser.RoleId)
                    {
                        existingUserRole.IsActive = false;
                        existingUserRole.IsDeleted = true;
                        existingUserRole.DeletedAt = DateTime.UtcNow;
                        existingUserRole.DeletedBy = EditUser.TenantId;

                        var newUserRole = new FactoryUserRoles
                        {
                            UserId = EditUser.UserId,
                            RoleId = EditUser.RoleId,
                            IsActive = true,
                            IsDeleted = false,
                            CreatedAt = DateTime.UtcNow,
                            CreatedBy = EditUser.TenantId
                        };

                        await tenantDb.FactoryUserRoles.AddAsync(newUserRole);
                    }
                    else
                    {
                        existingUserRole.IsActive = true;
                        existingUserRole.IsDeleted = false;
                    }
                }
                else
                {
                    var newUserRole = new FactoryUserRoles
                    {
                        UserId = EditUser.UserId,
                        RoleId = EditUser.RoleId,
                        IsActive = true,
                        IsDeleted = false,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = EditUser.TenantId
                    };

                    await tenantDb.FactoryUserRoles.AddAsync(newUserRole);
                }

                var role = await tenantDb.FactoryRoles
                    .AsNoTracking()
                    .FirstOrDefaultAsync(r => r.RoleId == EditUser.RoleId && !r.IsDeleted);

                if (role == null)
                {
                    response.StatusCode = StatusCode.BadRequest;
                    response.StatusMessage = FactoryUserStatusMessage.InvalidRole;
                    return response;
                }

                var globalUser = await _masterDbcontext.GlobalUsers
                    .FirstOrDefaultAsync(g =>
                        g.Email == originalUserEmail &&
                        g.TenantId == EditUser.TenantId &&
                        !g.IsDeleted);

                if (globalUser != null)
                {
                    globalUser.Email = EditUser.Email;
                    globalUser.RoleId = EditUser.RoleId;
                    globalUser.Roles = JsonSerializer.Serialize(new[] { role.RoleName });
                    globalUser.UpdatedAt = DateTime.UtcNow;
                    globalUser.UpdatedBy = EditUser.TenantId;
                }

                await _auditLogger.LogAuditAsync(
                    action: "Update",
                    details: "User updated",
                    tenantId: EditUser.TenantId,
                    email: EditUser.Email,
                    eventType: "UserManagement"
                );

                await tenantDb.SaveChangesAsync();
                await _masterDbcontext.SaveChangesAsync();

                await masterTx.CommitAsync();
                await tenantTx.CommitAsync();

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = FactoryUserStatusMessage.UserUpdated;
            }
            catch (Exception ex)
            {
                await masterTx.RollbackAsync();
                await tenantTx.RollbackAsync();

                await _exceptionLogger.LogExceptionAsync(
                    ex,
                    sourceModule: "UserManagement",
                    apiName: "edit-user",
                    tenantId: EditUser.TenantId,
                    userId: EditUser.UserId
                );

                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"{FactoryUserStatusMessage.UserUpdateFailed}: {ex.Message}";
            }

            return response;
        }

        public GetAllRecord<GetUsersListDto> GetAllUsers(int TenantId)
        {
            GetAllRecord<GetUsersListDto> response = new();
            using var tenantDb = _tenantDbContext.GetTenantDbContext(TenantId);
            try
            {
                var userList = (from user in tenantDb.FactoryUsers
                                join userRole in tenantDb.FactoryUserRoles on user.UserId equals userRole.UserId
                                join role in tenantDb.FactoryRoles on userRole.RoleId equals role.RoleId
                                where user.IsDeleted == false && userRole.IsDeleted == false && role.IsDeleted == false
                                orderby user.UserId descending
                                select new GetUsersListDto
                                {
                                    UserId = user.UserId,
                                    TenantId = user.TenantId,
                                    FirstName = user.FirstName,
                                    LastName = user.LastName,
                                    FullName = user.FirstName + user.LastName,
                                    Username = user.Username,
                                    Email = user.Email,
                                    MFAEnabled = user.MFAEnabled,
                                    LastLogin = user.LastLogin,
                                    RoleId = role.RoleId,
                                    Role = role.RoleName,
                                    ContactNumber = user.ContactNumber,
                                    AddressLine1 = user.AddressLine1!,
                                    AddressLine2 = user.AddressLine2,
                                    Status = user.Status,
                                    ForceLogout = user.ForceLogout,
                                    Suspend = user.Suspend,
                                    IsActive = user.IsActive,
                                    IsDeleted = user.IsDeleted
                                }).ToList();
                response.StatusCode = StatusCode.Success;
                response.StatusMessage = FactoryUserStatusMessage.UsersFetched;
                response.GetAllData = userList;
                return response;
            }
            catch (Exception ex)
            {
                _exceptionLogger.LogExceptionAsync(
                   ex,
                   sourceModule: "UserManagement",
                   apiName: "get-users",
                   tenantId: TenantId,
                   userId: null
               );
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"{FactoryUserStatusMessage.UsersFetchFailed}: {ex.Message}";
            }
            return response;
        }
        public async Task<CommonResponseModel> DeleteUser(int Id, int TenantId)
        {
            CommonResponseModel response = new CommonResponseModel();

            using var tenantDb = _tenantDbContext.GetTenantDbContext(TenantId);

            await using var tenantTx = await tenantDb.Database.BeginTransactionAsync();
            await using var masterTx = await _masterDbcontext.Database.BeginTransactionAsync();

            try
            {
                var existingUser = await tenantDb.FactoryUsers
                    .FirstOrDefaultAsync(u => u.UserId == Id && !u.IsDeleted);

                if (existingUser == null)
                {
                    response.StatusCode = StatusCode.BadRequest;
                    response.StatusMessage = FactoryUserStatusMessage.UserAlreadyDeleted;
                    return response;
                }

                existingUser.IsDeleted = true;
                existingUser.IsActive = false;

                var userRoles = await tenantDb.FactoryUserRoles
                    .Where(r => r.UserId == Id && !r.IsDeleted)
                    .ToListAsync();

                foreach (var role in userRoles)
                {
                    role.IsDeleted = true;
                    role.IsActive = false;
                }

                var userGroups = await tenantDb.FactoryGroupUsers
                    .Where(g => g.UserId == Id && !g.IsDeleted)
                    .ToListAsync();

                foreach (var group in userGroups)
                {
                    group.IsDeleted = true;
                    group.IsActive = false;
                }

                var existingGlobalUser = await _masterDbcontext.GlobalUsers
                    .FirstOrDefaultAsync(u =>
                        u.Email == existingUser.Email &&
                        u.TenantId == TenantId &&
                        !u.IsDeleted);

                if (existingGlobalUser != null)
                {
                    existingGlobalUser.IsDeleted = true;
                    existingGlobalUser.IsActive = false;
                }

                await _auditLogger.LogAuditAsync(
                    action: "Delete",
                    details: $"User '{existingUser.Email}' deleted",
                    tenantId: TenantId,
                    email: existingUser.Email,
                    eventType: "UserDelete"
                );

                await tenantDb.SaveChangesAsync();
                await _masterDbcontext.SaveChangesAsync();

                await masterTx.CommitAsync();
                await tenantTx.CommitAsync();

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = FactoryUserStatusMessage.UserDeleted;
            }
            catch (Exception ex)
            {
                await masterTx.RollbackAsync();
                await tenantTx.RollbackAsync();

                await _exceptionLogger.LogExceptionAsync(
                    ex,
                    sourceModule: "UserManagement",
                    apiName: "Delete-Users",
                    tenantId: TenantId,
                    userId: Id
                );

                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"{FactoryUserStatusMessage.UserDeleteFailed}: {ex.Message}";
            }

            return response;
        }

        public async Task<CommonResponseModel> ForceLogout(int Id, int TenantId)
        {
            CommonResponseModel response = new CommonResponseModel();
            using var tenantDb = _tenantDbContext.GetTenantDbContext(TenantId);
            try
            {
                var existingUser = await tenantDb.FactoryUsers
               .FirstOrDefaultAsync(u => u.UserId == Id);
                if (existingUser == null)
                {
                    response.StatusCode = StatusCode.NotFound;
                    response.StatusMessage = FactoryUserStatusMessage.UserNotFound;
                    return response;
                }

                if (existingUser.ForceLogout)
                {
                    response.StatusCode = StatusCode.BadRequest;
                    response.StatusMessage = FactoryUserStatusMessage.UserAlreadyForceLoggedOut;
                    return response;
                }

                existingUser.ForceLogout = true;


                var existingGlobalUser = await _masterDbcontext.GlobalUsers.FirstOrDefaultAsync(u => u.Email == existingUser.Email);

                if (existingGlobalUser == null)
                {
                    response.StatusCode = StatusCode.NotFound;
                    response.StatusMessage = FactoryUserStatusMessage.UserNotFound;
                    return response;
                }

                if (existingGlobalUser.ForceLogout)
                {
                    response.StatusCode = StatusCode.BadRequest;
                    response.StatusMessage = FactoryUserStatusMessage.UserAlreadyForceLoggedOut;
                    return response;
                }

                existingGlobalUser.ForceLogout = true;

                await _auditLogger.LogAuditAsync(
                   action: "Update",
                   details: "User ForceLogout",
                   tenantId: TenantId,
                   email: existingUser.Email,
                   eventType: "UserStatusChange"
               );

                await _masterDbcontext.SaveChangesAsync();
                await tenantDb.SaveChangesAsync();


                // Redis Cache Clear
                var redisDb = _redis.GetDatabase();
                string cacheKey = $"user-status:{TenantId}:{Id}";
                await redisDb.KeyDeleteAsync(cacheKey);

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = FactoryUserStatusMessage.UserForceLogout;

            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(
                    ex,
                    sourceModule: "UserManagement",
                    apiName: "force-logout-user",
                    tenantId: TenantId,
                    userId: Id
                );
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"{FactoryUserStatusMessage.ErrorForceLogoutUser}:{ex.Message}";
            }

            return response;
        }
        public async Task<CommonResponseModel> Suspend(SuspendUserDto dto)
        {
            var response = new CommonResponseModel();

            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(dto.TenantId);
                var existingTenantUser = await tenantDb.FactoryUsers
                    .FirstOrDefaultAsync(u => u.UserId == dto.UserId);

                if (existingTenantUser == null)
                {
                    response.StatusCode = StatusCode.NotFound;
                    response.StatusMessage = FactoryUserStatusMessage.TenantUserNotFound;
                    return response;
                }

                existingTenantUser.Suspend = !existingTenantUser.Suspend;
                existingTenantUser.Status = !existingTenantUser.Suspend;
                existingTenantUser.UpdatedAt = DateTime.UtcNow;

                if (!existingTenantUser.Suspend)
                {
                    existingTenantUser.ForceLogout = false;
                }

                var existingGlobalUser = await _masterDbcontext.GlobalUsers
                    .FirstOrDefaultAsync(u => u.Email == existingTenantUser.Email);

                if (existingGlobalUser == null)
                {
                    response.StatusCode = StatusCode.NotFound;
                    response.StatusMessage = FactoryUserStatusMessage.GlobalUserNotFound;
                    return response;
                }

                existingGlobalUser.Suspend = existingTenantUser.Suspend;
                existingGlobalUser.Status = existingTenantUser.Suspend
                    ? UserStatus.Suspended
                    : UserStatus.Active;

                if (existingTenantUser.Suspend)
                {
                    existingGlobalUser.SuspendedBy = 1;
                    existingGlobalUser.SuspensionReason = dto.SuspensionReason;
                }
                else
                {
                    existingGlobalUser.ForceLogout = false;
                    existingGlobalUser.SuspendedBy = null;
                    existingGlobalUser.SuspensionReason = null;
                }

                existingGlobalUser.UpdatedAt = DateTime.UtcNow;

                var auditAction = existingTenantUser.Suspend ? "Suspend" : "Unsuspend";
                await _auditLogger.LogAuditAsync(
                    action: auditAction,
                    details: $"User {auditAction.ToLower()}ed",
                    tenantId: dto.TenantId,
                    email: existingTenantUser.Email,
                    eventType: "UserStatusChange"
                );

                await using var transaction = await _masterDbcontext.Database.BeginTransactionAsync();
                try
                {
                    await tenantDb.SaveChangesAsync();
                    await _masterDbcontext.SaveChangesAsync();
                    await transaction.CommitAsync();

                    // Redis Cache Clear
                    var redisDb = _redis.GetDatabase();
                    string cacheKey = $"user-status:{dto.TenantId}:{dto.UserId}";
                    await redisDb.KeyDeleteAsync(cacheKey);

                    response.StatusCode = StatusCode.Success;
                    response.StatusMessage = $"User {auditAction.ToLower()}ed successfully";
                }
                catch (Exception ex)
                {
                    await _exceptionLogger.LogExceptionAsync(
                        ex,
                        sourceModule: "UserManagement",
                        apiName: "suspend-user",
                        tenantId: dto.TenantId,
                        userId: dto.UserId
                    );
                    await transaction.RollbackAsync();
                    response.StatusCode = StatusCode.Error;
                    response.StatusMessage = $"{FactoryUserStatusMessage.UserSuspendFailed}: {ex.Message}";
                }

                return response;
            }
            catch (Exception ex)
            {
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = ex.Message;
                return response;
            }
        }
        public GetAllRecord<GetManagerUserDto> GetAllManagersByTenantAsync(int tenantId)
        {
            var response = new GetAllRecord<GetManagerUserDto>();

            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                var managers = (from user in tenantDb.FactoryUsers
                                join userRole in tenantDb.FactoryUserRoles on user.UserId equals userRole.UserId
                                join role in tenantDb.FactoryRoles on userRole.RoleId equals role.RoleId
                                where !user.IsDeleted &&
                                      !user.Suspend &&
                                      !userRole.IsDeleted &&
                                      !role.IsDeleted &&
                                      role.RoleName.ToLower() == "manager" || role.RoleName.ToLower().Replace(" ", "") == "generalmanager"
                                select new GetManagerUserDto
                                {
                                    UserId = user.UserId,
                                    FullName = user.FirstName + " " + user.LastName,
                                    Email = user.Email,
                                    RoleId = role.RoleId,
                                    Role = role.RoleName
                                }).ToList();

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = FactoryUserStatusMessage.ManagersFetched;
                response.GetAllData = managers;
            }
            catch (Exception ex)
            {
                _exceptionLogger.LogExceptionAsync(
                   ex,
                   sourceModule: "UserManagement",
                   apiName: "get-managers",
                   tenantId: tenantId,
                   userId: null
               );

                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"{FactoryUserStatusMessage.ErrorFetchingManagers}: {ex.Message}";
            }

            return response;
        }
        public GetAllRecord<GetManagerUserDto> GetAllUsersExceptManager(int tenantId)
        {
            var response = new GetAllRecord<GetManagerUserDto>();

            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                var users = (from user in tenantDb.FactoryUsers
                             join userRole in tenantDb.FactoryUserRoles on user.UserId equals userRole.UserId
                             join role in tenantDb.FactoryRoles on userRole.RoleId equals role.RoleId
                             where !user.IsDeleted &&
                                   !user.Suspend &&
                                   !userRole.IsDeleted &&
                                   !role.IsDeleted &&
                                   role.RoleName.ToLower() != "manager" && role.RoleName.ToLower().Replace(" ", "") != "generalmanager"
                             select new GetManagerUserDto
                             {
                                 UserId = user.UserId,
                                 FullName = user.FirstName + " " + user.LastName,
                                 Email = user.Email,
                                 RoleId = role.RoleId,
                                 Role = role.RoleName
                             }).ToList();

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = FactoryUserStatusMessage.UsersExceptManagerFetched;
                response.GetAllData = users;
            }
            catch (Exception ex)
            {
                _exceptionLogger.LogExceptionAsync(
                    ex,
                    sourceModule: "UserManagement",
                    apiName: "get-AllMembers",
                    tenantId: tenantId,
                    userId: null
                );

                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"{FactoryUserStatusMessage.ErrorFetchingUsersExceptManagers}: {ex.Message}";
            }

            return response;
        }
    }
}
