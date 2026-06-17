using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;
using System.Threading.Tasks;

namespace FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.AssetManagement
{
    public interface IAssetLifecycleService
    {
        Task<CommonResponseModel> AddAssetLifecycleAsync(AssetLifecycleDto dto);
        Task<CommonResponseModel> UpdateAssetLifecycleAsync(AssetLifecycleDto dto);
        Task<CommonResponseModel> DeleteAssetLifecycleAsync(long lifecycleId, int tenantId);
        Task<GetAllRecord<GetAssetLifecycleDto>> GetAllAssetLifecyclesAsync(int tenantId, string? stageFilter = null);
        Task<GetSpecificRecord<GetAssetLifecycleDto>> GetAssetLifecycleByIdAsync(long lifecycleId, int tenantId);
        Task<GetSpecificRecord<GetAssetLifecycleDto>> GetAssetLifecycleByAssetIdAsync(int assetId, int tenantId);
        Task<GetSpecificRecord<AssetLifecycleMetricsDto>> GetAssetLifecycleMetricsAsync(int tenantId);
        Task<GetSpecificRecord<AssetLifecycleFinancialSummaryDTO>> GetAssetLifeCycleSummery(int tenatId);
        Task<GetAllRecordsCount<AssetLifeHistoryReportDTO>> GetAssetLifeHistoryReport(int tenantId, int assetId);
    }

    public interface IAssetFinancialAnalysisService
    {
        Task<CommonResponseModel> AddFinancialAnalysisAsync(AssetFinancialAnalysisDto dto);
        Task<CommonResponseModel> DeleteFinancialAnalysisAsync(long analysisId, int tenantId);
        Task<GetAllRecord<GetAssetFinancialAnalysisDto>> GetFinancialAnalysesByAssetIdAsync(int assetId, int tenantId, string? analysisType = null);
        Task<GetAllRecord<GetAssetFinancialAnalysisDto>> GetAllFinancialAnalysesAsync(int tenantId, string? analysisType = null);
    }
}