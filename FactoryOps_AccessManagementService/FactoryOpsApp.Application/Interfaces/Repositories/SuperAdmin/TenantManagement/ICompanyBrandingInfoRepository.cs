using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.SuperAdmin.TenantManagement
{
    public interface ICompanyBrandingInfoRepository
    {
        Task<CommonResponseModel> CreateCompanyBrandingAsync(CreateCompanyBrandingDto dto);
        Task<CommonResponseModel> UpdateCompanyBrandingAsync(UpdateCompanyBrandingDto dto);
        GetAllRecord<CompanyBrandingResponseDto> GetAllCompanyBrandings();
    }
}
