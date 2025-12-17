using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.TenantAdminManagement;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.SuperAdmin.AuditLogs;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.ExceptionLogger;
using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Domain.Entities.FactoryOpsTenants;
using FactoryOpsApp.Infrastructure.DBContext;
using Microsoft.EntityFrameworkCore;
using static FactoryOps_AccessManagementService.FactoryOpsApp.Common.CommonConstant;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Repository.TenantAdmin.TenantAdminManagement
{
    public class RoleRepository : IRoleRepository
    {

        private readonly TenantDbContextFactory _tenantDbContext;
        private readonly IExceptionLoggerService _exceptionLogger;
        private readonly IAuditLogService _auditLogger;
        public RoleRepository(TenantDbContextFactory tenantDbContext,
            IExceptionLoggerService exceptionLogger,
            IAuditLogService auditLogger)
        {
            _tenantDbContext = tenantDbContext;
            _exceptionLogger = exceptionLogger;
            _auditLogger = auditLogger;
        }
        public async Task<CommonResponseModel> AddRole(AddRoleDto dto)
        {
            var response = new CommonResponseModel();

            using var tenantDb = _tenantDbContext.GetTenantDbContext(dto.TenantId);
            using var transaction = await tenantDb.Database.BeginTransactionAsync();

            try
            {
                if (await tenantDb.FactoryRoles.AnyAsync(r => r.RoleName.ToLower() == dto.RoleName.ToLower() && !r.IsDeleted))
                {
                    response.StatusCode = StatusCode.BadRequest;
                    response.StatusMessage = RoleStatusMessage.RoleAlreadyExists;
                    return response;
                }

                // ================== CREATE ROLE ==================
                var newRole = new FactoryRoles
                {
                    RoleName = dto.RoleName,
                    TenantId = dto.TenantId,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = dto.TenantId
                };

                tenantDb.FactoryRoles.Add(newRole);
                await tenantDb.SaveChangesAsync();   // To get RoleId

                // ================== ASSIGN PARENT PERMISSIONS ==================
                foreach (var permissionId in dto.PermissionIds)
                {
                    tenantDb.FactoryRolePermissions.Add(new FactoryRolePermissions
                    {
                        RoleId = newRole.RoleId,
                        PermissionId = permissionId,
                        TenantId = dto.TenantId,
                        IsActive = true,
                        IsDeleted = false,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = dto.TenantId
                    });
                }

                // ================== ASSIGN SUB PERMISSIONS ==================
                foreach (var subPermissionId in dto.SubPermissionIds)
                {
                    tenantDb.FactoryRoleSubPermissions.Add(new FactoryRoleSubPermissions
                    {
                        RoleId = newRole.RoleId,
                        SubPermissionId = subPermissionId,
                        TenantId = dto.TenantId,
                        IsActive = true,
                        IsDeleted = false,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = dto.TenantId
                    });
                }

                // ================== AUDIT ==================
                await _auditLogger.LogAuditAsync(
                    "Create",
                    $"Role '{dto.RoleName}' added with {dto.PermissionIds.Count} permissions & {dto.SubPermissionIds.Count} sub-permissions",
                    dto.TenantId,
                    "",
                    "RoleModule"
                );

                await tenantDb.SaveChangesAsync();
                await transaction.CommitAsync();

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = RoleStatusMessage.RoleAdded;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _exceptionLogger.LogExceptionAsync(
                    ex,
                    "RoleModule",
                    "AddRole",
                    dto.TenantId,
                    null
                );

                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"{RoleStatusMessage.RoleAddFailed}: {ex.Message}";
            }

            return response;
        }

        public async Task<CommonResponseModel> UpdateRole(AddRoleDto dto)
        {
            CommonResponseModel response = new();

            using var tenantDb = _tenantDbContext.GetTenantDbContext(dto.TenantId);
            using var transaction = await tenantDb.Database.BeginTransactionAsync();

            try
            {
                var existingRole = await tenantDb.FactoryRoles
                    .FirstOrDefaultAsync(r => r.RoleId == dto.RoleId && !r.IsDeleted);

                if (existingRole == null)
                {
                    response.StatusCode = StatusCode.NotFound;
                    response.StatusMessage = RoleStatusMessage.RoleNotFound;
                    return response;
                }

                existingRole.RoleName = dto.RoleName;
                existingRole.UpdatedAt = DateTime.UtcNow;

                // ================================================================
                // UPDATE PARENT PERMISSIONS
                // ================================================================
                var existingParentMappings = await tenantDb.FactoryRolePermissions
                    .Where(rp => rp.RoleId == dto.RoleId)
                    .ToListAsync();

                foreach (var mapping in existingParentMappings)
                {
                    if (!dto.PermissionIds.Contains(mapping.PermissionId))
                    {
                        mapping.IsDeleted = true;
                        mapping.IsActive = false;
                        mapping.UpdatedAt = DateTime.UtcNow;
                    }
                    else
                    {
                        mapping.IsDeleted = false;
                        mapping.IsActive = true;
                        mapping.UpdatedAt = DateTime.UtcNow;
                    }
                }

                var existingPermissionIds = existingParentMappings
                    .Select(m => m.PermissionId).ToList();

                foreach (var permissionId in dto.PermissionIds)
                {
                    if (!existingPermissionIds.Contains(permissionId))
                    {
                        tenantDb.FactoryRolePermissions.Add(new FactoryRolePermissions
                        {
                            RoleId = dto.RoleId,
                            PermissionId = permissionId,
                            TenantId = dto.TenantId,
                            IsActive = true,
                            IsDeleted = false,
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }

                // ================================================================
                // UPDATE SUB PERMISSIONS
                // ================================================================
                var existingSubMappings = await tenantDb.FactoryRoleSubPermissions
                    .Where(rsp => rsp.RoleId == dto.RoleId)
                    .ToListAsync();

                foreach (var mapping in existingSubMappings)
                {
                    if (!dto.SubPermissionIds.Contains(mapping.SubPermissionId))
                    {
                        mapping.IsDeleted = true;
                        mapping.UpdatedAt = DateTime.UtcNow;
                    }
                    else
                    {
                        mapping.IsDeleted = false;
                        mapping.IsActive = true;
                        mapping.UpdatedAt = DateTime.UtcNow;
                    }
                }

                var existingSubIds = existingSubMappings.Select(m => m.SubPermissionId).ToList();

                foreach (var subPermissionId in dto.SubPermissionIds)
                {
                    if (!existingSubIds.Contains(subPermissionId))
                    {
                        tenantDb.FactoryRoleSubPermissions.Add(new FactoryRoleSubPermissions
                        {
                            RoleId = dto.RoleId,
                            SubPermissionId = subPermissionId,
                            TenantId = dto.TenantId,
                            IsActive = true,
                            IsDeleted = false,
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }

                await _auditLogger.LogAuditAsync(
                    "Update",
                    $"Updated role '{existingRole.RoleName}' with {dto.PermissionIds.Count} permissions and {dto.SubPermissionIds.Count} sub permissions",
                    dto.TenantId,
                    null,
                    "RoleModule"
                );

                await tenantDb.SaveChangesAsync();
                await transaction.CommitAsync();

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = RoleStatusMessage.RoleUpdated;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _exceptionLogger.LogExceptionAsync(ex, "RoleModule", "UpdateRole", dto.TenantId, null);

                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"{RoleStatusMessage.RoleUpdateFailed}: {ex.Message}";
            }

            return response;
        }

        public async Task<CommonResponseModel> DeleteRole(int id, int tenantId)
        {
            CommonResponseModel response = new();
            using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

            try
            {
                var role = await tenantDb.FactoryRoles
                    .FirstOrDefaultAsync(r => r.RoleId == id && !r.IsDeleted);

                if (role == null)
                {
                    response.StatusCode = StatusCode.NotFound;
                    response.StatusMessage = RoleStatusMessage.RoleNotFound;
                    return response;
                }

                role.IsDeleted = true;
                role.IsActive = false;
                role.DeletedAt = DateTime.UtcNow;
                role.DeletedBy = tenantId;

                var rolePermissions = await tenantDb.FactoryRolePermissions
                    .Where(rp => rp.RoleId == id)
                    .ToListAsync();

                foreach (var mapping in rolePermissions)
                {
                    mapping.IsDeleted = true;
                    mapping.IsActive = false;
                    mapping.UpdatedAt = DateTime.UtcNow;
                }

                var roleSubPermissions = await tenantDb.FactoryRoleSubPermissions
                    .Where(rsp => rsp.RoleId == id)
                    .ToListAsync();

                foreach (var subMapping in roleSubPermissions)
                {
                    subMapping.IsDeleted = true;
                    subMapping.IsActive = false;
                    subMapping.UpdatedAt = DateTime.UtcNow;
                }

                await tenantDb.SaveChangesAsync();

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = RoleStatusMessage.RoleDeleted;
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(
                    ex,
                    sourceModule: "RoleModule",
                    apiName: "DeleteRole",
                    tenantId: tenantId,
                    userId: null
                );
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"{RoleStatusMessage.RoleDeleteFailed}: {ex.Message}";
            }

            return response;
        }

        public GetAllRecord<GetRolePermissionDto> GetAllRoles(int tenantId)
        {
            GetAllRecord<GetRolePermissionDto> response = new();
            using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

            try
            {
                var roles = tenantDb.FactoryRoles
                    .Where(r => !r.IsDeleted)
                    .Select(r => new GetRolePermissionDto
                    {
                        RoleId = r.RoleId,
                        RoleName = r.RoleName,
                        TenantId = r.TenantId,

                        Permissions =
                            // GET MAPPED PARENT PERMISSIONS OF ROLE
                            tenantDb.FactoryRolePermissions
                            .Where(rp => rp.RoleId == r.RoleId && !rp.IsDeleted && rp.IsActive)
                            .Join(tenantDb.FactoryPermissions,
                                  rp => rp.PermissionId,
                                  p => p.PermissionId,
                                  (rp, p) => new GetRolePermissionMappingDto
                                  {
                                      PermissionId = p.PermissionId,
                                      PermissionName = p.Name,

                                      // GET MAPPED SUB PERMISSIONS OF ROLE
                                      SubPermissions =
                                           tenantDb.FactoryRoleSubPermissions
                                           .Where(rsp => rsp.RoleId == r.RoleId
                                                   && !rsp.IsDeleted
                                                   && rsp.IsActive)
                                           .Join(tenantDb.FactorySubPermission,
                                                 rsp => rsp.SubPermissionId,
                                                 sp => sp.SubPermissionId,
                                                 (rsp, sp) => new GetRoleSubPermissionMapDto
                                                 {
                                                     SubPermissionId = sp.SubPermissionId,
                                                     SubPermissionName = sp.Name
                                                 }).ToList()
                                  }).ToList()
                    }).ToList();

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = RoleStatusMessage.RolesFetched;
                response.GetAllData = roles;
            }
            catch (Exception ex)
            {
                _exceptionLogger.LogExceptionAsync(
                    ex,
                    sourceModule: "RoleModule",
                    apiName: "GetAll-Role-Permissions",
                    tenantId: tenantId,
                    userId: null
                );
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"{RoleStatusMessage.RolesFetchFailed}: {ex.Message}";
            }

            return response;
        }

    }
}
