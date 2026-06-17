using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOps_AccessManagementService.FactoryOpsApp.Application.DTOs;
using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.GlobalFilters;
using FactoryOps_AccessManagementService.FactoryOpsApp.Domain.Entities.FactoryOpsTenants;
using FactoryOpsApp.Infrastructure.DBContext;
using Microsoft.EntityFrameworkCore;
using static FactoryOps_AccessManagementService.FactoryOpsApp.Common.CommonConstant;

namespace FactoryOps_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Repository.TenantAdmin.GlobalFilters
{
    public class GlobalFiltersRepository : IGlobalFiltersRepositories
    {
        private readonly TenantDbContextFactory _tenantDbContextFactory;
        private readonly MasterFactoryOpsDbContext _masterDbContext;

        public GlobalFiltersRepository(TenantDbContextFactory tenantDbContextFactory, MasterFactoryOpsDbContext masterDbContext)
        {
            _tenantDbContextFactory = tenantDbContextFactory;
            _masterDbContext = masterDbContext;
        }

        public async Task<CommonResponseModel> CreateAsync(CreateFilterConfigurationDto dto)
        {
            using var tenantDb = _tenantDbContextFactory.GetTenantDbContext(dto.TenantId);

            var entity = new GlobalSearchFilters
            {
                FilterVariable = dto.FilterVariable,
                ModuleName = dto.ModuleName,
                SubModuleName = dto.SubModuleName,
                RoleId = dto.RoleId,
                CreatedBy = dto.CreatedBy,
                CreatedAt = DateTime.UtcNow,

                // RoleId == 1 => Global (TenantId NULL)
                // Else => Tenant specific
                TenantId = dto.RoleId == 1 ? null : dto.TenantId
            };

            await tenantDb.GlobalSearchFilters.AddAsync(entity);
            await tenantDb.SaveChangesAsync();

            return new CommonResponseModel
            {
                StatusCode = StatusCode.Success,
                StatusMessage = "Filter configuration created successfully."
            };
        }


        public async Task<GetAllRecord<GetFilterConfigurationDto>> GetAllAsync(int? roleId, int? tenantId, string? module, string? submodule)
        {
            var response = new GetAllRecord<GetFilterConfigurationDto>();

            using var tenantDb = _tenantDbContextFactory.GetTenantDbContext(tenantId);

            var query = tenantDb.GlobalSearchFilters
                .Where(x => !x.IsDeleted);

            if (roleId == 1)
            {
                query = query.Where(x => x.TenantId == null);
            }
            else
            {
                query = query.Where(x => x.TenantId == tenantId);
            }

            if (!string.IsNullOrWhiteSpace(module))
            {
                query = query.Where(x => x.ModuleName == module);
            }

            if (!string.IsNullOrWhiteSpace(submodule))
            {
                query = query.Where(x => x.SubModuleName == submodule);
            }

            var data = await query
                .OrderByDescending(x => x.CreatedAt)
                .Take(10)
                .Select(x => new GetFilterConfigurationDto
                {
                    PId = x.PId,
                    FilterVariable = x.FilterVariable
                })
                .ToListAsync();

            response.GetAllData = data;
            response.StatusCode = StatusCode.Success;
            response.StatusMessage = "Data fetched successfully.";

            return response;
        }

        public async Task<CommonResponseModel> DeleteAsync(int pId, int tenantId)
        {
            using var tenantDb = _tenantDbContextFactory.GetTenantDbContext(tenantId);

            var entity = await tenantDb.GlobalSearchFilters
                .FirstOrDefaultAsync(x => x.PId == pId && !x.IsDeleted);

            if (entity == null)
            {
                return new CommonResponseModel
                {
                    StatusCode = StatusCode.NotFound,
                    StatusMessage = "Record not found."
                };
            }

            entity.IsDeleted = true;
            entity.IsActive = false;

            await tenantDb.SaveChangesAsync();

            return new CommonResponseModel
            {
                StatusCode = StatusCode.Success,
                StatusMessage = "Record deleted successfully."
            };
        }

        public Task<GetAllRecord<GetFilterConfigurationDto>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<CommonResponseModel> DeleteAsync(int pId)
        {
            throw new NotImplementedException();
        }
    }
}
