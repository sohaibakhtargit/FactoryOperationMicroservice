using FactoryOperation_KafkaMqttService.FactoryOpsApp.Application.Common;
using FactoryOperation_KafkaMqttService.FactoryOpsApp.Application.DTOs.KafkaMqttBridge;
using FactoryOperation_KafkaMqttService.FactoryOpsApp.Application.Interfaces.Repositories.KafkaMqttBridge;
using FactoryOperation_KafkaMqttService.FactoryOpsApp.Domain.Entities.FactoryOpsTenants;
using FactoryOperation_KafkaMqttService.FactoryOpsApp.Infrastructure.DBContext;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace FactoryOperation_KafkaMqttService.FactoryOpsApp.Infrastructure.Implementations.Repositories.KafkaMqttBridge
{
    public class MqttConfigurationRepository : IMqttConfigurationRepository
    {
        private readonly TenantDbContextFactory _tenantDb;

        public MqttConfigurationRepository(TenantDbContextFactory tenantDb)
        {
            _tenantDb = tenantDb;
        }

        private static JsonDocument ParseOrEmpty(string? json) =>
            string.IsNullOrWhiteSpace(json) ? JsonDocument.Parse("{}") : JsonDocument.Parse(json);

        public async Task<GetAllRecord<MqttConfigurationDto>> GetAllAsync(int tenantId)
        {
            var response = new GetAllRecord<MqttConfigurationDto>();
            try
            {
                using var db = _tenantDb.GetTenantDbContext(tenantId);
                var data = await db.MqttConfigurations
                    .AsNoTracking()
                    .Where(x => x.TenantId == tenantId && !x.IsDeleted)
                    .Select(x => new MqttConfigurationDto
                    {
                        Id = x.Id,
                        TenantId = x.TenantId,
                        Name = x.Name,
                        Environment = x.Environment,
                        BrokerUrl = x.BrokerUrl,
                        BrokerPort = x.BrokerPort,
                        ClientIdTemplate = x.ClientIdTemplate,
                        CleanSession = x.CleanSession,
                        Username = x.Username,
                        Password = x.Password,
                        KeepAliveSeconds = x.KeepAliveSeconds,
                        TopicTemplate = x.TopicTemplate,
                        UseSsl = x.UseSsl,
                        LastWill = x.LastWill,
                        SubscriptionQos = x.SubscriptionQos,
                        PublishQos = x.PublishQos,
                        OfflineBuffering = x.OfflineBuffering,
                        IsActive = x.IsActive,
                        Description = x.Description,
                        CreatedAt = x.CreatedAt,
                        CreatedBy = x.CreatedBy,
                        UpdatedAt = x.UpdatedAt,
                        UpdatedBy = x.UpdatedBy
                    })
                    .ToListAsync();

                response.StatusCode = "200";
                response.StatusMessage = "MQTT configurations fetched successfully.";
                response.GetAllData = data;
            }
            catch (Exception ex)
            {
                response.StatusCode = "500";
                response.StatusMessage = $"Failed to fetch MQTT configurations: {ex.Message}";
            }
            return response;
        }

        public async Task<GetSpecificRecord<MqttConfigurationDto>> GetByIdAsync(int id, int tenantId)
        {
            var response = new GetSpecificRecord<MqttConfigurationDto>();
            try
            {
                using var db = _tenantDb.GetTenantDbContext(tenantId);
                var data = await db.MqttConfigurations
                    .AsNoTracking()
                    .Where(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted)
                    .Select(x => new MqttConfigurationDto
                    {
                        Id = x.Id,
                        TenantId = x.TenantId,
                        Name = x.Name,
                        Environment = x.Environment,
                        BrokerUrl = x.BrokerUrl,
                        BrokerPort = x.BrokerPort,
                        ClientIdTemplate = x.ClientIdTemplate,
                        CleanSession = x.CleanSession,
                        Username = x.Username,
                        Password = x.Password,
                        KeepAliveSeconds = x.KeepAliveSeconds,
                        TopicTemplate = x.TopicTemplate,
                        UseSsl = x.UseSsl,
                        LastWill = x.LastWill,
                        SubscriptionQos = x.SubscriptionQos,
                        PublishQos = x.PublishQos,
                        OfflineBuffering = x.OfflineBuffering,
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
                    response.StatusMessage = "MQTT configuration not found.";
                    return response;
                }

                response.StatusCode = "200";
                response.StatusMessage = "MQTT configuration fetched successfully.";
                response.Data = data;
            }
            catch (Exception ex)
            {
                response.StatusCode = "500";
                response.StatusMessage = $"Failed to fetch MQTT configuration: {ex.Message}";
            }
            return response;
        }

        public async Task<CommonResponseModel> CreateAsync(MqttConfigurationCreateDto dto)
        {
            var response = new CommonResponseModel();
            try
            {
                using var db = _tenantDb.GetTenantDbContext(dto.TenantId);
                var entity = new MqttConfigurations
                {
                    TenantId = dto.TenantId,
                    Name = dto.Name,
                    Environment = dto.Environment,
                    BrokerUrl = dto.BrokerUrl,
                    BrokerPort = dto.BrokerPort,
                    ClientIdTemplate = dto.ClientIdTemplate,
                    CleanSession = dto.CleanSession,
                    Username = dto.Username,
                    Password = dto.Password,
                    KeepAliveSeconds = dto.KeepAliveSeconds,
                    TopicTemplate = dto.TopicTemplate,
                    UseSsl = dto.UseSsl,
                    LastWill = ParseOrEmpty(dto.LastWillJson),
                    SubscriptionQos = dto.SubscriptionQos,
                    PublishQos = dto.PublishQos,
                    OfflineBuffering = ParseOrEmpty(dto.OfflineBufferingJson),
                    IsActive = dto.IsActive,
                    Description = dto.Description,
                    CreatedBy = dto.CreatedBy,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await db.MqttConfigurations.AddAsync(entity);
                await db.SaveChangesAsync();

                response.StatusCode = "200";
                response.StatusMessage = "MQTT configuration created successfully.";
            }
            catch (Exception ex)
            {
                response.StatusCode = "500";
                response.StatusMessage = $"Failed to create MQTT configuration: {ex.Message}";
            }
            return response;
        }

        public async Task<CommonResponseModel> UpdateAsync(MqttConfigurationUpdateDto dto)
        {
            var response = new CommonResponseModel();
            try
            {
                using var db = _tenantDb.GetTenantDbContext(dto.TenantId);
                var entity = await db.MqttConfigurations
                    .FirstOrDefaultAsync(x => x.Id == dto.Id && x.TenantId == dto.TenantId && !x.IsDeleted);

                if (entity == null)
                {
                    response.StatusCode = "404";
                    response.StatusMessage = "MQTT configuration not found.";
                    return response;
                }

                entity.Name = dto.Name;
                entity.Environment = dto.Environment;
                entity.BrokerUrl = dto.BrokerUrl;
                entity.BrokerPort = dto.BrokerPort;
                entity.ClientIdTemplate = dto.ClientIdTemplate;
                entity.CleanSession = dto.CleanSession;
                entity.Username = dto.Username;
                entity.Password = dto.Password;
                entity.KeepAliveSeconds = dto.KeepAliveSeconds;
                entity.TopicTemplate = dto.TopicTemplate;
                entity.UseSsl = dto.UseSsl;
                if (dto.LastWillJson != null) entity.LastWill = ParseOrEmpty(dto.LastWillJson);
                entity.SubscriptionQos = dto.SubscriptionQos;
                entity.PublishQos = dto.PublishQos;
                if (dto.OfflineBufferingJson != null) entity.OfflineBuffering = ParseOrEmpty(dto.OfflineBufferingJson);
                entity.IsActive = dto.IsActive;
                entity.Description = dto.Description;
                entity.UpdatedBy = dto.UpdatedBy;
                entity.UpdatedAt = DateTime.UtcNow;

                await db.SaveChangesAsync();

                response.StatusCode = "200";
                response.StatusMessage = "MQTT configuration updated successfully.";
            }
            catch (Exception ex)
            {
                response.StatusCode = "500";
                response.StatusMessage = $"Failed to update MQTT configuration: {ex.Message}";
            }
            return response;
        }

        public async Task<CommonResponseModel> DeleteAsync(int id, int tenantId)
        {
            var response = new CommonResponseModel();
            try
            {
                using var db = _tenantDb.GetTenantDbContext(tenantId);
                var entity = await db.MqttConfigurations
                    .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted);

                if (entity == null)
                {
                    response.StatusCode = "404";
                    response.StatusMessage = "MQTT configuration not found.";
                    return response;
                }

                entity.IsDeleted = true;
                entity.IsActive = false;
                entity.UpdatedAt = DateTime.UtcNow;

                await db.SaveChangesAsync();

                response.StatusCode = "200";
                response.StatusMessage = "MQTT configuration deleted successfully.";
            }
            catch (Exception ex)
            {
                response.StatusCode = "500";
                response.StatusMessage = $"Failed to delete MQTT configuration: {ex.Message}";
            }
            return response;
        }
    }
}
