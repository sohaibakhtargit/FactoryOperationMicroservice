using System;
using System.Collections.Generic;
using System.Linq;
using FactoryOperation_IOTDevices.FactoryOpsApp.Application.Common;
using FactoryOperation_IOTDevices.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.IOTDevices;
using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Domain.Entities.FactoryOpsTenants;
using FactoryOpsApp_IOTDevices.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.IOTDevices;

namespace FactoryOperation_IOTDevices.FactoryOpsApp.Infrastructure.Implementation.Services.TenantAdmin.IOTDevices
{
    public class AlertRuleService : IAlertRuleService
    {
        private readonly IAlertRuleRepository _repository;

        public AlertRuleService(IAlertRuleRepository repository)
        {
            _repository = repository;
        }

        public Task<CommonResponseModel> AddAlertRuleAsync(AlertRuleDto dto)
        {
            return _repository.AddAlertRuleAsync(dto);
        }

        public Task<CommonResponseModel> UpdateAlertRuleAsync(AlertRuleDto dto)
        {
            return _repository.UpdateAlertRuleAsync(dto);
        }

        public Task<CommonResponseModel> DeleteAlertRuleAsync(int alertRuleId, int tenantId)
        {
            return _repository.DeleteAlertRuleAsync(alertRuleId, tenantId);
        }

        public Task<GetAllRecord<GetAlertRuleDto>> GetAllAlertRulesAsync(int tenantId, AlertStatusEnum? statusFilter = null)
        {
            return _repository.GetAllAlertRulesAsync(tenantId, statusFilter);
        }

        public Task<GetSpecificRecord<GetAlertRuleDto>> GetAlertRuleByIdAsync(int alertRuleId, int tenantId)
        {
            return _repository.GetAlertRuleByIdAsync(alertRuleId, tenantId);
        }
    }
}