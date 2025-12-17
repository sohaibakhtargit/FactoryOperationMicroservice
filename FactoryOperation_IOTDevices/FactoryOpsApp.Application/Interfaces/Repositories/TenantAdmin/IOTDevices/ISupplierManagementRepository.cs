
using FactoryOperation_IOTDevices.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;

namespace FactoryOperation_IOTDevices.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.IOTDevices
{
    public interface ISupplierManagementRepository
    {
        Task<CommonResponseModel> AddSupplierAsync(SupplierManagementDto dto);
        Task<CommonResponseModel> UpdateSupplierAsync(SupplierManagementDto dto);
        Task<CommonResponseModel> DeleteSupplierAsync(int supplierId, int tenantId);
        Task<GetAllRecord<GetSupplierManagementDto>> GetAllSuppliersAsync(int tenantId);
        Task<GetSpecificRecord<GetSupplierManagementDto>> GetSupplierByIdAsync(int supplierId, int tenantId);
    }
}