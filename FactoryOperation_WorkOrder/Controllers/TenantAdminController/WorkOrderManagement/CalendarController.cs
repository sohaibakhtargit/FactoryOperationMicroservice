using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.WorkOrderManagement;
using Microsoft.AspNetCore.Mvc;

namespace FactoryOperation_Work_Order.Controllers.TenantAdminController.WorkOrderManagement
{
    [Route("api/[controller]")]
    [ApiController]
    public class CalendarController : ControllerBase
    {
        private readonly ICalendarService _service;

        public CalendarController(ICalendarService service)
        {
            _service = service;
        }

        [HttpGet("Get-Calendar")]
        public async Task<IActionResult> GetCalendar(int tenantId, DateOnly? from = null, DateOnly? to = null)
        {
            var result = await _service.GetCalendarAsync(tenantId, from, to);
            return Ok(result);
        }
    }
}