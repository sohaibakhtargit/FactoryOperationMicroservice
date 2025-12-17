using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;
using System.Threading.Tasks;

namespace FactoryOperation_PreventiveMaintenance.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.PreventiveMaintenance
{
    public interface IMaintenanceHistoryService
    {
        Task<CommonResponseModel> AddMaintenanceHistoryAsync(MaintenanceHistoryDto dto);
        Task<CommonResponseModel> UpdateMaintenanceHistoryAsync(MaintenanceHistoryDto dto);
        Task<CommonResponseModel> DeleteMaintenanceHistoryAsync(long maintenanceId, int tenantId);
        Task<GetAllRecord<GetMaintenanceHistoryDto>> GetAllMaintenanceHistoryAsync(int tenantId, string? searchTerm = null, string? statusFilter = null, string? typeFilter = null);
        Task<GetSpecificRecord<GetMaintenanceHistoryDto>> GetMaintenanceHistoryByIdAsync(long maintenanceId, int tenantId);
        Task<GetSpecificRecord<MaintenanceMetricsDto>> GetMaintenanceMetricsAsync(int tenantId);
        Task<GetAllRecord<GetMaintenanceHistoryDto>> GetMaintenanceHistoryByAssetIdAsync(int assetId, int tenantId);
    }
}