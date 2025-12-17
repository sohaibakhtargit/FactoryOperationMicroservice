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
    public class ReorderRuleRepository : IReorderRuleRepository
    {
        private readonly TenantDbContextFactory _tenantDbContextFactory;
        private readonly IExceptionLoggerService _exceptionLogger;
        private readonly IAuditLogService _auditLogger;

        public ReorderRuleRepository(
            TenantDbContextFactory tenantDbContextFactory,
            IExceptionLoggerService exceptionLogger,
            IAuditLogService auditLogger)
        {
            _tenantDbContextFactory = tenantDbContextFactory;
            _exceptionLogger = exceptionLogger;
            _auditLogger = auditLogger;
        }

        public async Task<CommonResponseModel> CreateAsync(CreateReorderRuleDto dto)
        {
            var response = new CommonResponseModel();
            try
            {
                using var context = _tenantDbContextFactory.GetTenantDbContext(dto.TenantId);

                var inventory = await context.Inventory.FindAsync(dto.InventoryId);
                if (inventory == null)
                {
                    response.StatusCode = StatusCode.NotFound;
                    response.StatusMessage = ReorderRuleStatusMessage.InventoryNotFound;
                    return response;
                }

                var supplier = await context.SupplierManagement
                    .FirstOrDefaultAsync(s => s.SupplierManagementId == dto.SupplierManagementId
                                           && s.TenantId == dto.TenantId
                                           && s.IsActive
                                           && !s.IsDeleted);

                if (supplier == null)
                {
                    response.StatusCode = StatusCode.NotFound;
                    response.StatusMessage = ReorderRuleStatusMessage.SupplierNotFound;
                    return response;
                }

                var reorderRule = new ReorderRule
                {
                    TenantId = dto.TenantId,
                    InventoryId = dto.InventoryId,
                    MinThreshold = dto.MinThreshold,
                    ReorderQuantity = dto.ReorderQuantity,
                    SupplierManagementId = dto.SupplierManagementId,
                    LeadTimeDays = dto.LeadTimeDays,
                    LastPrice = dto.LastPrice,
                    Priority = dto.Priority,
                    AutoGenerateOrders = dto.AutoGenerateOrders,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedBy = dto.CreatedBy,
                    CreatedDate = DateTime.UtcNow
                };

                context.ReorderRules.Add(reorderRule);
                await context.SaveChangesAsync();

                if (inventory.QuantityAvailable <= dto.MinThreshold)
                {
                    await GeneratePurchaseRequisitionAsync(dto.TenantId, reorderRule, dto.CreatedBy, context);
                }

                await _auditLogger.LogAuditAsync(
                    action: "Create",
                    details: $"Created reorder rule for {inventory.ItemName}",
                    tenantId: dto.TenantId,
                    email: "",
                    eventType: "ReorderRule"
                );

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = ReorderRuleStatusMessage.CreateSuccess;
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(
                    ex, "ReorderRuleRepository", "CreateAsync", dto.TenantId, null);
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"{ReorderRuleStatusMessage.CreateFailed}: {ex.Message}";
            }
            return response;
        }
        public async Task<CommonResponseModel> UpdateAsync(UpdateReorderRuleDto dto)
        {
            var response = new CommonResponseModel();
            try
            {
                using var context = _tenantDbContextFactory.GetTenantDbContext(dto.TenantId);

                var reorderRule = await context.ReorderRules
                    .Include(r => r.Inventory)
                    .FirstOrDefaultAsync(r => r.ReorderRuleId == dto.ReorderRuleId && !r.IsDeleted);

                if (reorderRule == null)
                {
                    response.StatusCode = StatusCode.NotFound;
                    response.StatusMessage = ReorderRuleStatusMessage.RuleNotFound;
                    return response;
                }

                var oldMinThreshold = reorderRule.MinThreshold;
                var oldAutoGenerate = reorderRule.AutoGenerateOrders;

                if (dto.MinThreshold.HasValue) reorderRule.MinThreshold = dto.MinThreshold.Value;
                if (dto.ReorderQuantity.HasValue) reorderRule.ReorderQuantity = dto.ReorderQuantity.Value;
                if (dto.SupplierManagementId.HasValue) reorderRule.SupplierManagementId = dto.SupplierManagementId.Value;
                if (dto.Priority != null) reorderRule.Priority = dto.Priority;
                if (dto.AutoGenerateOrders.HasValue) reorderRule.AutoGenerateOrders = dto.AutoGenerateOrders.Value;
                if (dto.IsActive.HasValue) reorderRule.IsActive = dto.IsActive.Value;

                if (dto.LeadTimeDays.HasValue) reorderRule.LeadTimeDays = dto.LeadTimeDays.Value;
                if (dto.LastPrice.HasValue) reorderRule.LastPrice = dto.LastPrice.Value;

                reorderRule.UpdatedBy = dto.UpdatedBy;
                reorderRule.UpdatedDate = DateTime.UtcNow;

                await context.SaveChangesAsync();

                if (dto.MinThreshold.HasValue && dto.MinThreshold.Value < oldMinThreshold &&
                     reorderRule.Inventory?.QuantityAvailable <= reorderRule.MinThreshold ||
                    dto.AutoGenerateOrders.HasValue && dto.AutoGenerateOrders.Value && !oldAutoGenerate &&
                     reorderRule.Inventory?.QuantityAvailable <= reorderRule.MinThreshold)
                {
                    await GeneratePurchaseRequisitionAsync(dto.TenantId, reorderRule, dto.UpdatedBy, context);
                }

                await _auditLogger.LogAuditAsync(
                    action: "Update",
                    details: $"Updated reorder rule ID: {dto.ReorderRuleId}",
                    tenantId: dto.TenantId,
                    email: "",
                    eventType: "ReorderRule"
                );

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = ReorderRuleStatusMessage.UpdateSuccess;
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(
                    ex, "ReorderRuleRepository", "UpdateAsync", dto.TenantId, null);
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"{ReorderRuleStatusMessage.UpdateFailed}: {ex.Message}";
            }
            return response;
        }
        public async Task<CommonResponseModel> DeleteAsync(int tenantId, int id, int deletedBy)
        {
            var response = new CommonResponseModel();
            try
            {
                using var context = _tenantDbContextFactory.GetTenantDbContext(tenantId);

                var reorderRule = await context.ReorderRules.FindAsync(id);
                if (reorderRule == null || reorderRule.IsDeleted)
                {
                    response.StatusCode = StatusCode.NotFound;
                    response.StatusMessage = ReorderRuleStatusMessage.RuleNotFound;
                    return response;
                }

                reorderRule.IsDeleted = true;
                reorderRule.DeletedBy = deletedBy;
                reorderRule.DeletedDate = DateTime.UtcNow;

                await context.SaveChangesAsync();

                await _auditLogger.LogAuditAsync(
                    action: "Delete",
                    details: $"Deleted reorder rule ID: {id}",
                    tenantId: tenantId,
                    email: "",
                    eventType: "ReorderRule"
                );

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = ReorderRuleStatusMessage.DeleteSuccess;
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(
                    ex, "ReorderRuleRepository", "DeleteAsync", tenantId, null);
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"{ReorderRuleStatusMessage.DeleteFailed}: {ex.Message}";
            }
            return response;
        }
        public async Task<GetAllRecord<ReorderRuleResponseDto>> GetAllAsync(int tenantId)
        {
            var response = new GetAllRecord<ReorderRuleResponseDto>();
            try
            {
                using var context = _tenantDbContextFactory.GetTenantDbContext(tenantId);

                var reorderRules = await context.ReorderRules
                    .Include(r => r.Inventory)
                    .Include(r => r.SupplierManagement)
                    .Where(r => !r.IsDeleted)
                    .ToListAsync();

                response.GetAllData = reorderRules.Select(r => new ReorderRuleResponseDto
                {
                    ReorderRuleId = r.ReorderRuleId,
                    TenantId = r.TenantId,
                    InventoryId = r.InventoryId,
                    PartCode = r.Inventory?.ItemCode,
                    PartName = r.Inventory?.ItemName,
                    MinThreshold = r.MinThreshold,
                    ReorderQuantity = r.ReorderQuantity,
                    SupplierManagementId = r.SupplierManagementId,
                    SupplierName = r.SupplierManagement?.SupplierName,
                    LeadTimeDays = r.LeadTimeDays,
                    LastPrice = r.LastPrice,
                    Priority = r.Priority,
                    AutoGenerateOrders = r.AutoGenerateOrders,
                    IsActive = r.IsActive,
                    CreatedDate = r.CreatedDate
                }).ToList();

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = ReorderRuleStatusMessage.FetchAllSuccess;
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(
                    ex, "ReorderRuleRepository", "GetAllAsync", tenantId, null);
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"{ReorderRuleStatusMessage.FetchAllFailed}: {ex.Message}";
            }
            return response;
        }
        public async Task<GetSpecificRecord<ReorderRuleResponseDto>> GetByIdAsync(int tenantId, int id)
        {
            var response = new GetSpecificRecord<ReorderRuleResponseDto>();
            try
            {
                using var context = _tenantDbContextFactory.GetTenantDbContext(tenantId);

                var reorderRule = await context.ReorderRules
                    .Include(r => r.Inventory)
                    .Include(r => r.SupplierManagement)
                    .FirstOrDefaultAsync(r => r.ReorderRuleId == id && !r.IsDeleted);

                if (reorderRule == null)
                {
                    response.StatusCode = StatusCode.NotFound;
                    response.StatusMessage = ReorderRuleStatusMessage.RuleNotFound;
                    return response;
                }

                response.Data = new ReorderRuleResponseDto
                {
                    ReorderRuleId = reorderRule.ReorderRuleId,
                    TenantId = reorderRule.TenantId,
                    InventoryId = reorderRule.InventoryId,
                    PartCode = reorderRule.Inventory?.ItemCode,
                    PartName = reorderRule.Inventory?.ItemName,
                    MinThreshold = reorderRule.MinThreshold,
                    ReorderQuantity = reorderRule.ReorderQuantity,
                    SupplierManagementId = reorderRule.SupplierManagementId,
                    SupplierName = reorderRule.SupplierManagement?.SupplierName,
                    LeadTimeDays = reorderRule.LeadTimeDays,
                    LastPrice = reorderRule.LastPrice,
                    Priority = reorderRule.Priority,
                    AutoGenerateOrders = reorderRule.AutoGenerateOrders,
                    IsActive = reorderRule.IsActive,
                    CreatedDate = reorderRule.CreatedDate
                };

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = ReorderRuleStatusMessage.FetchByIdSuccess;
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(
                    ex, "ReorderRuleRepository", "GetByIdAsync", tenantId, null);
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"{ReorderRuleStatusMessage.FetchByIdFailed}: {ex.Message}";
            }
            return response;
        }
        public async Task<GetSpecificRecord<AutomatedReplenishmentDashboardDto>> GetDashboardDataAsync(int tenantId)
        {
            var response = new GetSpecificRecord<AutomatedReplenishmentDashboardDto>();
            try
            {
                using var context = _tenantDbContextFactory.GetTenantDbContext(tenantId);

                var activeRules = await context.ReorderRules.CountAsync(r => r.IsActive && !r.IsDeleted);
                var pendingOrders = await context.PurchaseRequisitions.CountAsync(p => p.Status == RequisitionStatus.Pending && !p.IsDeleted);

                var today = DateOnly.FromDateTime(DateTime.UtcNow);
                var firstDayOfMonth = new DateOnly(today.Year, today.Month, 1);
                var thisMonthOrders = await context.PurchaseRequisitions.CountAsync(p => p.GeneratedDate >= firstDayOfMonth && !p.IsDeleted);

                var thisMonthRequisitions = await context.PurchaseRequisitions
                    .Where(p => p.GeneratedDate >= firstDayOfMonth && !p.IsDeleted)
                    .ToListAsync();
                var costSavings = thisMonthRequisitions.Sum(p => p.EstimatedCost) * 0.15m;

                var reorderRules = await context.ReorderRules
                    .Include(r => r.Inventory)
                    .Include(r => r.SupplierManagement)
                    .Where(r => r.IsActive && !r.IsDeleted)
                    .Take(10)
                    .ToListAsync();

                var purchaseRequisitions = await context.PurchaseRequisitions
                    .Include(p => p.Inventory)
                    .Include(p => p.SupplierManagement)
                    .Where(p => !p.IsDeleted)
                    .OrderByDescending(p => p.GeneratedDate)
                    .Take(5)
                    .ToListAsync();

                response.Data = new AutomatedReplenishmentDashboardDto
                {
                    ActiveRules = activeRules,
                    PendingOrders = pendingOrders,
                    ThisMonthOrders = thisMonthOrders,
                    CostSavings = costSavings,
                    ReorderRules = reorderRules.Select(r => new ReorderRuleResponseDto
                    {
                        ReorderRuleId = r.ReorderRuleId,
                        TenantId = r.TenantId,
                        InventoryId = r.InventoryId,
                        PartCode = r.Inventory?.ItemCode,
                        PartName = r.Inventory?.ItemName,
                        MinThreshold = r.MinThreshold,
                        ReorderQuantity = r.ReorderQuantity,
                        SupplierManagementId = r.SupplierManagementId,
                        SupplierName = r.SupplierManagement?.SupplierName,
                        Priority = r.Priority,
                        AutoGenerateOrders = r.AutoGenerateOrders,
                        IsActive = r.IsActive,
                        LeadTimeDays = r.SupplierManagement?.LeadTimeDays ?? 0,
                        LastPrice = r.SupplierManagement?.LastPrice,
                        CreatedDate = r.CreatedDate
                    }).ToList(),
                    PurchaseRequisitions = purchaseRequisitions.Select(p => new PurchaseRequisitionResponseDto
                    {
                        PurchaseRequisitionId = p.PurchaseRequisitionId,
                        RequisitionId = p.RequisitionId,
                        PartCode = p.Inventory?.ItemCode,
                        PartName = p.Inventory?.ItemName,
                        Quantity = p.Quantity,
                        SupplierName = p.SupplierManagement?.SupplierName,
                        EstimatedCost = p.EstimatedCost,
                        Priority = p.Priority,
                        GeneratedDate = p.GeneratedDate,
                        ExpectedDeliveryDate = p.ExpectedDeliveryDate,
                        Status = p.Status
                    }).ToList()
                };

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = ReorderRuleStatusMessage.DashboardSuccess;
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(
                    ex, "ReorderRuleRepository", "GetDashboardDataAsync", tenantId, null);
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"{ReorderRuleStatusMessage.DashboardFailed}: {ex.Message}";
            }
            return response;
        }
        private async Task GeneratePurchaseRequisitionAsync(int tenantId, ReorderRule reorderRule, int createdBy, FactoryOpsDBContext context)
        {
            try
            {
                var supplier = await context.SupplierManagement.FindAsync(reorderRule.SupplierManagementId);
                var inventory = await context.Inventory.FindAsync(reorderRule.InventoryId);

                if (supplier == null || inventory == null) return;

                var today = DateOnly.FromDateTime(DateTime.UtcNow);
                var requisitionCount = await context.PurchaseRequisitions
                    .CountAsync(p => p.GeneratedDate.Year == today.Year) + 1;

                var requisitionId = $"PR-{today.Year}-{requisitionCount:D3}";

                var unitPrice = reorderRule.LastPrice > 0 ? reorderRule.LastPrice : supplier.LastPrice ?? 0;

                var leadTime = reorderRule.LeadTimeDays > 0 ? reorderRule.LeadTimeDays : supplier.LeadTimeDays;

                var purchaseRequisition = new PurchaseRequisition
                {
                    TenantId = tenantId,
                    RequisitionId = requisitionId,
                    ReorderRuleId = reorderRule.ReorderRuleId,
                    InventoryId = reorderRule.InventoryId,
                    Quantity = reorderRule.ReorderQuantity,
                    SupplierManagementId = reorderRule.SupplierManagementId,
                    EstimatedCost = reorderRule.ReorderQuantity * unitPrice,
                    Priority = reorderRule.Priority,
                    GeneratedDate = today,
                    ExpectedDeliveryDate = today.AddDays(leadTime),
                    Status = reorderRule.AutoGenerateOrders ? RequisitionStatus.Ordered : RequisitionStatus.Pending,
                    IsDeleted = false,
                    CreatedBy = createdBy,
                    CreatedDate = DateTime.UtcNow
                };

                context.PurchaseRequisitions.Add(purchaseRequisition);
                await context.SaveChangesAsync();

                await _auditLogger.LogAuditAsync(
                    action: "AutoGenerate",
                    details: $"Auto-generated purchase requisition {requisitionId}",
                    tenantId: tenantId,
                    email: "",
                    eventType: "PurchaseRequisition"
                );
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(
                    ex, "ReorderRuleRepository", "GeneratePurchaseRequisitionAsync", tenantId, null);
            }
        }
    }
}
