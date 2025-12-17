using FactoryOperation_PreventiveMaintenance.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.PreventiveMaintenance;
using FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.PreventiveMaintenance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactoryOpsApp.Infrastructure.Service.TenantAdmin.PreventiveMaintenance
{
    public class AlertNotificationService : IAlertNotificationService
    {
        private readonly IAlertNotificationRepository _alertNotificationRepository;
        public AlertNotificationService(IAlertNotificationRepository alertNotificationRepository)
        {
            _alertNotificationRepository = alertNotificationRepository;
        }
        public async Task<CommonResponseModel> AddAlertNotificationAsync(AlertNotificationDTO dto)
        {
           return  await _alertNotificationRepository.AddAlertNotificationAsync(dto);
        }

        public async Task<CommonResponseModel> DeleteAlertNotificationAsync(int alertId, int tenantId)
        {
            return await _alertNotificationRepository.DeleteAlertNotificationAsync(alertId,tenantId);
        }

        public async Task<CommonResponseModel> UpdateAlertNotificationAsync(AlertNotificationDTO dto)
        {
            return await _alertNotificationRepository.UpdateAlertNotificationAsync(dto);
        }

        public async Task<GetSpecificRecord<AlertNotificationDTO>> GetAlertNotificationByIdAsync(int alertId, int tenantId)
        {
            return await _alertNotificationRepository.GetAlertNotificationByIdAsync(alertId, tenantId);
        }
    
        public async Task<GetAllRecord<AlertNotificationDTO>> GetAllAlertNotificationsAsync(int tenantId)
        {
            return await _alertNotificationRepository.GetAllAlertNotificationAsync(tenantId);
        }
    }
    
}
