using FactoryOperation_KafkaMqttService.FactoryOpsApp.Application.Common;
using FactoryOperation_KafkaMqttService.FactoryOpsApp.Application.DTOs.KafkaMqttBridge;
using FactoryOperation_KafkaMqttService.FactoryOpsApp.Application.Interfaces.Repositories.KafkaMqttBridge;
using FactoryOperation_KafkaMqttService.FactoryOpsApp.Domain.Entities.FactoryOpsTenants;
using FactoryOperation_KafkaMqttService.FactoryOpsApp.Infrastructure.DBContext;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace FactoryOperation_KafkaMqttService.FactoryOpsApp.Infrastructure.Implementations.Repositories.KafkaMqttBridge
{
    public class BridgeConfigurationRepository : IBridgeConfigurationRepository
    {
        private readonly TenantDbContextFactory _tenantDb;

        public BridgeConfigurationRepository(TenantDbContextFactory tenantDb)
        {
            _tenantDb = tenantDb;
        }

        private static JsonDocument ParseOrEmpty(string? json) =>
            string.IsNullOrWhiteSpace(json) ? JsonDocument.Parse("{}") : JsonDocument.Parse(json);

        public async Task<GetAllRecord<BridgeConfigurationDto>> GetAllAsync(int tenantId)
        {
            var response = new GetAllRecord<BridgeConfigurationDto>();
            try
            {
                using var db = _tenantDb.GetTenantDbContext(tenantId);
                var data = await db.BridgeConfigurations
                    .AsNoTracking()
                    .Where(x => x.TenantId == tenantId && !x.IsDeleted)
                    .Select(x => new BridgeConfigurationDto
                    {
                        Id = x.Id,
                        TenantId = x.TenantId,
                        Name = x.Name,
                        Environment = x.Environment,
                        Direction = x.Direction,
                        SourcePattern = x.SourcePattern,
                        TargetPattern = x.TargetPattern,
                        MappingRules = x.MappingRules,
                        Transformation = x.Transformation,
                        Enabled = x.Enabled,
                        Priority = x.Priority,
                        RetryPolicy = x.RetryPolicy,
                        DlqTopic = x.DlqTopic,
                        IsActive = x.IsActive,
                        Description = x.Description,
                        CreatedAt = x.CreatedAt,
                        CreatedBy = x.CreatedBy,
                        UpdatedAt = x.UpdatedAt,
                        UpdatedBy = x.UpdatedBy
                    })
                    .ToListAsync();

                response.StatusCode = "200";
                response.StatusMessage = "Bridge configurations fetched successfully.";
                response.GetAllData = data;
            }
            catch (Exception ex)
            {
                response.StatusCode = "500";
                response.StatusMessage = $"Failed to fetch bridge configurations: {ex.Message}";
            }
            return response;
        }

        public async Task<GetSpecificRecord<BridgeConfigurationDto>> GetByIdAsync(int id, int tenantId)
        {
            var response = new GetSpecificRecord<BridgeConfigurationDto>();
            try
            {
                using var db = _tenantDb.GetTenantDbContext(tenantId);
                var data = await db.BridgeConfigurations
                    .AsNoTracking()
                    .Where(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted)
                    .Select(x => new BridgeConfigurationDto
                    {
                        Id = x.Id,
                        TenantId = x.TenantId,
                        Name = x.Name,
                        Environment = x.Environment,
                        Direction = x.Direction,
                        SourcePattern = x.SourcePattern,
                        TargetPattern = x.TargetPattern,
                        MappingRules = x.MappingRules,
                        Transformation = x.Transformation,
                        Enabled = x.Enabled,
                        Priority = x.Priority,
                        RetryPolicy = x.RetryPolicy,
                        DlqTopic = x.DlqTopic,
                        IsActive = x.IsActive,
                        Description = x.Description,
                        CreatedAt = x.CreatedAt,
                        CreatedBy = x.CreatedBy,
                        UpdatedAt = x.UpdatedAt,
                        UpdatedBy = x.UpdatedBy
                    })
                    .FirstOrDefaultAsync();

                if (data == null)
                {
                    response.StatusCode = "404";
                    response.StatusMessage = "Bridge configuration not found.";
                    return response;
                }

                response.StatusCode = "200";
                response.StatusMessage = "Bridge configuration fetched successfully.";
                response.Data = data;
            }
            catch (Exception ex)
            {
                response.StatusCode = "500";
                response.StatusMessage = $"Failed to fetch bridge configuration: {ex.Message}";
            }
            return response;
        }

        public async Task<CommonResponseModel> CreateAsync(BridgeConfigurationCreateDto dto)
        {
            var response = new CommonResponseModel();
            try
            {
                using var db = _tenantDb.GetTenantDbContext(dto.TenantId);

                var entity = new BridgeConfigurations
                {
                    TenantId = dto.TenantId,
                    Name = dto.Name,
                    Environment = dto.Environment,
                    Direction = dto.Direction,
                    SourcePattern = dto.SourcePattern,
                    TargetPattern = dto.TargetPattern,
                    MappingRules = ParseOrEmpty(dto.MappingRulesJson),
                    Transformation = dto.Transformation,
                    Enabled = dto.Enabled,
                    Priority = dto.Priority,
                    RetryPolicy = ParseOrEmpty(dto.RetryPolicyJson),
                    DlqTopic = dto.DlqTopic,
                    IsActive = dto.IsActive,
                    Description = dto.Description,
                    CreatedBy = dto.CreatedBy,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await db.BridgeConfigurations.AddAsync(entity);
                await db.SaveChangesAsync();

                response.StatusCode = "200";
                response.StatusMessage = "Bridge configuration created successfully.";
            }
            catch (Exception ex)
            {
                response.StatusCode = "500";
                response.StatusMessage = $"Failed to create bridge configuration: {ex.Message}";
            }
            return response;
        }

        public async Task<CommonResponseModel> UpdateAsync(BridgeConfigurationUpdateDto dto)
        {
            var response = new CommonResponseModel();
            try
            {
                using var db = _tenantDb.GetTenantDbContext(dto.TenantId);
                var entity = await db.BridgeConfigurations
                    .FirstOrDefaultAsync(x => x.Id == dto.Id && x.TenantId == dto.TenantId && !x.IsDeleted);

                if (entity == null)
                {
                    response.StatusCode = "404";
                    response.StatusMessage = "Bridge configuration not found.";
                    return response;
                }

                entity.Name = dto.Name;
                entity.Environment = dto.Environment;
                entity.Direction = dto.Direction;
                entity.SourcePattern = dto.SourcePattern;
                entity.TargetPattern = dto.TargetPattern;
                if (dto.MappingRulesJson != null) entity.MappingRules = ParseOrEmpty(dto.MappingRulesJson);
                entity.Transformation = dto.Transformation;
                entity.Enabled = dto.Enabled;
                entity.Priority = dto.Priority;
                if (dto.RetryPolicyJson != null) entity.RetryPolicy = ParseOrEmpty(dto.RetryPolicyJson);
                entity.DlqTopic = dto.DlqTopic;
                entity.IsActive = dto.IsActive;
                entity.Description = dto.Description;
                entity.UpdatedBy = dto.UpdatedBy;
                entity.UpdatedAt = DateTime.UtcNow;

                await db.SaveChangesAsync();

                response.StatusCode = "200";
                response.StatusMessage = "Bridge configuration updated successfully.";
            }
            catch (Exception ex)
            {
                response.StatusCode = "500";
                response.StatusMessage = $"Failed to update bridge configuration: {ex.Message}";
            }
            return response;
        }

        public async Task<CommonResponseModel> DeleteAsync(int id, int tenantId)
        {
            var response = new CommonResponseModel();
            try
            {
                using var db = _tenantDb.GetTenantDbContext(tenantId);
                var entity = await db.BridgeConfigurations
                    .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted);

                if (entity == null)
                {
                    response.StatusCode = "404";
                    response.StatusMessage = "Bridge configuration not found.";
                    return response;
                }

                entity.IsDeleted = true;
                entity.IsActive = false;
                entity.UpdatedAt = DateTime.UtcNow;

                await db.SaveChangesAsync();

                response.StatusCode = "200";
                response.StatusMessage = "Bridge configuration deleted successfully.";
            }
            catch (Exception ex)
            {
                response.StatusCode = "500";
                response.StatusMessage = $"Failed to delete bridge configuration: {ex.Message}";
            }
            return response;
        }
    }
}
