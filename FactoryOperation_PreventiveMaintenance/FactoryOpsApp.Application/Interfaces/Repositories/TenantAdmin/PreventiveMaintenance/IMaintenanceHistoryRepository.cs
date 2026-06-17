using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;
using System;
using System.Threading.Tasks;

namespace FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.PreventiveMaintenance
{
    public interface IMaintenanceHistoryRepository
    {
        Task<CommonResponseModel> AddMaintenanceHistoryAsync(MaintenanceHistoryDto dto);
        Task<CommonResponseModel> UpdateMaintenanceHistoryAsync(UpdateMaintenanceHistoryDto dto);
        Task<CommonResponseModel> DeleteMaintenanceHistoryAsync(long maintenanceId, int tenantId);
        Task<GetAllRecord<GetMaintenanceHistoryDto>> GetAllMaintenanceHistoryAsync(int tenantId, string? searchTerm = null, string? statusFilter = null, string? typeFilter = null);
        Task<GetSpecificRecord<GetMaintenanceHistoryDto>> GetMaintenanceHistoryByIdAsync(long maintenanceId, int tenantId);
        Task<GetSpecificRecord<MaintenanceMetricsDto>> GetMaintenanceMetricsAsync(int tenantId);
        Task<GetAllRecord<GetMaintenanceHistoryDto>> GetMaintenanceHistoryByAssetIdAsync(int assetId, int tenantId);
    }
}