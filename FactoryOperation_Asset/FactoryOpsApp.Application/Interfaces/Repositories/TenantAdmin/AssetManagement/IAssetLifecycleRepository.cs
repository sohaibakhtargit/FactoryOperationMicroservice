using FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;

namespace FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.AssetManagement
{
    public interface IAssetLifecycleRepository
    {
        Task<CommonResponseModel> AddAssetLifecycleAsync(AssetLifecycleDto dto);
        Task<CommonResponseModel> UpdateAssetLifecycleAsync(AssetLifecycleDto dto);
        Task<CommonResponseModel> DeleteAssetLifecycleAsync(long lifecycleId, int tenantId);
        Task<GetAllRecord<GetAssetLifecycleDto>> GetAllAssetLifecyclesAsync(int tenantId, string? stageFilter = null);
        Task<GetSpecificRecord<GetAssetLifecycleDto>> GetAssetLifecycleByIdAsync(long lifecycleId, int tenantId);
        Task<GetSpecificRecord<GetAssetLifecycleDto>> GetAssetLifecycleByAssetIdAsync(int assetId, int tenantId);
        Task<GetSpecificRecord<AssetLifecycleMetricsDto>> GetAssetLifecycleMetricsAsync(int tenantId);
    }

    public interface IAssetFinancialAnalysisRepository
    {
        Task<CommonResponseModel> AddFinancialAnalysisAsync(AssetFinancialAnalysisDto dto);
        Task<CommonResponseModel> DeleteFinancialAnalysisAsync(long analysisId, int tenantId);
        Task<GetAllRecord<GetAssetFinancialAnalysisDto>> GetFinancialAnalysesByAssetIdAsync(int assetId, int tenantId, string? analysisType = null);
        Task<GetAllRecord<GetAssetFinancialAnalysisDto>> GetAllFinancialAnalysesAsync(int tenantId, string? analysisType = null);
    }
}