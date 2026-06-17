using FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.AssetManagement
{
    public interface IAssetDashboardReportService
    {
        Task<GetAllRecord<DashboardSummaryDto>> GetDashboardSummaryAsync(int tenantId);
        Task<GetAllRecord<DashboardDataDto>> FetchDashboardDataAsync(int tenantId);

    }
}
