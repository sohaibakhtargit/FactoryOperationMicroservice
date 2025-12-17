using FactoryOperation_Inventory.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.ExceptionLogger;
using FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.InventoryManagement;
using FactoryOpsApp.Domain.Entities.FactoryOpsTenants;
using FactoryOpsApp.Infrastructure.DBContext;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text;
using FactoryOperation_Inventory.FactoryOpsApp.Infrastructure.Implementation.Services.TenantAdmin.InventoryManagement;
using static FactoryOperation_Inventory.FactoryOpsApp.Common.CommonConstant;
using static FactoryOperation_WorkOrder.FactoryOpsApp.Common.CommonConstantURLs;

namespace FactoryOpsApp.Infrastructure.Repository.TenantAdmin.InventoryManagement
{
    public class PurchaseRequisitionRepository : IPurchaseRequisitionRepository
    {
        private readonly TenantDbContextFactory _tenantDbContextFactory;
        private readonly IExceptionLoggerService _exceptionLogger;

        public PurchaseRequisitionRepository(
            TenantDbContextFactory tenantDbContextFactory,
            IExceptionLoggerService exceptionLogger)
        {
            _tenantDbContextFactory = tenantDbContextFactory;
            _exceptionLogger = exceptionLogger;
        }

        public async Task<GetAllRecord<PurchaseRequisitionResponseDto>> GetAllAsync(int tenantId)
        {
            var response = new GetAllRecord<PurchaseRequisitionResponseDto>();
            try
            {
                using var context = _tenantDbContextFactory.GetTenantDbContext(tenantId);

                var requisitions = await context.PurchaseRequisitions
                    .Include(p => p.Inventory)
                    .Include(p => p.SupplierManagement)
                    .Include(p => p.ReorderRule)
                    .Where(p => !p.IsDeleted)
                    .OrderByDescending(p => p.GeneratedDate)
                    .ToListAsync();

                response.GetAllData = requisitions.Select(p => new PurchaseRequisitionResponseDto
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
                }).ToList();

                response.StatusCode = FactoryOperation_Inventory.FactoryOpsApp.Common.CommonConstant.StatusCode.Success;
                response.StatusMessage = PurchaseRequisitionStatusMessage.FetchAllSuccess;
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(
                    ex, "PurchaseRequisitionRepository", "GetAllAsync", tenantId, null);
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"{PurchaseRequisitionStatusMessage.FetchAllFailed}: {ex.Message}";
            }
            return response;
        }

        public async Task<GetSpecificRecord<PurchaseRequisitionResponseDto>> GetByIdAsync(int tenantId, int id)
        {
            var response = new GetSpecificRecord<PurchaseRequisitionResponseDto>();
            try
            {
                using var context = _tenantDbContextFactory.GetTenantDbContext(tenantId);

                var requisition = await context.PurchaseRequisitions
                    .Include(p => p.Inventory)
                    .Include(p => p.SupplierManagement)
                    .Include(p => p.ReorderRule)
                    .FirstOrDefaultAsync(p => p.PurchaseRequisitionId == id && !p.IsDeleted);

                if (requisition == null)
                {
                    response.StatusCode = StatusCode.NotFound;
                    response.StatusMessage = PurchaseRequisitionStatusMessage.NotFound;
                    return response;
                }

                response.Data = new PurchaseRequisitionResponseDto
                {
                    PurchaseRequisitionId = requisition.PurchaseRequisitionId,
                    RequisitionId = requisition.RequisitionId,
                    PartCode = requisition.Inventory?.ItemCode,
                    PartName = requisition.Inventory?.ItemName,
                    Quantity = requisition.Quantity,
                    SupplierName = requisition.SupplierManagement?.SupplierName,
                    EstimatedCost = requisition.EstimatedCost,
                    Priority = requisition.Priority,
                    GeneratedDate = requisition.GeneratedDate,
                    ExpectedDeliveryDate = requisition.ExpectedDeliveryDate,
                    Status = requisition.Status
                };

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = PurchaseRequisitionStatusMessage.FetchByIdSuccess;
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(
                    ex, "PurchaseRequisitionRepository", "GetByIdAsync", tenantId, null);
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"{PurchaseRequisitionStatusMessage.FetchByIdFailed}: {ex.Message}";
            }
            return response;
        }
        public async Task<CommonResponseModel> CreatePurchaseRequestAsync(PurchaseRequest dto)
        {
            var response = new CommonResponseModel();

            using var context = _tenantDbContextFactory.GetTenantDbContext(dto.TenantId);

            try
            {
                var newRequisition = new PurchaseRequisition
                {
                    TenantId = dto.TenantId,
                    RequisitionId = dto.RequisitionId,
                    ReorderRuleId = dto.ReorderRuleId,
                    InventoryId = dto.InventoryId,
                    Quantity = dto.Quantity,
                    SupplierManagementId = dto.SupplierManagementId,
                    EstimatedCost = dto.EstimatedCost,
                    Priority = dto.Priority,
                    SupplierAcceptanceStatus = dto.SupplierAcceptanceStatus,
                    ManagerAprovalStatus = dto.ManagerAprovalStatus,
                    GeneratedDate = dto.GeneratedDate,
                    ExpectedDeliveryDate = dto.ExpectedDeliveryDate,
                    Status = dto.Status,
                    CreatedBy = dto.CreatedBy ?? 0,
                    CreatedDate = DateTime.UtcNow
                };

                await context.PurchaseRequisitions.AddAsync(newRequisition);
                await context.SaveChangesAsync();

            /*    var supervisorRoleId = await context.FactoryRoles
                    .Where(x => x.RoleName == "Maintenance Supervisor")
                    .Select(x => x.RoleId)
                    .FirstOrDefaultAsync();

                var supervisorUserId = await context.FactoryUserRoles
                    .Where(x => x.TenantId == dto.TenantId && x.RoleId == supervisorRoleId)
                    .Select(x => x.UserId)
                    .ToListAsync();*/

                var productionSupervisorRoleId = await context.FactoryRoles
                    .Where(x => x.RoleName == "Production Supervisor")
                    .Select(x => x.RoleId)
                    .FirstOrDefaultAsync();

                var productionSupervisorUserId = await context.FactoryUserRoles
                    .Where(x => x.TenantId == dto.TenantId && x.RoleId == productionSupervisorRoleId)
                    .Select(x => x.UserId)
                    .ToListAsync();

                var supervisorUsers = productionSupervisorUserId
                    .Distinct()
                    .Select(x => (int?)x)
                    .ToList();

                var eventDto = new InventoryEventDto
                {
                    InventoryId = dto.InventoryId,
                    TenantId = dto.TenantId,
                    InventoryName = dto.InventoryName,
                    EventType = "PurchaseRequest",
                    EventTime = DateTime.UtcNow,
                    Priority = dto.Priority.ToString(),
                    Status = dto.Status.ToString(),
                    SupervisorUserIds = supervisorUsers,
                };


                {
                    var correlationId = Guid.NewGuid().ToString();
                    string topicName = KafkaCommonTopics.BuildTopicName(dto.TenantId, eventDto.EventType);

                    var kafkaRequest = new
                    {
                        Topic = topicName,
                        Key = $"Inventory-{dto.InventoryName}",
                        Payload = eventDto,
                        Source = "InevtoryService",
                        Headers = new Dictionary<string, string>
                        {
                            ["tenant-id"] = dto.TenantId.ToString(),
                            ["correlation-id"] = correlationId
                        }
                    };

                    var kafkaApiUrl = ConstantUrls.kafkaPublish;

                    using var httpClient = new HttpClient();
                    var jsonContent = new StringContent(JsonSerializer.Serialize(kafkaRequest), Encoding.UTF8, "application/json");

                    var kafkaResponse = await httpClient.PostAsync(kafkaApiUrl, jsonContent);
                    kafkaResponse.EnsureSuccessStatusCode();
                }

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = PurchaseRequisitionStatusMessage.CreateSuccess;
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(
                    ex, "PurchaseRequisitionRepository", "CreatePurchaseRequestAsync", dto.TenantId, null);

                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"{PurchaseRequisitionStatusMessage.CreateFailed}: {ex.Message}";
            }

            return response;
        }
    }

}
