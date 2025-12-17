using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FactoryOpsApp.Application.Common;
using System.Threading.Tasks;
using FactoryOpsApp.Application.DTOs;

namespace FactoryOperation_Inventory.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.InventoryManagement
{
    public interface IInventoryTransactionService
    {
        Task<CommonResponseModel> CreateTransactionAsync(InventoryTransactionDto dto);
        Task<CommonResponseModel> UpdateTransactionAsync(InventoryTransactionDto dto);
        Task<CommonResponseModel> DeleteTransactionAsync(int transactionId, int tenantId);
        Task<GetAllRecord<GetInventoryTransactionDto>> GetAllTransactionsAsync(int tenantId);
        Task<GetSpecificRecord<GetInventoryTransactionDto>> GetTransactionByIdAsync(int transactionId, int tenantId);
        Task<GetAllRecord<GetInventoryTransactionDto>> GetTransactionsByPartIdAsync(int partId, int tenantId);
        Task<GetAllRecord<GetInventoryTransactionDto>> GetTransactionsByDateRangeAsync(int tenantId, DateTime fromDate, DateTime toDate);
    }
}
