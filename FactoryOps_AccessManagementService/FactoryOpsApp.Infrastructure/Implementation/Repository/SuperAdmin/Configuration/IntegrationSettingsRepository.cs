using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.SuperAdmin.Configuration;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.SuperAdmin.AuditLogs;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.ExceptionLogger;
using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Domain.Entities;
using FactoryOpsApp.Infrastructure.DBContext;
using Microsoft.EntityFrameworkCore;
using static FactoryOps_AccessManagementService.FactoryOpsApp.Common.CommonConstant;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Repository.SuperAdmin.Configuration
{
    public class IntegrationSettingsRepository : IIntegrationSettingsRepository
    {
        private readonly TenantDbContextFactory _tenantDbContext;
        private readonly IExceptionLoggerService _exceptionLogger;
        private readonly IAuditLogService _auditLogger;

        public IntegrationSettingsRepository(
            TenantDbContextFactory tenantDbContext,
            IExceptionLoggerService exceptionLogger,
            IAuditLogService auditLogger)
        {
            _tenantDbContext = tenantDbContext;
            _exceptionLogger = exceptionLogger;
            _auditLogger = auditLogger;
        }

        public async Task<GetSpecificRecord<IntegrationSettingsDto>> GetIntegrationSettingByIdAsync(int id, int tenantId)
        {
            var response = new GetSpecificRecord<IntegrationSettingsDto>();
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                var data = await tenantDb.IntegrationSettings
                    .Where(x => x.IntegrationId == id && !x.IsDeleted)
                    .Select(x => new IntegrationSettingsDto
                    {
                        IntegrationId = x.IntegrationId,
                        Name = x.Name,
                        Category = x.Category,
                        Description = x.Description,
                        SettingValue = x.SettingValue,
                        IsActive = x.IsActive,
                        CreatedAt = x.CreatedAt
                    })
                    .FirstOrDefaultAsync();

                if (data == null)
                {
                    response.StatusCode = StatusCode.NotFound;
                    response.StatusMessage = IntegrationSettingsStatusMessage.NoRecordsFound;
                }
                else
                {
                    response.StatusCode = StatusCode.Success;
                    response.StatusMessage = IntegrationSettingsStatusMessage.DataFetched;
                    response.Data = data;
                }
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "IntegrationSettings-Module", "GetById-IntegrationSetting", tenantId, null);
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"Failed to fetch integration setting: {ex.Message}";
            }

            return response;
        }

        public async Task<GetAllRecord<IntegrationSettingsDto>> GetAllIntegrationSettingAsync(int tenantId)
        {
            var response = new GetAllRecord<IntegrationSettingsDto>();
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                var data = await tenantDb.IntegrationSettings
                    .Where(x => !x.IsDeleted)
                    .Select(x => new IntegrationSettingsDto
                    {
                        IntegrationId = x.IntegrationId,
                        Name = x.Name,
                        Category = x.Category,
                        Description = x.Description,
                        SettingValue = x.SettingValue,
                        IsActive = x.IsActive,
                        CreatedAt = x.CreatedAt
                    })
                    .ToListAsync();

                if (!data.Any())
                {
                    response.StatusCode = StatusCode.NotFound;
                    response.StatusMessage = IntegrationSettingsStatusMessage.NoRecordsFound;
                }
                else
                {
                    response.StatusCode = StatusCode.Success;
                    response.StatusMessage = IntegrationSettingsStatusMessage.DataFetched;
                    response.GetAllData = data;
                }
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "IntegrationSettings-Module", "GetAll-IntegrationSettings", tenantId, null);
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"Failed to fetch integration settings: {ex.Message}";
            }

            return response;
        }

        public async Task<GetAllRecord<IntegrationSettingsDto>> GetIntegrationSettingByCategoryAsync(IntegrationSettingsCategory category, int tenantId)
        {
            var response = new GetAllRecord<IntegrationSettingsDto>();
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId); 

                var data = await tenantDb.IntegrationSettings
                    .Where(x => x.Category == category && x.TenantId == tenantId && !x.IsDeleted)

                    .Select(x => new IntegrationSettingsDto

                    {
                        TenantId = x.TenantId,
                        IntegrationId = x.IntegrationId,
                        Name = x.Name,
                        Category = x.Category,
                        Description = x.Description,
                        SettingValue = x.SettingValue,
                        IsActive = x.IsActive,
                        CreatedAt = x.CreatedAt
                    })
                    .ToListAsync();

                if (!data.Any())
                {
                    response.StatusCode = StatusCode.NotFound;
                    response.StatusMessage = $"{IntegrationSettingsStatusMessage.NoCategoryRecordsFound} {category}";
                }
                else
                {
                    response.StatusCode = StatusCode.Success;
                    response.StatusMessage = $"{IntegrationSettingsStatusMessage.CategoryDataFetched} {category} ";
                    response.GetAllData = data;
                }
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "IntegrationSettings-Module", "GetByCategory-IntegrationSettings", 0, null);
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"Failed to fetch integration settings: {ex.Message}";
            }

            return response;
        }

        public async Task<CommonResponseModel> AddIntegrationSettingAsync(CreateIntegrationSettingsDto dto)
        {
            var response = new CommonResponseModel();
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(dto.TenantId);

                var entity = new IntegrationSettings
                {
                    TenantId = dto.TenantId,
                    Name = dto.Name,
                    Category = dto.Category,
                    Description = dto.Description,
                    SettingValue = dto.SettingValue,
                    IsActive = dto.IsActive ?? true,
                    CreatedBy = dto.CreatedBy,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsDeleted = false
                };

                await tenantDb.IntegrationSettings.AddAsync(entity);
                await tenantDb.SaveChangesAsync();

                await _auditLogger.LogAuditAsync("Create", $"Created IntegrationSetting: {entity.Name}", dto.TenantId, "", "AddIntegrationSetting");

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = IntegrationSettingsStatusMessage.IntegrationAdded;
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "IntegrationSettings-Module", "Add-IntegrationSetting", dto.TenantId, null);
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"Failed to create integration setting: {ex.Message}";
            }

            return response;
        }

        public async Task<CommonResponseModel> UpdateIntegrationSettingAsync(UpdateIntegrationSettingsDto dto, int tenantId)
        {
            var response = new CommonResponseModel();
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                var entity = await tenantDb.IntegrationSettings.FirstOrDefaultAsync(x => x.IntegrationId == dto.IntegrationId && !x.IsDeleted);

                if (entity == null)
                {
                    response.StatusCode = StatusCode.NotFound;
                    response.StatusMessage = IntegrationSettingsStatusMessage.NoRecordsFound;
                    return response;
                }

                if (!string.IsNullOrEmpty(dto.Name))
                    entity.Name = dto.Name;

                if (dto.Category.HasValue)
                    entity.Category = dto.Category.Value;

                if (!string.IsNullOrEmpty(dto.Description))
                    entity.Description = dto.Description;

                if (dto.SettingValue != null)
                    entity.SettingValue = dto.SettingValue;

                if (dto.IsActive.HasValue)
                    entity.IsActive = dto.IsActive.Value;

                entity.UpdatedBy = dto.UpdatedBy;
                entity.UpdatedAt = DateTime.UtcNow;

                await tenantDb.SaveChangesAsync();

                await _auditLogger.LogAuditAsync("Update", $"Updated IntegrationSetting Id: {dto.IntegrationId}", tenantId, "", "UpdateIntegrationSetting");

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = IntegrationSettingsStatusMessage.IntegrationUpdated;
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "IntegrationSettings-Module", "Update-IntegrationSetting", tenantId, null);
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"Failed to update integration setting: {ex.Message}";
            }

            return response;
        }

        public async Task<CommonResponseModel> DeleteIntegrationSettingAsync(int id, int deletedBy, int tenantId)
        {
            var response = new CommonResponseModel();
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                var entity = await tenantDb.IntegrationSettings.FirstOrDefaultAsync(x => x.IntegrationId == id && !x.IsDeleted);

                if (entity == null)
                {
                    response.StatusCode = StatusCode.NotFound;
                    response.StatusMessage = IntegrationSettingsStatusMessage.NoRecordsFound;
                    return response;
                }

                entity.IsDeleted = true;
                entity.DeletedAt = DateTime.UtcNow;
                entity.DeletedBy = deletedBy;

                await tenantDb.SaveChangesAsync();

                await _auditLogger.LogAuditAsync("Delete", $"Deleted IntegrationSetting Id: {id}", tenantId, "", "DeleteIntegrationSetting");

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = IntegrationSettingsStatusMessage.IntegrationDeleted;
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "IntegrationSettings-Module", "Delete-IntegrationSetting", tenantId, null);
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"Failed to delete integration setting: {ex.Message}";
            }

            return response;
        }
    }
}
