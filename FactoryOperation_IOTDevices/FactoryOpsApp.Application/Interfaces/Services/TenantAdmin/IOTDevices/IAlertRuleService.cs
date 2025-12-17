using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FactoryOperation_IOTDevices.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Domain.Entities.FactoryOpsTenants;

namespace FactoryOpsApp_IOTDevices.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.IOTDevices
{
    public interface IAlertRuleService
    {
        Task<CommonResponseModel> AddAlertRuleAsync(AlertRuleDto dto);
        Task<CommonResponseModel> UpdateAlertRuleAsync(AlertRuleDto dto);
        Task<CommonResponseModel> DeleteAlertRuleAsync(int alertRuleId, int tenantId);
        Task<GetAllRecord<GetAlertRuleDto>> GetAllAlertRulesAsync(int tenantId, AlertStatusEnum? statusFilter = null);
        Task<GetSpecificRecord<GetAlertRuleDto>> GetAlertRuleByIdAsync(int alertRuleId, int tenantId);
    }
}