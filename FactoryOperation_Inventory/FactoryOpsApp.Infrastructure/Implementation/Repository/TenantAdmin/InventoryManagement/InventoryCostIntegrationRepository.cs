using FactoryOperation_Inventory.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.ExceptionLogger;
using FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.InventoryManagement;
using FactoryOpsApp.Application.Interfaces.Services.SuperAdmin.AuditLogs;
using FactoryOpsApp.Domain.Entities.FactoryOpsTenants;
using FactoryOpsApp.Infrastructure.DBContext;
using Microsoft.EntityFrameworkCore;
using static FactoryOperation_Inventory.FactoryOpsApp.Common.CommonConstant;

namespace FactoryOpsApp.Infrastructure.Repository.TenantAdmin.InventoryManagement
{
    public class InventoryCostIntegrationRepository : IInventoryCostIntegrationRepository
    {
        private readonly TenantDbContextFactory _tenantDbContext;
        private readonly IExceptionLoggerService _exceptionLogger;
        private readonly IAuditLogService _auditLogger;

        public InventoryCostIntegrationRepository(
            TenantDbContextFactory tenantDbContext,
            IExceptionLoggerService exceptionLogger,
            IAuditLogService auditLogger)
        {
            _tenantDbContext = tenantDbContext;
            _exceptionLogger = exceptionLogger;
            _auditLogger = auditLogger;
        }

        public async Task<InventoryCostSummaryResponse> GetInventoryCostsAsync(int tenantId)
        {
            var summaryResponse = new InventoryCostSummaryResponse();
            var commonResponse = new GetAllRecord<InventoryCostIntegrationDto>();

            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                var total = await tenantDb.Inventory
                    .Where(x => x.TenantId == tenantId && x.IsActive && !x.IsDeleted)
                    .SumAsync(x => x.UnitPrice * x.QuantityAvailable);

                commonResponse.StatusCode = StatusCode.Success;
                commonResponse.StatusMessage = InventoryCostStatusMessage.CostRetrieved;

                summaryResponse.Response = commonResponse;
                summaryResponse.TotalInventoryValue = total;
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "InventoryCostIntegration-Module", "GetInventoryCostsAsync", tenantId, null);

                commonResponse.StatusCode = StatusCode.Error;
                commonResponse.StatusMessage = $"{InventoryCostStatusMessage.Error}: {ex.Message}";
                summaryResponse.Response = commonResponse;
                summaryResponse.TotalInventoryValue = 0;
            }

            return summaryResponse;
        }

        public async Task<GetAllRecord<InventoryItemInfoDto>> GetInventoryItemInfo(int tenantId)
        {
            var response = new GetAllRecord<InventoryItemInfoDto>();
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                var data = await tenantDb.Inventory
                    .Where(x => x.TenantId == tenantId && x.IsActive && !x.IsDeleted)
                    .Select(x => new InventoryItemInfoDto
                    {
                        TenantId = x.TenantId,
                        InventoryId = x.ItemId,
                        PartNumber = x.ItemCode,
                        PartName = x.ItemName,
                        Catagory = x.Category.HasValue ? x.Category.Value.ToString() : null,
                        Stock = x.QuantityAvailable,
                        UnitCost = x.UnitPrice,
                        TotalCost = x.UnitPrice * x.QuantityAvailable,
                        Status = x.Status.ToString(),
                    }).ToListAsync();

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = InventoryCostStatusMessage.CostRetrieved;
                response.GetAllData = data;
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "InventoryCostIntegration-Module", "GetInventoryItemInfo", tenantId, null);
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"{InventoryCostStatusMessage.Error}: {ex.Message}";
            }

            return response;
        }

        public async Task<GetAllRecord<WorkOrderCostIntegrationDto>> GetWorkOrderIntegration(int tenantId)
        {
            var response = new GetAllRecord<WorkOrderCostIntegrationDto>();
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                var data = await tenantDb.WorkOrders
                    .Where(x => x.TenantId == tenantId && x.IsActive && !x.IsDeleted)
                    .Select(x => new WorkOrderCostIntegrationDto
                    {
                        TenantId = x.TenantId,
                        WorkOredrNumber = x.WorkOrderNumber,
                        Title = x.Title,
                        LabourCost = x.LaborCost,
                        PartCost = x.PartCost,
                        TotalCost = x.TotalCost,
                        Status = x.Status.ToString(),
                        AssignedToTeamId = x.AssignedToTeamId,
                        AssigedToTeam = x.AssignedToTeam != null ? x.AssignedToTeam.Name : null,
                        AssignedToUserId = x.AssignedToUserId,
                        AssigedToUserName = x.AssignedToUser != null ? x.AssignedToUser.FirstName + " " + x.AssignedToUser.LastName : null,
                    }).ToListAsync();

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = InventoryCostStatusMessage.CostRetrieved;
                response.GetAllData = data;
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "InventoryCostIntegration-Module", "GetWorkOrderIntegration", tenantId, null);
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"{InventoryCostStatusMessage.Error}: {ex.Message}";
            }

            return response;
        }

        public async Task<GetSpecificRecord<InventoryCostIntegrationDto>> GetInventoryCostByIdAsync(int id, int tenantId)
        {
            var response = new GetSpecificRecord<InventoryCostIntegrationDto>();
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                var data = await tenantDb.InventoryCostIntegrations
                    .Where(x => x.TenantId == tenantId && x.WorkOrderPartId == id && x.IsActive && !x.IsDeleted)
                    .Select(x => new InventoryCostIntegrationDto
                    {
                        WorkOrderPartId = x.WorkOrderPartId,
                        TenantId = x.TenantId,
                        WorkOrderId = x.WorkOrderId,
                        WorkOrderName = x.WorkOrderName,
                        InventoryId = x.InventoryId,
                        PartName = x.PartName,
                        Quantity = x.Quantity,
                        UnitCost = x.UnitCost,
                        IsActive = x.IsActive
                    }).FirstOrDefaultAsync();

                if (data == null)
                {
                    response.StatusCode = StatusCode.NotFound;
                    response.StatusMessage = InventoryCostStatusMessage.NotFound;
                    return response;
                }

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = InventoryCostStatusMessage.CostRetrieved;
                response.Data = data;
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "InventoryCostIntegration-Module", "GetInventoryCostByIdAsync", tenantId, null);
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"{InventoryCostStatusMessage.Error}: {ex.Message}";
            }

            return response;
        }

        public async Task<CommonResponseModel> AddInventoryCostAsync(CreateInventoryCostIntegrationDto dto)
        {
            var response = new CommonResponseModel();
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(dto.TenantId);
                var workOrder = await tenantDb.WorkOrders
                    .FirstOrDefaultAsync(w => w.WorkOrderId == dto.WorkOrderId && !w.IsDeleted);

                if (workOrder == null)
                {
                    response.StatusCode = StatusCode.NotFound;
                    response.StatusMessage = InventoryCostStatusMessage.WorkOrderNotFound;
                    return response;
                }
                //var inventoryItem = await tenantDb.Inventory
                //    .FirstOrDefaultAsync(i => i.ItemId == workOrder.RequiredTools && !i.IsDeleted);

                //if (inventoryItem == null)
                //{
                //    response.StatusCode = StatusCode.NotFound;
                //    response.StatusMessage = InventoryCostStatusMessage.InventoryItemNotFound;
                //    return response;
                //}
                var requiredTools = await tenantDb.WorkOrderRequiredTools
                .Where(t => t.WorkOrderId == dto.WorkOrderId && !t.IsDeleted)
                .ToListAsync();

                if (requiredTools == null || requiredTools.Count == 0)
                {
                    response.StatusCode = StatusCode.NotFound;
                    response.StatusMessage = "No tools found for this WorkOrder.";
                    return response;
                }

                foreach (var tool in requiredTools)
                {
                    var inventoryItem = await tenantDb.Inventory
                        .FirstOrDefaultAsync(i => i.ItemId == tool.ToolId && !i.IsDeleted);

                    if (inventoryItem == null)
                        continue;

                    var entity = new InventoryCostIntegration
                    {
                        TenantId = dto.TenantId,
                        WorkOrderId = dto.WorkOrderId,
                        WorkOrderName = workOrder.WorkOrderNumber,
                        InventoryId = inventoryItem.ItemId,
                        PartName = inventoryItem.ItemName,
                        Quantity = dto.Quantity,
                        UnitCost = inventoryItem.UnitPrice,
                        CreatedBy = dto.CreatedBy,
                        CreatedAt = NormalizeToUnspecified(dto.CreatedAt),
                        UpdatedAt = NormalizeToUnspecified(dto.UpdatedAt),
                    };

                    tenantDb.InventoryCostIntegrations.Add(entity);
                    await tenantDb.SaveChangesAsync();

                    await _auditLogger.LogAuditAsync("Inventory cost", "Create", dto.CreatedBy, dto.TenantId.ToString(), entity.WorkOrderPartId.ToString());

                    response.StatusCode = StatusCode.Success;
                    response.StatusMessage = InventoryCostStatusMessage.Added;
                }
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "InventoryCostIntegration-Module", "AddInventoryCostAsync", dto.TenantId, dto.CreatedBy);
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"{InventoryCostStatusMessage.Error}: {ex.Message}";
            }

            return response;
        }

        private DateTime NormalizeToUnspecified(DateTime value)
        {
            return DateTime.SpecifyKind(value, DateTimeKind.Unspecified);
        }

        public async Task<CommonResponseModel> UpdateInventoryCostAsync(CreateInventoryCostIntegrationDto dto)
        {
            var response = new CommonResponseModel();
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(dto.TenantId);

                var entity = await tenantDb.InventoryCostIntegrations
                    .FirstOrDefaultAsync(x => x.WorkOrderPartId == dto.WorkOrderPartId && x.TenantId == dto.TenantId && !x.IsDeleted);

                if (entity == null)
                {
                    response.StatusCode = StatusCode.NotFound;
                    response.StatusMessage = InventoryCostStatusMessage.NotFound;
                    return response;
                }

                var workOrder = await tenantDb.WorkOrders
                    .FirstOrDefaultAsync(w => w.WorkOrderId == dto.WorkOrderId && !w.IsDeleted);

                if (workOrder == null)
                {
                    response.StatusCode = StatusCode.NotFound;
                    response.StatusMessage = InventoryCostStatusMessage.WorkOrderNotFound;
                    return response;
                }

                //var inventoryItem = await tenantDb.Inventory
                //    .FirstOrDefaultAsync(i => i.ItemId == workOrder.RequiredTools && !i.IsDeleted);

                //if (inventoryItem == null)
                //{
                //    response.StatusCode = StatusCode.NotFound;
                //    response.StatusMessage = InventoryCostStatusMessage.InventoryItemNotFound;
                //    return response;
                //}
                var requiredTools = await tenantDb.WorkOrderRequiredTools
              .Where(t => t.WorkOrderId == dto.WorkOrderId && !t.IsDeleted)
              .ToListAsync();

                if (requiredTools == null || requiredTools.Count == 0)
                {
                    response.StatusCode = StatusCode.NotFound;
                    response.StatusMessage = "No tools found for this WorkOrder.";
                    return response;
                }

                foreach (var tool in requiredTools)
                {
                    var inventoryItem = await tenantDb.Inventory
                        .FirstOrDefaultAsync(i => i.ItemId == tool.ToolId && !i.IsDeleted);

                    if (inventoryItem == null)
                        continue;

                    entity.WorkOrderId = dto.WorkOrderId;
                    entity.WorkOrderName = workOrder.WorkOrderNumber;
                    entity.InventoryId = inventoryItem.ItemId;
                    entity.PartName = inventoryItem.ItemName;
                    entity.Quantity = dto.Quantity;
                    entity.UnitCost = inventoryItem.UnitPrice;
                    entity.UpdatedBy = dto.UpdatedBy;
                    entity.UpdatedAt = DateTime.UtcNow;

                    tenantDb.InventoryCostIntegrations.Update(entity);
                    await tenantDb.SaveChangesAsync();

                    await _auditLogger.LogAuditAsync("WorkOrder", "Update", dto.UpdatedBy, dto.TenantId.ToString(), entity.WorkOrderId.ToString());

                    response.StatusCode = StatusCode.Success;
                    response.StatusMessage = InventoryCostStatusMessage.Updated;
                }
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "InventoryCostIntegration-Module", "UpdateInventoryCostAsync", dto.TenantId, dto.UpdatedBy);
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"{InventoryCostStatusMessage.Error}: {ex.Message}";
            }

            return response;
        }

        public async Task<CommonResponseModel> DeleteInventoryCostAsync(int id, int tenantId)
        {
            var response = new CommonResponseModel();
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                var entity = await tenantDb.InventoryCostIntegrations
                    .FirstOrDefaultAsync(x => x.WorkOrderPartId == id && x.TenantId == tenantId);

                if (entity == null)
                {
                    response.StatusCode = StatusCode.NotFound;
                    response.StatusMessage = InventoryCostStatusMessage.NotFound;
                    return response;
                }

                entity.IsDeleted = true;
                entity.IsActive = false;
                entity.DeletedAt = DateTime.UtcNow;

                tenantDb.InventoryCostIntegrations.Update(entity);
                await tenantDb.SaveChangesAsync();

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = InventoryCostStatusMessage.Deleted;
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "InventoryCostIntegration-Module", "DeleteInventoryCostAsync", tenantId, null);
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"{InventoryCostStatusMessage.Error}: {ex.Message}";
            }

            return response;
        }
    }
}
