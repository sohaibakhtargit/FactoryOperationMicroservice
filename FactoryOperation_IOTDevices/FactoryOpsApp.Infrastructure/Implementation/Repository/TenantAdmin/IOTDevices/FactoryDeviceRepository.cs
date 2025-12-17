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
    public class FactoryDeviceRepository : IFactoryDeviceRepository
    {
        private readonly MasterFactoryOpsDbContext _masterDbContext;
        private readonly TenantDbContextFactory _tenantDbContext;
        private readonly IAuditLogService _auditLogger;
        private readonly IExceptionLoggerService _exceptionLogger;

        public FactoryDeviceRepository(TenantDbContextFactory tenantDbContext,
                                       IAuditLogService auditLogger,
                                       IExceptionLoggerService exceptionLogger,
                                       MasterFactoryOpsDbContext masterDbContext)
        {
            _tenantDbContext = tenantDbContext;
            _auditLogger = auditLogger;
            _exceptionLogger = exceptionLogger;
            _masterDbContext = masterDbContext;
        }

        public async Task<CommonResponseModel> AddDeviceAsync(DeviceDto dto)
        {
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(dto.TenantId);
                bool exists = await tenantDb.FactoryDevices
            .AnyAsync(d => d.DeviceCode == dto.DeviceCode && d.IsActive && !d.IsDeleted);

                if (exists)
                {
                    return new CommonResponseModel
                    {
                        StatusCode = StatusCode.BadRequest,
                        StatusMessage = $"{FactoryDeviceStatusMessage.AlreadyExists}: {dto.DeviceCode}"
                    };
                }
                var entity = new FactoryDevice
                {
                    TenantId = dto.TenantId,
                    DeviceName = dto.DeviceName,
                    DeviceCode = dto.DeviceCode,
                    Category = dto.Category,
                    Status = DeviceStatusEnum.Offline,
                    LastSeen = dto.LastSeen,
                    LocationId = dto.LocationId,
                    GroupId = dto.GroupId,
                    DataFormat = dto.DataFormat,
                    PhysicalLocation = dto.PhysicalLocation,
                    Description = dto.Description,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = dto.CreatedBy,
                    DeviceTopics = new List<FactoryDeviceTopic>()
                };

                if (dto.TopicIds != null)
                {
                    foreach (var topicId in dto.TopicIds)
                    {
                        entity.DeviceTopics.Add(new FactoryDeviceTopic
                        {
                            TenantId = dto.TenantId,
                            DeviceId = entity.DeviceId,
                            TopicId = topicId,
                            IsActive = true,
                            IsDeleted = false,
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }

                await tenantDb.FactoryDevices.AddAsync(entity);
                await tenantDb.SaveChangesAsync();

                await _auditLogger.LogAuditAsync("Create", $"Created Device '{entity.DeviceName}'", dto.TenantId, "", "AddDeviceAsync");

                return new CommonResponseModel { StatusCode = StatusCode.Success, StatusMessage = FactoryDeviceStatusMessage.AddSuccess };
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "Device-Module", "AddDeviceAsync", dto.TenantId, null);
                return new CommonResponseModel { StatusCode = StatusCode.Error, StatusMessage = $"{FactoryDeviceStatusMessage.AddFailed}: {ex.Message}" };
            }
        }
        public async Task<CommonResponseModel> UpdateDeviceAsync(DeviceDto dto)
        {
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(dto.TenantId);

                var entity = await tenantDb.FactoryDevices
                    .Include(d => d.DeviceTopics)
                    .FirstOrDefaultAsync(d => d.DeviceId == dto.DeviceId && !d.IsDeleted);

                if (entity == null)
                    return new CommonResponseModel { StatusCode = StatusCode.NotFound, StatusMessage = FactoryDeviceStatusMessage.NotFound };

                bool duplicateCodeExists = await tenantDb.FactoryDevices
                    .AnyAsync(d => d.DeviceCode == dto.DeviceCode && d.DeviceId != dto.DeviceId && d.IsActive && !d.IsDeleted);

                if (duplicateCodeExists)
                    return new CommonResponseModel
                    {
                        StatusCode = StatusCode.BadRequest,
                        StatusMessage = $"{FactoryDeviceStatusMessage.DuplicateCode}: {dto.DeviceCode}"
                    };

                entity.DeviceName = dto.DeviceName;
                entity.DeviceCode = dto.DeviceCode;
                entity.Category = dto.Category;
                entity.Status = dto.Status;
                entity.LastSeen = dto.LastSeen;
                entity.LocationId = dto.LocationId;
                entity.GroupId = dto.GroupId;
                entity.DataFormat = dto.DataFormat;
                entity.PhysicalLocation = dto.PhysicalLocation;
                entity.Description = dto.Description;
                entity.UpdatedAt = DateTime.UtcNow;
                entity.UpdatedBy = dto.UpdatedBy;

                var existingTopics = entity.DeviceTopics.ToList();

                if (dto.TopicIds != null && dto.TopicIds.Any())
                {
                    foreach (var topicId in dto.TopicIds)
                    {
                        var existingMapping = existingTopics.FirstOrDefault(dt => dt.TopicId == topicId);
                        if (existingMapping != null)
                        {
                            if (existingMapping.IsDeleted || !existingMapping.IsActive)
                            {
                                existingMapping.IsDeleted = false;
                                existingMapping.IsActive = true;
                                existingMapping.UpdatedAt = DateTime.UtcNow;
                            }
                        }
                        else
                        {
                            var newDeviceTopic = new FactoryDeviceTopic
                            {
                                TenantId = dto.TenantId,
                                DeviceId = entity.DeviceId,
                                TopicId = topicId,
                                IsActive = true,
                                IsDeleted = false,
                                CreatedAt = DateTime.UtcNow
                            };
                            await tenantDb.FactoryDeviceTopics.AddAsync(newDeviceTopic);
                        }
                    }

                    foreach (var existing in existingTopics)
                    {
                        if (!dto.TopicIds.Contains(existing.TopicId) && existing.IsActive && !existing.IsDeleted)
                        {
                            existing.IsActive = false;
                            existing.IsDeleted = true;
                            existing.UpdatedAt = DateTime.UtcNow;
                        }
                    }
                }

                await tenantDb.SaveChangesAsync();
                await _auditLogger.LogAuditAsync("Update", $"Updated Device '{entity.DeviceName}'", dto.TenantId, "", "UpdateDeviceAsync");

                return new CommonResponseModel { StatusCode = StatusCode.Success, StatusMessage = FactoryDeviceStatusMessage.UpdateSuccess };
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "Device-Module", "UpdateDeviceAsync", dto.TenantId, null);
                return new CommonResponseModel { StatusCode = StatusCode.Error, StatusMessage = $"{FactoryDeviceStatusMessage.UpdateFailed}: {ex.Message}" };
            }
        }
        public async Task<CommonResponseModel> DeleteDeviceAsync(int deviceId, int tenantId)
        {
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                var entity = await tenantDb.FactoryDevices
                    .Include(d => d.DeviceTopics)
                    .FirstOrDefaultAsync(d => d.DeviceId == deviceId && !d.IsDeleted);

                if (entity == null)
                    return new CommonResponseModel { StatusCode = StatusCode.NotFound, StatusMessage = FactoryDeviceStatusMessage.NotFound };

                entity.IsDeleted = true;
                entity.IsActive = false;
                entity.DeletedAt = DateTime.UtcNow;
                entity.DeletedBy = tenantId;

                foreach (var mapping in entity.DeviceTopics)
                {
                    mapping.IsDeleted = true;
                    mapping.IsActive = false;
                    mapping.UpdatedAt = DateTime.UtcNow;
                }

                await tenantDb.SaveChangesAsync();

                await _auditLogger.LogAuditAsync(
                    "Delete",
                    $"Deleted Device '{entity.DeviceName}'",
                    tenantId,
                    "",
                    "DeleteDeviceAsync"
                );

                return new CommonResponseModel { StatusCode = StatusCode.Success, StatusMessage = FactoryDeviceStatusMessage.DeleteSuccess };
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "Device-Module", "DeleteDeviceAsync", tenantId, null);
                return new CommonResponseModel { StatusCode = StatusCode.Error, StatusMessage = $"{FactoryDeviceStatusMessage.DeleteFailed}: {ex.Message}" };
            }
        }
        public async Task<GetAllRecord<GetDeviceDto>> GetAllDevicesAsync(int tenantId)
        {
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                var entities = await tenantDb.FactoryDevices
                    .Where(d => d.TenantId == tenantId && !d.IsDeleted).Include(g => g.Group)
                    .Include(d => d.DeviceTopics)
                        .ThenInclude(dt => dt.Topic)
                    .Include(d => d.Location)
                        .ThenInclude(l => l.ParentLocation)
                    .ToListAsync();

                var dtoList = entities.Select(d => new GetDeviceDto
                {
                    DeviceId = d.DeviceId,
                    TenantId = d.TenantId,
                    DeviceName = d.DeviceName,
                    DeviceCode = d.DeviceCode,
                    Category = d.Category,
                    Status = d.Status,
                    LastSeen = d.LastSeen,
                    LocationId = d.LocationId,
                    Location = d.Location?.LocationName,
                    ParentLocationId = d.Location?.ParentLocationId,
                    ParentLocationName = d.Location?.ParentLocation?.LocationName,
                    GroupId = d.GroupId,
                    GroupName = d.Group.Name,
                    DataFormat = d.DataFormat,
                    PhysicalLocation = d.PhysicalLocation,
                    Description = d.Description,
                    IsActive = d.IsActive,
                    IsDeleted = d.IsDeleted,
                    CreatedBy = d.CreatedBy,
                    CreatedAt = d.CreatedAt,
                    UpdatedBy = d.UpdatedBy,
                    UpdatedAt = d.UpdatedAt,
                    Topics = d.DeviceTopics
                        .Where(u => u.Topic != null)
                        .Select(u => new TopicMap
                        {
                            TopicId = u.TopicId,
                            TopicName = u.Topic.Name
                        })
                        .ToList()
                }).ToList();

                return new GetAllRecord<GetDeviceDto>
                {
                    StatusCode = StatusCode.Success,
                    StatusMessage = FactoryDeviceStatusMessage.FetchAllSuccess,
                    GetAllData = dtoList
                };
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "Device-Module", "GetAllDevicesAsync", tenantId, null);
                return new GetAllRecord<GetDeviceDto>
                {
                    StatusCode = StatusCode.Error,
                    StatusMessage = $"{FactoryDeviceStatusMessage.FetchAllFailed}: {ex.Message}"
                };
            }
        }
        public async Task<GetSpecificRecord<GetDeviceDto>> GetDeviceByIdAsync(int deviceId, int tenantId)
        {
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                var entity = await tenantDb.FactoryDevices
                    .Include(d => d.DeviceTopics)
                        .ThenInclude(dt => dt.Topic)
                    .Include(d => d.Location)
                        .ThenInclude(l => l.ParentLocation)
                    .FirstOrDefaultAsync(d => d.DeviceId == deviceId && !d.IsDeleted);

                if (entity == null)
                    return new GetSpecificRecord<GetDeviceDto>
                    {
                        StatusCode = StatusCode.NotFound,
                        StatusMessage = FactoryDeviceStatusMessage.NotFound
                    };

                var dto = new GetDeviceDto
                {
                    DeviceId = entity.DeviceId,
                    TenantId = entity.TenantId,
                    DeviceName = entity.DeviceName,
                    DeviceCode = entity.DeviceCode,
                    Category = entity.Category,
                    Status = entity.Status,
                    LastSeen = entity.LastSeen,
                    LocationId = entity.LocationId,
                    Location = entity.Location?.LocationName,
                    ParentLocationId = entity.Location?.ParentLocationId,
                    ParentLocationName = entity.Location?.ParentLocation?.LocationName,
                    GroupId = entity.GroupId,
                    DataFormat = entity.DataFormat,
                    PhysicalLocation = entity.PhysicalLocation,
                    Description = entity.Description,
                    IsActive = entity.IsActive,
                    IsDeleted = entity.IsDeleted,
                    CreatedBy = entity.CreatedBy,
                    CreatedAt = entity.CreatedAt,
                    UpdatedBy = entity.UpdatedBy,
                    UpdatedAt = entity.UpdatedAt,
                    Topics = entity.DeviceTopics
                        .Where(u => u.Topic != null)
                        .Select(u => new TopicMap
                        {
                            TopicId = u.TopicId,
                            TopicName = u.Topic.Name
                        })
                        .ToList()
                };

                return new GetSpecificRecord<GetDeviceDto>
                {
                    StatusCode = StatusCode.Success,
                    StatusMessage = FactoryDeviceStatusMessage.FetchByIdSuccess,
                    Data = dto
                };
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "Device-Module", "GetDeviceByIdAsync", tenantId, null);
                return new GetSpecificRecord<GetDeviceDto>
                {
                    StatusCode = StatusCode.Error,
                    StatusMessage = $"{FactoryDeviceStatusMessage.FetchByIdFailed}: {ex.Message}"
                };
            }
        }
        public async Task<GetAllRecord<GetDeviceDto>> GetAllDevicesFromAllTenantsAsync(int deviceId, string deviceCode)
        {
            var response = new GetAllRecord<GetDeviceDto>();

            try
            {
                var tenants = await _masterDbContext.FactoryTenants
                    .Where(t => !t.IsDeleted && t.IsActive)
                    .ToListAsync();

                var tasks = tenants.Select(async tenant =>
                {
                    try
                    {
                        using var tenantDb = _tenantDbContext.GetTenantDbContext(tenant.TenantId);

                        var devices = await tenantDb.FactoryDevices
                            .AsNoTracking()
                            .Where(d => !d.IsDeleted && d.DeviceId == deviceId && d.DeviceCode == deviceCode)
                            .Include(d => d.Group)
                            .Include(d => d.Location)
                                .ThenInclude(l => l.ParentLocation)
                            .Include(d => d.DeviceTopics)
                                .ThenInclude(dt => dt.Topic)
                            .ToListAsync();

                        return devices.Select(d => new GetDeviceDto
                        {
                            DeviceId = d.DeviceId,
                            TenantId = tenant.TenantId,
                            DeviceName = d.DeviceName,
                            DeviceCode = d.DeviceCode,
                            Category = d.Category,
                            Status = d.Status,
                            LastSeen = d.LastSeen,
                            LocationId = d.LocationId,
                            Location = d.Location?.LocationName,
                            ParentLocationId = d.Location?.ParentLocationId,
                            ParentLocationName = d.Location?.ParentLocation?.LocationName,
                            GroupId = d.GroupId,
                            GroupName = d.Group?.Name,
                            DataFormat = d.DataFormat,
                            PhysicalLocation = d.PhysicalLocation,
                            Description = d.Description,
                            IsActive = d.IsActive,
                            IsDeleted = d.IsDeleted,
                            CreatedBy = d.CreatedBy,
                            CreatedAt = d.CreatedAt,
                            UpdatedBy = d.UpdatedBy,
                            UpdatedAt = d.UpdatedAt,
                            Topics = d.DeviceTopics
                                .Where(dt => dt.Topic != null)
                                .Select(dt => new TopicMap
                                {
                                    TopicId = dt.TopicId,
                                    TopicName = dt.Topic.Name
                                })
                                .ToList()
                        }).ToList();
                    }
                    catch (Exception)
                    {
                        return new List<GetDeviceDto>();
                    }
                });

                var allDevices = (await Task.WhenAll(tasks)).SelectMany(d => d).ToList();

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = FactoryDeviceStatusMessage.FetchAllSuccess;
                response.GetAllData = allDevices;
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(
                    ex,
                    sourceModule: "Device-Module",
                    apiName: "GetAllDevicesFromAllTenantsAsync",
                    tenantId: null,
                    userId: null
                );

                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"{FactoryDeviceStatusMessage.FetchAllFailed}: {ex.Message}";
            }

            return response;
        }

    }
}