using FactoryOperation_Asset.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.ExceptionLogger;
using FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.AssetManagement;
using FactoryOpsApp.Application.Interfaces.Services.SuperAdmin.AuditLogs;
using FactoryOpsApp.Domain.Entities.FactoryOpsTenants;
using FactoryOpsApp.Infrastructure.DBContext;
using Microsoft.EntityFrameworkCore;
using static FactoryOpsApp.Common.CommonConstant;

namespace FactoryOpsApp.Infrastructure.Repository.TenantAdmin.AssetManagement
{
    public class AssetTypeRepository : IAssetTypeRepository
    {
        private readonly TenantDbContextFactory _tenantDbContext;
        private readonly IExceptionLoggerService _exceptionLogger;
        private readonly IAuditLogService _auditLogger;

        public AssetTypeRepository(
            TenantDbContextFactory tenantDbContext,
            IExceptionLoggerService exceptionLogger,
            IAuditLogService auditLogger)
        {
            _tenantDbContext = tenantDbContext;
            _exceptionLogger = exceptionLogger;
            _auditLogger = auditLogger;
        }

        public async Task<CommonResponseModel> CreateAssetTypeAsync(CreateAssetTypeDto dto)
        {
            var response = new CommonResponseModel();
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(dto.TenantId);

                var exists = await tenantDb.FactoryAssetTypes
                    .AnyAsync(x => x.TenantId == dto.TenantId && x.Type_Name == dto.Type_Name && !x.IsDeleted);

                if (exists)
                {
                    response.StatusCode = StatusCode.BadRequest;
                    response.StatusMessage = AssetTypeStatusMessage.Duplicate;
                    return response;
                }

                var entity = new FactoryAssetType
                {
                    TenantId = dto.TenantId,
                    Type_Name = dto.Type_Name,
                    Description = dto.Description,
                    Default_Depreciation_Rate = dto.Default_Depreciation_Rate,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedBy = dto.CreatedBy,
                };

                tenantDb.FactoryAssetTypes.Add(entity);
                await tenantDb.SaveChangesAsync();

                await _auditLogger.LogAuditAsync("Create", $"Created AssetType: {dto.Type_Name}", dto.TenantId, "", "CreateAssetType");

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = AssetTypeStatusMessage.Created;
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "AssetType-Module", "Create-AssetType", dto.TenantId, null);
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"{AssetTypeStatusMessage.CreateFailed}: {ex.Message}";
            }

            return response;
        }

        public async Task<CommonResponseModel> UpdateAssetTypeAsync(UpdateAssetTypeDto dto)
        {
            var response = new CommonResponseModel();
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(dto.TenantId);
                var entity = await tenantDb.FactoryAssetTypes
                    .FirstOrDefaultAsync(x => x.AssetTypeId == dto.AssetTypeId && !x.IsDeleted);

                if (entity == null)
                {
                    response.StatusCode = StatusCode.NotFound;
                    response.StatusMessage = AssetTypeStatusMessage.NotFound;
                    return response;
                }

                var duplicate = await tenantDb.FactoryAssetTypes
                    .AnyAsync(x => x.TenantId == dto.TenantId && x.Type_Name == dto.Type_Name && x.AssetTypeId != dto.AssetTypeId && !x.IsDeleted);

                if (duplicate)
                {
                    response.StatusCode = StatusCode.BadRequest;
                    response.StatusMessage = AssetTypeStatusMessage.Duplicate;
                    return response;
                }

                entity.Type_Name = dto.Type_Name;
                entity.Description = dto.Description;
                entity.Default_Depreciation_Rate = dto.Default_Depreciation_Rate;
                entity.UpdatedAt = DateTime.UtcNow;
                entity.UpdatedBy = dto.UpdatedBy;

                await tenantDb.SaveChangesAsync();

                await _auditLogger.LogAuditAsync("Update", $"Updated AssetType: {dto.Type_Name}", dto.TenantId, "", "UpdateAssetType");

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = AssetTypeStatusMessage.Updated;
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "AssetType-Module", "Update-AssetType", dto.TenantId, null);
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"{AssetTypeStatusMessage.UpdateFailed}: {ex.Message}";
            }

            return response;
        }

        public async Task<CommonResponseModel> DeleteAssetTypeAsync(int id, int tenantId)
        {
            var response = new CommonResponseModel();
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);
                var entity = await tenantDb.FactoryAssetTypes
                    .FirstOrDefaultAsync(x => x.AssetTypeId == id && !x.IsDeleted);

                if (entity == null)
                {
                    response.StatusCode = StatusCode.NotFound;
                    response.StatusMessage = AssetTypeStatusMessage.NotFound;
                    return response;
                }

                entity.IsDeleted = true;
                entity.IsActive = false;
                entity.DeletedAt = DateTime.UtcNow;
                entity.DeletedBy = tenantId;

                await tenantDb.SaveChangesAsync();

                await _auditLogger.LogAuditAsync("Delete", $"Deleted AssetType: {entity.Type_Name}", tenantId, "", "DeleteAssetType");

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = AssetTypeStatusMessage.Deleted;
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "AssetType-Module", "Delete-AssetType", tenantId, null);
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"{AssetTypeStatusMessage.DeleteFailed}: {ex.Message}";
            }

            return response;
        }

        public async Task<GetAllRecord<AssetTypeResponseDto>> GetAllAssetTypesAsync(int tenantId)
        {
            var response = new GetAllRecord<AssetTypeResponseDto>();
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                var data = await tenantDb.FactoryAssetTypes
                    .Where(x => !x.IsDeleted)
                    .Select(x => new AssetTypeResponseDto
                    {
                        AssetTypeId = x.AssetTypeId,
                        Type_Name = x.Type_Name,
                        Description = x.Description,
                        Default_Depreciation_Rate = x.Default_Depreciation_Rate,
                        IsActive = x.IsActive,
                        IsDeleted = x.IsDeleted,
                        CreatedBy = x.CreatedBy,
                        CreatedAt = x.CreatedAt,
                        UpdatedBy = x.UpdatedBy,
                        UpdatedAt = x.UpdatedAt,
                        TenantId = x.TenantId
                    })
                    .ToListAsync();

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = AssetTypeStatusMessage.FetchedAll;
                response.GetAllData = data;
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "AssetType-Module", "GetAll-AssetType", tenantId, null);
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"{AssetTypeStatusMessage.FetchFailed}: {ex.Message}";
            }

            return response;
        }

        public async Task<AssetTypeResponseDto?> GetAssetTypeByIdAsync(int id, int tenantId)
        {
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                return await tenantDb.FactoryAssetTypes
                    .Where(x => x.AssetTypeId == id && !x.IsDeleted)
                    .Select(x => new AssetTypeResponseDto
                    {
                        AssetTypeId = x.AssetTypeId,
                        Type_Name = x.Type_Name,
                        Description = x.Description,
                        Default_Depreciation_Rate = x.Default_Depreciation_Rate,
                        IsActive = x.IsActive,
                        IsDeleted = x.IsDeleted,
                        CreatedBy = x.CreatedBy,
                        CreatedAt = x.CreatedAt,
                        UpdatedBy = x.UpdatedBy,
                        UpdatedAt = x.UpdatedAt,
                        TenantId = x.TenantId
                    })
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "AssetType-Module", "GetById-AssetType", tenantId, null);
                return null;
            }
        }
    }
}
