using FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactoryOperation_PreventiveMaintenance.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.PreventiveMaintenance
{
    public interface IAlertNotificationService
    {
        Task<CommonResponseModel> AddAlertNotificationAsync(AlertNotificationDTO dto);
        Task<CommonResponseModel> UpdateAlertNotificationAsync(AlertNotificationDTO dto);
        Task<CommonResponseModel> DeleteAlertNotificationAsync(int alertId, int tenantId);
        Task<GetAllRecord<AlertNotificationDTO>> GetAllAlertNotificationsAsync(int tenantId);
        Task<GetSpecificRecord<AlertNotificationDTO>> GetAlertNotificationByIdAsync(int alertId, int tenantId);
    }
}
