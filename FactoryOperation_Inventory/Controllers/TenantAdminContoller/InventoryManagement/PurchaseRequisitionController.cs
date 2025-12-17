using FactoryOperation_Inventory.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.InventoryManagement;
using FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FactoryOperation_Inventory.Controllers.TenantAdminContoller.InventoryManagement
{
    /// <summary>
    /// Purchase Requisition Management API
    /// Manages purchase requisitions, procurement requests, and purchase order preparation
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class PurchaseRequisitionController : ControllerBase
    {
        private readonly IPurchaseRequisitionService _purchaseRequisitionService;

        public PurchaseRequisitionController(IPurchaseRequisitionService purchaseRequisitionService)
        {
            _purchaseRequisitionService = purchaseRequisitionService;
        }

        /// <summary>
        /// Get purchase requisitions
        /// Retrieves all purchase requisitions for tenant
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <returns>List of purchase requisitions</returns>
        /// <response code="200">Successfully retrieved purchase requisitions</response>
        [HttpGet("get-PurchaseRequisition")]
        //public async Task<ActionResult<GetAllRecord<PurchaseRequisitionResponseDto>>> GetAll(int tenantId)
        //{
        //    var result = await _purchaseRequisitionService.GetAllAsync(tenantId);
        //    return Ok(result);
        //}

        /// <summary>
        /// Get purchase requisition by ID
        /// Retrieves specific purchase requisition details
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <param name="id">Purchase requisition identifier</param>
        /// <returns>Purchase requisition details</returns>
        /// <response code="200">Successfully retrieved purchase requisition</response>
        /// <response code="404">Purchase requisition not found</response>
        [HttpGet("get-PurchaseRequisition-byId")]
        public async Task<ActionResult<GetSpecificRecord<PurchaseRequisitionResponseDto>>> GetById(int tenantId, int id)
        {
            var result = await _purchaseRequisitionService.GetByIdAsync(tenantId, id);
            if (result.StatusCode == "404")
                return NotFound(result);
            return Ok(result);
        }

        [HttpPost("Create-PurchaseRequest")]
        public async Task<ActionResult<CommonResponseModel>> CreatePurchaseRequestAsync([FromBody] PurchaseRequest dto)
        {
            var result = await _purchaseRequisitionService.CreatePurchaseRequestAsync(dto);
            return Ok(result);
        }
    }
}