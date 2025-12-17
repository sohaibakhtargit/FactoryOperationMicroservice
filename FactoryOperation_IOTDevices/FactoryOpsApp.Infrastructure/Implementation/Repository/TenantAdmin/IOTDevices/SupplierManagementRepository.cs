using FactoryOperation_IOTDevices.FactoryOpsApp.Application.Common;
using FactoryOperation_IOTDevices.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.IOTDevices;
using FactoryOperation_IOTDevices.FactoryOpsApp.Application.Interfaces.Services.SuperAdmin.AuditLogs;
using FactoryOps_IOTDeviceService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.ExceptionLogger;
using FactoryOpsApp.Application.DTOs;

using FactoryOpsApp.Domain.Entities.FactoryOpsTenants;
using FactoryOpsApp.Infrastructure.DBContext;
using Microsoft.EntityFrameworkCore;
using static FactoryOperation_IOTDevices.FactoryOpsApp.Common.CommonConstant;

namespace FactoryOperation_IOTDevices.FactoryOpsApp.Infrastructure.Implementation.Repository.TenantAdmin.IOTDevices
{
    public class SupplierManagementRepository : ISupplierManagementRepository
    {
        private readonly TenantDbContextFactory _tenantDbContext;
        private readonly IAuditLogService _auditLogger;
        private readonly IExceptionLoggerService _exceptionLogger;

        public SupplierManagementRepository(TenantDbContextFactory tenantDbContext,
                                          IAuditLogService auditLogger,
                                          IExceptionLoggerService exceptionLogger)
        {
            _tenantDbContext = tenantDbContext;
            _auditLogger = auditLogger;
            _exceptionLogger = exceptionLogger;
        }

        public async Task<CommonResponseModel> AddSupplierAsync(SupplierManagementDto dto)
        {
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(dto.TenantId);

                var entity = new SupplierManagement
                {
                    TenantId = dto.TenantId,
                    SupplierName = dto.SupplierName,
                    SupplierCode = dto.SupplierCode,
                    ContactPerson = dto.ContactPerson,
                    UpdatedDate = dto.UpdatedDate,
                    Email = dto.Email,
                    Phone = dto.Phone,
                    Address = dto.Address,
                    LeadTimeDays = dto.LeadTimeDays,
                    PerformanceRating = dto.PerformanceRating,
                    LastPrice = dto.LastPrice,
                    IsActive = dto.IsActive,
                    CreatedBy = dto.CreatedBy ?? dto.TenantId,
                    CreatedDate = DateTime.UtcNow,
                    IsDeleted = false

                };

                await tenantDb.SupplierManagement.AddAsync(entity);
                await tenantDb.SaveChangesAsync();

                await _auditLogger.LogAuditAsync("Create", $"Created Supplier '{entity.SupplierName}'", dto.TenantId, "", "AddSupplierAsync");

                return new CommonResponseModel { StatusCode = StatusCode.Success, StatusMessage = SupplierManagementStatusMessage.CreateSuccess };
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "SupplierManagement-Module", "AddSupplierAsync", dto.TenantId, null);
                return new CommonResponseModel { StatusCode = StatusCode.Error, StatusMessage = $"{SupplierManagementStatusMessage.CreateFailed}: {ex.Message}" };
            }
        }
        public async Task<CommonResponseModel> UpdateSupplierAsync(SupplierManagementDto dto)
        {
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(dto.TenantId);

                var entity = await tenantDb.SupplierManagement
                    .FirstOrDefaultAsync(s => s.SupplierManagementId == dto.SupplierManagementId &&
                                            s.TenantId == dto.TenantId && !s.IsDeleted);

                if (entity == null)
                    return new CommonResponseModel { StatusCode = StatusCode.NotFound, StatusMessage = SupplierManagementStatusMessage.SupplierNotFound };

                entity.SupplierName = dto.SupplierName;
                entity.SupplierCode = dto.SupplierCode;
                entity.ContactPerson = dto.ContactPerson;
                entity.Email = dto.Email;
                entity.Phone = dto.Phone;
                entity.Address = dto.Address;
                entity.LeadTimeDays = dto.LeadTimeDays;
                entity.PerformanceRating = dto.PerformanceRating;
                entity.LastPrice = dto.LastPrice;
                entity.IsActive = dto.IsActive;
                entity.UpdatedBy = dto.UpdatedBy ?? dto.TenantId;
                entity.UpdatedDate = DateTime.UtcNow;

                await tenantDb.SaveChangesAsync();

                await _auditLogger.LogAuditAsync("Update", $"Updated Supplier '{entity.SupplierName}'", dto.TenantId, "", "UpdateSupplierAsync");

                return new CommonResponseModel { StatusCode = StatusCode.Success, StatusMessage = SupplierManagementStatusMessage.UpdateSuccess };
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "SupplierManagement-Module", "UpdateSupplierAsync", dto.TenantId, null);
                return new CommonResponseModel { StatusCode = StatusCode.Error, StatusMessage = $"{SupplierManagementStatusMessage.UpdateFailed}: {ex.Message}" };
            }
        }
        public async Task<CommonResponseModel> DeleteSupplierAsync(int supplierId, int tenantId)
        {
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                var entity = await tenantDb.SupplierManagement
                    .FirstOrDefaultAsync(s => s.SupplierManagementId == supplierId &&
                                            s.TenantId == tenantId && !s.IsDeleted);

                if (entity == null)
                    return new CommonResponseModel { StatusCode = StatusCode.NotFound, StatusMessage = SupplierManagementStatusMessage.SupplierNotFound };

                entity.IsDeleted = true;
                entity.IsActive = false;
                entity.DeletedBy = tenantId;
                entity.DeletedDate = DateTime.UtcNow;

                await tenantDb.SaveChangesAsync();

                await _auditLogger.LogAuditAsync("Delete", $"Deleted Supplier '{entity.SupplierName}'", tenantId, "", "DeleteSupplierAsync");

                return new CommonResponseModel { StatusCode = StatusCode.Success, StatusMessage = SupplierManagementStatusMessage.DeleteSuccess };
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "SupplierManagement-Module", "DeleteSupplierAsync", tenantId, null);
                return new CommonResponseModel { StatusCode = StatusCode.Error, StatusMessage = $"{SupplierManagementStatusMessage.DeleteFailed}: {ex.Message}" };
            }
        }
        public async Task<GetAllRecord<GetSupplierManagementDto>> GetAllSuppliersAsync(int tenantId)
        {
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                var entities = await tenantDb.SupplierManagement
                    .Include(s => s.ReorderRules)
                    .Include(s => s.PurchaseRequisitions)
                    .Where(s => s.TenantId == tenantId && !s.IsDeleted)
                    .OrderBy(s => s.SupplierName)
                    .ToListAsync();

                var dtoList = entities.Select(s => new GetSupplierManagementDto
                {
                    SupplierManagementId = s.SupplierManagementId,
                    TenantId = s.TenantId ?? 0,
                    SupplierName = s.SupplierName,
                    SupplierCode = s.SupplierCode,
                    ContactPerson = s.ContactPerson,
                    Email = s.Email,
                    Phone = s.Phone,
                    Address = s.Address,
                    LeadTimeDays = s.LeadTimeDays,
                    PerformanceRating = s.PerformanceRating,
                    LastPrice = s.LastPrice,
                    IsActive = s.IsActive,
                    CreatedBy = s.CreatedBy,
                    UpdatedBy = s.UpdatedBy,
                    CreatedDate = s.CreatedDate,
                    UpdatedDate = s.UpdatedDate,
                    TotalReorderRules = s.ReorderRules?.Count(r => !r.IsDeleted) ?? 0,
                    TotalPurchaseRequisitions = s.PurchaseRequisitions?.Count(pr => !pr.IsDeleted) ?? 0
                }).ToList();

                return new GetAllRecord<GetSupplierManagementDto>
                {
                    StatusCode = StatusCode.Success,
                    StatusMessage = SupplierManagementStatusMessage.FetchAllSuccess,
                    GetAllData = dtoList
                };
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "SupplierManagement-Module", "GetAllSuppliersAsync", tenantId, null);
                return new GetAllRecord<GetSupplierManagementDto>
                {
                    StatusCode = StatusCode.Error,
                    StatusMessage = $"{SupplierManagementStatusMessage.FetchAllFailed}: {ex.Message}"
                };
            }
        }
        public async Task<GetSpecificRecord<GetSupplierManagementDto>> GetSupplierByIdAsync(int supplierId, int tenantId)
        {
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                var entity = await tenantDb.SupplierManagement
                    .Include(s => s.ReorderRules)
                    .Include(s => s.PurchaseRequisitions)
                    .FirstOrDefaultAsync(s => s.SupplierManagementId == supplierId &&
                                            s.TenantId == tenantId && !s.IsDeleted);

                if (entity == null)
                    return new GetSpecificRecord<GetSupplierManagementDto>
                    {
                        StatusCode = StatusCode.NotFound,
                        StatusMessage = SupplierManagementStatusMessage.SupplierNotFound
                    };

                var dto = new GetSupplierManagementDto
                {
                    SupplierManagementId = entity.SupplierManagementId,
                    TenantId = entity.TenantId ?? 0,
                    SupplierName = entity.SupplierName,
                    SupplierCode = entity.SupplierCode,
                    ContactPerson = entity.ContactPerson,
                    Email = entity.Email,
                    Phone = entity.Phone,
                    Address = entity.Address,
                    LeadTimeDays = entity.LeadTimeDays,
                    PerformanceRating = entity.PerformanceRating,
                    LastPrice = entity.LastPrice,
                    IsActive = entity.IsActive,
                    CreatedBy = entity.CreatedBy,
                    UpdatedBy = entity.UpdatedBy,
                    CreatedDate = entity.CreatedDate,
                    UpdatedDate = entity.UpdatedDate,
                    TotalReorderRules = entity.ReorderRules?.Count(r => !r.IsDeleted) ?? 0,
                    TotalPurchaseRequisitions = entity.PurchaseRequisitions?.Count(pr => !pr.IsDeleted) ?? 0
                };

                return new GetSpecificRecord<GetSupplierManagementDto>
                {
                    StatusCode = StatusCode.Success,
                    StatusMessage = SupplierManagementStatusMessage.FetchByIdSuccess,
                    Data = dto
                };
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "SupplierManagement-Module", "GetSupplierByIdAsync", tenantId, null);
                return new GetSpecificRecord<GetSupplierManagementDto>
                {
                    StatusCode = StatusCode.Error,
                    StatusMessage = $"{SupplierManagementStatusMessage.FetchByIdFailed}: {ex.Message}"
                };
            }
        }
    }
}