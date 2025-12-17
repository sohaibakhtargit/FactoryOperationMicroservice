using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.SuperAdmin.Configuration;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.ExceptionLogger;
using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Infrastructure.DBContext;
using static FactoryOps_AccessManagementService.FactoryOpsApp.Common.CommonConstant;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Repository.SuperAdmin.Configuration
{
    public class GlobalDropdownRepository : IGlobalDropdownRepository
    {
        private readonly MasterFactoryOpsDbContext _masterDbcontext;
        private readonly IExceptionLoggerService _exceptionLogger;

        public GlobalDropdownRepository(
            MasterFactoryOpsDbContext masterDbcontext,
            IExceptionLoggerService exceptionLogger)
        {
            _masterDbcontext = masterDbcontext;
            _exceptionLogger = exceptionLogger;
        }

        public GetAllRecord<GetGlobalDropdownDto> GetAllDropdowns(DropdownFilterDto? filter = null)
        {
            GetAllRecord<GetGlobalDropdownDto> response = new();
            try
            {
                var query = _masterDbcontext.GlobalDropdowns
                    .Where(d => d.IsDeleted == false)
                    .AsQueryable();

                if (filter != null)
                {
                    if (!string.IsNullOrEmpty(filter.Module))
                        query = query.Where(d => d.Module == filter.Module);

                    if (!string.IsNullOrEmpty(filter.SubModule))
                        query = query.Where(d => d.SubModule == filter.SubModule);

                    if (filter.TenantId.HasValue)
                        query = query.Where(d => d.TenantId == filter.TenantId || d.TenantId == null);
                }

                var dropdownList = query
                    .OrderBy(d => d.Module)
                    .ThenBy(d => d.SubModule)
                    .ThenBy(d => d.GlobalDropdownsId)
                    .Select(d => new GetGlobalDropdownDto
                    {
                        GlobalDropdownsId = d.GlobalDropdownsId,
                        TenantId = d.TenantId,
                        Module = d.Module,
                        SubModule = d.SubModule,
                        Description = d.Description,
                        IsActive = d.IsActive,
                        CreatedAt = d.CreatedAt,
                        UpdatedAt = d.UpdatedAt
                    }).ToList();

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = GlobalDropdownStatusMessage.GlobalDropdownFetched;
                response.GetAllData = dropdownList;
                return response;
            }
            catch (Exception ex)
            {
                _exceptionLogger.LogExceptionAsync(
                    ex,
                    sourceModule: "GlobalDropdownModule",
                    apiName: "getAll-dropdowns",
                    tenantId: null,
                    userId: null
                );
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = ex.Message;
            }
            return response;
        }

        public GetAllRecord<GetGlobalDropdownDto> GetDropdownsByModule(string module)
        {
            GetAllRecord<GetGlobalDropdownDto> response = new();
            try
            {
                var dropdownList = _masterDbcontext.GlobalDropdowns
                    .Where(d => d.IsDeleted == false && d.Module == module && d.IsActive)
                    .OrderBy(d => d.SubModule)
                    .ThenBy(d => d.GlobalDropdownsId)
                    .Select(d => new GetGlobalDropdownDto
                    {
                        GlobalDropdownsId = d.GlobalDropdownsId,
                        TenantId = d.TenantId,
                        Module = d.Module,
                        SubModule = d.SubModule,
                        Description = d.Description,
                        IsActive = d.IsActive,
                        CreatedAt = d.CreatedAt,
                        UpdatedAt = d.UpdatedAt
                    }).ToList();

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = $"{GlobalDropdownStatusMessage.ModuleDropdownFetched} {module}";
                response.GetAllData = dropdownList;
                return response;
            }
            catch (Exception ex)
            {
                _exceptionLogger.LogExceptionAsync(
                    ex,
                    sourceModule: "GlobalDropdownModule",
                    apiName: "getDropdownsByModule",
                    tenantId: null,
                    userId: null
                );
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = ex.Message;
            }
            return response;
        }

        public GetAllRecord<GetGlobalDropdownDto> GetDropdownsByModuleAndSubModule(string module, string subModule)
        {
            GetAllRecord<GetGlobalDropdownDto> response = new();
            try
            {
                var dropdownList = _masterDbcontext.GlobalDropdowns
                    .Where(d => d.IsDeleted == false &&
                               d.Module == module &&
                               d.SubModule == subModule &&
                               d.IsActive)
                    .OrderBy(d => d.GlobalDropdownsId)
                    .Select(d => new GetGlobalDropdownDto
                    {
                        GlobalDropdownsId = d.GlobalDropdownsId,
                        TenantId = d.TenantId,
                        Module = d.Module,
                        SubModule = d.SubModule,
                        Description = d.Description,
                        IsActive = d.IsActive,
                        CreatedAt = d.CreatedAt,
                        UpdatedAt = d.UpdatedAt
                    }).ToList();

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = $"{GlobalDropdownStatusMessage.ModuleSubModuleDropdown}{module} - {subModule}";
                response.GetAllData = dropdownList;
                return response;
            }
            catch (Exception ex)
            {
                _exceptionLogger.LogExceptionAsync(
                    ex,
                    sourceModule: "GlobalDropdownModule",
                    apiName: "getDropdownsByModuleAndSubModule",
                    tenantId: null,
                    userId: null
                );
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = ex.Message;
            }
            return response;
        }
    }
}
