using FactoryOperation_IOTDevices.FactoryOpsApp.Application.Common;
using FactoryOperation_IOTDevices.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.IOTDevices;
using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp_IOTDevices.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.IOTDevices;

namespace FactoryOperation_IOTDevices.FactoryOpsApp.Infrastructure.Implementation.Services.TenantAdmin.IOTDevices
{
    public class SupplierManagementService : ISupplierManagementService
    {
        private readonly ISupplierManagementRepository _repository;

        public SupplierManagementService(ISupplierManagementRepository repository)
        {
            _repository = repository;
        }

        public Task<CommonResponseModel> AddSupplierAsync(SupplierManagementDto dto)
        {
            return _repository.AddSupplierAsync(dto);
        }

        public Task<CommonResponseModel> UpdateSupplierAsync(SupplierManagementDto dto)
        {
            return _repository.UpdateSupplierAsync(dto);
        }

        public Task<CommonResponseModel> DeleteSupplierAsync(int supplierId, int tenantId)
        {
            return _repository.DeleteSupplierAsync(supplierId, tenantId);
        }

        public Task<GetAllRecord<GetSupplierManagementDto>> GetAllSuppliersAsync(int tenantId)
        {
            return _repository.GetAllSuppliersAsync(tenantId);
        }

        public Task<GetSpecificRecord<GetSupplierManagementDto>> GetSupplierByIdAsync(int supplierId, int tenantId)
        {
            return _repository.GetSupplierByIdAsync(supplierId, tenantId);
        }
    }
}
