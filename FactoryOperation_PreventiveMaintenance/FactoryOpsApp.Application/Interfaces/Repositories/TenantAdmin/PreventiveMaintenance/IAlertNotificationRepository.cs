using FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Domain.Entities.FactoryOpsTenants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.PreventiveMaintenance
{
    public interface IAlertNotificationRepository
    {
        Task<CommonResponseModel> AddAlertNotificationAsync(AlertNotificationDTO dto);
        Task<CommonResponseModel> UpdateAlertNotificationAsync(AlertNotificationDTO dto);
        Task<CommonResponseModel> DeleteAlertNotificationAsync(int alertId, int tenantId);
        Task<GetAllRecord<AlertNotificationDTO>> GetAllAlertNotificationAsync(int tenantId);
        Task<GetSpecificRecord<AlertNotificationDTO>> GetAlertNotificationByIdAsync(int alertId, int tenantId);
    }
}
