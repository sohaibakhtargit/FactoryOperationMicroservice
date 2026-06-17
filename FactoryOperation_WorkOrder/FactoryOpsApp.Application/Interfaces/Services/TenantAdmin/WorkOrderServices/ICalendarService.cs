using FactoryOperation_WorkOrder.FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Application.Common;
using System;
using System.Threading.Tasks;

namespace FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.WorkOrderManagement
{
    public interface ICalendarService
    {
        Task<GetSpecificRecord<CalendarDataDto>> GetCalendarAsync(
            int tenantId,
            DateOnly? from = null,
            DateOnly? to = null
        );
    }
}