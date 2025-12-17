using FactoryOperation_IOTDevices.FactoryOpsApp.Application.Common;
using FactoryOperation_IOTDevices.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.IOTDevices;
using FactoryOperation_IOTDevices.FactoryOpsApp.Application.Interfaces.Services.SuperAdmin.AuditLogs;
using FactoryOps_IOTDeviceService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.ExceptionLogger;
using FactoryOpsApp.Application.DTOs;

using FactoryOpsApp.Domain.Entities.FactoryOpsTenants;
using FactoryOpsApp.Infrastructure.DBContext;
using Microsoft.EntityFrameworkCore;
using static FactoryOperation_IOTDevices.FactoryOpsApp.Common.CommonConstant;


namespace FactoryOperation_IOTDevices.FactoryOpsApp.Infrastructure.Implementation.Repository.TenantAdmin.IOTDevices
{
    public class FactoryMqttTopicRepository : IFactoryMqttTopicRepository
    {
        private readonly TenantDbContextFactory _tenantDbContext;
        private readonly IAuditLogService _auditLogger;
        private readonly IExceptionLoggerService _exceptionLogger;

        public FactoryMqttTopicRepository(
            TenantDbContextFactory tenantDbContext,
            IAuditLogService auditLogger,
            IExceptionLoggerService exceptionLogger)
        {
            _tenantDbContext = tenantDbContext;
            _auditLogger = auditLogger;
            _exceptionLogger = exceptionLogger;
        }

        public async Task<CommonResponseModel> AddMqttTopicAsync(MqttTopicDto dto)
        {
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(dto.TenantId);

                var entity = new FactoryMqttTopic
                {
                    Name = dto.Name,
                    TenantId = dto.TenantId,
                    MqttPath = dto.MqttPath,
                    Type = dto.Type,
                    QoS = dto.QoS,
                    Description = dto.Description,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = dto.TenantId
                };

                if (dto.SchemaDefinitions != null && dto.SchemaDefinitions.Any())
                {
                    int order = 0;
                    foreach (var schemaDto in dto.SchemaDefinitions)
                    {
                        entity.SchemaDefinitions.Add(new TopicSchemaDefinition
                        {
                            TenantId = dto.TenantId,
                            KeyName = schemaDto.KeyName,
                            DataType = schemaDto.DataType,
                            DisplayOrder = order++,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        });
                    }
                }

                await tenantDb.FactoryMqttTopics.AddAsync(entity);
                await tenantDb.SaveChangesAsync();

                await _auditLogger.LogAuditAsync(
                    "Create",
                    $"Created MQTT Topic '{entity.Name}' with {entity.SchemaDefinitions.Count} schema definitions",
                    dto.TenantId,
                    "",
                    "AddMqttTopicAsync"
                );

                return new CommonResponseModel
                {
                    StatusCode = StatusCode.Success,
                    StatusMessage = MqttTopicStatusMessage.AddSuccess
                };
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(
                    ex,
                    "MqttTopic-Module",
                    "AddMqttTopicAsync",
                    dto.TenantId,
                    null
                );

                return new CommonResponseModel
                {
                    StatusCode = StatusCode.Error,
                    StatusMessage = $"{MqttTopicStatusMessage.AddFailed}: {ex.Message}"
                };
            }
        }
        public async Task<CommonResponseModel> UpdateMqttTopicAsync(MqttTopicDto dto)
        {
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(dto.TenantId);
                using var transaction = await tenantDb.Database.BeginTransactionAsync();

                try
                {
                    var entity = await tenantDb.FactoryMqttTopics
                        .Include(t => t.SchemaDefinitions)
                        .FirstOrDefaultAsync(t => t.TopicId == dto.TopicId && !t.IsDeleted);

                    if (entity == null)
                        return new CommonResponseModel { StatusCode = StatusCode.NotFound, StatusMessage = MqttTopicStatusMessage.NotFound };

                    entity.Name = dto.Name;
                    entity.MqttPath = dto.MqttPath;
                    entity.Type = dto.Type;
                    entity.QoS = dto.QoS;
                    entity.Description = dto.Description;
                    entity.UpdatedAt = DateTime.UtcNow;
                    entity.UpdatedBy = dto.TenantId;

                    if (dto.SchemaDefinitions != null)
                    {
                        var existingSchemas = entity.SchemaDefinitions.ToList();
                        var existingSchemaKeys = existingSchemas.Select(sd => sd.KeyName).ToList();
                        var newSchemaKeys = dto.SchemaDefinitions.Select(sd => sd.KeyName).ToList();

                        int order = 0;

                        foreach (var existingSchema in existingSchemas)
                        {
                            var matchingDto = dto.SchemaDefinitions.FirstOrDefault(sd => sd.KeyName == existingSchema.KeyName);

                            if (matchingDto == null)
                            {
                                if (!existingSchema.IsDeleted)
                                {
                                    existingSchema.IsDeleted = true;
                                    existingSchema.DeletedAt = DateTime.UtcNow;
                                    existingSchema.DeletedBy = dto.TenantId;
                                    existingSchema.UpdatedAt = DateTime.UtcNow;
                                }
                            }
                            else
                            {
                                if (existingSchema.IsDeleted)
                                {
                                    existingSchema.IsDeleted = false;
                                    existingSchema.DeletedAt = null;
                                    existingSchema.DeletedBy = null;
                                    existingSchema.IsActive = true;
                                    existingSchema.UpdatedAt = DateTime.UtcNow;
                                }

                                existingSchema.DataType = matchingDto.DataType;
                                existingSchema.DisplayOrder = order++;
                                existingSchema.UpdatedAt = DateTime.UtcNow;
                                existingSchema.UpdatedBy = dto.TenantId;
                            }
                        }

                        foreach (var schemaDto in dto.SchemaDefinitions)
                        {
                            var existingSchema = existingSchemas.FirstOrDefault(sd =>
                                sd.KeyName == schemaDto.KeyName && !sd.IsDeleted);

                            if (existingSchema == null)
                            {
                                entity.SchemaDefinitions.Add(new TopicSchemaDefinition
                                {
                                    KeyName = schemaDto.KeyName,
                                    DataType = schemaDto.DataType,
                                    TenantId = dto.TenantId,
                                    DisplayOrder = order++,
                                    IsActive = true,
                                    IsDeleted = false,
                                    CreatedAt = DateTime.UtcNow,
                                    CreatedBy = dto.TenantId,
                                    UpdatedAt = DateTime.UtcNow
                                });
                            }
                        }
                    }

                    await tenantDb.SaveChangesAsync();
                    await transaction.CommitAsync();

                    await _auditLogger.LogAuditAsync(
                        "Update",
                        $"Updated MQTT Topic '{entity.Name}' with schema definitions",
                        dto.TenantId,
                        "",
                        "UpdateMqttTopicAsync"
                    );

                    return new CommonResponseModel
                    {
                        StatusCode = StatusCode.Success,
                        StatusMessage = MqttTopicStatusMessage.UpdateSuccess
                    };
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(
                    ex,
                    "MqttTopic-Module",
                    "UpdateMqttTopicAsync",
                    dto.TenantId,
                    null
                );

                return new CommonResponseModel
                {
                    StatusCode = StatusCode.Error,
                    StatusMessage = $"{MqttTopicStatusMessage.UpdateFailed}: {ex.Message}"
                };
            }
        }
        public async Task<CommonResponseModel> DeleteMqttTopicAsync(int topicId, int tenantId)
        {
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                var entity = await tenantDb.FactoryMqttTopics
                    .Include(t => t.SchemaDefinitions)
                    .FirstOrDefaultAsync(t => t.TopicId == topicId && !t.IsDeleted);

                if (entity == null)
                    return new CommonResponseModel { StatusCode = StatusCode.NotFound, StatusMessage = MqttTopicStatusMessage.NotFound };

                entity.IsDeleted = true;
                entity.IsActive = false;
                entity.DeletedAt = DateTime.UtcNow;
                entity.DeletedBy = tenantId;

                await tenantDb.SaveChangesAsync();

                await _auditLogger.LogAuditAsync(
                    "Delete",
                    $"Deleted MQTT Topic '{entity.Name}' with {entity.SchemaDefinitions.Count} schema definitions",
                    tenantId,
                    "",
                    "DeleteMqttTopicAsync"
                );

                return new CommonResponseModel
                {
                    StatusCode = StatusCode.Success,
                    StatusMessage = MqttTopicStatusMessage.DeleteSuccess
                };
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(
                    ex,
                    "MqttTopic-Module",
                    "DeleteMqttTopicAsync",
                    tenantId,
                    null
                );

                return new CommonResponseModel
                {
                    StatusCode = StatusCode.Error,
                    StatusMessage = $"{MqttTopicStatusMessage.DeleteFailed}: {ex.Message}"
                };
            }
        }
        public async Task<GetAllRecord<MqttTopicDto>> GetAllMqttTopicAsync(int tenantId)
        {
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                var entities = await tenantDb.FactoryMqttTopics
                    .Where(t => t.TenantId == tenantId && !t.IsDeleted)
                    .Include(t => t.SchemaDefinitions)
                    .ToListAsync();

                var dtoList = entities.Select(t => new MqttTopicDto
                {
                    TopicId = t.TopicId,
                    Name = t.Name,
                    TenantId = t.TenantId,
                    MqttPath = t.MqttPath,
                    Type = t.Type,
                    QoS = t.QoS,
                    Description = t.Description,
                    SchemaDefinitions = t.SchemaDefinitions.Select(s => new TopicSchemaDefinitionDto
                    {
                        KeyName = s.KeyName,
                        DataType = s.DataType
                    }).ToList()
                }).ToList();

                return new GetAllRecord<MqttTopicDto>
                {
                    StatusCode = dtoList.Any() ? StatusCode.Success : StatusCode.NotFound,
                    StatusMessage = dtoList.Any() ? MqttTopicStatusMessage.FetchAllSuccess : MqttTopicStatusMessage.NotFound,
                    GetAllData = dtoList
                };
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "MqttTopic-Module", "GetAllMqttTopicAsync", tenantId, null);
                return new GetAllRecord<MqttTopicDto>
                {
                    StatusCode = StatusCode.Error,
                    StatusMessage = $"{MqttTopicStatusMessage.FetchAllFailed}: {ex.Message}",
                    GetAllData = new List<MqttTopicDto>()
                };
            }
        }
        public async Task<GetSpecificRecord<FactoryMqttTopic?>> GetMqttTopicByIdAsync(int topicId, int tenantId)
        {
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                var entity = await tenantDb.FactoryMqttTopics
                    .Include(t => t.SchemaDefinitions)
                    .FirstOrDefaultAsync(t => t.TopicId == topicId && !t.IsDeleted);

                if (entity == null)
                    return new GetSpecificRecord<FactoryMqttTopic?>
                    {
                        StatusCode = StatusCode.NotFound,
                        StatusMessage = MqttTopicStatusMessage.NotFound,
                        Data = null
                    };

                return new GetSpecificRecord<FactoryMqttTopic?>
                {
                    StatusCode = StatusCode.Success,
                    StatusMessage = MqttTopicStatusMessage.FetchByIdSuccess,
                    Data = entity
                };
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "MqttTopic-Module", "GetMqttTopicByIdAsync", tenantId, null);
                return new GetSpecificRecord<FactoryMqttTopic?>
                {
                    StatusCode = StatusCode.Error,
                    StatusMessage = $"{MqttTopicStatusMessage.FetchByIdFailed}: {ex.Message}",
                    Data = null
                };
            }
        }
    }
}