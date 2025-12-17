using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using FactoryOperation_Inventory.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.InventoryManagement;
using FactoryOpsApp.Application.DTOs;

namespace FactoryOperation_Inventory.Controllers.TenantAdminContoller.InventoryManagement
{
    /// <summary>
    /// Inventory Transaction Management API
    /// Manages inventory transactions, stock movements, and transaction history
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class InventoryTransactionController : ControllerBase
    {
        private readonly IInventoryTransactionService _transactionService;

        public InventoryTransactionController(IInventoryTransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        /// <summary>
        /// Create transaction
        /// Creates new inventory transaction for stock movement
        /// </summary>
        /// <param name="dto">Inventory transaction data</param>
        /// <returns>Transaction creation result</returns>
        /// <response code="200">Transaction successfully created</response>
        /// <response code="400">Invalid transaction data provided</response>
        [HttpPost("Create-Transaction")]
        public async Task<IActionResult> CreateTransactionAsync([FromBody] InventoryTransactionDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _transactionService.CreateTransactionAsync(dto);
            return Ok(result);
        }

        /// <summary>
        /// Update transaction
        /// Modifies existing inventory transaction details
        /// </summary>
        /// <param name="dto">Updated transaction data</param>
        /// <returns>Update operation result</returns>
        /// <response code="200">Transaction successfully updated</response>
        /// <response code="400">Invalid transaction data provided</response>
        [HttpPost("Update-Transaction")]
        public async Task<IActionResult> UpdateTransactionAsync([FromBody] InventoryTransactionDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _transactionService.UpdateTransactionAsync(dto);
            return Ok(result);
        }

        /// <summary>
        /// Delete transaction
        /// Removes inventory transaction from system
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <param name="transactionId">Transaction identifier</param>
        /// <returns>Deletion operation result</returns>
        /// <response code="200">Transaction successfully deleted</response>
        [HttpPost("Delete-Transaction")]
        public async Task<IActionResult> DeleteTransactionAsync(int tenantId, int transactionId)
        {
            var result = await _transactionService.DeleteTransactionAsync(transactionId, tenantId);
            return Ok(result);
        }

        /// <summary>
        /// Get all transactions
        /// Retrieves complete history of all inventory transactions
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <returns>List of inventory transactions</returns>
        /// <response code="200">Successfully retrieved all transactions</response>
        [HttpGet("Get-AllTransactions")]
        public async Task<IActionResult> GetAllTransactionsAsync(int tenantId)
        {
            var result = await _transactionService.GetAllTransactionsAsync(tenantId);
            return Ok(result);
        }

        /// <summary>
        /// Get transaction by ID
        /// Retrieves specific inventory transaction details
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <param name="transactionId">Transaction identifier</param>
        /// <returns>Transaction details</returns>
        /// <response code="200">Successfully retrieved transaction</response>
        [HttpGet("Get-TransactionById")]
        public async Task<IActionResult> GetTransactionByIdAsync(int tenantId, int transactionId)
        {
            var result = await _transactionService.GetTransactionByIdAsync(transactionId, tenantId);
            return Ok(result);
        }

        /// <summary>
        /// Get transactions by part
        /// Retrieves transaction history for specific inventory part
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <param name="partId">Part identifier</param>
        /// <returns>Part-specific transactions</returns>
        /// <response code="200">Successfully retrieved part transactions</response>
        [HttpGet("Get-TransactionsByPart")]
        public async Task<IActionResult> GetTransactionsByPartIdAsync(int tenantId, int partId)
        {
            var result = await _transactionService.GetTransactionsByPartIdAsync(partId, tenantId);
            return Ok(result);
        }

        /// <summary>
        /// Get transactions by date range
        /// Retrieves transactions within specified date range
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <param name="fromDate">Start date for range</param>
        /// <param name="toDate">End date for range</param>
        /// <returns>Date-filtered transactions</returns>
        /// <response code="200">Successfully retrieved transactions by date range</response>
        [HttpGet("Get-TransactionsByDateRange")]
        public async Task<IActionResult> GetTransactionsByDateRangeAsync(int tenantId, DateTime fromDate, DateTime toDate)
        {
            var result = await _transactionService.GetTransactionsByDateRangeAsync(tenantId, fromDate, toDate);
            return Ok(result);
        }
    }
}