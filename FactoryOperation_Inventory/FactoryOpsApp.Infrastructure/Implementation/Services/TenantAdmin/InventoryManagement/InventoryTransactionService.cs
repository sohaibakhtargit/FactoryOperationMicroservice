using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;
using System;
using System.Threading.Tasks;
using FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.InventoryManagement;
using FactoryOperation_Inventory.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.InventoryManagement;

namespace FactoryOpsApp.Infrastructure.Service.TenantAdmin.InventoryManagement
{
    public class InventoryTransactionService : IInventoryTransactionService
    {
        private readonly IInventoryTransactionRepository _repository;

        public InventoryTransactionService(IInventoryTransactionRepository repository)
        {
            _repository = repository;
        }

        public Task<CommonResponseModel> CreateTransactionAsync(InventoryTransactionDto dto)
        {
            return _repository.CreateTransactionAsync(dto);
        }

        public Task<CommonResponseModel> UpdateTransactionAsync(InventoryTransactionDto dto)
        {
            return _repository.UpdateTransactionAsync(dto);
        }

        public Task<CommonResponseModel> DeleteTransactionAsync(int transactionId, int tenantId)
        {
            return _repository.DeleteTransactionAsync(transactionId, tenantId);
        }

        public Task<GetAllRecord<GetInventoryTransactionDto>> GetAllTransactionsAsync(int tenantId)
        {
            return _repository.GetAllTransactionsAsync(tenantId);
        }

        public Task<GetSpecificRecord<GetInventoryTransactionDto>> GetTransactionByIdAsync(int transactionId, int tenantId)
        {
            return _repository.GetTransactionByIdAsync(transactionId, tenantId);
        }

        public Task<GetAllRecord<GetInventoryTransactionDto>> GetTransactionsByPartIdAsync(int partId, int tenantId)
        {
            return _repository.GetTransactionsByPartIdAsync(partId, tenantId);
        }

        public Task<GetAllRecord<GetInventoryTransactionDto>> GetTransactionsByDateRangeAsync(int tenantId, DateTime fromDate, DateTime toDate)
        {
            return _repository.GetTransactionsByDateRangeAsync(tenantId, fromDate, toDate);
        }

        public Task<GetAllRecord<GetInventoryTransactionDto>> GetAllTransactionsAsync(int tenantId, TransactionQueryDto? query = null)
        {
            throw new NotImplementedException();
        }
    }
}