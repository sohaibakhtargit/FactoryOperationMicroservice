using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.SuperAdmin.Configuration;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.SuperAdmin.Configuration;
using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Service.SuperAdmin.Configuration
{
    public class GlobalDropdownService : IGlobalDropdownService
    {
        private readonly IGlobalDropdownRepository _repository;

        public GlobalDropdownService(IGlobalDropdownRepository repository)
        {
            _repository = repository;
        }

        public GetAllRecord<GetGlobalDropdownDto> GetAllDropdowns(DropdownFilterDto? filter = null)
        {
            return _repository.GetAllDropdowns(filter);
        }

        public GetAllRecord<GetGlobalDropdownDto> GetDropdownsByModule(string module)
        {
            return _repository.GetDropdownsByModule(module);
        }

        public GetAllRecord<GetGlobalDropdownDto> GetDropdownsByModuleAndSubModule(string module, string subModule)
        {
            return _repository.GetDropdownsByModuleAndSubModule(module, subModule);
        }
    }
}