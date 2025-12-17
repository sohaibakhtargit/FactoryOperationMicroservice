using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.SuperAdmin.Configuration
{
    public interface IGlobalDropdownService
    {
        public GetAllRecord<GetGlobalDropdownDto> GetAllDropdowns(DropdownFilterDto? filter = null);
        public GetAllRecord<GetGlobalDropdownDto> GetDropdownsByModule(string module);
        public GetAllRecord<GetGlobalDropdownDto> GetDropdownsByModuleAndSubModule(string module, string subModule);
    }
}
