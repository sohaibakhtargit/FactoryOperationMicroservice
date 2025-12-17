using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;
using System.Threading.Tasks;
using FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.AssetManagement;
using FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.AssetManagement;

namespace FactoryOpsApp.Infrastructure.Service.TenantAdmin.AssetManagement
{
    public class AssetLifecycleService : IAssetLifecycleService
    {
        private readonly IAssetLifecycleRepository _repository;

        public AssetLifecycleService(IAssetLifecycleRepository repository)
        {
            _repository = repository;
        }

        public Task<CommonResponseModel> AddAssetLifecycleAsync(AssetLifecycleDto dto)
        {
            return _repository.AddAssetLifecycleAsync(dto);
        }

        public Task<CommonResponseModel> UpdateAssetLifecycleAsync(AssetLifecycleDto dto)
        {
            return _repository.UpdateAssetLifecycleAsync(dto);
        }

        public Task<CommonResponseModel> DeleteAssetLifecycleAsync(long lifecycleId, int tenantId)
        {
            return _repository.DeleteAssetLifecycleAsync(lifecycleId, tenantId);
        }

        public Task<GetAllRecord<GetAssetLifecycleDto>> GetAllAssetLifecyclesAsync(int tenantId, string? stageFilter = null)
        {
            return _repository.GetAllAssetLifecyclesAsync(tenantId, stageFilter);
        }

        public Task<GetSpecificRecord<GetAssetLifecycleDto>> GetAssetLifecycleByIdAsync(long lifecycleId, int tenantId)
        {
            return _repository.GetAssetLifecycleByIdAsync(lifecycleId, tenantId);
        }

        public Task<GetSpecificRecord<GetAssetLifecycleDto>> GetAssetLifecycleByAssetIdAsync(int assetId, int tenantId)
        {
            return _repository.GetAssetLifecycleByAssetIdAsync(assetId, tenantId);
        }

        public Task<GetSpecificRecord<AssetLifecycleMetricsDto>> GetAssetLifecycleMetricsAsync(int tenantId)
        {
            return _repository.GetAssetLifecycleMetricsAsync(tenantId);
        }
    }

    public class AssetFinancialAnalysisService : IAssetFinancialAnalysisService
    {
        private readonly IAssetFinancialAnalysisRepository _repository;

        public AssetFinancialAnalysisService(IAssetFinancialAnalysisRepository repository)
        {
            _repository = repository;
        }

        public Task<CommonResponseModel> AddFinancialAnalysisAsync(AssetFinancialAnalysisDto dto)
        {
            return _repository.AddFinancialAnalysisAsync(dto);
        }

        public Task<CommonResponseModel> DeleteFinancialAnalysisAsync(long analysisId, int tenantId)
        {
            return _repository.DeleteFinancialAnalysisAsync(analysisId, tenantId);
        }

        public Task<GetAllRecord<GetAssetFinancialAnalysisDto>> GetFinancialAnalysesByAssetIdAsync(int assetId, int tenantId, string? analysisType = null)
        {
            return _repository.GetFinancialAnalysesByAssetIdAsync(assetId, tenantId, analysisType);
        }

        public Task<GetAllRecord<GetAssetFinancialAnalysisDto>> GetAllFinancialAnalysesAsync(int tenantId, string? analysisType = null)
        {
            return _repository.GetAllFinancialAnalysesAsync(tenantId, analysisType);
        }
    }
}