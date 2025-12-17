using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.SuperAdmin.TenantManagement
{
    public interface ICompanyBrandingInfoService
    {
        Task<CommonResponseModel> CreateCompanyBrandingAsync(CreateCompanyBrandingDto dto);
        Task<CommonResponseModel> UpdateCompanyBrandingAsync(UpdateCompanyBrandingDto dto);
        //Task<CommonResponseModel> DeleteCompanyBrandingAsync(int id, int tenantId);
        GetAllRecord<CompanyBrandingResponseDto> GetAllCompanyBrandings();
        //GetAllRecord<CompanyBrandingResponseDto> GetActiveCompanyBrandings(int tenantId);
    }
}
