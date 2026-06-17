using FactoryOperation_KafkaMqttService.FactoryOpsApp.Application.Common;
using FactoryOperation_KafkaMqttService.FactoryOpsApp.Application.DTOs.KafkaMqttBridge;
using FactoryOperation_KafkaMqttService.FactoryOpsApp.Application.Interfaces.Repositories.KafkaMqttBridge;
using FactoryOperation_KafkaMqttService.FactoryOpsApp.Domain.Entities.FactoryOpsTenants;
using FactoryOperation_KafkaMqttService.FactoryOpsApp.Infrastructure.DBContext;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace FactoryOperation_KafkaMqttService.FactoryOpsApp.Infrastructure.Implementations.Repositories.KafkaMqttBridge
{

    public class KafkaConfigurationRepository : IKafkaConfigurationRepository
    {
        private readonly TenantDbContextFactory _tenantDb;

        public KafkaConfigurationRepository(TenantDbContextFactory tenantDb)
        {
            _tenantDb = tenantDb;
        }

        private static JsonDocument ParseOrEmpty(string? json) =>
            string.IsNullOrWhiteSpace(json) ? JsonDocument.Parse("{}") : JsonDocument.Parse(json);

        public async Task<GetAllRecord<KafkaConfigurationDto>> GetAllAsync(int tenantId)
        {
            var response = new GetAllRecord<KafkaConfigurationDto>();
            try
            {
                using var db = _tenantDb.GetTenantDbContext(tenantId);
                var data = await db.KafkaConfigurations
                    .AsNoTracking()
                    .Where(x => x.TenantId == tenantId && !x.IsDeleted)
                    .Select(x => new KafkaConfigurationDto
                    {
                        Id = x.Id,
                        TenantId = x.TenantId,
                        Name = x.Name,
                        Environment = x.Environment,
                        BootstrapServers = x.BootstrapServers,
                        GroupId = x.GroupId,
                        TopicPattern = x.TopicPattern,
                        UsePerTenantTopic = x.UsePerTenantTopic,
                        EnableAutoOffsetStore = x.EnableAutoOffsetStore,
                        EnableAutoCommit = x.EnableAutoCommit,
                        SecurityProtocol = x.SecurityProtocol,
                        SaslMechanism = x.SaslMechanism,
                        SaslUsername = x.SaslUsername,
                        SaslPassword = x.SaslPassword,
                        ProducerConfig = x.ProducerConfig,
                        ConsumerConfig = x.ConsumerConfig,
                        DlqConfig = x.DlqConfig,
                        IsActive = x.IsActive,
                        Description = x.Description,
                        CreatedAt = x.CreatedAt,
                        CreatedBy = x.CreatedBy,
                        UpdatedAt = x.UpdatedAt,
                        UpdatedBy = x.UpdatedBy
                    })
                    .ToListAsync();

                response.StatusCode = "200";
                response.StatusMessage = "Kafka configurations fetched successfully.";
                response.GetAllData = data;
            }
            catch (Exception ex)
            {
                response.StatusCode = "500";
                response.StatusMessage = $"Failed to fetch Kafka configurations: {ex.Message}";
            }
            return response;
        }

        public async Task<GetSpecificRecord<KafkaConfigurationDto>> GetByIdAsync(int id, int tenantId)
        {
            var response = new GetSpecificRecord<KafkaConfigurationDto>();
            try
            {
                using var db = _tenantDb.GetTenantDbContext(tenantId);
                var data = await db.KafkaConfigurations
                    .AsNoTracking()
                    .Where(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted)
                    .Select(x => new KafkaConfigurationDto
                    {
                        Id = x.Id,
                        TenantId = x.TenantId,
                        Name = x.Name,
                        Environment = x.Environment,
                        BootstrapServers = x.BootstrapServers,
                        GroupId = x.GroupId,
                        TopicPattern = x.TopicPattern,
                        UsePerTenantTopic = x.UsePerTenantTopic,
                        EnableAutoOffsetStore = x.EnableAutoOffsetStore,
                        EnableAutoCommit = x.EnableAutoCommit,
                        SecurityProtocol = x.SecurityProtocol,
                        SaslMechanism = x.SaslMechanism,
                        SaslUsername = x.SaslUsername,
                        SaslPassword = x.SaslPassword,
                        ProducerConfig = x.ProducerConfig,
                        ConsumerConfig = x.ConsumerConfig,
                        DlqConfig = x.DlqConfig,
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
                    response.StatusMessage = "Kafka configuration not found.";
                    return response;
                }

                response.StatusCode = "200";
                response.StatusMessage = "Kafka configuration fetched successfully.";
                response.Data = data;
            }
            catch (Exception ex)
            {
                response.StatusCode = "500";
                response.StatusMessage = $"Failed to fetch Kafka configuration: {ex.Message}";
            }
            return response;
        }

        public async Task<CommonResponseModel> CreateAsync(KafkaConfigurationCreateDto dto)
        {
            var response = new CommonResponseModel();
            try
            {
                using var db = _tenantDb.GetTenantDbContext(dto.TenantId);

                var entity = new KafkaConfigurations
                {
                    TenantId = dto.TenantId,
                    Name = dto.Name,
                    Environment = dto.Environment,
                    BootstrapServers = dto.BootstrapServers,
                    GroupId = dto.GroupId,
                    TopicPattern = dto.TopicPattern,
                    UsePerTenantTopic = dto.UsePerTenantTopic,
                    EnableAutoOffsetStore = dto.EnableAutoOffsetStore,
                    EnableAutoCommit = dto.EnableAutoCommit,
                    SecurityProtocol = dto.SecurityProtocol,
                    SaslMechanism = dto.SaslMechanism,
                    SaslUsername = dto.SaslUsername,
                    SaslPassword = dto.SaslPassword,
                    ProducerConfig = ParseOrEmpty(dto.ProducerConfigJson),
                    ConsumerConfig = ParseOrEmpty(dto.ConsumerConfigJson),
                    DlqConfig = ParseOrEmpty(dto.DlqConfigJson),
                    IsActive = dto.IsActive,
                    Description = dto.Description,
                    CreatedBy = dto.CreatedBy,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await db.KafkaConfigurations.AddAsync(entity);
                await db.SaveChangesAsync();

                response.StatusCode = "200";
                response.StatusMessage = "Kafka configuration created successfully.";
            }
            catch (Exception ex)
            {
                response.StatusCode = "500";
                response.StatusMessage = $"Failed to create Kafka configuration: {ex.Message}";
            }
            return response;
        }

        public async Task<CommonResponseModel> UpdateAsync(KafkaConfigurationUpdateDto dto)
        {
            var response = new CommonResponseModel();
            try
            {
                using var db = _tenantDb.GetTenantDbContext(dto.TenantId);
                var entity = await db.KafkaConfigurations
                    .FirstOrDefaultAsync(x => x.Id == dto.Id && x.TenantId == dto.TenantId && !x.IsDeleted);

                if (entity == null)
                {
                    response.StatusCode = "404";
                    response.StatusMessage = "Kafka configuration not found.";
                    return response;
                }
                entity.Name = dto.Name;
                entity.Environment = dto.Environment;
                entity.BootstrapServers = dto.BootstrapServers;
                entity.GroupId = dto.GroupId;
                entity.TopicPattern = dto.TopicPattern;
                entity.UsePerTenantTopic = dto.UsePerTenantTopic;
                entity.EnableAutoOffsetStore = dto.EnableAutoOffsetStore;
                entity.EnableAutoCommit = dto.EnableAutoCommit;
                entity.SecurityProtocol = dto.SecurityProtocol;
                entity.SaslMechanism = dto.SaslMechanism;
                entity.SaslUsername = dto.SaslUsername;
                entity.SaslPassword = dto.SaslPassword;
                if (dto.ProducerConfigJson != null) entity.ProducerConfig = ParseOrEmpty(dto.ProducerConfigJson);
                if (dto.ConsumerConfigJson != null) entity.ConsumerConfig = ParseOrEmpty(dto.ConsumerConfigJson);
                if (dto.DlqConfigJson != null) entity.DlqConfig = ParseOrEmpty(dto.DlqConfigJson);
                entity.IsActive = dto.IsActive;
                entity.Description = dto.Description;
                entity.UpdatedBy = dto.UpdatedBy;
                entity.UpdatedAt = DateTime.UtcNow;

                await db.SaveChangesAsync();

                response.StatusCode = "200";
                response.StatusMessage = "Kafka configuration updated successfully.";
            }
            catch (Exception ex)
            {
                response.StatusCode = "500";
                response.StatusMessage = $"Failed to update Kafka configuration: {ex.Message}";
            }
            return response;
        }

        public async Task<CommonResponseModel> DeleteAsync(int id, int tenantId)
        {
            var response = new CommonResponseModel();
            try
            {
                using var db = _tenantDb.GetTenantDbContext(tenantId);
                var entity = await db.KafkaConfigurations
                    .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted);

                if (entity == null)
                {
                    response.StatusCode = "404";
                    response.StatusMessage = "Kafka configuration not found.";
                    return response;
                }

                entity.IsDeleted = true;
                entity.IsActive = false;
                entity.UpdatedAt = DateTime.UtcNow;

                await db.SaveChangesAsync();

                response.StatusCode = "200";
                response.StatusMessage = "Kafka configuration deleted successfully.";
            }
            catch (Exception ex)
            {
                response.StatusCode = "500";
                response.StatusMessage = $"Failed to delete Kafka configuration: {ex.Message}";
            }
            return response;
        }
    }
}
