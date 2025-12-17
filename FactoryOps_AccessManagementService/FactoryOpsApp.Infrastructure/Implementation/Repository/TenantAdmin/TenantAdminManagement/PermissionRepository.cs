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
    public class PermissionRepository : IPermissionRepository
    {
        private readonly TenantDbContextFactory _tenantDbContext;
        private readonly IExceptionLoggerService _exceptionLogger;
        private readonly IAuditLogService _auditLogger;

        public PermissionRepository(
            TenantDbContextFactory tenantDbContext,
            IExceptionLoggerService exceptionLogger,
            IAuditLogService auditLogger)
        {
            _tenantDbContext = tenantDbContext;
            _exceptionLogger = exceptionLogger;
            _auditLogger = auditLogger;
        }

        #region ================== MODULE CRUD ==================

        public async Task<CommonResponseModel> AddPermission(AddPermissionDto dto)
        {
            CommonResponseModel response = new();
            using var tenantDb = _tenantDbContext.GetTenantDbContext(dto.TenantId);

            try
            {
                bool exists = await tenantDb.FactoryPermissions
                    .AnyAsync(x => x.Name.ToLower() == dto.Name.ToLower() && !x.IsDeleted);

                if (exists)
                {
                    response.StatusCode = StatusCode.BadRequest;
                    response.StatusMessage = PermissionStatusMessage.PermissionAlreadyExists;
                    return response;
                }

                tenantDb.FactoryPermissions.Add(new FactoryPermission
                {
                    Name = dto.Name,
                    TenantId = dto.TenantId,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                    IsDeleted = false
                });

                await tenantDb.SaveChangesAsync();

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = PermissionStatusMessage.PermissionAdded;
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "PermissionModule", "AddPermission", dto.TenantId, null);
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"Error Adding Permission: {ex.Message}";
            }
            return response;
        }


        public async Task<CommonResponseModel> UpdatePermission(AddPermissionDto dto)
        {
            CommonResponseModel response = new();
            using var tenantDb = _tenantDbContext.GetTenantDbContext(dto.TenantId);

            try
            {
                var entity = await tenantDb.FactoryPermissions
                    .FirstOrDefaultAsync(x => x.PermissionId == dto.PermissionId && !x.IsDeleted);

                if (entity == null)
                {
                    response.StatusCode = StatusCode.NotFound;
                    response.StatusMessage = PermissionStatusMessage.PermissionNotFound;
                    return response;
                }

                entity.Name = dto.Name;
                entity.UpdatedAt = DateTime.UtcNow;

                await tenantDb.SaveChangesAsync();

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = PermissionStatusMessage.PermissionUpdated;
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "PermissionModule", "UpdatePermission", dto.TenantId, null);
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"Error Updating Permission: {ex.Message}";
            }
            return response;
        }


        public async Task<CommonResponseModel> DeletePermission(int id, int tenantId)
        {
            CommonResponseModel response = new();
            using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

            try
            {
                var entity = await tenantDb.FactoryPermissions
                    .FirstOrDefaultAsync(x => x.PermissionId == id && !x.IsDeleted);

                if (entity == null)
                {
                    response.StatusCode = StatusCode.NotFound;
                    response.StatusMessage = PermissionStatusMessage.PermissionNotFound;
                    return response;
                }

                entity.IsDeleted = true;
                entity.DeletedAt = DateTime.UtcNow;

                await tenantDb.SaveChangesAsync();

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = PermissionStatusMessage.PermissionDeleted;
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "PermissionModule", "DeletePermission", tenantId, null);
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"Error Deleting Permission: {ex.Message}";
            }
            return response;
        }


        public GetAllRecord<AddPermissionDto> GetAllPermissions(int tenantId)
        {
            GetAllRecord<AddPermissionDto> response = new();
            using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

            try
            {
                var list = tenantDb.FactoryPermissions
                    .AsNoTracking()
                    .Where(x => !x.IsDeleted)
                    .Select(x => new AddPermissionDto
                    {
                        PermissionId = x.PermissionId,
                        Name = x.Name,
                        TenantId = x.TenantId
                    })
                    .ToList();

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = PermissionStatusMessage.PermissionsFetched;
                response.GetAllData = list;
            }
            catch (Exception ex)
            {
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"Error Fetching Permissions: {ex.Message}";
                response.GetAllData = new List<AddPermissionDto>();
            }
            return response;
        }

        #endregion

        #region ================== SUBMODULE CRUD ==================

        public async Task<CommonResponseModel> AddSubPermission(AddSubPermissionDto dto)
        {
            CommonResponseModel response = new();
            using var tenantDb = _tenantDbContext.GetTenantDbContext(dto.TenantId);

            try
            {
                tenantDb.FactorySubPermission.Add(new FactorySubPermission
                {
                    ParentPermissionId = dto.ParentPermissionId,
                    TenantId = dto.TenantId,
                    Name = dto.Name,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                    IsDeleted = false
                });

                await tenantDb.SaveChangesAsync();

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = "Sub Permission Added";
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "PermissionModule", "AddSubPermission", dto.TenantId, null);
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"Error Adding SubPermission: {ex.Message}";
            }
            return response;
        }


        public async Task<CommonResponseModel> UpdateSubPermission(AddSubPermissionDto dto)
        {
            CommonResponseModel response = new();
            using var tenantDb = _tenantDbContext.GetTenantDbContext(dto.TenantId);

            try
            {
                var entity = await tenantDb.FactorySubPermission
                    .FirstOrDefaultAsync(x => x.SubPermissionId == dto.SubPermissionId && !x.IsDeleted);

                if (entity == null)
                {
                    response.StatusCode = StatusCode.NotFound;
                    response.StatusMessage = "Sub Permission Not Found";
                    return response;
                }

                entity.Name = dto.Name;
                entity.UpdatedAt = DateTime.UtcNow;

                await tenantDb.SaveChangesAsync();

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = "Sub Permission Updated";
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "PermissionModule", "UpdateSubPermission", dto.TenantId, null);
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"Error Updating SubPermission: {ex.Message}";
            }
            return response;
        }


        public async Task<CommonResponseModel> DeleteSubPermission(int subPermissionId, int tenantId)
        {
            CommonResponseModel response = new();
            using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

            try
            {
                var entity = await tenantDb.FactorySubPermission
                    .FirstOrDefaultAsync(x => x.SubPermissionId == subPermissionId && !x.IsDeleted);

                if (entity == null)
                {
                    response.StatusCode = StatusCode.NotFound;
                    response.StatusMessage = "Sub Permission Not Found";
                    return response;
                }

                entity.IsDeleted = true;
                entity.DeletedAt = DateTime.UtcNow;

                await tenantDb.SaveChangesAsync();

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = "Sub Permission Deleted";
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "PermissionModule", "DeleteSubPermission", tenantId, null);
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"Error Deleting SubPermission: {ex.Message}";
            }
            return response;
        }

        #endregion

        #region ================== MODULE + SUBMODULE FETCH ==================

        public async Task<List<PermissionWithSubDto>> GetPermissionWithSubList(int tenantId)
        {
            using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

            try
            {
                return await tenantDb.FactoryPermissions
                    .AsNoTracking()
                    .Where(x => !x.IsDeleted)
                    .Select(x => new PermissionWithSubDto
                    {
                        PermissionId = x.PermissionId,
                        PermissionName = x.Name,

                        SubPermissions = x.SubPermissions
                            .Where(s => !s.IsDeleted)
                            .Select(s => new SubPermissionResponseDto
                            {
                                SubPermissionId = s.SubPermissionId,
                                SubPermissionName = s.Name
                            }).ToList()
                    }).ToListAsync();
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "PermissionModule", "GetPermissionWithSubList", tenantId, null);
                return new List<PermissionWithSubDto>();
            }
        }

        #endregion
    }
}
