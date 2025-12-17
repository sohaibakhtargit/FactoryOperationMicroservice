using Microsoft.AspNetCore.Mvc;
using FactoryOpsApp.Application.DTOs;
using FactoryOperation_IOTDevices.FactoryOpsApp.Application.Common;
using FactoryOpsApp_IOTDevices.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.IOTDevices;

namespace FactoryOpsApp.API.Controllers.TenantAdminContoller.IOTDevices
{
    /// <summary>
    /// Supplier Management API
    /// Manages supplier information, vendor relationships, and supplier operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class SupplierManagementController : ControllerBase
    {
        private readonly ISupplierManagementService _supplierManagementService;

        public SupplierManagementController(ISupplierManagementService supplierManagementService)
        {
            _supplierManagementService = supplierManagementService;
        }

        /// <summary>
        /// Create supplier
        /// Creates new supplier record and vendor information
        /// </summary>
        /// <param name="dto">Supplier data</param>
        /// <returns>Supplier creation result</returns>
        /// <response code="200">Supplier successfully created</response>
        /// <response code="400">Invalid supplier data provided</response>
        [HttpPost("CreateSupplier")]
        public async Task<ActionResult<CommonResponseModel>> CreateSupplier([FromBody] SupplierManagementDto dto)
        {
            var result = await _supplierManagementService.AddSupplierAsync(dto);
            return result.StatusCode == "200" ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Update supplier
        /// Modifies existing supplier information and details
        /// </summary>
        /// <param name="dto">Updated supplier data</param>
        /// <returns>Update operation result</returns>
        /// <response code="200">Supplier successfully updated</response>
        /// <response code="400">Invalid supplier data provided</response>
        [HttpPost("UpdateSupplier")]
        public async Task<ActionResult<CommonResponseModel>> UpdateSupplier([FromBody] SupplierManagementDto dto)
        {
            var result = await _supplierManagementService.UpdateSupplierAsync(dto);
            return result.StatusCode == "200" ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Delete supplier
        /// Removes supplier from vendor management system
        /// </summary>
        /// <param name="supplierId">Supplier identifier</param>
        /// <param name="tenantId">Tenant identifier</param>
        /// <returns>Deletion operation result</returns>
        /// <response code="200">Supplier successfully deleted</response>
        /// <response code="400">Invalid deletion request</response>
        [HttpPost("DeleteSupplier")]
        public async Task<ActionResult<CommonResponseModel>> DeleteSupplier(int supplierId, int tenantId)
        {
            var result = await _supplierManagementService.DeleteSupplierAsync(supplierId, tenantId);
            return result.StatusCode == "200" ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Get suppliers
        /// Retrieves all suppliers for tenant
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <returns>List of suppliers</returns>
        /// <response code="200">Successfully retrieved suppliers</response>
        /// <response code="400">Invalid retrieval request</response>
        [HttpGet("GetSupplier")]
        public async Task<ActionResult<GetAllRecord<GetSupplierManagementDto>>> GetSuppliers(int tenantId)
        {
            var result = await _supplierManagementService.GetAllSuppliersAsync(tenantId);
            return result.StatusCode == "200" ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Get supplier details
        /// Retrieves specific supplier information and details
        /// </summary>
        /// <param name="supplierId">Supplier identifier</param>
        /// <param name="tenantId">Tenant identifier</param>
        /// <returns>Supplier details</returns>
        /// <response code="200">Successfully retrieved supplier details</response>
        /// <response code="404">Supplier not found</response>
        [HttpGet("details/{supplierId}/{tenantId}")]
        public async Task<ActionResult<GetSpecificRecord<GetSupplierManagementDto>>> GetSupplier(int supplierId, int tenantId)
        {
            var result = await _supplierManagementService.GetSupplierByIdAsync(supplierId, tenantId);
            return result.StatusCode == "200" ? Ok(result) : NotFound(result);
        }
    }
}