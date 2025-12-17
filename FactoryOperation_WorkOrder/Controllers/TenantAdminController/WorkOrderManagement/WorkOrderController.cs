using FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.WorkOrderManagement;
using FactoryOpsApp.Infrastructure.Implementation.Service.TenantAdmin.WorkOrderManagement;
using Microsoft.AspNetCore.Mvc;
using static FactoryOpsApp.Application.DTOs.WorkOrderCreateDto;

namespace FactoryOperation_Work_Order.Controllers.TenantAdminController.WorkOrderManagement
{
    /// <summary>
    /// Work Order Management API
    /// Manages work orders, work order lifecycle, and work order analytics
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class WorkOrderController : ControllerBase
    {
        private readonly IWorkOrderService _service;

        public WorkOrderController(IWorkOrderService service)
        {
            _service = service;
        }

        /// <summary>
        /// Create work order
        /// Creates new work order with specified details and requirements
        /// </summary>
        /// <param name="dto">Work order creation data</param>
        /// <returns>Work order creation result</returns>
        /// <response code="200">Work order successfully created</response>
        //[Authorize]
        [HttpPost("Create-WorkOrder")]
        public async Task<IActionResult> CreateWorkOrderAsync([FromBody] WorkOrderCreateDto dto)
        {
            var result = await _service.CreateWorkOrderAsync(dto);
            return StatusCode(int.Parse(result.StatusCode), result);
        }

        [HttpPost("bulk-import")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> ImportWorkOrders(
      [FromForm] BulkWorkOrderImportRequest request)
        {
            var result = await _service.ImportBulkWorkOrdersAsync(request);
            return Ok(result);
        }



        /// <summary>
        /// Update work order
        /// Modifies existing work order details and specifications
        /// </summary>
        /// <param name="dto">Updated work order data</param>
        /// <returns>Update operation result</returns>
        /// <response code="200">Work order successfully updated</response>
        [HttpPost("Update-WorkOrder")]
        public async Task<IActionResult> UpdateWorkOrderAsync([FromBody] WorkOrderUpdateDto dto)
        {
            var result = await _service.UpdateWorkOrderAsync(dto);
            return StatusCode(int.Parse(result.StatusCode), result);
        }

        /// <summary>
        /// Delete work order
        /// Removes work order from system
        /// </summary>
        /// <param name="WorkOrderId">Work order identifier</param>
        /// <param name="tenantId">Tenant identifier</param>
        /// <returns>Deletion operation result</returns>
        /// <response code="200">Work order successfully deleted</response>
        [HttpPost("delete-WorkOrder")]
        public async Task<IActionResult> DeleteWorkOrderAsync(int WorkOrderId, int tenantId)
        {
            var result = await _service.DeleteWorkOrderAsync(WorkOrderId, tenantId);
            return StatusCode(int.Parse(result.StatusCode), result);
        }

        /// <summary>
        /// Get all work orders
        /// Retrieves all work orders with optional type filtering
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <param name="workOrderType">Work order type filter</param>
        /// <returns>List of work orders</returns>
        /// <response code="200">Successfully retrieved work orders</response>
        //[HttpGet("Get-all-WorkOrder")]
        //public async Task<IActionResult> GetWorkOrderAllAsync(int tenantId, WorkOrderTypeEnum workOrderType)
        //{
        //    var result = await _service.GetWorkOrderAllAsync(tenantId, workOrderType);
        //    return StatusCode(int.Parse(result.StatusCode), result);
        //}


        [HttpGet("Get-all-WorkOrder")]
        public async Task<IActionResult> GetWorkOrders(int tenantId, WorkOrderTypeEnum type)
        {
            var workOrders = await _service.GetWorkOrderAllAsync(tenantId, type);
            var criticalCount = workOrders.GetAllData.Count(x => x.Priority == PriorityLevel.Critical);
            return Ok(new
            {
                totalCriticalCount = criticalCount,
                getAllData = workOrders.GetAllData
            });
        }


        /// <summary>
        /// Get work order by ID
        /// Retrieves specific work order details
        /// </summary>
        /// <param name="WorkOrderId">Work order identifier</param>
        /// <param name="tenantId">Tenant identifier</param>
        /// <returns>Work order details</returns>
        /// <response code="200">Successfully retrieved work order</response>
        [HttpGet("Get-WorkOrder-ById")]
        public async Task<IActionResult> GetWorkOrderByIdAsync(int WorkOrderId, int tenantId)
        {
            var result = await _service.GetWorkOrderByIdAsync(tenantId, WorkOrderId);
            return StatusCode(int.Parse(result.StatusCode), result);
        }

        /// <summary>
        /// Get labor utilization analytics
        /// Retrieves labor utilization metrics and workforce analytics
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <returns>Labor utilization analytics</returns>
        /// <response code="200">Successfully retrieved labor analytics</response>
        [HttpGet("labor-utilization")]
        public async Task<ActionResult<GetSpecificRecord<LaborAnalyticsDto>>> GetLaborAnalyticsAsync(int tenantId)
        {
            return await _service.GetLaborAnalyticsAsync(tenantId);
        }

        /// <summary>
        /// Get resource usage analytics
        /// Retrieves resource utilization metrics and consumption analytics
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <returns>Resource usage analytics</returns>
        /// <response code="200">Successfully retrieved resource usage analytics</response>
        [HttpGet("resource-usage")]
        public async Task<ActionResult<GetSpecificRecord<ResourceUsageAnalyticsDto>>> GetResourceUsage(int tenantId)
        {
            return await _service.GetResourceUsageAnalyticsAsync(tenantId);
        }

        /// <summary>
        /// Get analytic report
        /// Retrieves comprehensive labor and resource analytics report
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <returns>Combined labor and resource analytics</returns>
        /// <response code="200">Successfully retrieved analytic report</response>
        [HttpGet("analytic-report")]
        public async Task<ActionResult<GetSpecificRecord<LaborResourceAnalyticsDto>>> GetLaborResourceAnalytics(int tenantId)
        {
            return await _service.GetLaborResourceAnalyticsAsync(tenantId);
        }

        [HttpPost("Update-WorkOrderProgress")]
        public async Task<IActionResult> UpdateWorkOrderProgressAsync([FromForm] WorkOrderProgresssUpdateDto dto)
        {
            var result = await _service.UpdateWorkOrderProgressAsync(dto);
            return StatusCode(int.Parse(result.StatusCode), result);
        }
        [HttpGet("Get-all-WorkOrderProgress")]
        public async Task<IActionResult> GetWorkOrderProgressAsync(int tenantId)
        {
            var result = await _service.GetWorkOrderProgressAsync(tenantId);
            return StatusCode(int.Parse(result.StatusCode), result);
        }
    }
}