using FactoryOperation_IOTDevices.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;

namespace FactoryOpsApp_IOTDevices.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.IOTDevices
{
    public interface ISupplierManagementService
    {
        Task<CommonResponseModel> AddSupplierAsync(SupplierManagementDto dto);
        Task<CommonResponseModel> UpdateSupplierAsync(SupplierManagementDto dto);
        Task<CommonResponseModel> DeleteSupplierAsync(int supplierId, int tenantId);
        Task<GetAllRecord<GetSupplierManagementDto>> GetAllSuppliersAsync(int tenantId);
        Task<GetSpecificRecord<GetSupplierManagementDto>> GetSupplierByIdAsync(int supplierId, int tenantId);
    }
}