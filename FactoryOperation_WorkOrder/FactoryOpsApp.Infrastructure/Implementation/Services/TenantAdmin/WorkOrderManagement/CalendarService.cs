using FactoryOperation_WorkOrder.FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.WorkOrderManagement;
using FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.WorkOrderManagement;
using System;
using System.Threading.Tasks;

namespace FactoryOpsApp.Infrastructure.Implementation.Service.TenantAdmin.WorkOrderManagement
{
    public class CalendarService : ICalendarService
    {
        private readonly ICalendarRepository _repository;

        public CalendarService(ICalendarRepository repository)
        {
            _repository = repository;
        }

        public async Task<GetSpecificRecord<CalendarDataDto>> GetCalendarAsync(
            int tenantId,
            DateOnly? from = null,
            DateOnly? to = null)
        {
            return await _repository.GetCalendarAsync(tenantId, from, to);
        }
    }
}