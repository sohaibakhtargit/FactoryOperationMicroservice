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
    public class AssetTrackingRepository : IAsssetTrackingRepository
    {
        private readonly TenantDbContextFactory _tenantDbContext;
        private readonly IExceptionLoggerService _exceptionLogger;
        private readonly IAuditLogService _auditLogger;

        public AssetTrackingRepository(
            TenantDbContextFactory tenantDbContext,
            IExceptionLoggerService exceptionLogger,
            IAuditLogService auditLogger)
        {
            _tenantDbContext = tenantDbContext;
            _exceptionLogger = exceptionLogger;
            _auditLogger = auditLogger;
        }

        public async Task<CommonResponseModel> CreateAssetTrackingAsync(AssetTrackingCreateDto dto)
        {
            var response = new CommonResponseModel();
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(dto.TenantId);

                var entity = new AssetTracking
                {
                    AssetId = dto.AssetId,
                    CurrentLocation = dto.CurrentLocation,
                    TenantId = dto.TenantId,
                    Status = dto.Status,
                    AssignedTo = dto.AssignedTo,
                    LastMovedOn = dto.LastMovedOn,
                    GpsCoordinates = dto.GpsCoordinates,
                    Remarks = dto.Remarks,
                    CreatedBy = dto.CreatedBy,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsActive = true,
                    IsDeleted = false
                };

                tenantDb.AssetTracking.Add(entity);
                await tenantDb.SaveChangesAsync();

                await _auditLogger.LogAuditAsync("Create", $"Created AssetTracking for AssetId: {dto.AssetId}", dto.TenantId, "", "CreateAssetTracking");

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = AssetTrackingStatusMessage.Created;
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "AssetTracking-Module", "Create-AssetTracking", dto.TenantId, null);
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"{AssetTrackingStatusMessage.CreateFailed}: {ex.Message}";
            }

            return response;
        }
        public async Task<CommonResponseModel> UpdateAssetTrackingAsync(AssetTrackingUpdateDto dto)
        {
            var response = new CommonResponseModel();
            using var tenantDb = _tenantDbContext.GetTenantDbContext(dto.TenantId);
            using var transaction = await tenantDb.Database.BeginTransactionAsync();

            try
            {
                var entity = await tenantDb.AssetTracking
                    .FirstOrDefaultAsync(x => x.TrackingId == dto.TrackingId && !x.IsDeleted);

                if (entity == null)
                {
                    response.StatusCode = StatusCode.NotFound;
                    response.StatusMessage = AssetTrackingStatusMessage.NotFound;
                    return response;
                }

                var previousStatus = entity.Status;

                // -------- BASIC UPDATES --------
                if (dto.AssetId.HasValue)
                    entity.AssetId = dto.AssetId.Value;

                if (dto.CurrentLocation.HasValue)
                    entity.CurrentLocation = dto.CurrentLocation.Value;

                if (dto.AssignedTo.HasValue)
                    entity.AssignedTo = dto.AssignedTo.Value;

                if (dto.LastMovedOn.HasValue)
                    entity.LastMovedOn = dto.LastMovedOn;

                if (!string.IsNullOrEmpty(dto.GpsCoordinates))
                    entity.GpsCoordinates = dto.GpsCoordinates;

                if (!string.IsNullOrEmpty(dto.Remarks))
                    entity.Remarks = dto.Remarks;

                entity.Status = dto.Status;
                entity.TenantId = dto.TenantId;
                entity.UpdatedBy = dto.UpdatedBy;
                entity.UpdatedAt = DateTime.UtcNow;
                entity.StatusUpdatedOn = DateTime.UtcNow;

                // Case 1: Asset just went DOWN
                if (previousStatus != AssetTrackingStatusEnum.Down &&
                    dto.Status == AssetTrackingStatusEnum.Down)
                {
                    entity.DownStartTime = DateTime.UtcNow;
                }

                // Case 2: Asset recovered FROM DOWN
                if (previousStatus == AssetTrackingStatusEnum.Down &&
                    dto.Status != AssetTrackingStatusEnum.Down &&
                    entity.DownStartTime.HasValue)
                {
                    entity.DownEndTime = DateTime.UtcNow;

                    var downMinutes =
                        (entity.DownEndTime.Value - entity.DownStartTime.Value).TotalMinutes;

                    downMinutes = downMinutes > 0 ? downMinutes : 0;

                    entity.TotalDownMinutes = downMinutes;
                    entity.TotalDownAccumulatedMinutes =
                        (entity.TotalDownAccumulatedMinutes ?? 0) + downMinutes;

                    // Reset markers
                    entity.DownStartTime = null;
                    entity.DownEndTime = null;
                }

                // -------- SYNC ASSET LOCATION --------
                if (dto.CurrentLocation.HasValue)
                {
                    var asset = await tenantDb.AssetRegistry
                        .FirstOrDefaultAsync(a => a.AssetId == entity.AssetId && !a.IsDeleted);

                    if (asset != null)
                    {
                        asset.LocationId = dto.CurrentLocation.Value;
                        asset.UpdatedBy = dto.UpdatedBy;
                        asset.UpdatedAt = DateTime.UtcNow;
                    }
                }

                await tenantDb.SaveChangesAsync();
                await transaction.CommitAsync();

                await _auditLogger.LogAuditAsync(
                    "Update",
                    $"Updated AssetTracking Id: {dto.TrackingId}, Status: {previousStatus} → {dto.Status}",
                    dto.TenantId,
                    "",
                    "UpdateAssetTrackingAsync"
                );

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = AssetTrackingStatusMessage.Updated;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _exceptionLogger.LogExceptionAsync(
                    ex,
                    "AssetTracking-Module",
                    "UpdateAssetTrackingAsync",
                    dto.TenantId,
                    null
                );

                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"{AssetTrackingStatusMessage.UpdateFailed}: {ex.Message}";
            }

            return response;
        }

        public async Task<CommonResponseModel> DeleteAssetTrackingAsync(long trackingId, int tenantId)
        {
            var response = new CommonResponseModel();
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                var entity = await tenantDb.AssetTracking
                    .FirstOrDefaultAsync(x => x.TrackingId == trackingId && !x.IsDeleted);

                if (entity == null)
                {
                    response.StatusCode = StatusCode.NotFound;
                    response.StatusMessage = AssetTrackingStatusMessage.NotFound;
                    return response;
                }

                entity.IsDeleted = true;
                entity.IsActive = false;
                entity.DeletedAt = DateTime.UtcNow;
                entity.DeletedBy = entity.UpdatedBy;

                await tenantDb.SaveChangesAsync();

                await _auditLogger.LogAuditAsync("Delete", $"Deleted AssetTracking Id: {trackingId}", tenantId, "", "DeleteAssetTracking");

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = AssetTrackingStatusMessage.Deleted;
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "AssetTracking-Module", "Delete-AssetTracking", tenantId, null);
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"{AssetTrackingStatusMessage.DeleteFailed}: {ex.Message}";
            }

            return response;
        }

        public async Task<GetAllRecord<AssetTrackingDto>> GetAllAssetTrackingsAsync(int tenantId)
        {
            var response = new GetAllRecord<AssetTrackingDto>();
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                var data = await tenantDb.AssetTracking
                    .Where(x => !x.IsDeleted)
                    .Select(x => new AssetTrackingDto
                    {
                        TrackingId = x.TrackingId,
                        AssetId = x.AssetId,
                        AssetName = x.Asset.AssetName,
                        TenantId = x.TenantId,
                        CurrentLocationId = x.CurrentLocation,
                        CurrentLocationName = x.Location.LocationName,
                        Status = x.Status,
                        AssignedToId = x.AssignedTo,
                        AssignedToName = x.AssignedUser != null ? x.AssignedUser.FirstName + " " + x.AssignedUser.FirstName : null,
                        LastMovedOn = x.LastMovedOn,
                        GpsCoordinates = x.GpsCoordinates,
                        StatusUpdatedOn = x.StatusUpdatedOn,
                        Remarks = x.Remarks,
                        DownStartTime = x.DownStartTime,
                        DownEndTime = x.DownEndTime,
                        TotalDownMinutes = x.TotalDownMinutes,
                        TotalDownAccumulatedMinutes = x.TotalDownAccumulatedMinutes,
                        IsActive = x.IsActive,
                        IsDeleted = x.IsDeleted,
                        CreatedBy = x.CreatedBy,
                        UpdatedBy = x.UpdatedBy,
                        CreatedAt = x.CreatedAt,
                        UpdatedAt = x.UpdatedAt
                    })
                    .ToListAsync();

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = AssetTrackingStatusMessage.FetchedAll;
                response.GetAllData = data;
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "AssetTracking-Module", "GetAll-AssetTracking", tenantId, null);
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"{AssetTrackingStatusMessage.FetchFailed}: {ex.Message}";
            }

            return response;
        }

        public async Task<GetSpecificRecord<AssetTrackingDto>> GetAssetTrackingByIdAsync(long trackingId, int tenantId)
        {
            var response = new GetSpecificRecord<AssetTrackingDto>();
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                var data = await tenantDb.AssetTracking
                    .Where(x => x.TrackingId == trackingId && !x.IsDeleted)
                    .Select(x => new AssetTrackingDto
                    {
                        TrackingId = x.TrackingId,
                        AssetId = x.AssetId,
                        AssetName = x.Asset.AssetName,
                        TenantId = x.TenantId,
                        CurrentLocationId = x.CurrentLocation,
                        CurrentLocationName = x.Location.LocationName,
                        Status = x.Status,
                        AssignedToId = x.AssignedTo,
                        AssignedToName = x.AssignedUser != null ? x.AssignedUser.FirstName + " " + x.AssignedUser.FirstName : null,
                        LastMovedOn = x.LastMovedOn,
                        GpsCoordinates = x.GpsCoordinates,
                        StatusUpdatedOn = x.StatusUpdatedOn,
                        Remarks = x.Remarks,
                        DownStartTime = x.DownStartTime,
                        DownEndTime = x.DownEndTime,
                        TotalDownMinutes = x.TotalDownMinutes,
                        TotalDownAccumulatedMinutes = x.TotalDownAccumulatedMinutes,
                        IsActive = x.IsActive,
                        IsDeleted = x.IsDeleted,
                        CreatedBy = x.CreatedBy,
                        UpdatedBy = x.UpdatedBy,
                        CreatedAt = x.CreatedAt,
                        UpdatedAt = x.UpdatedAt
                    })
                    .FirstOrDefaultAsync();

                if (data == null)
                {
                    response.StatusCode = StatusCode.NotFound;
                    response.StatusMessage = AssetTrackingStatusMessage.NotFound;
                }
                else
                {
                    response.StatusCode = StatusCode.Success;
                    response.StatusMessage = AssetTrackingStatusMessage.FetchedById;
                    response.Data = data;
                }
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "AssetTracking-Module", "GetById-AssetTracking", tenantId, null);
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"{AssetTrackingStatusMessage.FetchByIdFailed}: {ex.Message}";
            }

            return response;
        }

        public async Task<GetAllRecord<AssetTrackingDto>> GetLatestAssetTrackingsAsync(int tenantId, int count = 3)
        {
            var response = new GetAllRecord<AssetTrackingDto>();
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                var data = await tenantDb.AssetTracking
                    .Where(x => !x.IsDeleted)
                    .OrderByDescending(a => a.StatusUpdatedOn)
                    .Take(count)
                    .Include(a => a.Asset)
                    .Include(a => a.Location)
                    .Include(a => a.AssignedUser)
                    .ToListAsync();

                var dtoList = data.Select(a => new AssetTrackingDto
                {
                    TenantId = a.TenantId,
                    TrackingId = a.TrackingId,
                    AssetId = a.AssetId,
                    AssetName = a.Asset?.AssetName,
                    CurrentLocationId = a.CurrentLocation,
                    CurrentLocationName = a.Location?.LocationName,
                    Status = a.Status,
                    AssignedToId = a.AssignedTo,
                    AssignedToName = a.AssignedUser != null ? a.AssignedUser.FirstName + " " + a.AssignedUser.FirstName : null,
                    LastMovedOn = a.LastMovedOn,
                    GpsCoordinates = a.GpsCoordinates,
                    StatusUpdatedOn = a.StatusUpdatedOn,
                    IsActive = a.IsActive,
                }).ToList();

                response.GetAllData = dtoList;
                response.StatusCode = dtoList.Any() ? StatusCode.Success : StatusCode.NotFound;
                response.StatusMessage = dtoList.Any() ? AssetTrackingStatusMessage.FetchedLatest : AssetTrackingStatusMessage.NoRecordsFound;
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "AssetTracking-Module", "GetLatest3-AssetTracking", tenantId, null);
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"{AssetTrackingStatusMessage.FetchLatestFailed}: {ex.Message}";
            }

            return response;
        }
    }
}
