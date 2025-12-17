using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.SuperAdmin.TenantManagement;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.SuperAdmin.TenantManagement;
using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Service.SuperAdmin.TenantManagement
{
    public class CompanyBrandingInfoService : ICompanyBrandingInfoService
    {
        private readonly ICompanyBrandingInfoRepository _repository;

        public CompanyBrandingInfoService(ICompanyBrandingInfoRepository repository)
        {
            _repository = repository;
        }

        public async Task<CommonResponseModel> CreateCompanyBrandingAsync(CreateCompanyBrandingDto dto)
            => await _repository.CreateCompanyBrandingAsync(dto);

        public async Task<CommonResponseModel> UpdateCompanyBrandingAsync(UpdateCompanyBrandingDto dto)
            => await _repository.UpdateCompanyBrandingAsync(dto);

        public GetAllRecord<CompanyBrandingResponseDto> GetAllCompanyBrandings()
            => _repository.GetAllCompanyBrandings();
    }
}
