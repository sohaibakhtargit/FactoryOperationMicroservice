using FactoryOperation_WorkOrder.FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;
using static FactoryOpsApp.Application.DTOs.WorkOrderCreateDto;
using System;

namespace FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.WorkOrderManagement
{
    public interface ICalendarRepository
    {
        Task<GetSpecificRecord<CalendarDataDto>> GetCalendarAsync(int tenantId, DateOnly? from = null, DateOnly? to = null);
    }
}
