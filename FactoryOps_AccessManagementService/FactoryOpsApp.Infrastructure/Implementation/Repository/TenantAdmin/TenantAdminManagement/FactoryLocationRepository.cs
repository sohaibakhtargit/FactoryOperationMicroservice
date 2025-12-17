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
    public class FactoryLocationRepository : IFactoryLocationRepository
    {
        private readonly TenantDbContextFactory _tenantDbContext;
        private readonly IAuditLogService _auditLogger;
        private readonly IExceptionLoggerService _exceptionLogger;

        public FactoryLocationRepository(TenantDbContextFactory tenantDbContext,
                                  IAuditLogService auditLogger,
                                  IExceptionLoggerService exceptionLogger)
        {
            _tenantDbContext = tenantDbContext;
            _auditLogger = auditLogger;
            _exceptionLogger = exceptionLogger;
        }

        public async Task<CommonResponseModel> AddLocationAsync(LocationDto dto)
        {
            using var tenantDb = _tenantDbContext.GetTenantDbContext(dto.TenantId);
            try
            {
                if (await tenantDb.Locations.AnyAsync(l => l.TenantId == dto.TenantId &&
                                                        l.ParentLocationId == dto.ParentLocationId &&
                                                        !l.IsDeleted &&
                                                        l.LocationName.ToLower() == dto.LocationName.ToLower()))
                {
                    return new CommonResponseModel
                    {
                        StatusCode = StatusCode.BadRequest,
                        StatusMessage = $"Location '{dto.LocationName}' {FactoryLocationStatusMessage.LocationAlreadyExists}"
                    };
                }

                var entity = new Location
                {
                    TenantId = dto.TenantId,
                    LocationName = dto.LocationName,
                    LocationType = dto.LocationType,
                    ParentLocationId = dto.ParentLocationId,
                    Description = dto.Description,
                    CreatedBy = dto.CreatedBy,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                    IsDeleted = false
                };

                tenantDb.Locations.Add(entity);
                await tenantDb.SaveChangesAsync();

                await _auditLogger.LogAuditAsync(
                    "Create",
                    $"Created Location '{entity.LocationName}'",
                    dto.TenantId,
                    "",
                    "CreateLocation");

                return new CommonResponseModel { StatusCode = StatusCode.Success, StatusMessage = FactoryLocationStatusMessage.LocationAdded };
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "Location-Module", "AddLocation", dto.TenantId, null);
                return new CommonResponseModel { StatusCode = StatusCode.Error, StatusMessage = $"{FactoryLocationStatusMessage.LocationAddFailed}: {ex.Message}" };
            }
        }
        public async Task<CommonResponseModel> UpdateLocationAsync(LocationDto dto)
        {
            using var tenantDb = _tenantDbContext.GetTenantDbContext(dto.TenantId);
            try
            {
                var entity = await tenantDb.Locations.FirstOrDefaultAsync(l => l.LocationId == dto.LocationId && !l.IsDeleted);

                if (entity == null)
                    return new CommonResponseModel { StatusCode = StatusCode.NotFound, StatusMessage = FactoryLocationStatusMessage.LocationNotFound };

                if (await tenantDb.Locations.AnyAsync(l => l.TenantId == dto.TenantId
                                                        && l.ParentLocationId == dto.ParentLocationId
                                                        && !l.IsDeleted
                                                        && l.LocationName.ToLower() == dto.LocationName.ToLower()
                                                        && l.LocationId != dto.LocationId))
                {
                    return new CommonResponseModel
                    {
                        StatusCode = StatusCode.BadRequest,
                        StatusMessage = $"Location '{dto.LocationName}' {FactoryLocationStatusMessage.LocationAlreadyExists}"
                    };
                }

                entity.LocationName = dto.LocationName;
                entity.LocationType = dto.LocationType;
                entity.ParentLocationId = dto.ParentLocationId;
                entity.Description = dto.Description;
                entity.UpdatedBy = dto.UpdatedBy;
                entity.UpdatedAt = DateTime.UtcNow;

                await tenantDb.SaveChangesAsync();

                await _auditLogger.LogAuditAsync("Update",
                    $"Updated Location '{entity.LocationName}'",
                    dto.TenantId,
                    "",
                    "UpdateLocation");

                return new CommonResponseModel { StatusCode = StatusCode.Success, StatusMessage = FactoryLocationStatusMessage.LocationUpdated };
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "Location-Module", "UpdateLocation", dto.TenantId, null);
                return new CommonResponseModel { StatusCode = StatusCode.Error, StatusMessage = $"{FactoryLocationStatusMessage.LocationUpdateFailed}: {ex.Message}" };
            }
        }
        public async Task<CommonResponseModel> DeleteLocationAsync(int TenantId, int LocationId)
        {
            using var tenantDb = _tenantDbContext.GetTenantDbContext(TenantId);
            try
            {
                var entity = await tenantDb.Locations.FirstOrDefaultAsync(l => l.LocationId == LocationId && !l.IsDeleted);

                if (entity == null)
                    return new CommonResponseModel { StatusCode = StatusCode.NotFound, StatusMessage = FactoryLocationStatusMessage.LocationNotFound };

                entity.IsDeleted = true;
                entity.IsActive = false;
                entity.DeletedAt = DateTime.UtcNow;
                entity.DeletedBy = TenantId;

                await tenantDb.SaveChangesAsync();

                await _auditLogger.LogAuditAsync("Delete",
                    $"Deleted Location '{entity.LocationName}'",
                    TenantId,
                    "",
                    "DeleteLocation");

                return new CommonResponseModel { StatusCode = StatusCode.Success, StatusMessage = FactoryLocationStatusMessage.LocationDeleted };
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "Location-Module", "DeleteLocation", TenantId, null);
                return new CommonResponseModel { StatusCode = StatusCode.Error, StatusMessage = $"{FactoryLocationStatusMessage.LocationDeleteFailed}: {ex.Message}" };
            }
        }
        public async Task<GetSpecificRecord<LocationDto>> GetLocationByIdAsync(int tenantId, int locationId)
        {
            using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);
            try
            {
                var entity = await tenantDb.Locations
                    .Include(l => l.ChildLocations)
                    .Include(l => l.ParentLocation)
                    .FirstOrDefaultAsync(l => l.LocationId == locationId && !l.IsDeleted);

                if (entity == null)
                    return new GetSpecificRecord<LocationDto>
                    {
                        StatusCode = StatusCode.NotFound,
                        StatusMessage = FactoryLocationStatusMessage.LocationNotFound
                    };

                var dto = new LocationDto
                {
                    LocationId = entity.LocationId,
                    TenantId = entity.TenantId,
                    LocationName = entity.LocationName,
                    LocationType = entity.LocationType,
                    ParentLocationId = entity.ParentLocationId,
                    Description = entity.Description,
                    IsActive = entity.IsActive,
                    IsDeleted = entity.IsDeleted,
                    CreatedBy = entity.CreatedBy,
                    CreatedAt = entity.CreatedAt
                };

                return new GetSpecificRecord<LocationDto>
                {
                    StatusCode = StatusCode.Success,
                    StatusMessage = FactoryLocationStatusMessage.LocationFetched,
                    Data = dto
                };
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "Location-Module", "GetLocationById", tenantId, null);
                return new GetSpecificRecord<LocationDto>
                {
                    StatusCode = StatusCode.Error,
                    StatusMessage = $"{FactoryLocationStatusMessage.LocationFetchFailed}: {ex.Message}"
                };
            }
        }
        public async Task<GetAllRecord<LocationDto>> GetAllLocationsAsync(int tenantId)
        {
            using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);
            try
            {
                var entities = await tenantDb.Locations
                    .Include(l => l.ChildLocations)
                    .Include(l => l.ParentLocation)
                    .Where(l => !l.IsDeleted)
                    .ToListAsync();

                var dtoList = entities.Select(e => new LocationDto
                {
                    LocationId = e.LocationId,
                    TenantId = e.TenantId,
                    LocationName = e.LocationName,
                    LocationType = e.LocationType,
                    ParentLocationId = e.ParentLocationId,
                    ParentLocationName = e.ParentLocation != null ? e.ParentLocation.LocationName : null,
                    Description = e.Description,
                    IsActive = e.IsActive,
                    IsDeleted = e.IsDeleted,
                    CreatedBy = e.CreatedBy,
                    CreatedAt = e.CreatedAt
                }).ToList();

                return new GetAllRecord<LocationDto>
                {
                    StatusCode = StatusCode.Success,
                    StatusMessage = FactoryLocationStatusMessage.LocationsFetched,
                    GetAllData = dtoList,
                };
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "Location-Module", "GetAllLocations", tenantId, null);
                return new GetAllRecord<LocationDto>
                {
                    StatusCode = StatusCode.Error,
                    StatusMessage = $"{FactoryLocationStatusMessage.LocationsFetchFailed}: {ex.Message}"
                };
            }
        }

        public async Task<LocationResponse> GetLocationWithChildrenAsync(int tenantId, int selectedLocationId)
        {
            using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

            try
            {
                var allLocations = await tenantDb.Locations
                    .Where(l => l.TenantId == tenantId && !l.IsDeleted)
                    .ToListAsync();

                List<LocationSimpleDto> GetChildren(int parentId, HashSet<int> visited)
                {
                    if (visited.Contains(parentId))
                        return new List<LocationSimpleDto>();

                    visited.Add(parentId);

                    return allLocations
                        .Where(l => l.ParentLocationId == parentId && l.LocationId != parentId)
                        .Select(l => new LocationSimpleDto
                        {
                            LocationId = l.LocationId,
                            LocationName = l.LocationName,
                            ParentLocationId = l.ParentLocationId,
                            ParentLocationName = allLocations.FirstOrDefault(p => p.LocationId == l.ParentLocationId)?.LocationName,
                            IsActive = l.IsActive,
                            IsDeleted = l.IsDeleted,
                            ChildLocations = GetChildren(l.LocationId, visited)
                        })
                        .ToList();
                }

                var childLocations = GetChildren(selectedLocationId, new HashSet<int>());

                return new LocationResponse
                {
                    StatusCode = StatusCode.Success,
                    StatusMessage = FactoryLocationStatusMessage.LocationsFetched,
                    ChildLocations = childLocations
                };
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "Location-Module", "GetLocationWithChildren", tenantId, null);
                return new LocationResponse
                {
                    StatusCode = StatusCode.Error,
                    StatusMessage = $"{FactoryLocationStatusMessage.LocationsFetchFailed}: {ex.Message}"
                };
            }
        }

    }

}