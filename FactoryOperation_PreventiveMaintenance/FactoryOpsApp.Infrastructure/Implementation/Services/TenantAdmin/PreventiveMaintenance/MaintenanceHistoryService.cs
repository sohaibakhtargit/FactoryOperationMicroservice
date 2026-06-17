using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;
using System.Threading.Tasks;
using FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.PreventiveMaintenance;
using FactoryOperation_PreventiveMaintenance.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.PreventiveMaintenance;

namespace FactoryOpsApp.Infrastructure.Service.TenantAdmin.PreventiveMaintenance
{
    public class MaintenanceHistoryService : IMaintenanceHistoryService
    {
        private readonly IMaintenanceHistoryRepository _repository;

        public MaintenanceHistoryService(IMaintenanceHistoryRepository repository)
        {
            _repository = repository;
        }

        public Task<CommonResponseModel> AddMaintenanceHistoryAsync(MaintenanceHistoryDto dto)
        {
            return _repository.AddMaintenanceHistoryAsync(dto);
        }

        public Task<CommonResponseModel> UpdateMaintenanceHistoryAsync(UpdateMaintenanceHistoryDto dto)
        {
            return _repository.UpdateMaintenanceHistoryAsync(dto);
        }

        public Task<CommonResponseModel> DeleteMaintenanceHistoryAsync(long maintenanceId, int tenantId)
        {
            return _repository.DeleteMaintenanceHistoryAsync(maintenanceId, tenantId);
        }

        public Task<GetAllRecord<GetMaintenanceHistoryDto>> GetAllMaintenanceHistoryAsync(int tenantId, string? searchTerm = null, string? statusFilter = null, string? typeFilter = null)
        {
            return _repository.GetAllMaintenanceHistoryAsync(tenantId, searchTerm, statusFilter, typeFilter);
        }

        public Task<GetSpecificRecord<GetMaintenanceHistoryDto>> GetMaintenanceHistoryByIdAsync(long maintenanceId, int tenantId)
        {
            return _repository.GetMaintenanceHistoryByIdAsync(maintenanceId, tenantId);
        }

        public Task<GetSpecificRecord<MaintenanceMetricsDto>> GetMaintenanceMetricsAsync(int tenantId)
        {
            return _repository.GetMaintenanceMetricsAsync(tenantId);
        }
        public Task<GetAllRecord<GetMaintenanceHistoryDto>> GetMaintenanceHistoryByAssetIdAsync(int assetId, int tenantId)
        {
            return _repository.GetMaintenanceHistoryByAssetIdAsync(assetId, tenantId);
        }
    }
}