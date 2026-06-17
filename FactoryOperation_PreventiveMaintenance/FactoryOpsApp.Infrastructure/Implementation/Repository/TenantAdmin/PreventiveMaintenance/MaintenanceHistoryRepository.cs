using FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.PreventiveMaintenance;
using FactoryOpsApp.Application.Interfaces.Services.SuperAdmin.AuditLogs;
using Microsoft.EntityFrameworkCore;
using FactoryOpsApp.Domain.Entities.FactoryOpsTenants;
using FactoryOpsApp.Infrastructure.DBContext;
using static FactoryOpsApp.Common.CommonConstant;
using FactoryOperation_PreventiveMaintenance.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.ExceptionLogger;

namespace FactoryOpsApp.Infrastructure.Repository.TenantAdmin.PreventiveMaintenance
{
    public class MaintenanceHistoryRepository : IMaintenanceHistoryRepository
    {
        private readonly TenantDbContextFactory _tenantDbContext;
        private readonly IAuditLogService _auditLogger;
        private readonly IExceptionLoggerService _exceptionLogger;

        public MaintenanceHistoryRepository(TenantDbContextFactory tenantDbContext,
                                           IAuditLogService auditLogger,
                                           IExceptionLoggerService exceptionLogger)
        {
            _tenantDbContext = tenantDbContext;
            _auditLogger = auditLogger;
            _exceptionLogger = exceptionLogger;
        }

        public async Task<CommonResponseModel> AddMaintenanceHistoryAsync(MaintenanceHistoryDto dto)
        {
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(dto.TenantId);

                var entity = new MaintenanceHistory
                {
                    AssetId = dto.AssetId,
                    WorkOrderId = dto.WorkOrderId,
                    TenantId = dto.TenantId,
                    Technician = dto.TechnicianId,
                    MaintenanceType = dto.MaintenanceType,
                    Description = dto.Description,
                    PartsUsed = dto.PartsUsed,
                    Priority = dto.Priority,
                    LaborHours = dto.LaborHours,
                    Cost = dto.Cost,
                    PerformedBy = dto.PerformedBy,
                    PerformedOn = dto.PerformedOn,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = dto.CreatedBy
                };

                if (dto.MaintenanceType == MaintenanceTypeEnum.Corrective)
                {
                    entity.FailureReportedOn = DateTime.UtcNow;
                }

                await tenantDb.MaintenanceHistory.AddAsync(entity);
                await tenantDb.SaveChangesAsync();

                return new CommonResponseModel
                {
                    StatusCode = StatusCode.Success,
                    StatusMessage = MaintenanceHistoryStatusMessage.CreateSuccess
                };
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "Maintenance-Module", "AddMaintenanceHistoryAsync", dto.TenantId, null);
                return new CommonResponseModel
                {
                    StatusCode = StatusCode.Error,
                    StatusMessage = $"{MaintenanceHistoryStatusMessage.CreateFailed}: {ex.Message}"
                };
            }
        }

        public async Task<CommonResponseModel> UpdateMaintenanceHistoryAsync(UpdateMaintenanceHistoryDto dto)
        {
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(dto.TenantId);

                var entity = await tenantDb.MaintenanceHistory
                    .FirstOrDefaultAsync(m => m.MaintenanceId == dto.MaintenanceId && !m.IsDeleted);

                if (entity == null)
                    return new CommonResponseModel
                    {
                        StatusCode = StatusCode.NotFound,
                        StatusMessage = MaintenanceHistoryStatusMessage.NotFound
                    };

                entity.Description = dto.Description;
                entity.PartsUsed = dto.PartsUsed;
                entity.Priority = dto.Priority;
                entity.LaborHours = dto.LaborHours;
                entity.Cost = dto.Cost;
                entity.PerformedBy = dto.PerformedBy;
                entity.PerformedOn = dto.PerformedOn;
                entity.UpdatedAt = DateTime.UtcNow;
                entity.UpdatedBy = dto.UpdatedBy;

                if (entity.MaintenanceType == MaintenanceTypeEnum.Corrective &&
                    entity.FailureReportedOn.HasValue &&
                    !entity.RepairCompletedOn.HasValue)
                {
                    entity.RepairCompletedOn = DateTime.UtcNow;

                    // -------- MTTR --------
                    var repairMinutes =
                        (entity.RepairCompletedOn.Value - entity.FailureReportedOn.Value).TotalMinutes;

                    entity.MTTR = repairMinutes > 0 ? (decimal)repairMinutes : 0;

                    // -------- MTBF --------
                    entity.MTBF = await CalculateMTBFAsync(
                        dto.TenantId,
                        entity.AssetId
                    );
                }

                await tenantDb.SaveChangesAsync();

                return new CommonResponseModel
                {
                    StatusCode = StatusCode.Success,
                    StatusMessage = MaintenanceHistoryStatusMessage.UpdateSuccess
                };
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "Maintenance-Module", "UpdateMaintenanceHistoryAsync", dto.TenantId, null);
                return new CommonResponseModel
                {
                    StatusCode = StatusCode.Error,
                    StatusMessage = $"{MaintenanceHistoryStatusMessage.UpdateFailed}: {ex.Message}"
                };
            }
        }



        public async Task<CommonResponseModel> DeleteMaintenanceHistoryAsync(long maintenanceId, int tenantId)
        {
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                var entity = await tenantDb.MaintenanceHistory
                    .FirstOrDefaultAsync(m => m.MaintenanceId == maintenanceId && !m.IsDeleted);

                if (entity == null)
                    return new CommonResponseModel { StatusCode = StatusCode.NotFound, StatusMessage = MaintenanceHistoryStatusMessage.NotFound };

                entity.IsDeleted = true;
                entity.IsActive = false;
                entity.DeletedAt = DateTime.UtcNow;
                entity.DeletedBy = tenantId;

                await tenantDb.SaveChangesAsync();

                await _auditLogger.LogAuditAsync(
                    "Delete",
                    $"Deleted Maintenance Record '{entity.WorkOrderId}'",
                    tenantId,
                    "",
                    "DeleteMaintenanceHistoryAsync"
                );

                return new CommonResponseModel { StatusCode = StatusCode.Success, StatusMessage = MaintenanceHistoryStatusMessage.DeleteSuccess };
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "Maintenance-Module", "DeleteMaintenanceHistoryAsync", tenantId, null);
                return new CommonResponseModel { StatusCode = StatusCode.Error, StatusMessage = $"{MaintenanceHistoryStatusMessage.DeleteFailed}: {ex.Message}" };
            }
        }
        public async Task<GetAllRecord<GetMaintenanceHistoryDto>> GetAllMaintenanceHistoryAsync(int tenantId, string? searchTerm = null, string? statusFilter = null, string? typeFilter = null)
        {
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                var query = tenantDb.MaintenanceHistory
                    .Where(m => !m.IsDeleted)
                    .Include(m => m.Asset).Include(m => m.Technicians)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(m => m.WorkOrderId.Contains(searchTerm) ||
                                           m.Description.Contains(searchTerm) ||
                                           m.PerformedBy.Contains(searchTerm) ||
                                           m.Asset.AssetName.Contains(searchTerm));
                }

                if (!string.IsNullOrEmpty(typeFilter))
                {
                    query = query.Where(m => m.MaintenanceType == MaintenanceTypeEnum.Preventive);
                }

                if (!string.IsNullOrEmpty(statusFilter))
                {
                    var currentDate = DateTime.UtcNow;
                    if (statusFilter == "Completed")
                    {
                        query = query.Where(m => m.PerformedOn <= currentDate);
                    }
                    else if (statusFilter == "In Progress")
                    {
                        query = query.Where(m => m.PerformedOn > currentDate);
                    }
                }

                var entities = await query
                    .OrderByDescending(m => m.PerformedOn)
                    .ToListAsync();

                var dtoList = entities.Select(m => new GetMaintenanceHistoryDto
                {
                    MaintenanceId = m.MaintenanceId,
                    AssetId = m.AssetId,
                    AssetName = m.Asset.AssetName,
                    TenantId = m.TenantId,
                    TechnicianId = m.Technicians?.UserId,
                    Technician = m.Technicians?.FirstName + " " + m.Technicians?.LastName,
                    WorkOrderId = m.WorkOrderId,
                    MaintenanceType = m.MaintenanceType,
                    Description = m.Description,
                    PartsUsed = m.PartsUsed,
                    Priority = m.Priority,
                    LaborHours = m.LaborHours,
                    Cost = m.Cost,
                    PerformedBy = m.PerformedBy,
                    PerformedOn = m.PerformedOn,
                    MTBF = m.MTBF,
                    MTTR = m.MTTR,
                    IsActive = m.IsActive,
                    Status = m.PerformedOn <= DateTime.UtcNow ? "Completed" : "In Progress",
                    PerformedOnFormatted = GetFormattedDate(m.PerformedOn)
                }).ToList();

                return new GetAllRecord<GetMaintenanceHistoryDto>
                {
                    StatusCode = StatusCode.Success,
                    StatusMessage = MaintenanceHistoryStatusMessage.FetchAllSuccess,
                    GetAllData = dtoList
                };
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "Maintenance-Module", "GetAllMaintenanceHistoryAsync", tenantId, null);
                return new GetAllRecord<GetMaintenanceHistoryDto>
                {
                    StatusCode = StatusCode.Error,
                    StatusMessage = $"{MaintenanceHistoryStatusMessage.FetchAllFailed}: {ex.Message}"
                };
            }
        }
        private string GetFormattedDate(DateTime date)
        {
            return date.ToString("yyyy-MM-dd");
        }
        public async Task<GetSpecificRecord<GetMaintenanceHistoryDto>> GetMaintenanceHistoryByIdAsync(long maintenanceId, int tenantId)
        {
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                var entity = await tenantDb.MaintenanceHistory
                    .Include(m => m.Asset).Include(m => m.Technicians)
                    .FirstOrDefaultAsync(m => m.MaintenanceId == maintenanceId && !m.IsDeleted);

                if (entity == null)
                    return new GetSpecificRecord<GetMaintenanceHistoryDto>
                    {
                        StatusCode = StatusCode.NotFound,
                        StatusMessage = MaintenanceHistoryStatusMessage.NotFound
                    };

                var dto = new GetMaintenanceHistoryDto
                {
                    MaintenanceId = entity.MaintenanceId,
                    AssetId = entity.AssetId,
                    AssetName = entity.Asset.AssetName,
                    TenantId = entity.TenantId,
                    TechnicianId = entity.Technicians?.UserId,
                    Technician = entity.Technicians?.FirstName + " " + entity.Technicians?.LastName,
                    WorkOrderId = entity.WorkOrderId,
                    MaintenanceType = entity.MaintenanceType,
                    Description = entity.Description,
                    PartsUsed = entity.PartsUsed,
                    Priority = entity.Priority,
                    LaborHours = entity.LaborHours,
                    Cost = entity.Cost,
                    PerformedBy = entity.PerformedBy,
                    PerformedOn = entity.PerformedOn,
                    MTBF = entity.MTBF,
                    MTTR = entity.MTTR,
                    IsActive = entity.IsActive,
                    Status = entity.PerformedOn <= DateTime.UtcNow ? "Completed" : "In Progress",
                    PerformedOnFormatted = GetFormattedDate(entity.PerformedOn)
                };

                return new GetSpecificRecord<GetMaintenanceHistoryDto>
                {
                    StatusCode = StatusCode.Success,
                    StatusMessage = MaintenanceHistoryStatusMessage.FetchByIdFailed,
                    Data = dto
                };
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "Maintenance-Module", "GetMaintenanceHistoryByIdAsync", tenantId, null);
                return new GetSpecificRecord<GetMaintenanceHistoryDto>
                {
                    StatusCode = StatusCode.Error,
                    StatusMessage = $"{MaintenanceHistoryStatusMessage.FetchByIdFailed}: {ex.Message}"
                };
            }
        }
        public async Task<GetAllRecord<GetMaintenanceHistoryDto>> GetMaintenanceHistoryByAssetIdAsync(int assetId, int tenantId)
        {
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                var entities = await tenantDb.MaintenanceHistory
                    .Where(m => m.AssetId == assetId && !m.IsDeleted)
                    .Include(m => m.Asset)
                    .Include(m => m.Technicians)
                    .OrderByDescending(m => m.PerformedOn)
                    .ToListAsync();

                if (entities == null || !entities.Any())
                    return new GetAllRecord<GetMaintenanceHistoryDto>
                    {
                        StatusCode = StatusCode.NotFound,
                        StatusMessage = MaintenanceHistoryStatusMessage.AssetRecordNotFound
                    };

                var dtoList = entities.Select(entity => new GetMaintenanceHistoryDto
                {
                    MaintenanceId = entity.MaintenanceId,
                    AssetId = entity.AssetId,
                    AssetName = entity.Asset?.AssetName ?? "Unknown",
                    TenantId = entity.TenantId,
                    TechnicianId = entity.Technicians?.UserId,
                    Technician = entity.Technicians != null ?
                        $"{entity.Technicians.FirstName} {entity.Technicians.LastName}" : "Unknown",
                    WorkOrderId = entity.WorkOrderId,
                    MaintenanceType = entity.MaintenanceType,
                    Description = entity.Description,
                    PartsUsed = entity.PartsUsed,
                    Priority = entity.Priority,
                    LaborHours = entity.LaborHours,
                    Cost = entity.Cost,
                    PerformedBy = entity.PerformedBy,
                    PerformedOn = entity.PerformedOn,
                    MTBF = entity.MTBF,
                    MTTR = entity.MTTR,
                    IsActive = entity.IsActive,
                    Status = entity.PerformedOn <= DateTime.UtcNow ? "Completed" : "In Progress",
                    PerformedOnFormatted = GetFormattedDate(entity.PerformedOn)
                }).ToList();

                return new GetAllRecord<GetMaintenanceHistoryDto>
                {
                    StatusCode = StatusCode.Success,
                    StatusMessage = MaintenanceHistoryStatusMessage.FetchByAssetSuccess,
                    GetAllData = dtoList
                };
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "Maintenance-Module", "GetMaintenanceHistoryByAssetIdAsync", tenantId, null);
                return new GetAllRecord<GetMaintenanceHistoryDto>
                {
                    StatusCode = StatusCode.Error,
                    StatusMessage = $"{MaintenanceHistoryStatusMessage.FetchByAssetFailed}: {ex.Message}"
                };
            }
        }
        public async Task<GetSpecificRecord<MaintenanceMetricsDto>> GetMaintenanceMetricsAsync(int tenantId)
        {
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                var maintenanceRecords = await tenantDb.MaintenanceHistory
                    .Where(m => !m.IsDeleted)
                    .ToListAsync();

                var currentDate = DateTime.UtcNow;
                var completedCount = maintenanceRecords.Count(m => m.PerformedOn <= currentDate);
                var inProgressCount = maintenanceRecords.Count(m => m.PerformedOn > currentDate);

                var metrics = new MaintenanceMetricsDto
                {
                    TenantId = tenantId,
                    AvgMTBF = maintenanceRecords.Any(m => m.MTBF.HasValue)
                        ? maintenanceRecords.Where(m => m.MTBF.HasValue).Average(m => m.MTBF.Value)
                        : 0,
                     AvgMTTR = maintenanceRecords.Any(m => m.MTTR.HasValue)
                        ? maintenanceRecords.Where(m => m.MTTR.HasValue).Average(m => m.MTTR.Value)
                        : 0,

                    TotalCost = maintenanceRecords.Sum(m => m.Cost ?? 0),
                    Efficiency = completedCount > 0 ? (decimal)completedCount / (completedCount + inProgressCount) * 100 : 0,
                    TotalMaintenanceCount = maintenanceRecords.Count,
                    CompletedCount = completedCount,
                    InProgressCount = inProgressCount
                };

                return new GetSpecificRecord<MaintenanceMetricsDto>
                {
                    StatusCode = StatusCode.Success,
                    StatusMessage = MaintenanceHistoryStatusMessage.MetricsFetchSuccess,
                    Data = metrics
                };
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "Maintenance-Module", "GetMaintenanceMetricsAsync", tenantId, null);
                return new GetSpecificRecord<MaintenanceMetricsDto>
                {
                    StatusCode = StatusCode.Error,
                    StatusMessage = $"{MaintenanceHistoryStatusMessage.MetricsFetchFailed}: {ex.Message}"
                };
            }
        }

        private async Task<decimal> CalculateMTBFAsync(int tenantId,int assetId)
        {
                    using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);
                    var failureCount = await tenantDb.MaintenanceHistory
                .CountAsync(m =>
                    m.AssetId == assetId &&
                    m.MaintenanceType == MaintenanceTypeEnum.Corrective &&
                    !m.IsDeleted);

            if (failureCount == 0)
                return 0;

            var totalDowntimeMinutes = await tenantDb.AssetTracking
                .Where(a => a.AssetId == assetId && a.TotalDownMinutes != null)
                .SumAsync(a => a.TotalDownMinutes.Value);

            var assetCreatedAt = await tenantDb.AssetRegistry
                .Where(a => a.AssetId == assetId)
                .Select(a => a.CreatedAt)
                .FirstAsync();

            var totalTimeMinutes =
                (DateTime.UtcNow - assetCreatedAt).TotalMinutes;

            var operatingMinutes = totalTimeMinutes - totalDowntimeMinutes;

            return operatingMinutes > 0
                ? (decimal)(operatingMinutes / failureCount)
                : 0;
        }
    }
}