using FactoryOperation_IOTDevices.FactoryOpsApp.Application.Interfaces.Services.SuperAdmin.AuditLogs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FactoryOperation_IOTDevices.Controllers.SuperAdmin.Audit
{
    /// <summary>
    /// Audit Management API
    /// Provides access to system audit trails and activity monitoring
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AuditController : ControllerBase
    {
        //private readonly IAuditService _iAuditService;
        //public AuditController(IAuditService iAuditService)
        //{
        //    _iAuditService = iAuditService;
        //}

        ///// <summary>
        ///// Get all audit records
        ///// Retrieves complete system audit trail with all user activities
        ///// </summary>
        ///// <returns>List of all audit records</returns>
        ///// <response code="200">Successfully retrieved audit records</response>
        //[HttpGet]
        //[Route("get-Audits")]
        //public IActionResult GetAuditRecords()
        //{
        //    var result = _iAuditService.GetAuditRecords();
        //    return Ok(result);
        //}

        ///// <summary>
        ///// Get tenant audit trails
        ///// Retrieves audit records specific to a particular tenant
        ///// </summary>
        ///// <param name="TenandId">Tenant identifier</param>
        ///// <returns>Tenant-specific audit records</returns>
        ///// <response code="200">Successfully retrieved tenant audit records</response>
        //[HttpGet]
        //[Route("get-Tenant-AuditTrials")]
        //public IActionResult GetTenantAuditRecords(int TenandId)
        //{
        //    var result = _iAuditService.GetTenantAuditRecords(TenandId);
        //    return Ok(result);
        //}
    }
}