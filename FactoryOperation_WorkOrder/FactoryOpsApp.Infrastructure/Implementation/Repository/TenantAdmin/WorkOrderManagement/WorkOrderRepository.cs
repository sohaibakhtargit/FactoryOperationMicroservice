using CsvHelper;
using FactoryOperation_WorkOrder.FactoryOpsApp.Application.DTOs;
using FactoryOperation_WorkOrder.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.AuditLogs;
using FactoryOperation_WorkOrder.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.Common;
using FactoryOperation_WorkOrder.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.Notification;
using FactoryOperation_WorkOrder.FactoryOpsApp.Domain.Entities.FactoryOpsTenants;
using FactoryOperation_WorkOrder.FactoryOpsApp.Infrastructure.Implementation.Services.TenantAdmin.WorkOrderManagement;
using FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.WorkOrderManagement;
using FactoryOpsApp.Domain.Entities;
using FactoryOpsApp.Domain.Entities.FactoryOpsTenants;
using FactoryOpsApp.Infrastructure.DBContext;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using System.Formats.Asn1;
using System.Globalization;
using System.Text;
using System.Text.Json;
using static FactoryOperation_WorkOrder.FactoryOpsApp.Common.CommonConstantURLs;
using static FactoryOpsApp.Application.DTOs.WorkOrderCreateDto;
using static FactoryOpsApp.Common.CommonConstant;

namespace FactoryOpsApp.Infrastructure.Repository.TenantAdmin.WorkOrderManagement
{
    public class WorkOrderRepository : IWorkOrderRepository
    {
        private readonly MasterFactoryOpsDbContext _masterDbcontext;
        private readonly TenantDbContextFactory _tenantDbContext;
        private readonly IAuditLogService _auditLogger;
        private readonly IFileStorageService _fileStorageService;

        public WorkOrderRepository(
           TenantDbContextFactory tenantDbContext,
           MasterFactoryOpsDbContext masterDbcontext,
           IExceptionLoggerService exceptionLogger,
           IAuditLogService auditLogger,
           INotificationService notificationService,
          IFileStorageService fileStorageService)
        {
            _masterDbcontext = masterDbcontext;
            _tenantDbContext = tenantDbContext;    
            _auditLogger = auditLogger;
           _fileStorageService = fileStorageService;

        }
        public async Task<GetAllRecord<WorkOrderDto>> GetWorkOrderAllAsync(int tenantId, WorkOrderTypeEnum workOrderType)
        {
            var response = new GetAllRecord<WorkOrderDto>();
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);
                var data = await tenantDb.WorkOrders
                   .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.WorkOrderType == workOrderType)
                   .Select(x => new WorkOrderDto
                   {
                       WorkOrderId = x.WorkOrderId,
                       WorkOrderNumber = x.WorkOrderNumber,
                       Title = x.Title,
                       Description = x.Description,
                       LocationId = x.LocationId,
                       LocationName = x.Location != null ? x.Location.LocationName : null,
                       AssetId = x.AssetId,
                       AssetName = x.Asset != null ? x.Asset.AssetName : null,
                       Status = x.Status,
                       Priority = x.Priority,
                       WorkOrderType = x.WorkOrderType,
                       AssignedToUserId = x.AssignedToUserId,
                       AssignedToUser = x.AssignedToUser != null ? x.AssignedToUser.FirstName + " " + x.AssignedToUser.LastName : null,
                       AssignedToTeamId = x.AssignedToTeamId,
                       AssignedToTeam = x.AssignedToTeam != null ? x.AssignedToTeam.Name : null,
                       DueDate = x.DueDate,
                       ScheduleDate = x.ScheduleDate,
                       EstimatedDurationMinutes = x.EstimatedDurationMinutes,
                       Instructions = x.Instructions,
                       CompletionNotes = x.CompletionNotes,
                       LaborCost = x.LaborCost,
                       PartCost = x.PartCost,
                       TotalCost = x.LaborCost + x.PartCost,

                       RequiredTools = tenantDb.WorkOrderRequiredTools
                        .Where(t => t.WorkOrderId == x.WorkOrderId && !t.IsDeleted)
                        .Select(t => new WorkOrderToolDto
                        {
                            ToolId = t.ToolId,
                            ToolName = t.Tool != null ? t.Tool.ItemName : null,
                            QuantityRequired = t.QuantityRequired ?? 0
                        })
                        .ToList()
                   })
                   .ToListAsync();


                response.StatusCode = StatusCode.Success;
                response.StatusMessage = WorkOrderStatusMessage.WorkOrdersFetchedSuccessfully;
                response.GetAllData = data;
            
            return response;
        }
        public async Task<GetSpecificRecord<WorkOrderDto>> GetWorkOrderByIdAsync(int WorkOrderId, int tenantId)
        {
            var response = new GetSpecificRecord<WorkOrderDto>();
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);
                var data = await tenantDb.WorkOrders
                        .Where(x => x.WorkOrderId == WorkOrderId && x.TenantId == tenantId && !x.IsDeleted)
                        .Select(x => new WorkOrderDto
                        {
                            WorkOrderId = x.WorkOrderId,
                            WorkOrderNumber = x.WorkOrderNumber,
                            Title = x.Title,
                            Description = x.Description,
                            LocationId = x.LocationId,
                            LocationName = x.Location != null ? x.Location.LocationName : null,
                            AssetId = x.AssetId,
                            AssetName = x.Asset != null ? x.Asset.AssetName : null,
                            Status = x.Status,
                            Priority = x.Priority,
                            WorkOrderType = x.WorkOrderType,
                            AssignedToUserId = x.AssignedToUserId,
                            AssignedToUser = x.AssignedToUser != null ? x.AssignedToUser.FirstName + " " + x.AssignedToUser.LastName : null,
                            AssignedToTeamId = x.AssignedToTeamId,
                            AssignedToTeam = x.AssignedToTeam != null ? x.AssignedToTeam.Name : null,
                            DueDate = x.DueDate,
                            ScheduleDate = x.ScheduleDate,
                            EstimatedDurationMinutes = x.EstimatedDurationMinutes,
                            Instructions = x.Instructions,
                            CompletionNotes = x.CompletionNotes,
                            LaborCost = x.LaborCost,
                            PartCost = x.PartCost,
                            TotalCost = x.LaborCost + x.PartCost,

                        })
                        .FirstOrDefaultAsync();

                if (data != null)
                {
                    response.Data = data;
                    response.StatusCode = "200";
                    response.StatusMessage = "Dashboard report fetched successfully";

                }
                else
                {
                    response.StatusCode = "404";
                    response.StatusMessage = "Dashboard report not found";

                }
            }
            catch (Exception ex)
            {
                //await _exceptionLogger.LogExceptionAsync(ex, "AssetDashboardReport-Module", "GetById-DashboardReport", tenantId, null);
                response.StatusCode = "500";
                response.StatusMessage = $"Failed to fetch dashboard report: {ex.Message}";
            }

            return response;
        }
        private async Task PublishLowStockKafkaAsync(int tenantId, LowStockEventDto evt)
        {
            var topic = KafkaCommonTopics.BuildTopicName(tenantId, evt.EventType);

            var request = new
            {
                Topic = topic,
                Key = $"inventory-{evt.ItemId}",
                Payload = evt,
                Source = "InventoryService",
                Headers = new Dictionary<string, string>
                {
                    ["tenant-id"] = tenantId.ToString(),
                    ["correlation-id"] = Guid.NewGuid().ToString()
                }
            };

            using var http = new HttpClient();
            var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            var res = await http.PostAsync(ConstantUrls.kafkaPublish, content);
            res.EnsureSuccessStatusCode();
        }

        private List<T> ReadCsvFile<T>(IFormFile file) where T : new()
        {
            using var reader = new StreamReader(file.OpenReadStream());
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            return csv.GetRecords<T>().ToList();
        }

        public async Task<BulkWorkOrderImportResult> ImportBulkWorkOrdersAsync(
            BulkWorkOrderImportRequest request)
        {

            using var tenantDb = _tenantDbContext.GetTenantDbContext(request.TenantId);

            var relativePath = await _fileStorageService.SaveFileAsync(
                request.File,
                "uploads/bulk-workorders"
            );

            var bulkImport = new WorkOrderBulkImport
            {
                TenantId = request.TenantId,
                FileName = request.File.FileName,
                FilePath = relativePath, 
                UploadedBy = request.CreatedBy,
                UploadedAt = DateTime.UtcNow
            };
            tenantDb.WorkOrderBulkImports.Add(bulkImport);
            await tenantDb.SaveChangesAsync(); 

            var result = new BulkWorkOrderImportResult();
            var rows = ReadCsvFile<WorkOrderImportCsvDto>(request.File);

            result.TotalRecords = rows.Count;

            int rowNumber = 1;

            foreach (var row in rows)
            {
                try
                {
                    var dto = new WorkOrderCreateDto
                    {
                        TenantId = request.TenantId,
                        Title = row.Title,
                        Description = row.Description,
                        LocationId = row.LocationId,
                        Priority = Enum.TryParse<PriorityLevel>(row.Priority, true, out var p)
                            ? p
                            : PriorityLevel.Medium,
                        WorkOrderType = Enum.TryParse<WorkOrderTypeEnum>(row.WorkOrderType, true, out var t)
                            ? t
                            : WorkOrderTypeEnum.Preventive,
                        AssignedToUserId = row.AssignedToUserId > 0 ? row.AssignedToUserId : null,
                        AssignedToTeamId = row.AssignedToTeamId > 0 ? row.AssignedToTeamId : null,
              
                        DueDate = row.DueDate.HasValue
                        ? ToUtc(row.DueDate.Value)
                        : DateTime.UtcNow,

                                        ScheduleDate = row.ScheduleDate.HasValue
                        ? ToUtc(row.ScheduleDate.Value)
                        : (DateTime?)null,

                        EstimatedDurationMinutes = row.EstimatedDurationMinutes,
                        AssetId = row.AssetId,
                        Instructions = row.Instructions,
                        LaborCost = row.LaborCost ?? 0,
                        PartCost = row.PartCost ?? 0,
                        CreatedBy = request.CreatedBy,
                        BulkImportId = bulkImport.BulkImportId, 
                        RequiredTools = row.ToolId.HasValue
                            ? new List<WorkOrderToolDto>
                            {
                        new WorkOrderToolDto
                        {
                            ToolId = row.ToolId.Value,
                            QuantityRequired = row.QuantityRequired.HasValue && row.QuantityRequired > 0
                                ? row.QuantityRequired.Value
                                : 1
                        }
                            }
                            : new List<WorkOrderToolDto>()
                    };

                    await CreateWorkOrderAsync(dto);
                    result.SuccessCount++;
                }
                catch (Exception ex)
                {
                    result.FailureCount++;
                    result.Errors.Add(new BulkWorkOrderError
                    {
                        RowNumber = rowNumber,
                        ErrorMessage = ex.InnerException?.Message ?? ex.Message
                    });
                }

                rowNumber++;
            }
         
            bulkImport.TotalRecords = result.TotalRecords;
            bulkImport.SuccessCount = result.SuccessCount;
            bulkImport.FailureCount = result.FailureCount;
            await tenantDb.SaveChangesAsync();

            return result;
        }

        private static DateTime ToUtc(DateTime dateTime)
        {
            return dateTime.Kind switch
            {
                DateTimeKind.Utc => dateTime,
                DateTimeKind.Local => dateTime.ToUniversalTime(),
                _ => DateTime.SpecifyKind(dateTime, DateTimeKind.Utc)
            };
        }
        public async Task<CommonResponseModel> CreateWorkOrderAsync(WorkOrderCreateDto dto)
        {
            var response = new CommonResponseModel();
            using var tenantDb = _tenantDbContext.GetTenantDbContext(dto.TenantId);
            using var transaction = await tenantDb.Database.BeginTransactionAsync();


            if (dto.RequiredTools != null && dto.RequiredTools.Any())
            {
                foreach (var toolDto in dto.RequiredTools)
                {
                    var requiredQty = (toolDto.QuantityRequired == null || toolDto.QuantityRequired <= 0)
                        ? 1 : toolDto.QuantityRequired.Value;

                    var inventoryItem = await tenantDb.Inventory
                        .FirstOrDefaultAsync(i => i.ItemId == toolDto.ToolId);

                    if (inventoryItem == null)
                        throw new Exception($"Tool with ID {toolDto.ToolId} not found in inventory.");

      
                    if (inventoryItem.QuantityAvailable < requiredQty)
                    {
                        var StoreKeeperRoleId = await tenantDb.FactoryRoles
                            .Where(x => x.RoleName == "Store Keeper")
                            .Select(x => x.RoleId)
                            .FirstOrDefaultAsync();

                        var StoreKeeperUserId = await tenantDb.FactoryUserRoles
                            .Where(x => x.TenantId == dto.TenantId && x.RoleId == StoreKeeperRoleId)
                            .Select(x => x.UserId)
                            .ToListAsync();
                        var userIds = StoreKeeperUserId.ToList();

                        var nullableUserIds = userIds.Select(id => (int?)id).ToList();
                        var lowStock = new LowStockEventDto
                        {
                            TenantId = dto.TenantId,
                            ItemId = inventoryItem.ItemId,
                            ItemName = inventoryItem.ItemName,
                            QuantityAvailable = inventoryItem.QuantityAvailable,
                            ReorderLevel = inventoryItem.ReorderLevel,
                            EventType = "LowStockError",
                            TargetUserIds = nullableUserIds
                        };

                        await PublishLowStockKafkaAsync(dto.TenantId, lowStock);
                        throw new Exception(
                            $"Insufficient quantity for tool '{inventoryItem.ItemName}'. Required: {requiredQty}, Available: {inventoryItem.QuantityAvailable}");
                    }
                }
            }

            var entity = new WorkOrder
            {
                TenantId = dto.TenantId,
                Title = dto.Title,
                Description = dto.Description,
                LocationId = dto.LocationId,
                Status = dto.Status ?? WorkOrderStatus.Started,
                Priority = dto.Priority ?? PriorityLevel.Medium,
                WorkOrderType = dto.WorkOrderType,
                AssignedToUserId = dto.AssignedToUserId,
                AssignedToTeamId = dto.AssignedToTeamId,
                DueDate = dto.DueDate,
                ScheduleDate = dto.ScheduleDate,
                EstimatedDurationMinutes = dto.EstimatedDurationMinutes,
                AssetId = dto.AssetId,
                BulkImportId = dto.BulkImportId, 
                Instructions = dto.Instructions,
                LaborCost = dto.LaborCost,
                PartCost = dto.PartCost,
                TotalCost = dto.LaborCost + dto.PartCost,
                WorkOrderNumber = $"WO-{DateTime.UtcNow.Year}-{Guid.NewGuid().ToString()[..4]}",
                CreatedBy = dto.CreatedBy,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true,
                IsDeleted = false
            };

            tenantDb.WorkOrders.Add(entity);
            await tenantDb.SaveChangesAsync();

            List<LowStockEventDto> lowStockWarnings = new();

            if (dto.RequiredTools != null && dto.RequiredTools.Any())
            {
                foreach (var toolDto in dto.RequiredTools)
                {
                    var requiredQty = toolDto.QuantityRequired.GetValueOrDefault(1);

                    var inventoryItem = await tenantDb.Inventory
                        .FirstOrDefaultAsync(i => i.ItemId == toolDto.ToolId);

                    if (inventoryItem != null)
                    {
                        inventoryItem.QuantityAvailable -= requiredQty;
                        inventoryItem.UpdatedAt = DateTime.UtcNow;
                        inventoryItem.UpdatedBy = dto.CreatedBy ?? 0;

                        if (inventoryItem.QuantityAvailable <= inventoryItem.ReorderLevel)
                        {
                            var StoreKeeperRoleId = await tenantDb.FactoryRoles
                           .Where(x => x.RoleName == "Store Keeper")
                           .Select(x => x.RoleId)
                           .FirstOrDefaultAsync();

                            var StoreKeeperUserId = await tenantDb.FactoryUserRoles
                                .Where(x => x.TenantId == dto.TenantId && x.RoleId == StoreKeeperRoleId)
                                .Select(x => x.UserId)
                                .ToListAsync();
                            var userIds = StoreKeeperUserId.ToList();

                            var nullableUserIds = userIds.Select(id => (int?)id).ToList();

                            var lowStockWarning = new LowStockEventDto
                            {
                                TenantId = dto.TenantId,
                                ItemId = inventoryItem.ItemId,
                                ItemName = inventoryItem.ItemName,
                                QuantityAvailable = inventoryItem.QuantityAvailable,
                                ReorderLevel = inventoryItem.ReorderLevel,
                                EventType = "LowStockWarning", 
                                TargetUserIds = nullableUserIds

                            };

                            lowStockWarnings.Add(lowStockWarning);
                        }
                    }

                    var workOrderTool = new WorkOrderRequiredTool
                    {
                        WorkOrderId = entity.WorkOrderId,
                        ToolId = toolDto.ToolId,
                        QuantityRequired = requiredQty,
                        CreatedBy = dto.CreatedBy ?? 0,
                        CreatedAt = DateTime.UtcNow
                    };

                    tenantDb.WorkOrderRequiredTools.Add(workOrderTool);
                }

                await tenantDb.SaveChangesAsync();
            }

 
            var supervisorRoleId = await tenantDb.FactoryRoles
                .Where(x => x.RoleName == "Maintenance Supervisor")
                .Select(x => x.RoleId)
                .FirstOrDefaultAsync();

            var supervisorUserId = await tenantDb.FactoryUserRoles
                .Where(x => x.TenantId == entity.TenantId && x.RoleId == supervisorRoleId)
                .Select(x => x.UserId)
                .ToListAsync();

            var productionSupervisorRoleId = await tenantDb.FactoryRoles
                .Where(x => x.RoleName == "Production Supervisor")
                .Select(x => x.RoleId)
                .FirstOrDefaultAsync();

            var productionSupervisorUserId = await tenantDb.FactoryUserRoles
                .Where(x => x.TenantId == entity.TenantId && x.RoleId == productionSupervisorRoleId)
                .Select(x => x.UserId)
                .ToListAsync();

            // Fetch the assigned user for the asset

            var assignedToUserId = await tenantDb.AssetRegistry
                 .Where(x => x.AssetId == dto.AssetId && x.IsActive && !x.IsDeleted)
                 .Join(
                     tenantDb.AssetTracking,
                     assetRegistry => assetRegistry.AssetId,
                     assetTracking => assetTracking.AssetId,
                     (assetRegistry, assetTracking) => assetTracking
                 )
                 .Where(x => !x.IsDeleted)
                 .Select(x => x.AssignedTo)
                 .FirstOrDefaultAsync();

            var supervisorUsers = supervisorUserId
                .Concat(productionSupervisorUserId)
                .Distinct()
                .Select(x => (int?)x)
                .ToList();

            if (assignedToUserId.HasValue)
            {
                supervisorUsers.Add(assignedToUserId.Value);
            }





            var eventDto = new WorkOrderEventDto
            {
                WorkOrderId = entity.WorkOrderId,
                TenantId = entity.TenantId,
                WorkOrderNumber = entity.WorkOrderNumber,
                Title = entity.Title,
                EventType = "Created",
                EventTime = DateTime.UtcNow,
                Priority = entity.Priority.ToString(),
                Status = entity.Status.ToString(),
                AssignedToUserId = entity.AssignedToUserId,
                AssignedToTeamId = entity.AssignedToTeamId,
                SupervisorUserIds = supervisorUsers
            };


            {
                var correlationId = Guid.NewGuid().ToString();
                string topicName = KafkaCommonTopics.BuildTopicName(entity.TenantId, eventDto.EventType);

                var kafkaRequest = new
                {
                    Topic = topicName,
                    Key = $"workorder-{entity.WorkOrderId}",
                    Payload = eventDto,
                    Source = "WorkOrderService",
                    Headers = new Dictionary<string, string>
                    {
                        ["tenant-id"] = entity.TenantId.ToString(),
                        ["correlation-id"] = correlationId
                    }
                };

                var kafkaApiUrl = ConstantUrls.kafkaPublish;

                using var httpClient = new HttpClient();
                var jsonContent = new StringContent(JsonSerializer.Serialize(kafkaRequest), Encoding.UTF8, "application/json");

                var kafkaResponse = await httpClient.PostAsync(kafkaApiUrl, jsonContent);
                kafkaResponse.EnsureSuccessStatusCode();
            }


            foreach (var lowStockWarning in lowStockWarnings)
            {
                await PublishLowStockKafkaAsync(dto.TenantId, lowStockWarning);
            }

  
            await _auditLogger.LogAuditAsync("WorkOrder", "Create", dto.CreatedBy, dto.TenantId.ToString(), entity.WorkOrderId.ToString());
            await transaction.CommitAsync();

            response.StatusCode = StatusCode.Success;
            response.StatusMessage = WorkOrderStatusMessage.WorkOrderCreatedSuccessfully;

            return response;
        }

        public async Task<CommonResponseModel> UpdateWorkOrderAsync(WorkOrderUpdateDto dto)
        {
            var response = new CommonResponseModel();
            using var tenantDb = _tenantDbContext.GetTenantDbContext(dto.TenantId);
            using var transaction = await tenantDb.Database.BeginTransactionAsync();

                var entity = await tenantDb.WorkOrders
                    .FirstOrDefaultAsync(x => x.WorkOrderId == dto.WorkOrderId && !x.IsDeleted);

                if (entity == null)
                {
                    response.StatusCode = StatusCode.NotFound;
                    response.StatusMessage = WorkOrderStatusMessage.WorkOrderNotFound;
                    return response;
                }

                entity.Title = dto.Title;
                entity.AssetId = dto.AssetId;
                entity.Description = dto.Description;
                entity.LocationId = dto.LocationId;
                entity.Priority = dto.Priority;
                entity.WorkOrderType = dto.WorkOrderType;
                entity.AssignedToUserId = dto.AssignedToUserId;
                entity.AssignedToTeamId = dto.AssignedToTeamId;
                entity.Status = dto.Status;
                entity.DueDate = dto.DueDate;
                entity.ScheduleDate = dto.ScheduleDate;
                entity.EstimatedDurationMinutes = dto.EstimatedDurationMinutes;
                entity.Instructions = dto.Instructions;
                entity.CompletionNotes = dto.CompletionNotes;
                entity.LaborCost = dto.LaborCost;
                entity.PartCost = dto.PartCost;
                entity.TotalCost = dto.LaborCost + dto.PartCost;
                entity.UpdatedBy = dto.UpdatedBy;
                entity.UpdatedAt = DateTime.UtcNow;

                if (dto.RequiredTools != null && dto.RequiredTools.Any())
                {
                    var existingTools = await tenantDb.WorkOrderRequiredTools
                        .Where(t => t.WorkOrderId == dto.WorkOrderId && !t.IsDeleted)
                        .ToListAsync();

                    var newToolIds = dto.RequiredTools.Select(t => t.ToolId).ToList();


                    var toolsToRemove = existingTools.Where(t => !newToolIds.Contains(t.ToolId)).ToList();

                    foreach (var removedTool in toolsToRemove)
                    {
                        var inventoryItem = await tenantDb.Inventory
                            .FirstOrDefaultAsync(i => i.ItemId == removedTool.ToolId);

                        if (inventoryItem != null)
                        {
                            inventoryItem.QuantityAvailable += removedTool.QuantityRequired ?? 1;
                            inventoryItem.UpdatedAt = DateTime.UtcNow;
                            inventoryItem.UpdatedBy = dto.UpdatedBy ?? 0;
                        }

                        removedTool.IsDeleted = true;
                        removedTool.UpdatedBy = dto.UpdatedBy ?? 0;
                        removedTool.UpdatedAt = DateTime.UtcNow;
                    }


                    foreach (var toolDto in dto.RequiredTools)
                    {
                        var requiredQty = toolDto.QuantityRequired.HasValue && toolDto.QuantityRequired > 0
                            ? toolDto.QuantityRequired.Value
                            : 1;

                        var inventoryItem = await tenantDb.Inventory.FirstOrDefaultAsync(i => i.ItemId == toolDto.ToolId);

                        if (inventoryItem == null)
                            throw new Exception($"Tool with ID {toolDto.ToolId} not found in inventory.");

                        var existingTool = existingTools.FirstOrDefault(t => t.ToolId == toolDto.ToolId);

                        if (existingTool != null)
                        {

                            var oldQty = existingTool.QuantityRequired ?? 0;
                            var diff = requiredQty - oldQty;

                        if (diff > 0)
                        {
                            if (inventoryItem.QuantityAvailable < diff)
                            {
                                var StoreKeeperRoleId = await tenantDb.FactoryRoles
                                  .Where(x => x.RoleName == "Store Keeper")
                                  .Select(x => x.RoleId)
                                  .FirstOrDefaultAsync();

                                var StoreKeeperUserId = await tenantDb.FactoryUserRoles
                                    .Where(x => x.TenantId == dto.TenantId && x.RoleId == StoreKeeperRoleId)
                                    .Select(x => x.UserId)
                                    .ToListAsync();
                                var userIds = StoreKeeperUserId.ToList();

                                var nullableUserIds = userIds.Select(id => (int?)id).ToList();

                                var lowStock = new LowStockEventDto
                                {
                                    TenantId = dto.TenantId,
                                    ItemId = inventoryItem.ItemId,
                                    ItemName = inventoryItem.ItemName,
                                    QuantityAvailable = inventoryItem.QuantityAvailable,
                                    ReorderLevel = inventoryItem.ReorderLevel,
                                    EventType = "LowStockError",
                                    TargetUserIds = nullableUserIds
                                };

                                await PublishLowStockKafkaAsync(dto.TenantId, lowStock);

                                throw new Exception(
                                    $"Insufficient stock for '{inventoryItem.ItemName}'. Required additional: {diff}, Available: {inventoryItem.QuantityAvailable}");
                            }

                            inventoryItem.QuantityAvailable -= diff;
                        }
                        else if (diff < 0)
                            {
                                inventoryItem.QuantityAvailable += Math.Abs(diff);
                            }

                            inventoryItem.UpdatedAt = DateTime.UtcNow;
                            inventoryItem.UpdatedBy = dto.UpdatedBy ?? 0;

                            existingTool.QuantityRequired = requiredQty;
                            existingTool.UpdatedAt = DateTime.UtcNow;
                            existingTool.UpdatedBy = dto.UpdatedBy ?? 0;
                        }
                    else
                    {
                  
                        if (inventoryItem.QuantityAvailable < requiredQty)
                        {
                            var StoreKeeperRoleId = await tenantDb.FactoryRoles
                               .Where(x => x.RoleName == "Store Keeper")
                               .Select(x => x.RoleId)
                               .FirstOrDefaultAsync();

                            var StoreKeeperUserId = await tenantDb.FactoryUserRoles
                                .Where(x => x.TenantId == dto.TenantId && x.RoleId == StoreKeeperRoleId)
                                .Select(x => x.UserId)
                                .ToListAsync();
                            var userIds = StoreKeeperUserId.ToList();

                            var nullableUserIds = userIds.Select(id => (int?)id).ToList();

                            var lowStock = new LowStockEventDto
                            {
                                TenantId = dto.TenantId,
                                ItemId = inventoryItem.ItemId,
                                ItemName = inventoryItem.ItemName,
                                QuantityAvailable = inventoryItem.QuantityAvailable,
                                ReorderLevel = inventoryItem.ReorderLevel,
                                EventType = "LowStockError",
                                TargetUserIds = nullableUserIds
                            };

                            await PublishLowStockKafkaAsync(dto.TenantId, lowStock);

                            throw new Exception(
                                $"Insufficient stock for new tool '{inventoryItem.ItemName}'. Required: {requiredQty}, Available: {inventoryItem.QuantityAvailable}");
                        }

                            inventoryItem.QuantityAvailable -= requiredQty;
                            inventoryItem.UpdatedAt = DateTime.UtcNow;
                            inventoryItem.UpdatedBy = dto.UpdatedBy ?? 0;

                            var newTool = new WorkOrderRequiredTool
                            {
                                WorkOrderId = entity.WorkOrderId,
                                ToolId = toolDto.ToolId,
                                QuantityRequired = requiredQty,
                                CreatedBy = dto.UpdatedBy ?? 0,
                                CreatedAt = DateTime.UtcNow
                            };

                            tenantDb.WorkOrderRequiredTools.Add(newTool);
                        }
                    }

                    await tenantDb.SaveChangesAsync();
                }

                await tenantDb.SaveChangesAsync();

        if (entity.AssignedToUserId.HasValue)
                
            {
                var assignedToUserId = await tenantDb.AssetRegistry
                         .Where(x => x.AssetId == dto.AssetId && x.IsActive && !x.IsDeleted)
                         .Join(
                             tenantDb.AssetTracking,
                             assetRegistry => assetRegistry.AssetId,
                             assetTracking => assetTracking.AssetId,
                             (assetRegistry, assetTracking) => assetTracking
                         )
                         .Where(x => !x.IsDeleted)
                         .Select(x => x.AssignedTo)
                         .FirstOrDefaultAsync();

                var supervisorUserIds = new List<int?> { entity.AssignedToUserId }; 

                if (assignedToUserId.HasValue)
                {
                    supervisorUserIds.Add(assignedToUserId.Value);
                }

                supervisorUserIds = supervisorUserIds.Distinct().ToList();

                var eventDto = new WorkOrderEventDto
                {
                    WorkOrderId = entity.WorkOrderId,
                    TenantId = entity.TenantId,
                    WorkOrderNumber = entity.WorkOrderNumber,
                    Title = entity.Title,
                    EventType = "Assigned",
                    EventTime = DateTime.UtcNow,
                    Priority = entity.Priority.ToString(),
                    Status = entity.Status.ToString(),
                    TargetUserId = entity.AssignedToUserId,
                    AssignedToUserId = entity.AssignedToUserId,
                    AssignedToTeamId = entity.AssignedToTeamId,
                    SupervisorUserIds = supervisorUserIds
                };

                var correlationId = Guid.NewGuid().ToString();
                string topicName = KafkaCommonTopics.BuildTopicName(entity.TenantId, eventDto.EventType);

                var kafkaRequest = new
                {
                    Topic = topicName,
                    Key = $"workorder-{entity.WorkOrderId}",
                    Payload = eventDto,
                    Source = "WorkOrderService",
                    Headers = new Dictionary<string, string>
                    {
                        ["tenant-id"] = entity.TenantId.ToString(),
                        ["correlation-id"] = correlationId
                    }
                };

                var kafkaApiUrl = ConstantUrls.kafkaPublish;

                using (var httpClient = new HttpClient())
                {
                    var jsonContent = new StringContent(JsonSerializer.Serialize(kafkaRequest),
                                                        Encoding.UTF8, "application/json");

                    var kafkaResponse = await httpClient.PostAsync(kafkaApiUrl, jsonContent);
                    kafkaResponse.EnsureSuccessStatusCode();
                }
            }
                /*var eventDto = new WorkOrderEventDto
                {
                    WorkOrderId = entity.WorkOrderId,
                    TenantId = entity.TenantId,
                    WorkOrderNumber = entity.WorkOrderNumber,
                    Title = entity.Title,
                    EventType = "Assigned",
                    EventTime = DateTime.UtcNow,
                    Priority = entity.Priority.ToString(),
                    Status = entity.Status.ToString(),
                    TargetUserId = entity.AssignedToUserId,
                    AssignedToUserId = entity.AssignedToUserId,
                    AssignedToTeamId = entity.AssignedToTeamId
                };

                var correlationId = Guid.NewGuid().ToString();
                string topicName = KafkaCommonTopics.BuildTopicName(entity.TenantId, eventDto.EventType);

                var kafkaRequest = new
                {
                    Topic = topicName,
                    Key = $"workorder-{entity.WorkOrderId}",
                    Payload = eventDto,
                    Source = "WorkOrderService",
                    Headers = new Dictionary<string, string>
                    {
                        ["tenant-id"] = entity.TenantId.ToString(),
                        ["correlation-id"] = correlationId
                    }
                };

                var kafkaApiUrl = ConstantUrls.kafkaPublish;

                using (var httpClient = new HttpClient())
                {
                    var jsonContent = new StringContent(JsonSerializer.Serialize(kafkaRequest),
                                                        Encoding.UTF8, "application/json");

                    var kafkaResponse = await httpClient.PostAsync(kafkaApiUrl, jsonContent);
                    kafkaResponse.EnsureSuccessStatusCode();
                }*/

                await _auditLogger.LogAuditAsync("WorkOrder", "Update", dto.UpdatedBy,
                    dto.TenantId.ToString(), entity.WorkOrderId.ToString());

                await transaction.CommitAsync();

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = WorkOrderStatusMessage.WorkOrderUpdated;
           
            return response;
        }

        public async Task<CommonResponseModel> DeleteWorkOrderAsync(int WorkOrderId, int tenantId)
        {
            var response = new CommonResponseModel();
            
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                var entity = await tenantDb.WorkOrders.FirstOrDefaultAsync(x => x.WorkOrderId == WorkOrderId && !x.IsDeleted);
                if (entity == null)
                {
                    response.StatusCode = StatusCode.NotFound;
                    response.StatusMessage = WorkOrderStatusMessage.WorkOrderNotFound;
                    return response;
                }

                entity.IsDeleted = true;
                entity.IsActive = false;
                entity.DeletedBy = entity.UpdatedBy;
                entity.DeletedAt = DateTime.UtcNow;

                var relatedTools = await tenantDb.WorkOrderRequiredTools
           .Where(rt => rt.WorkOrderId == WorkOrderId && !rt.IsDeleted)
           .ToListAsync();

                foreach (var tool in relatedTools)
                {
                    tool.IsDeleted = true;
                    tool.IsActive = false;
                    tool.DeletedBy = entity.UpdatedBy;
                    tool.DeletedAt = DateTime.UtcNow;
                }
                await tenantDb.SaveChangesAsync();
                await _auditLogger.LogAuditAsync("WorkOrder", "Delete", entity.UpdatedBy, entity.TenantId.ToString(), entity.WorkOrderId.ToString()); ;

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = WorkOrderStatusMessage.WorkOrderDeletion;
           

            return response;
        }
        public async Task<GetSpecificRecord<LaborAnalyticsDto>> GetLaborAnalyticsAsync(int tenantId)
        {
            var response = new GetSpecificRecord<LaborAnalyticsDto>();

                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                var users = await tenantDb.FactoryUsers
                    .Where(u => u.TenantId == tenantId && !u.IsDeleted)
                    .Include(u => u.FactoryUserRoles)
                        .ThenInclude(ur => ur.FactoryRoles)
                    .ToListAsync();

                var technicians = users
                    .Where(u => u.FactoryUserRoles.Any(ur => ur.FactoryRoles.RoleName == "Technician"))
                    .ToList();

                var technicianIds = technicians.Select(t => t.UserId).ToList();

                var workOrders = await tenantDb.WorkOrders
                    .Where(wo => wo.TenantId == tenantId && !wo.IsDeleted && wo.AssignedToUserId != null)
                    .Where(wo => technicianIds.Contains(wo.AssignedToUserId.Value))
                    .ToListAsync();

                var technicianAnalytics = technicians.Select(t =>
                {
                    var assignedWorkOrders = workOrders.Where(wo => wo.AssignedToUserId == t.UserId).ToList();
                    var activeWorkOrders = assignedWorkOrders.Count(wo => wo.Status == WorkOrderStatus.InProgress);

                    var utilization = assignedWorkOrders.Any()
                        ? (double)activeWorkOrders / assignedWorkOrders.Count * 100
                        : 0;

                    return new TechnicianAnalyticsDto
                    {
                        TechnicianName = $"{t.FirstName} {t.LastName}",
                        Skills = null,
                        Status = activeWorkOrders > 0 ? "Busy" : "Available",
                        Utilization = Math.Round(utilization, 2),
                        ActiveWorkOrders = activeWorkOrders
                    };
                }).ToList();

                var totalTechnicians = technicianAnalytics.Count;
                var availableTechnicians = technicianAnalytics.Count(x => x.Status == "Available");
                var avgUtilization = technicianAnalytics.Any() ? technicianAnalytics.Average(x => x.Utilization) : 0;
                var activeWorkOrdersTotal = technicianAnalytics.Sum(x => x.ActiveWorkOrders);

                var resourceEfficiency = totalTechnicians > 0
                    ? avgUtilization * (availableTechnicians / (double)totalTechnicians)
                    : 0;

                response.Data = new LaborAnalyticsDto
                {
                    TotalTechnicians = totalTechnicians,
                    AvailableTechnicians = availableTechnicians,
                    AvgUtilization = Math.Round(avgUtilization, 2),
                    ActiveWorkOrders = activeWorkOrdersTotal,
                    ResourceEfficiency = Math.Round(resourceEfficiency, 2),
                    TechnicianDetails = technicianAnalytics
                };

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = WorkOrderStatusMessage.LaborAnalyticsFetchedSuccessfully;
            

            return response;
        }
        public async Task<GetSpecificRecord<ResourceUsageAnalyticsDto>> GetResourceUsageAnalyticsAsync(int tenantId)
        {
            var response = new GetSpecificRecord<ResourceUsageAnalyticsDto>();
            
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                var inventoryItems = await tenantDb.Inventory
                    .Where(i => i.TenantId == tenantId && !i.IsDeleted && i.IsActive)
                    .Include(i => i.StorageLocation)
                    .ToListAsync();

                var resourceUsage = inventoryItems.Select(i =>
                {
                    var totalCapacity = i.QuantityAvailable + i.ReservedQuantity;
                    var utilization = totalCapacity > 0 ? (double)i.ReservedQuantity / totalCapacity * 100 : 0;

                    var status = i.AvailableQuantity switch
                    {
                        0 => "Out of Stock",
                        _ when i.AvailableQuantity <= i.ReorderLevel => "Low Stock",
                        _ => "In Stock"
                    };

                    return new ResourceUsageDto
                    {
                        ResourceId = i.ItemId,
                        ResourceName = i.ItemName,
                        ResourceCode = i.ItemCode,
                        ResourceType = i.Category?.ToString() ?? "Uncategorized",
                        Manufacturer = i.Manufacturer,
                        TotalQuantity = totalCapacity,
                        InUse = i.ReservedQuantity,
                        Available = i.AvailableQuantity,
                        Utilization = Math.Round(utilization, 2),
                        UnitPrice = i.UnitPrice,
                        TotalValue = i.UnitPrice * totalCapacity,
                        Status = status,
                        ReorderLevel = i.ReorderLevel,
                        MaxStockLevel = i.MaxStockLevel,
                        Location = i.StorageLocation?.LocationName ?? "Not Assigned"
                    };
                }).ToList();

                var categoryUtilization = resourceUsage
                    .GroupBy(r => r.ResourceType)
                    .Select(g => new ResourceCategoryUtilizationDto
                    {
                        Category = g.Key,
                        Utilization = Math.Round(g.Average(r => r.Utilization), 2),
                        TotalItems = g.Count(),
                        ItemsInUse = g.Sum(r => r.InUse),
                        TotalValue = g.Sum(r => r.TotalValue)
                    })
                    .ToList();

                var totalResources = resourceUsage.Count;
                var totalItems = resourceUsage.Sum(r => r.TotalQuantity);
                var totalInUse = resourceUsage.Sum(r => r.InUse);
                var totalValue = resourceUsage.Sum(r => r.TotalValue);

                var overallUtilization = totalItems > 0 ? (double)totalInUse / totalItems * 100 : 0;

                var lowStockItems = resourceUsage
                    .Where(r => r.Available > 0 && r.Available <= r.ReorderLevel)
                    .OrderBy(r => r.Available)
                    .ToList();

                var outOfStockItems = resourceUsage
                    .Where(r => r.Available == 0)
                    .ToList();

                response.Data = new ResourceUsageAnalyticsDto
                {
                    ResourceUsage = resourceUsage,
                    CategoryUtilization = categoryUtilization,
                    TotalResources = totalResources,
                    TotalItems = totalItems,
                    ItemsInUse = totalInUse,
                    TotalInventoryValue = totalValue,
                    AverageUtilization = Math.Round(overallUtilization, 2),
                    LowStockItems = lowStockItems,
                    OutOfStockItems = outOfStockItems,
                    LowStockCount = lowStockItems.Count,
                    OutOfStockCount = outOfStockItems.Count,
                    Summary = new ResourceSummaryDto
                    {
                        OptimalUtilization = resourceUsage.Count(r => r.Utilization >= 60 && r.Utilization <= 85),
                        UnderUtilized = resourceUsage.Count(r => r.Utilization < 60),
                        OverUtilized = resourceUsage.Count(r => r.Utilization > 85)
                    }
                };

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = WorkOrderStatusMessage.ResourceUsageAnalyticsFetched;
            

            return response;
        }
        public async Task<GetSpecificRecord<LaborResourceAnalyticsDto>> GetLaborResourceAnalyticsAsync(int tenantId)
        {
            var response = new GetSpecificRecord<LaborResourceAnalyticsDto>();

          
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                var users = await tenantDb.FactoryUsers
                    .Where(u => u.TenantId == tenantId && !u.IsDeleted)
                    .Include(u => u.FactoryUserRoles)
                        .ThenInclude(ur => ur.FactoryRoles)
                    .ToListAsync();

                var technicians = users
                    .Where(u => u.FactoryUserRoles.Any(ur => ur.FactoryRoles.RoleName == "Technician"))
                    .ToList();

                var technicianIds = technicians.Select(t => t.UserId).ToList();

                var workOrders = await tenantDb.WorkOrders
                    .Where(wo => wo.TenantId == tenantId && !wo.IsDeleted && wo.AssignedToUserId != null)
                    .Where(wo => technicianIds.Contains(wo.AssignedToUserId.Value))
                    .ToListAsync();

                var totalTechnicians = technicians.Count;
                var availableTechnicians = totalTechnicians > 0
                    ? technicians.Count(t => !workOrders.Any(wo => wo.AssignedToUserId == t.UserId && wo.Status == WorkOrderStatus.InProgress))
                    : 0;

                var activeWorkOrders = workOrders.Count(wo => wo.Status == WorkOrderStatus.InProgress);

                var avgUtilization = totalTechnicians > 0
                    ? (double)activeWorkOrders / totalTechnicians * 100
                    : 0;

                var resourceEfficiency = totalTechnicians > 0
                    ? avgUtilization * (availableTechnicians / (double)totalTechnicians)
                    : 0;

                var balanced = workOrders
                    .GroupBy(wo => wo.AssignedToUserId)
                    .Count(g => g.Count() >= 1 && g.Count() <= 3);

                var overloaded = workOrders
                    .GroupBy(wo => wo.AssignedToUserId)
                    .Count(g => g.Count() > 3);

                var underutilized = totalTechnicians - (balanced + overloaded);

                double avgResponseTime = 0;
                if (workOrders.Any(wo => wo.ScheduleDate != null))
                {
                    avgResponseTime = workOrders
                        .Where(wo => wo.ScheduleDate != null)
                        .Average(wo => (wo.ScheduleDate.Value - wo.CreatedAt).TotalHours);
                }

                var teamEfficiency = workOrders.Any()
                    ? (int)((double)workOrders.Count(wo => wo.Status == WorkOrderStatus.Completed) / workOrders.Count * 100)
                    : 0;

                var performanceMetrics = new FactoryOpsApp.Application.DTOs.PerformanceMetricsDto
                {
                    AvgResponseTime = Math.Round(avgResponseTime, 2),
                    FirstTimeFixRate = 0,
                    TeamEfficiency = teamEfficiency
                };


                decimal regularHours = workOrders.Sum(wo => (decimal)wo.EstimatedDurationMinutes);
                decimal overtime = 0; 
                decimal resourceCosts = 0; 

                decimal totalCost = regularHours + overtime + resourceCosts;

                var costAnalysis = new CostAnalysisDto
                {
                    RegularHours = regularHours,
                    Overtime = overtime,
                    ResourceCosts = resourceCosts,
                    TotalCost = totalCost
                };

                var laborCostBreakdown = new LaborCostBreakdownDto
                {
                    RegularHours = regularHours,
                    Overtime = overtime,
                    ResourceCosts = resourceCosts,
                    TotalCost = totalCost
                };

                response.Data = new LaborResourceAnalyticsDto
                {
                    TotalTechnicians = totalTechnicians,
                    AvailableTechnicians = availableTechnicians,
                    AvgUtilization = Math.Round(avgUtilization, 2),
                    AvgUtilizationChange = 0,
                    ActiveWorkOrders = activeWorkOrders,
                    ResourceEfficiency = Math.Round(resourceEfficiency, 2),
                    ResourceUsage = Math.Round(avgUtilization, 2),
                    WorkloadDistribution = new WorkloadDistributionDto
                    {
                        Balanced = balanced,
                        Overloaded = overloaded,
                        Underutilized = underutilized
                    },
                    PerformanceMetrics = performanceMetrics,
                    CostAnalysis = costAnalysis,
                    LaborCostBreakdown = laborCostBreakdown
                };

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = WorkOrderStatusMessage.ResourceUsageAnalyticsFetched;
            

            return response;
        }
        public async Task<CommonResponseModel> UpdateWorkOrderProgressAsync(WorkOrderProgresssUpdateDto dto)
        {
            var response = new CommonResponseModel();

                using var tenantDb = _tenantDbContext.GetTenantDbContext(dto.TenantId);
                await using var masterTransaction = await _masterDbcontext.Database.BeginTransactionAsync();
                await using var tenantTransaction = await tenantDb.Database.BeginTransactionAsync();

                var workOrder = await tenantDb.WorkOrders
                    .FirstOrDefaultAsync(w => w.WorkOrderId == dto.WorkOrderId && !w.IsDeleted);

                if (workOrder == null)
                {
                    response.StatusCode = "404";
                    response.StatusMessage = "Work order not found.";
                    return response;
                }

                if (dto.WorkOrderPhoto != null)
                {
                    var relativePath = await _fileStorageService.SaveFileAsync(dto.WorkOrderPhoto, "WorkOrderImages");
                    var fullPath = Path.Combine("wwwroot", relativePath);

                    workOrder.WorkOrderPhoto = dto.WorkOrderPhoto?.FileName ?? "";
                    workOrder.WorkOrderPhotoPath = relativePath;
                }

                if (dto.UpdateType != null)
                {
                    workOrder.UpdateType =
                        (UpdateTypeEnum)dto.UpdateType.Value;

                }
                if (!string.IsNullOrWhiteSpace(dto.LastUpdateMessage))
                    workOrder.LastUpdateMessage = dto.LastUpdateMessage;

                if (!string.IsNullOrWhiteSpace(dto.Comments))
                    workOrder.Comments = dto.Comments;

                if (dto.UpdatedBy != null)
                    workOrder.UpdatedBy = dto.UpdatedBy;

                workOrder.UpdatedAt = DateTime.UtcNow;

                if (dto.ProgressPercentage != null)
                    workOrder.ProgressPercentage = dto.ProgressPercentage.Value;

                if (dto.Status != null)
                {
                    workOrder.Status = dto.Status;
                }
                decimal GetExistingTotalTime()
                {
                    if (string.IsNullOrWhiteSpace(workOrder.TotalTime))
                        return 0;

                    return decimal.TryParse(workOrder.TotalTime, out var val) ? val : 0;
                }

                if (dto.Action == "Start")
                {
                    workOrder.IsStarted = true;
                    workOrder.IsPaused = false;
                    workOrder.StartTime = DateTime.UtcNow;
                    workOrder.Status = WorkOrderStatus.InProgress;
                }
                else if (dto.Action == "Pause")
                {
                    workOrder.IsPaused = true;
                    workOrder.IsStarted = false;
                    workOrder.PauseTime = DateTime.UtcNow;

                    if (workOrder.StartTime.HasValue)
                    {
                        decimal previous = GetExistingTotalTime();
                        var timeWorked = (decimal)(workOrder.PauseTime.Value - workOrder.StartTime.Value).TotalMinutes;
                        workOrder.TotalTime = (previous + timeWorked).ToString();
                    }
                }
                else if (dto.Action == "Resume")
                {
                    workOrder.IsPaused = false;
                    workOrder.IsStarted = true;
                    workOrder.StartTime = DateTime.UtcNow;
                }
                else if (dto.Action == "Complete")
                {
                    if (workOrder.StartTime.HasValue)
                    {
                        decimal previous = GetExistingTotalTime();
                        var timeWorked = (decimal)(DateTime.UtcNow - workOrder.StartTime.Value).TotalMinutes;
                        workOrder.TotalTime = (previous + timeWorked).ToString();
                    }

                    workOrder.ProgressPercentage = 100;
                    workOrder.Status = WorkOrderStatus.Completed;
                    workOrder.IsStarted = false;
                    workOrder.IsPaused = false;
                }
              
                await tenantDb.SaveChangesAsync();
                await tenantTransaction.CommitAsync();
                await masterTransaction.CommitAsync();
              //  await masterTransaction.RollbackAsync();

            if (workOrder.Status == WorkOrderStatus.Completed || workOrder.Status == WorkOrderStatus.InProgress)
            {
                var supervisorRoleId = await tenantDb.FactoryRoles
               .Where(x => x.RoleName == "Production Supervisor")
               .Select(x => x.RoleId)
               .FirstOrDefaultAsync();

                var supervisorUserId = await tenantDb.FactoryUserRoles
                    .Where(x => x.TenantId == dto.TenantId && x.RoleId == supervisorRoleId)
                    .Select(x => x.UserId)
                    .ToListAsync();
                 var userIds = supervisorUserId.ToList();

                List<int?> nullableUserIds;

                if (workOrder.Status == WorkOrderStatus.Completed)
                {
                    var assignedToUserId = await tenantDb.AssetRegistry
                        .Where(x => x.AssetId == workOrder.AssetId && x.IsActive && !x.IsDeleted)
                        .Join(
                            tenantDb.AssetTracking,
                            assetRegistry => assetRegistry.AssetId,
                            assetTracking => assetTracking.AssetId,
                            (assetRegistry, assetTracking) => assetTracking
                        )
                        .Where(x => !x.IsDeleted)
                        .Select(x => x.AssignedTo)
                        .FirstOrDefaultAsync();

                    if (assignedToUserId.HasValue)
                    {
                        userIds.Add(assignedToUserId.Value);
                    }

                    nullableUserIds = userIds.Select(id => (int?)id).ToList();
                }
                else
                {
                    nullableUserIds = userIds.Select(id => (int?)id).ToList();
                }

                var progressEvent = new WorkOrderProgressUpdatedEventDto
                {
                    TenantId = workOrder.TenantId,
                    WorkOrderId = workOrder.WorkOrderId,
                    WorkOrderNumber = workOrder.WorkOrderNumber,
                    NewStatus = workOrder.Status.ToString(),
                    EventType = "WorkOrderProgressUpdated",
                    ProgressPercentage = workOrder.ProgressPercentage,
                    Action = dto.Action,
                    Status = dto.Status,
                    UpdatedBy = dto.UpdatedBy,
                    UpdatedAt = DateTime.UtcNow,
                    TargetUserIds = nullableUserIds

                };

                var correlationId = Guid.NewGuid().ToString();
                string topic = KafkaCommonTopics.BuildTopicName(workOrder.TenantId, progressEvent.EventType);

                var kafkaRequest = new
                {
                    Topic = topic,
                    Key = $"workorder-progress-{workOrder.WorkOrderId}",
                    Payload = progressEvent,
                    Source = "WorkOrderService",
                    Headers = new Dictionary<string, string>
                    {
                        ["tenant-id"] = workOrder.TenantId.ToString(),
                        ["correlation-id"] = correlationId
                    }
                };

                using var httpClient = new HttpClient();
                var jsonBody = new StringContent(JsonSerializer.Serialize(kafkaRequest), Encoding.UTF8, "application/json");
                var kafkaResponse = await httpClient.PostAsync(ConstantUrls.kafkaPublish, jsonBody);
                kafkaResponse.EnsureSuccessStatusCode();

            }

            return new CommonResponseModel
            {
                StatusCode = "200",
                StatusMessage = "Work order progress updated successfully."
            };
        }
        public async Task<GetAllRecord<GetWorkOrderProgresssUpdateDto>> GetWorkOrderProgressAsync(int tenantId)
        {
            var response = new GetAllRecord<GetWorkOrderProgresssUpdateDto>();

                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                var excludedStatuses = new WorkOrderStatus?[]                {
                                WorkOrderStatus.Cancelled,
                                WorkOrderStatus.Overdue,
                                WorkOrderStatus.Inactive,
                            };

                var data = await tenantDb.WorkOrders
                    .Where(x =>
                        x.TenantId == tenantId &&
                        !x.IsDeleted &&
                        x.IsActive && x.AssignedToUserId != null
                        &&
                        !excludedStatuses.Contains(x.Status)
                    )

                    .Select(x => new GetWorkOrderProgresssUpdateDto
                    {
                        WorkOrderId = x.WorkOrderId,
                        WorkOrderNumber = x.WorkOrderNumber,
                        TenantId = x.TenantId,
                        Status = x.Status,
                        ProgressPercentage = x.ProgressPercentage,
                        WorkOrderPhotoPath = x.WorkOrderPhotoPath,
                        StartTime = x.StartTime,
                        PauseTime = x.PauseTime,
                        IsStarted = x.IsStarted,
                        IsPaused = x.IsPaused,
                        TotalTime = x.TotalTime,
                        AssignedToUser = x.AssignedToUser != null ? x.AssignedToUser.FirstName + " " + x.AssignedToUser.LastName : null,
                        LocationId = x.LocationId,
                        LocationName = x.Location != null ? x.Location.LocationName : null,
                        LastUpdateMessage = x.LastUpdateMessage,
                        Comments = x.Comments,
                        UpdatedAt = x.UpdatedAt,
                        AssignedToUserId = x.AssignedToUserId,
                        UpdateType = x.UpdateType,

                    })
                    .OrderByDescending(x => x.UpdatedAt)
                    .ToListAsync();

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = WorkOrderStatusMessage.WorkOrdersFetchedSuccessfully;
                response.GetAllData = data;
           

            return response;
        }

    }
}

