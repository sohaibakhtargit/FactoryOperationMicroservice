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
    public class RolePermissionMappingRepository : IRolePermissionMappingRepository
    {
        private readonly TenantDbContextFactory _tenantDbContext;
        private readonly IExceptionLoggerService _exceptionLogger;
        private readonly IAuditLogService _auditLogger;

        public RolePermissionMappingRepository(
            TenantDbContextFactory tenantDbContext,
            IExceptionLoggerService exceptionLogger,
            IAuditLogService auditLogger)
        {
            _tenantDbContext = tenantDbContext;
            _exceptionLogger = exceptionLogger;
            _auditLogger = auditLogger;
        }

        #region ========================= ASSIGN MAPPING ============================
        public async Task<CommonResponseModel> AssignPermissionsToRole(RolePermissionDto dto)
        {
            var response = new CommonResponseModel();
            using var tenantDb = _tenantDbContext.GetTenantDbContext(dto.TenantId);

            using var trx = await tenantDb.Database.BeginTransactionAsync();
            try
            {
                // Assign Parent Permissions
                foreach (var permissionId in dto.PermissionIds)
                {
                    bool exists = await tenantDb.FactoryRolePermissions
                        .AnyAsync(rp => rp.RoleId == dto.RoleId && rp.PermissionId == permissionId && !rp.IsDeleted);

                    if (!exists)
                    {
                        tenantDb.FactoryRolePermissions.Add(new FactoryRolePermissions
                        {
                            RoleId = dto.RoleId,
                            PermissionId = permissionId,
                            TenantId = dto.TenantId,
                            IsActive = true,
                            IsDeleted = false,
                            CreatedAt = DateTime.UtcNow,
                            CreatedBy = dto.TenantId
                        });
                    }
                }

                // Assign SubPermissions
                if (dto.SubPermissionIds != null && dto.SubPermissionIds.Any())
                {
                    foreach (var subPermissionId in dto.SubPermissionIds)
                    {
                        bool exists = await tenantDb.FactoryRoleSubPermissions
                            .AnyAsync(sp => sp.RoleId == dto.RoleId && sp.SubPermissionId == subPermissionId && !sp.IsDeleted);

                        if (!exists)
                        {
                            tenantDb.FactoryRoleSubPermissions.Add(new FactoryRoleSubPermissions
                            {
                                RoleId = dto.RoleId,
                                SubPermissionId = subPermissionId,
                                TenantId = dto.TenantId,
                                IsActive = true,
                                IsDeleted = false,
                                CreatedAt = DateTime.UtcNow,
                                CreatedBy = dto.TenantId
                            });
                        }
                    }
                }

                await tenantDb.SaveChangesAsync();
                await trx.CommitAsync();

                await _auditLogger.LogAuditAsync("Create", $"Mapped permission/subpermission to role {dto.RoleId}", dto.TenantId, "", "RolePermissionMapping");

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = "Permissions & SubPermissions assigned successfully";
            }
            catch (Exception ex)
            {
                await trx.RollbackAsync();
                await _exceptionLogger.LogExceptionAsync(ex, "RolePermissionMapping", "AssignPermissionsToRole", dto.TenantId, null);

                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"Assign failed: {ex.Message}";
            }

            return response;
        }
        #endregion

        #region ========================= REMOVE MAPPING ===============================
        public async Task<CommonResponseModel> RemovePermissionsFromRole(int roleId, List<int> permissionIds, List<int> subPermissionIds, int tenantId)
        {
            var response = new CommonResponseModel();
            using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);
            using var trx = await tenantDb.Database.BeginTransactionAsync();

            try
            {
                // Remove Parent Permissions
                var permissions = tenantDb.FactoryRolePermissions
                    .Where(rp => rp.RoleId == roleId && permissionIds.Contains(rp.PermissionId) && !rp.IsDeleted);

                foreach (var m in permissions)
                {
                    m.IsDeleted = true;
                    m.IsActive = false;
                    m.UpdatedAt = DateTime.UtcNow;
                }

                // Remove SubPermissions
                var subs = tenantDb.FactoryRoleSubPermissions
                    .Where(rsp => rsp.RoleId == roleId && subPermissionIds.Contains(rsp.SubPermissionId) && !rsp.IsDeleted);

                foreach (var m in subs)
                {
                    m.IsDeleted = true;
                    m.IsActive = false;
                    m.UpdatedAt = DateTime.UtcNow;
                }

                await tenantDb.SaveChangesAsync();
                await trx.CommitAsync();

                await _auditLogger.LogAuditAsync("Delete", "Permissions removed", tenantId, "", "RolePermissionMapping");

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = "Permissions & SubPermissions removed successfully";
            }
            catch (Exception ex)
            {
                await trx.RollbackAsync();
                await _exceptionLogger.LogExceptionAsync(ex, "RolePermissionMapping", "RemovePermissionsFromRole", tenantId, null);

                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"Remove failed: {ex.Message}";
            }

            return response;
        }
        #endregion

        #region ========================= GET MAPPING ===============================

        public GetAllRecord<PermissionDto> GetPermissionsByRoleId(int roleId, int tenantId)
        {
            var response = new GetAllRecord<PermissionDto>();
            using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

            try
            {
                var permissions = tenantDb.FactoryRolePermissions
                    .Where(rp =>
                        rp.RoleId == roleId &&
                        rp.IsActive &&
                        !rp.IsDeleted)
                    .Join(
                        tenantDb.FactoryPermissions.Where(p => p.IsActive && !p.IsDeleted),
                        rp => rp.PermissionId,
                        p => p.PermissionId,
                        (rp, p) => new PermissionDto
                        {
                            PermissionId = p.PermissionId,
                            PermissionName = p.Name,
                            TenantId = p.TenantId,

                            SubPermissions = (
                                from rsp in tenantDb.FactoryRoleSubPermissions
                                join sp in tenantDb.FactorySubPermission
                                    on rsp.SubPermissionId equals sp.SubPermissionId
                                where
                                    rsp.RoleId == roleId &&
                                    rsp.IsActive &&
                                    !rsp.IsDeleted &&
                                    sp.IsActive &&
                                    !sp.IsDeleted &&
                                    sp.ParentPermissionId == p.PermissionId 
                                select new SubPermissionListResponseDto
                                {
                                    SubPermissionId = sp.SubPermissionId,
                                    SubPermissionName = sp.Name
                                }
                            ).ToList()
                        })
                    .ToList();

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = "Fetched Successfully";
                response.GetAllData = permissions;
            }
            catch (Exception ex)
            {
                _exceptionLogger.LogExceptionAsync(
                    ex,
                    "RolePermissionMapping",
                    "GetPermissionsByRoleId",
                    tenantId,
                    null
                );

                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"Fetch failed: {ex.Message}";
            }

            return response;
        }

        #endregion
    }
}
