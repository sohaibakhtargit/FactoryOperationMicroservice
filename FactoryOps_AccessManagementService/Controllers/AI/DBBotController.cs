using FactoryOps_AccessManagementService.FactoryOpsApp.Application.DTOs;
using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.AI;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FactoryOps_AccessManagementService.Controllers.AI
{
    [ApiController]
    [Route("api/dbbot")]
    public class DBBotController : ControllerBase
    {
        private readonly IDBBotService _service;

        public DBBotController(IDBBotService service)
        {
            _service = service;
        }

        [HttpPost("ask")]
        public async Task<IActionResult> Ask(DbBotRequestDto request)
        {
            var result = await _service.AskAsync(request);
            return Ok(result);
        }
    }
}
