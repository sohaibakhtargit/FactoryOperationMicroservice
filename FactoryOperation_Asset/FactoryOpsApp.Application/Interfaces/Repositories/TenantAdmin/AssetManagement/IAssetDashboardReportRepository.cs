using FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.AssetManagement
{
    public interface IAssetDashboardReportRepository
    {
        Task<GetAllRecord<DashboardSummaryDto>> GetDashboardSummaryAsync(int tenantId);
    }
}
