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
    public class AssetLifecycleRepository : IAssetLifecycleRepository
    {
        private readonly TenantDbContextFactory _tenantDbContext;
        private readonly IAuditLogService _auditLogger;
        private readonly IExceptionLoggerService _exceptionLogger;

        public AssetLifecycleRepository(TenantDbContextFactory tenantDbContext,
                                       IAuditLogService auditLogger,
                                       IExceptionLoggerService exceptionLogger)
        {
            _tenantDbContext = tenantDbContext;
            _auditLogger = auditLogger;
            _exceptionLogger = exceptionLogger;
        }

        public async Task<CommonResponseModel> AddAssetLifecycleAsync(AssetLifecycleDto dto)
        {
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(dto.TenantId);

                var entity = new AssetLifecycle
                {
                    AssetId = dto.AssetId,
                    Stage = dto.Stage,
                    AcquisitionDate = dto.AcquisitionDate,
                    ExpectedRetirementDate = dto.ExpectedRetirementDate,
                    ActualRetirementDate = dto.ActualRetirementDate,
                    AcquisitionCost = dto.AcquisitionCost,
                    CurrentValue = dto.CurrentValue,
                    DepreciationValue = dto.DepreciationValue,
                    TCO = dto.TCO,
                    ROI = dto.ROI,
                    EstimatedRepairCost = dto.EstimatedRepairCost,
                    ReplacementCost = dto.ReplacementCost,
                    AnnualMaintenanceCost = dto.AnnualMaintenanceCost,
                    ResidualValue = dto.ResidualValue,
                    AnalysisNotes = dto.AnalysisNotes,
                    AnalysisType = dto.AnalysisType,
                    ReplacementDue = dto.ReplacementDue,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = dto.CreatedBy
                };

                await tenantDb.AssetLifecycles.AddAsync(entity);
                await tenantDb.SaveChangesAsync();

                await _auditLogger.LogAuditAsync("Create", $"Created Asset Lifecycle for Asset ID {dto.AssetId}", dto.TenantId, "", "AddAssetLifecycleAsync");

                return new CommonResponseModel { StatusCode = StatusCode.Success, StatusMessage = AssetLifecycleStatusMessage.LifecycleCreated };
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "AssetLifecycle-Module", "AddAssetLifecycleAsync", dto.TenantId, null);
                return new CommonResponseModel { StatusCode = StatusCode.Error, StatusMessage = $"{AssetLifecycleStatusMessage.CreateFailed}: {ex.Message}" };
            }
        }
        public async Task<CommonResponseModel> UpdateAssetLifecycleAsync(AssetLifecycleDto dto)
        {
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(dto.TenantId);

                var entity = await tenantDb.AssetLifecycles
                    .FirstOrDefaultAsync(a => a.LifecycleId == dto.LifecycleId && !a.IsDeleted);

                if (entity == null)
                    return new CommonResponseModel { StatusCode = StatusCode.NotFound, StatusMessage = AssetLifecycleStatusMessage.LifecycleNotFound };

                entity.Stage = dto.Stage;
                entity.ExpectedRetirementDate = dto.ExpectedRetirementDate;
                entity.ActualRetirementDate = dto.ActualRetirementDate;
                entity.CurrentValue = dto.CurrentValue;
                entity.DepreciationValue = dto.DepreciationValue;
                entity.TCO = dto.TCO;
                entity.ROI = dto.ROI;
                entity.EstimatedRepairCost = dto.EstimatedRepairCost;
                entity.ReplacementCost = dto.ReplacementCost;
                entity.AnnualMaintenanceCost = dto.AnnualMaintenanceCost;
                entity.ResidualValue = dto.ResidualValue;
                entity.AnalysisNotes = dto.AnalysisNotes;
                entity.AnalysisType = dto.AnalysisType;
                entity.ReplacementDue = dto.ReplacementDue;
                entity.UpdatedOn = DateTime.UtcNow;
                entity.UpdatedAt = DateTime.UtcNow;
                entity.UpdatedBy = dto.UpdatedBy;

                await tenantDb.SaveChangesAsync();
                await _auditLogger.LogAuditAsync("Update", $"Updated Asset Lifecycle {dto.LifecycleId}", dto.TenantId, "", "UpdateAssetLifecycleAsync");

                return new CommonResponseModel { StatusCode = StatusCode.Success, StatusMessage = AssetLifecycleStatusMessage.LifecycleUpdated };
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "AssetLifecycle-Module", "UpdateAssetLifecycleAsync", dto.TenantId, null);
                return new CommonResponseModel { StatusCode = StatusCode.Error, StatusMessage = $"{AssetLifecycleStatusMessage.UpdateFailed}: {ex.Message}" };
            }
        }
        public async Task<CommonResponseModel> DeleteAssetLifecycleAsync(long lifecycleId, int tenantId)
        {
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                var entity = await tenantDb.AssetLifecycles
                    .FirstOrDefaultAsync(a => a.LifecycleId == lifecycleId && !a.IsDeleted);

                if (entity == null)
                    return new CommonResponseModel { StatusCode = StatusCode.NotFound, StatusMessage = AssetLifecycleStatusMessage.LifecycleNotFound };

                entity.IsDeleted = true;
                entity.IsActive = false;
                entity.DeletedAt = DateTime.UtcNow;
                entity.DeletedBy = tenantId;

                await tenantDb.SaveChangesAsync();

                await _auditLogger.LogAuditAsync("Delete", $"Deleted Asset Lifecycle {lifecycleId}", tenantId, "", "DeleteAssetLifecycleAsync");

                return new CommonResponseModel { StatusCode = StatusCode.Success, StatusMessage = AssetLifecycleStatusMessage.LifecycleDeleted};
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "AssetLifecycle-Module", "DeleteAssetLifecycleAsync", tenantId, null);
                return new CommonResponseModel { StatusCode = StatusCode.Error, StatusMessage = $"{AssetLifecycleStatusMessage.DeleteFailed}: {ex.Message}" };
            }
        }
        public async Task<GetAllRecord<GetAssetLifecycleDto>> GetAllAssetLifecyclesAsync(int tenantId, string? stageFilter = null)
        {
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                var query = tenantDb.AssetLifecycles
                    .Where(a => !a.IsDeleted)
                    .Include(a => a.Asset)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(stageFilter))
                {
                    query = query.Where(a => a.Stage == AssetLifecycleStageEnum.Operation);
                }

                var entities = await query.ToListAsync();

                var dtoList = entities.Select(a => new GetAssetLifecycleDto
                {
                    LifecycleId = a.LifecycleId,
                    AssetId = a.AssetId,
                    AssetName = a.Asset?.AssetName ?? "Unknown",
                    Stage = a.Stage,
                    AcquisitionDate = a.AcquisitionDate,
                    ExpectedRetirementDate = a.ExpectedRetirementDate,
                    ActualRetirementDate = a.ActualRetirementDate,
                    AcquisitionCost = a.AcquisitionCost,
                    CurrentValue = a.CurrentValue,
                    DepreciationValue = a.DepreciationValue,
                    TCO = a.TCO,
                    ROI = a.ROI,
                    EstimatedRepairCost = a.EstimatedRepairCost,
                    ReplacementCost = a.ReplacementCost,
                    AnnualMaintenanceCost = a.AnnualMaintenanceCost,
                    ResidualValue = a.ResidualValue,
                    AnalysisNotes = a.AnalysisNotes,
                    AnalysisType = a.AnalysisType,
                    ReplacementDue = a.ReplacementDue,
                    UpdatedOn = a.UpdatedOn,
                    IsActive = a.IsActive,
                    YearsInService = GetYearsInService(a.AcquisitionDate),
                    RetirementTimespan = GetRetirementTimespan(a.ExpectedRetirementDate)
                }).ToList();

                return new GetAllRecord<GetAssetLifecycleDto>
                {
                    StatusCode = StatusCode.Success,
                    StatusMessage = AssetLifecycleStatusMessage.LifecyclesFetched,
                    GetAllData = dtoList
                };
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "AssetLifecycle-Module", "GetAllAssetLifecyclesAsync", tenantId, null);
                return new GetAllRecord<GetAssetLifecycleDto>
                {
                    StatusCode = StatusCode.Error,
                    StatusMessage = $"{AssetLifecycleStatusMessage.FetchFailed}: {ex.Message}"
                };
            }
        }

        private string GetYearsInService(DateTime acquisitionDate)
        {
            var timeSpan = DateTime.UtcNow - acquisitionDate;
            var years = (int)(timeSpan.TotalDays / 365);
            return $"{years} years";
        }

        private string GetRetirementTimespan(DateTime? expectedRetirementDate)
        {
            if (!expectedRetirementDate.HasValue) return "Not set";

            var timeSpan = expectedRetirementDate.Value - DateTime.UtcNow;
            var years = (int)(timeSpan.TotalDays / 365);
            return years > 0 ? $"{years} years remaining" : "Overdue";
        }

        public async Task<GetSpecificRecord<GetAssetLifecycleDto>> GetAssetLifecycleByIdAsync(long lifecycleId, int tenantId)
        {
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                var entity = await tenantDb.AssetLifecycles
                    .Include(a => a.Asset)
                    .FirstOrDefaultAsync(a => a.LifecycleId == lifecycleId && !a.IsDeleted);

                if (entity == null)
                    return new GetSpecificRecord<GetAssetLifecycleDto>
                    {
                        StatusCode = StatusCode.NotFound,
                        StatusMessage = AssetLifecycleStatusMessage.LifecycleNotFound
                    };

                var dto = new GetAssetLifecycleDto
                {
                    LifecycleId = entity.LifecycleId,
                    AssetId = entity.AssetId,
                    AssetName = entity.Asset?.AssetName ?? "Unknown",
                    Stage = entity.Stage,
                    AcquisitionDate = entity.AcquisitionDate,
                    ExpectedRetirementDate = entity.ExpectedRetirementDate,
                    ActualRetirementDate = entity.ActualRetirementDate,
                    AcquisitionCost = entity.AcquisitionCost,
                    CurrentValue = entity.CurrentValue,
                    DepreciationValue = entity.DepreciationValue,
                    TCO = entity.TCO,
                    ROI = entity.ROI,
                    EstimatedRepairCost = entity.EstimatedRepairCost,
                    ReplacementCost = entity.ReplacementCost,
                    AnnualMaintenanceCost = entity.AnnualMaintenanceCost,
                    ResidualValue = entity.ResidualValue,
                    AnalysisNotes = entity.AnalysisNotes,
                    AnalysisType = entity.AnalysisType,
                    ReplacementDue = entity.ReplacementDue,
                    UpdatedOn = entity.UpdatedOn,
                    IsActive = entity.IsActive,
                    YearsInService = GetYearsInService(entity.AcquisitionDate),
                    RetirementTimespan = GetRetirementTimespan(entity.ExpectedRetirementDate)
                };

                return new GetSpecificRecord<GetAssetLifecycleDto>
                {
                    StatusCode = StatusCode.Success,
                    StatusMessage = AssetLifecycleStatusMessage.LifecyclesFetched,
                    Data = dto
                };
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "AssetLifecycle-Module", "GetAssetLifecycleByIdAsync", tenantId, null);
                return new GetSpecificRecord<GetAssetLifecycleDto>
                {
                    StatusCode = StatusCode.Error,
                    StatusMessage = $"{AssetLifecycleStatusMessage.FetchFailed}: {ex.Message}"
                };
            }
        }

        public async Task<GetSpecificRecord<GetAssetLifecycleDto>> GetAssetLifecycleByAssetIdAsync(int assetId, int tenantId)
        {
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                var entity = await tenantDb.AssetLifecycles
                    .Include(a => a.Asset)
                    .FirstOrDefaultAsync(a => a.AssetId == assetId && !a.IsDeleted);

                if (entity == null)
                    return new GetSpecificRecord<GetAssetLifecycleDto>
                    {
                        StatusCode = StatusCode.NotFound,
                        StatusMessage = AssetLifecycleStatusMessage.LifecycleNotFoundForAsset
                    };

                var dto = new GetAssetLifecycleDto
                {
                    LifecycleId = entity.LifecycleId,
                    AssetId = entity.AssetId,
                    AssetName = entity.Asset?.AssetName ?? "Unknown",
                    Stage = entity.Stage,
                    AcquisitionDate = entity.AcquisitionDate,
                    ExpectedRetirementDate = entity.ExpectedRetirementDate,
                    ActualRetirementDate = entity.ActualRetirementDate,
                    AcquisitionCost = entity.AcquisitionCost,
                    CurrentValue = entity.CurrentValue,
                    DepreciationValue = entity.DepreciationValue,
                    TCO = entity.TCO,
                    ROI = entity.ROI,
                    EstimatedRepairCost = entity.EstimatedRepairCost,
                    ReplacementCost = entity.ReplacementCost,
                    AnnualMaintenanceCost = entity.AnnualMaintenanceCost,
                    ResidualValue = entity.ResidualValue,
                    AnalysisNotes = entity.AnalysisNotes,
                    AnalysisType = entity.AnalysisType,
                    ReplacementDue = entity.ReplacementDue,
                    UpdatedOn = entity.UpdatedOn,
                    IsActive = entity.IsActive,
                    YearsInService = GetYearsInService(entity.AcquisitionDate),
                    RetirementTimespan = GetRetirementTimespan(entity.ExpectedRetirementDate)
                };

                return new GetSpecificRecord<GetAssetLifecycleDto>
                {
                    StatusCode = StatusCode.Success,
                    StatusMessage = AssetLifecycleStatusMessage.LifecyclesFetched,
                    Data = dto
                };
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "AssetLifecycle-Module", "GetAssetLifecycleByAssetIdAsync", tenantId, null);
                return new GetSpecificRecord<GetAssetLifecycleDto>
                {
                    StatusCode = StatusCode.Error,
                    StatusMessage = $"{AssetLifecycleStatusMessage.FetchFailed}: {ex.Message}"
                };
            }
        }

        public async Task<GetSpecificRecord<AssetLifecycleMetricsDto>> GetAssetLifecycleMetricsAsync(int tenantId)
        {
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                var lifecycles = await tenantDb.AssetLifecycles
                    .Where(a => !a.IsDeleted)
                    .Include(a => a.Asset)
                    .ToListAsync();

                var metrics = new AssetLifecycleMetricsDto
                {
                    TotalAssetValue = lifecycles.Sum(a => a.CurrentValue ?? 0),
                    TotalTCO = lifecycles.Sum(a => a.TCO ?? 0),
                    AverageROI = lifecycles.Average(a => a.ROI ?? 0),
                    TotalAssets = lifecycles.Count,
                    AssetsInOperation = lifecycles.Count(a => a.Stage == AssetLifecycleStageEnum.Operation),
                    AssetsNeedingReplacement = lifecycles.Count(a => a.ReplacementDue.HasValue && a.ReplacementDue.Value <= DateTime.UtcNow.AddDays(30))
                };

                return new GetSpecificRecord<AssetLifecycleMetricsDto>
                {
                    StatusCode = StatusCode.Success,
                    StatusMessage = AssetLifecycleStatusMessage.LifecyclesFetched,
                    Data = metrics
                };
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "AssetLifecycle-Module", "GetAssetLifecycleMetricsAsync", tenantId, null);
                return new GetSpecificRecord<AssetLifecycleMetricsDto>
                {
                    StatusCode = StatusCode.Error,
                    StatusMessage = $"{AssetLifecycleStatusMessage.FetchFailed}: {ex.Message}"
                };
            }
        }
    }

    public class AssetFinancialAnalysisRepository : IAssetFinancialAnalysisRepository
    {
        private readonly TenantDbContextFactory _tenantDbContext;
        private readonly IAuditLogService _auditLogger;
        private readonly IExceptionLoggerService _exceptionLogger;

        public AssetFinancialAnalysisRepository(TenantDbContextFactory tenantDbContext,
                                              IAuditLogService auditLogger,
                                              IExceptionLoggerService exceptionLogger)
        {
            _tenantDbContext = tenantDbContext;
            _auditLogger = auditLogger;
            _exceptionLogger = exceptionLogger;
        }

        public async Task<CommonResponseModel> AddFinancialAnalysisAsync(AssetFinancialAnalysisDto dto)
        {
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(dto.TenantId);

                var entity = new AssetFinancialAnalysis
                {
                    AssetId = dto.AssetId,
                    AnalysisType = dto.AnalysisType,
                    RepairCost = dto.RepairCost,
                    ReplacementCost = dto.ReplacementCost,
                    ROI = dto.ROI,
                    Recommendations = dto.Recommendations,
                    AnalysisDate = dto.AnalysisDate,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = dto.CreatedBy
                };

                await tenantDb.AssetFinancialAnalysis.AddAsync(entity);
                await tenantDb.SaveChangesAsync();

                await _auditLogger.LogAuditAsync("Create", $"Created Financial Analysis for Asset ID {dto.AssetId}", dto.TenantId, "", "AddFinancialAnalysisAsync");

                return new CommonResponseModel { StatusCode = StatusCode.Success, StatusMessage = AssetFinancialAnalysisStatusMessage.AnalysisCreated };
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "AssetFinancialAnalysis-Module", "AddFinancialAnalysisAsync", dto.TenantId, null);
                return new CommonResponseModel { StatusCode = StatusCode.Error, StatusMessage = $"{AssetFinancialAnalysisStatusMessage.CreateFailed}: {ex.Message}" };
            }
        }

        public async Task<CommonResponseModel> DeleteFinancialAnalysisAsync(long analysisId, int tenantId)
        {
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                var entity = await tenantDb.AssetFinancialAnalysis
                    .FirstOrDefaultAsync(a => a.AnalysisId == analysisId && !a.IsDeleted);

                if (entity == null)
                    return new CommonResponseModel { StatusCode = StatusCode.NotFound, StatusMessage = AssetFinancialAnalysisStatusMessage.AnalysisNotFound };

                entity.IsDeleted = true;
                entity.IsActive = false;

                await tenantDb.SaveChangesAsync();

                await _auditLogger.LogAuditAsync("Delete", $"Deleted Financial Analysis {analysisId}", tenantId, "", "DeleteFinancialAnalysisAsync");

                return new CommonResponseModel { StatusCode = StatusCode.Success, StatusMessage = AssetFinancialAnalysisStatusMessage.AnalysisDeleted };
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "AssetFinancialAnalysis-Module", "DeleteFinancialAnalysisAsync", tenantId, null);
                return new CommonResponseModel { StatusCode = StatusCode.Error, StatusMessage = $"{AssetFinancialAnalysisStatusMessage.DeleteFailed}: {ex.Message}" };
            }
        }

        public async Task<GetAllRecord<GetAssetFinancialAnalysisDto>> GetFinancialAnalysesByAssetIdAsync(int assetId, int tenantId, string? analysisType = null)
        {
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                var query = tenantDb.AssetFinancialAnalysis
                    .Where(a => a.AssetId == assetId && !a.IsDeleted)
                    .Include(a => a.Asset)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(analysisType))
                {
                    query = query.Where(a => a.AnalysisType == analysisType);
                }

                var entities = await query
                    .OrderByDescending(a => a.AnalysisDate)
                    .ToListAsync();

                var dtoList = entities.Select(a => new GetAssetFinancialAnalysisDto
                {
                    AnalysisId = a.AnalysisId,
                    AssetId = a.AssetId,
                    AssetName = a.Asset?.AssetName ?? "Unknown",
                    AnalysisType = a.AnalysisType,
                    RepairCost = a.RepairCost,
                    ReplacementCost = a.ReplacementCost,
                    ROI = a.ROI,
                    Recommendations = a.Recommendations,
                    AnalysisDate = a.AnalysisDate,
                    IsActive = a.IsActive,
                    AnalysisDateFormatted = a.AnalysisDate.ToString("yyyy-MM-dd HH:mm")
                }).ToList();

                return new GetAllRecord<GetAssetFinancialAnalysisDto>
                {
                    StatusCode = StatusCode.Success,
                    StatusMessage = AssetFinancialAnalysisStatusMessage.AnalysesFetched,
                    GetAllData = dtoList
                };
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "AssetFinancialAnalysis-Module", "GetFinancialAnalysesByAssetIdAsync", tenantId, null);
                return new GetAllRecord<GetAssetFinancialAnalysisDto>
                {
                    StatusCode = StatusCode.Error,
                    StatusMessage = $"{AssetFinancialAnalysisStatusMessage.FetchFailed}: {ex.Message}"
                };
            }
        }

        public async Task<GetAllRecord<GetAssetFinancialAnalysisDto>> GetAllFinancialAnalysesAsync(int tenantId, string? analysisType = null)
        {
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                var query = tenantDb.AssetFinancialAnalysis
                    .Where(a => !a.IsDeleted)
                    .Include(a => a.Asset)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(analysisType))
                {
                    query = query.Where(a => a.AnalysisType == analysisType);
                }

                var entities = await query
                    .OrderByDescending(a => a.AnalysisDate)
                    .ToListAsync();

                var dtoList = entities.Select(a => new GetAssetFinancialAnalysisDto
                {
                    AnalysisId = a.AnalysisId,
                    AssetId = a.AssetId,
                    AssetName = a.Asset?.AssetName ?? "Unknown",
                    AnalysisType = a.AnalysisType,
                    RepairCost = a.RepairCost,
                    ReplacementCost = a.ReplacementCost,
                    ROI = a.ROI,
                    Recommendations = a.Recommendations,
                    AnalysisDate = a.AnalysisDate,
                    IsActive = a.IsActive,
                    AnalysisDateFormatted = a.AnalysisDate.ToString("yyyy-MM-dd HH:mm")
                }).ToList();

                return new GetAllRecord<GetAssetFinancialAnalysisDto>
                {
                    StatusCode = StatusCode.Success,
                    StatusMessage = AssetFinancialAnalysisStatusMessage.AnalysesFetched,
                    GetAllData = dtoList
                };
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "AssetFinancialAnalysis-Module", "GetAllFinancialAnalysesAsync", tenantId, null);
                return new GetAllRecord<GetAssetFinancialAnalysisDto>
                {
                    StatusCode = StatusCode.Error,
                    StatusMessage = $"{AssetFinancialAnalysisStatusMessage.FetchFailed}: {ex.Message}"
                };
            }
        }
    }
}