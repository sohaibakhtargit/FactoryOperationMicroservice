using CsvHelper;
using FactoryOperation_WorkOrder.FactoryOpsApp.Application.DTOs;
using FactoryOperation_WorkOrder.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.AuditLogs;
using FactoryOperation_WorkOrder.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.Common;
using FactoryOperation_WorkOrder.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.EventTrace;
using FactoryOperation_WorkOrder.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.Notification;
using FactoryOperation_WorkOrder.FactoryOpsApp.Application.Models;
using FactoryOperation_WorkOrder.FactoryOpsApp.Domain.Entities.FactoryOpsTenants;
using FactoryOperation_WorkOrder.FactoryOpsApp.Infrastructure.Implementation.Services.Common;
using FactoryOperation_WorkOrder.FactoryOpsApp.Infrastructure.Implementation.Services.TenantAdmin.WorkOrderManagement;
using FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.WorkOrderManagement;
using FactoryOpsApp.Domain.Entities;
using FactoryOpsApp.Domain.Entities.FactoryOpsTenants;
using FactoryOpsApp.Infrastructure.DBContext;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text;
using System.Text.Json;
using static FactoryOperation_WorkOrder.FactoryOpsApp.Application.DTOs.TechnicianLoadDto;
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
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConfiguration _configuration;
        private readonly INotificationBuilderService _notificationBuilderService;

        public WorkOrderRepository(
           TenantDbContextFactory tenantDbContext,
           MasterFactoryOpsDbContext masterDbcontext,
           IExceptionLoggerService exceptionLogger,
           IAuditLogService auditLogger,
           INotificationService notificationService,
           IFileStorageService fileStorageService,
           IServiceScopeFactory scopeFactory,
           IConfiguration configuration,
           INotificationBuilderService notificationBuilderService
          )
        {
            _masterDbcontext = masterDbcontext;
            _tenantDbContext = tenantDbContext;
            _auditLogger = auditLogger;
            _fileStorageService = fileStorageService;
            _scopeFactory = scopeFactory;
            _configuration = configuration;
            _notificationBuilderService = notificationBuilderService;
        }

        public async Task<GetAllRecord<WorkOrderDto>> GetWorkOrderAllAsync(int tenantId, WorkOrderTypeEnum? workOrderType)
        {
            string baseUrl = _configuration["BaseUrl:Staging"] ?? "https://ms.stagingsdei.com:8125";
            var response = new GetAllRecord<WorkOrderDto>();
           
            using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

            var query = tenantDb.WorkOrders
                .Where(x => x.TenantId == tenantId && !x.IsDeleted);

            if (workOrderType.HasValue)
            {
                query = query.Where(x => x.WorkOrderType == workOrderType.Value);
            }
            else
            {
                query = query.Where(x => x.WorkOrderType != WorkOrderTypeEnum.Preventive);
            }

            var workOrders = await query
                .Include(x => x.Asset)
                .Include(x => x.AssignedToUser)
                .Include(x => x.AssignedToTeam)
                .Include(x => x.RequiredTools)
                .Include(x => x.Location)
                .OrderByDescending(x => x.WorkOrderId)
                .ToListAsync();

            var workOrderIds = workOrders.Select(x => x.WorkOrderId).ToList();

            var toolsData = await tenantDb.WorkOrderRequiredTools
                .Where(t => workOrderIds.Contains(t.WorkOrderId!.Value) && !t.IsDeleted)
                .ToListAsync();

            var inventory = await tenantDb.Inventory
                .Select(i => new { i.ItemId, i.ItemName })
                .ToListAsync();

            var data = workOrders.Select(x =>
            {
                var tools = toolsData.Where(t => t.WorkOrderId == x.WorkOrderId).ToList();
                var firstTool = tools.FirstOrDefault();

                return new WorkOrderDto
                {
                    WorkOrderId = x.WorkOrderId,
                    WorkOrderNumber = x.WorkOrderNumber,
                    Title = x.Title,
                    Description = x.Description,
                    LocationId = x.LocationId,
                    LocationName = x.Location?.LocationName,
                    AssetId = x.AssetId,
                    AssetName = x.Asset != null ? x.Asset.AssetName : "N/A",
                    Status = x.Status,
                    Priority = x.Priority,
                    WorkOrderType = x.WorkOrderType,
                    AssignedToUserId = x.AssignedToUserId,
                    AssignedToUser = x.AssignedToUser != null
                        ? x.AssignedToUser.FirstName + " " + x.AssignedToUser.LastName
                        : null,
                    AssignedToTeamId = x.AssignedToTeamId,
                    AssignedToTeam = x.AssignedToTeam?.Name,

                    DueDate = x.DueDate,
                    ScheduleDate = x.ScheduleDate,
                    WorkOrderMediaPath = !string.IsNullOrEmpty(x.WorkOrderMediaPath)
                     ? $"{baseUrl}/{x.WorkOrderMediaPath}"
                     : null,
                    FileType = x.FileType,
                    EstimatedDurationMinutes = x.EstimatedDurationMinutes,
                    Instructions = x.Instructions,
                    CompletionNotes = x.CompletionNotes,
                    LaborCost = x.LaborCost,
                    PartCost = x.PartCost,
                    TotalCost = x.LaborCost + x.PartCost,
                    CreatedBy = x.CreatedBy,
                    CreatedAt = x.CreatedAt,
                    RequiredToolId = firstTool?.ToolId,

                    RequiredToolName = inventory
                        .Where(i => i.ItemId == firstTool?.ToolId)
                        .Select(i => i.ItemName)
                        .FirstOrDefault(),

                    SparePart = tools.Select(t => new WorkOrderToolDto
                    {
                        ToolId = t.ToolId,
                        ToolName = inventory
                            .Where(i => i.ItemId == t.ToolId)
                            .Select(i => i.ItemName)
                            .FirstOrDefault(),
                        QuantityRequired = t.QuantityRequired ?? 0
                    }).ToList()
                };
            }).ToList();

            foreach (var item in data)
            {
                if (item.DueDate.HasValue)
                    item.DueDate = ToUtc(item.DueDate.Value);

                if (item.ScheduleDate.HasValue)
                    item.ScheduleDate = ToUtc(item.ScheduleDate.Value);
            }

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
                string baseUrl = _configuration["BaseUrl:Staging"] ?? "https://ms.stagingsdei.com:8125";
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                var workOrder = await tenantDb.WorkOrders
                    .Where(x => x.WorkOrderId == WorkOrderId && x.TenantId == tenantId && !x.IsDeleted)
                    .Include(x => x.Asset)
                    .Include(x=> x.AssignedToUser)
                    .Include(x => x.AssignedToTeam)
                    .FirstOrDefaultAsync();

                if (workOrder == null)
                {
                    response.StatusCode = StatusCode.NotFound;
                    response.StatusMessage = WorkOrderStatusMessage.WorkOrderNotFound;
                    return response;
                }

               
                var tools = await tenantDb.WorkOrderRequiredTools
                    .Where(t => t.WorkOrderId == WorkOrderId && !t.IsDeleted)
                    .ToListAsync();

                
                var inventory = await tenantDb.Inventory
                    .Select(i => new { i.ItemId, i.ItemName })
                    .ToListAsync();

                var firstTool = tools.FirstOrDefault();

                
                var data = new WorkOrderDto
                {
                    WorkOrderId = workOrder.WorkOrderId,
                    WorkOrderNumber = workOrder.WorkOrderNumber,
                    Title = workOrder.Title,
                    Description = workOrder.Description,
                    LocationId = workOrder.LocationId,
                    LocationName = workOrder.Location?.LocationName,
                    AssetId = workOrder.AssetId,
                    AssetName = workOrder.Asset != null ? workOrder.Asset.AssetName : "N/A",
                    Status = workOrder.Status,
                    Priority = workOrder.Priority,
                    WorkOrderType = workOrder.WorkOrderType,
                    AssignedToUserId = workOrder.AssignedToUserId,
                    AssignedToUser = workOrder.AssignedToUser != null
                        ? workOrder.AssignedToUser.FirstName + " " + workOrder.AssignedToUser.LastName
                        : null,
                    AssignedToTeamId = workOrder.AssignedToTeamId,
                    AssignedToTeam = workOrder.AssignedToTeam?.Name,
                    FileType = workOrder.FileType,

                    DueDate = workOrder.DueDate,
                    ScheduleDate = workOrder.ScheduleDate,

                    EstimatedDurationMinutes = workOrder.EstimatedDurationMinutes,
                    Instructions = workOrder.Instructions,
                    CompletionNotes = workOrder.CompletionNotes,
                    LaborCost = workOrder.LaborCost,
                    PartCost = workOrder.PartCost,
                    TotalCost = workOrder.LaborCost + workOrder.PartCost,

                    WorkOrderMediaPath = !string.IsNullOrEmpty(workOrder.WorkOrderMediaPath)
                        ? $"{baseUrl.TrimEnd('/')}/{workOrder.WorkOrderMediaPath}"
                        : null,

                    CreatedBy = workOrder.CreatedBy,
                    CreatedAt = workOrder.CreatedAt,
                    RequiredToolId = firstTool?.ToolId,

                    RequiredToolName = inventory
                        .Where(i => i.ItemId == firstTool?.ToolId)
                        .Select(i => i.ItemName)
                        .FirstOrDefault(),

                    SparePart = tools.Select(t => new WorkOrderToolDto
                    {
                        ToolId = t.ToolId,
                        ToolName = inventory
                            .Where(i => i.ItemId == t.ToolId)
                            .Select(i => i.ItemName)
                            .FirstOrDefault(),
                        QuantityRequired = t.QuantityRequired ?? 0
                    }).ToList()
                };

                if (data.DueDate.HasValue)
                    data.DueDate = ToUtc(data.DueDate.Value);

                if (data.ScheduleDate.HasValue)
                    data.ScheduleDate = ToUtc(data.ScheduleDate.Value);

                response.Data = data;
                response.StatusCode = StatusCode.Success;
                response.StatusMessage = WorkOrderStatusMessage.WorkOrdersFetchedSuccessfully;
            }
            catch (Exception ex)
            {
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"{WorkOrderStatusMessage.WorkOrdersFetchFailed} {ex.Message}";
            }

            return response;
        }
        public async Task<BulkWorkOrderImportResult> ImportBulkWorkOrdersAsync(BulkWorkOrderImportRequest request)
        {

            using var tenantDb = _tenantDbContext.GetTenantDbContext(request.TenantId);

            var relativePath = await _fileStorageService.SaveFileAsync(
                request.File!,
                "uploads/bulk-workorders"
            );

            var bulkImport = new WorkOrderBulkImport
            {
                TenantId = request.TenantId,
                FileName = request.File!.FileName,
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
                        Title = row.Title!,
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
        public async Task<CommonWorkOrderResponseModel> CreateWorkOrderAsync(WorkOrderCreateDto dto)
        {
            var correlationId = Guid.NewGuid().ToString();
            var response = new CommonWorkOrderResponseModel();
            using var tenantDb = _tenantDbContext.GetTenantDbContext(dto.TenantId);
            using var transaction = await tenantDb.Database.BeginTransactionAsync();

            //Getting tenantEmail
            var TenantEmail = await _masterDbcontext.FactoryTenants.Where(x => x.TenantId == dto.TenantId).Select(x => x.AdminEmail).FirstOrDefaultAsync();

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
                        var userEmails = (await tenantDb.FactoryUsers
                                         .Where(u => userIds.Contains(u.UserId) && !u.IsDeleted)
                                         .Select(u => u.Email)
                                         .ToListAsync())
                                     .Append(TenantEmail)
                                     .Where(e => !string.IsNullOrWhiteSpace(e))
                                     .Distinct()
                                     .ToList();

                        var lowStock = new LowStockEventDto
                        {
                            TenantId = dto.TenantId,
                            TenantEmail = TenantEmail,
                            ItemId = inventoryItem.ItemId,
                            ItemCode = inventoryItem.ItemCode,
                            ItemName = inventoryItem.ItemName,
                            QuantityAvailable = inventoryItem.QuantityAvailable,
                            ReorderLevel = inventoryItem.ReorderLevel,
                            EventType = "LowStockError",
                            TargetUsersIds = nullableUserIds,
                            incomingNotifications = nullableUserIds,
                            TargetEmailAddresses = userEmails!
                        };

                        await PublishLowStockKafkaAsync(dto.TenantId, lowStock);
                        await transaction.RollbackAsync();

                        response.StatusCode = StatusCode.BadRequest;
                        response.StatusMessage = $"Required tool '{inventoryItem.ItemName}' is not enough. Available quantity: {inventoryItem.QuantityAvailable}, Required: {requiredQty}";
                        response.WorkOrderId = 0;

                        return response;
                    }
                }
            }

            if (dto.ScheduleDate.HasValue)
            {
                var scheduleDate = dto.ScheduleDate.Value.ToUniversalTime();
                var dueDate = dto.DueDate.ToUniversalTime();

                if (scheduleDate > dueDate)
                {
                    response.StatusCode = StatusCode.BadRequest;
                    response.StatusMessage = "Schedule date cannot be later than due date.";
                    response.WorkOrderId = 0;

                    return response;
                }
            }


            var entity = new WorkOrder
            {
                TenantId = dto.TenantId,
                WorkOrderProgressMedia = null,
                WorkOrderProgressMediaPath = null,
                Title = dto.Title,
                Description = dto.Description,
                LocationId = dto.LocationId,
                Status = dto.Status ?? WorkOrderStatus.Open,
                Priority = dto.Priority ?? PriorityLevel.Medium,
                WorkOrderType = dto.WorkOrderType,
                AssignedToUserId = dto.AssignedToUserId,
                AssignedToTeamId = dto.AssignedToTeamId,
                ScheduleDate = dto.ScheduleDate,
                DueDate = dto.DueDate.ToUniversalTime(),
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

            using var scope = _scopeFactory.CreateScope();
            var _trace = scope.ServiceProvider.GetRequiredService<IEventTraceLogger>();

            await _trace.TrackAsync(new EventTraceEntry
            {
                CorrelationId = correlationId,
                TenantId = dto.TenantId,
                Service = "WorkOrderService",
                Stage = "CREATED",
                Status = "SUCCESS",
                Message = $"WO {entity.WorkOrderNumber} created"
            });


            List<LowStockEventDto> lowStockWarnings = new();

            if (dto.RequiredTools != null && dto.RequiredTools.Any())
            {
                foreach (var toolDto in dto.RequiredTools)
                {
                    var requiredQty = (toolDto.QuantityRequired == null || toolDto.QuantityRequired <= 0)
                    ? 1: toolDto.QuantityRequired.Value;

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
                           .Where(x => x.RoleName == "Store Keeper" && !x.IsDeleted)
                           .Select(x => x.RoleId)
                           .FirstOrDefaultAsync();

                            var StoreKeeperUserId = await tenantDb.FactoryUserRoles
                                .Where(x => x.TenantId == dto.TenantId && x.RoleId == StoreKeeperRoleId && !x.IsDeleted)
                                .Select(x => x.UserId)
                                .ToListAsync();
                            var userIds = StoreKeeperUserId.ToList();

                            var nullableUserIds = userIds.Select(id => (int?)id).ToList();
                            var userEmails = (await tenantDb.FactoryUsers
                                            .Where(u => userIds.Contains(u.UserId) && !u.IsDeleted)
                                            .Select(u => u.Email)
                                            .ToListAsync())
                                        .Append(TenantEmail)
                                        .Where(e => !string.IsNullOrWhiteSpace(e))
                                        .Distinct()
                                        .ToList();

                            var lowStockWarning = new LowStockEventDto
                            {
                                TenantId = dto.TenantId,
                                TenantEmail = TenantEmail,
                                ItemId = inventoryItem.ItemId,
                                ItemCode = inventoryItem.ItemCode,
                                ItemName = inventoryItem.ItemName,
                                QuantityAvailable = inventoryItem.QuantityAvailable,
                                ReorderLevel = inventoryItem.ReorderLevel,
                                EventType = "LowStockWarning",
                                TargetUsersIds = nullableUserIds,
                                incomingNotifications = nullableUserIds,
                                TargetEmailAddresses = userEmails!
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

            var assignedToUserIdAsset = await tenantDb.AssetRegistry
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

            var notificationData = await _notificationBuilderService.BuildAsync(
                                    _tenantDbContext,
                                    entity.TenantId,
                                    dto.CreatedBy,
                                    entity.AssignedToUserId,
                                    entity.CreatedBy,
                                    assignedToUserIdAsset  
                                );

            var targetEmails = notificationData.Emails?
                   .Where(x => !string.IsNullOrWhiteSpace(x))
                   .Append(TenantEmail ?? string.Empty)
                   .Where(x => !string.IsNullOrWhiteSpace(x))
                   .Distinct()
                   .ToList();

            var eventDto = new WorkOrderEventDto
            {
                WorkOrderId = entity.WorkOrderId,
                TenantId = entity.TenantId,
                TenantEmail = TenantEmail,
                WorkOrderType = entity.WorkOrderType,
                WorkOrderTypeName = entity.WorkOrderType?.ToString(),
                WorkOrderNumber = entity.WorkOrderNumber,
                Title = entity.Title,
                EventType = "Created",
                EventTime = DateTime.UtcNow,
                Priority = entity.Priority.ToString(),
                Status = entity.Status.ToString(),
                AssignedToUserId = entity.AssignedToUserId,
                AssignedToTeamId = entity.AssignedToTeamId,

                outgoingNotifications = notificationData.Outgoing,
                incomingNotifications = notificationData.Incoming,
                TargetUsersIds = notificationData.AllUsers,
                TargetEmailAddresses = targetEmails!,

                CreatedBy = entity.CreatedBy,
            };


            {
                //var correlationId = Guid.NewGuid().ToString();
                string topicName = KafkaCommonTopics.BuildTopicName(entity.TenantId, eventDto.EventType);

                await _trace.TrackAsync(new EventTraceEntry
                {
                    CorrelationId = correlationId,
                    TenantId = dto.TenantId,
                    Service = "WorkOrderService",
                    Stage = "PUBLISH_REQUESTED",
                    Topic = topicName,
                    Status = "SUCCESS"
                });

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

                try
                {
                    var kafkaResponse = await httpClient.PostAsync(kafkaApiUrl, jsonContent);
                    kafkaResponse.EnsureSuccessStatusCode();
                }
                catch (Exception ex)
                {
                    await _trace.TrackAsync(new EventTraceEntry
                    {
                        CorrelationId = correlationId,
                        TenantId = dto.TenantId,
                        Service = "WorkOrderService",
                        Stage = "FAILED",
                        Topic = topicName,
                        Status = "ERROR",
                        Error = ex.Message
                    });

                    throw;
                }

            }


            foreach (var lowStockWarning in lowStockWarnings)
            {
                await PublishLowStockKafkaAsync(dto.TenantId, lowStockWarning);
            }

       
            await transaction.CommitAsync();

            response.StatusCode = StatusCode.Success;
            response.StatusMessage = WorkOrderStatusMessage.WorkOrderCreatedSuccessfully;
            response.WorkOrderId = entity.WorkOrderId;
            return response;
        }
        public async Task<CommonResponseModel> UploadWorkOrderMediaAsync(WorkOrderProgressMediaDto dto)
        {
            var response = new CommonResponseModel();

            await using var tenantDb = _tenantDbContext.GetTenantDbContext(dto.TenantId);
            await using var transaction = await tenantDb.Database.BeginTransactionAsync();

            try
            {
                var existingWorkOrder = await tenantDb.WorkOrders
                    .FirstOrDefaultAsync(l => l.WorkOrderId == dto.WorkOrderId && !l.IsDeleted);

                if (existingWorkOrder == null)
                {
                    response.StatusCode = StatusCode.NotFound;
                    response.StatusMessage = WorkOrderStatusMessage.WorkOrderNotFound;
                    return response;
                }

                var file = dto.WorkOrderProgressMedia;

                if (file == null || file.Length == 0)
                {
                    response.StatusCode = StatusCode.BadRequest;
                    response.StatusMessage = "No file provided";
                    return response;
                }

                if (file.Length > 100 * 1024 * 1024)
                {
                    response.StatusCode = StatusCode.BadRequest;
                    response.StatusMessage = "File size exceeds 100 MB limit";
                    return response;
                }

                var allowedExtensions = new[]
                {
                    ".jpg", ".jpeg", ".png", ".gif", ".webp",
                    ".mp4", ".mov", ".avi", ".mkv", ".webm",
                    ".pdf", ".doc", ".docx", ".xls", ".xlsx"
                };

                var extension = Path.GetExtension(file.FileName).ToLower();

                if (!allowedExtensions.Contains(extension))
                {
                    response.StatusCode = StatusCode.BadRequest;
                    response.StatusMessage = "Unsupported file format";
                    return response;
                }

                var fileType = GetFileType(file);

                string folder = fileType switch
                {
                    "image" => "WorkOrderImages",
                    "video" => "WorkOrderVideos",
                    "document" => "WorkOrderDocuments",
                    _ => "Others"
                };

                var relativePath = await _fileStorageService.SaveFileAsync(file, folder);

                existingWorkOrder.WorkOrderMedia = file.FileName ?? string.Empty;
                existingWorkOrder.WorkOrderMediaPath = relativePath ?? string.Empty;
                existingWorkOrder.FileType = fileType;
                existingWorkOrder.UpdatedAt = DateTime.UtcNow;
                existingWorkOrder.UpdatedBy = dto.UpdatedBy ?? 0;

                await tenantDb.SaveChangesAsync();
                await transaction.CommitAsync();

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = "Work order media uploaded successfully";

                return response;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                response.StatusCode = StatusCode.Error;
                response.StatusMessage = ex.Message;

                return response;
            }
        }
        public async Task<CommonResponseModel> UpdateWorkOrderAsync(WorkOrderUpdateDto dto)
        {
            var response = new CommonResponseModel();
            using var tenantDb = _tenantDbContext.GetTenantDbContext(dto.TenantId);
            using var transaction = await tenantDb.Database.BeginTransactionAsync();

            //Getting tenantEmail
            var TenantEmail = await _masterDbcontext.FactoryTenants.Where(x => x.TenantId == dto.TenantId).Select(x => x.AdminEmail).FirstOrDefaultAsync();
           
            var entity = await tenantDb.WorkOrders
                .FirstOrDefaultAsync(x => x.WorkOrderId == dto.WorkOrderId && !x.IsDeleted);

            if (entity == null)
            {
                response.StatusCode = StatusCode.NotFound;
                response.StatusMessage = WorkOrderStatusMessage.WorkOrderNotFound;
                return response;
            }

            if (!dto.UpdatedBy.HasValue)
                return new CommonResponseModel
                {
                    StatusCode = StatusCode.BadRequest,
                    StatusMessage = "UpdatedBy is required"
                };

            if (dto.ScheduleDate.HasValue)
            {
                var scheduleDate = dto.ScheduleDate.Value.ToUniversalTime();
                var dueDate = dto.DueDate.ToUniversalTime();

                if (scheduleDate > dueDate)
                {
                    return new CommonResponseModel
                    {
                        StatusCode = StatusCode.BadRequest,
                        StatusMessage = "Schedule date cannot be later than due date."
                    };
                }
            }

            var oldAssignedUserId = entity.AssignedToUserId;

            entity.Title = dto.Title;
            entity.AssetId = dto.AssetId;
            entity.Description = dto.Description;
            entity.LocationId = dto.LocationId;
            entity.Priority = dto.Priority;
            entity.WorkOrderType = dto.WorkOrderType;
            entity.AssignedToUserId = dto.AssignedToUserId;
            entity.AssignedToTeamId = dto.AssignedToTeamId;

            entity.DueDate = dto.DueDate.ToUniversalTime();
            entity.ScheduleDate = dto.ScheduleDate;
            entity.EstimatedDurationMinutes = dto.EstimatedDurationMinutes;
            entity.Instructions = dto.Instructions;
            entity.CompletionNotes = dto.CompletionNotes;
            entity.LaborCost = dto.LaborCost;
            entity.PartCost = dto.PartCost;
            entity.TotalCost = dto.LaborCost + dto.PartCost;
            entity.UpdatedBy = dto.UpdatedBy;
            entity.UpdatedAt = DateTime.UtcNow;

            if (dto.AssignedToUserId != null)
            {
                entity.Status = WorkOrderStatus.Assigned;
            }
            if (entity.ServiceRequestId.HasValue &&
                dto.AssignedToUserId.HasValue &&
                oldAssignedUserId != dto.AssignedToUserId)
            {
                var serviceRequest = await tenantDb.ServiceRequests
                    .FirstOrDefaultAsync(x =>
                        x.ServiceRequestId == entity.ServiceRequestId &&
                        x.TenantId == dto.TenantId &&
                        !x.IsDeleted);

                if (serviceRequest != null)
                {
                    serviceRequest.AssignedToUserId = dto.AssignedToUserId;
                    serviceRequest.Status = ServiceRequestStatus.Assigned;
                    serviceRequest.UpdatedAt = DateTime.UtcNow;
                    serviceRequest.UpdatedBy = dto.UpdatedBy;
                }
            }
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
                                  .Where(x => x.RoleName == "Store Keeper" && !x.IsDeleted)
                                  .Select(x => x.RoleId)
                                  .FirstOrDefaultAsync();

                                var StoreKeeperUserId = await tenantDb.FactoryUserRoles
                                    .Where(x => x.TenantId == dto.TenantId && x.RoleId == StoreKeeperRoleId && !x.IsDeleted)
                                    .Select(x => x.UserId)
                                    .ToListAsync();
                                var userIds = StoreKeeperUserId.ToList();

                                var nullableUserIds = userIds.Select(id => (int?)id).ToList();

                                var userEmails = (await tenantDb.FactoryUsers
                                            .Where(u => userIds.Contains(u.UserId) && !u.IsDeleted)
                                            .Select(u => u.Email)
                                            .ToListAsync())
                                        .Append(TenantEmail)
                                        .Where(e => !string.IsNullOrWhiteSpace(e))
                                        .Distinct()
                                        .ToList();

                                var lowStock = new LowStockEventDto
                                {
                                    TenantId = dto.TenantId,
                                    ItemId = inventoryItem.ItemId,
                                    ItemName = inventoryItem.ItemName,
                                    QuantityAvailable = inventoryItem.QuantityAvailable,
                                    ReorderLevel = inventoryItem.ReorderLevel,
                                    EventType = "LowStockError",
                                    TargetUsersIds = nullableUserIds,
                                    TargetEmailAddresses = userEmails!
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
                               .Where(x => x.RoleName == "Store Keeper" && !x.IsDeleted)
                               .Select(x => x.RoleId)
                               .FirstOrDefaultAsync();

                            var StoreKeeperUserId = await tenantDb.FactoryUserRoles
                                .Where(x => x.TenantId == dto.TenantId && x.RoleId == StoreKeeperRoleId && !x.IsDeleted)
                                .Select(x => x.UserId)
                                .ToListAsync();
                            var userIds = StoreKeeperUserId.ToList();

                            var nullableUserIds = userIds.Select(id => (int?)id).ToList();

                            var userEmails = (await tenantDb.FactoryUsers
                                            .Where(u => userIds.Contains(u.UserId) && !u.IsDeleted)
                                            .Select(u => u.Email)
                                            .ToListAsync())
                                        .Append(TenantEmail)
                                        .Where(e => !string.IsNullOrWhiteSpace(e))
                                        .Distinct()
                                        .ToList();

                            var lowStock = new LowStockEventDto
                            {
                                TenantId = dto.TenantId,
                                ItemId = inventoryItem.ItemId,
                                ItemName = inventoryItem.ItemName,
                                QuantityAvailable = inventoryItem.QuantityAvailable,
                                ReorderLevel = inventoryItem.ReorderLevel,
                                EventType = "LowStockError",
                                TargetUsersIds = nullableUserIds,
                                TargetEmailAddresses = userEmails!
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

          

            if (dto.UpdatedBy.HasValue)

            {

                var assignedToUserIdAsset = await tenantDb.AssetRegistry
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

                //notification builder helps to get all the required data for notifications in one call, instead of multiple calls to database in notification service
                var notificationData = await _notificationBuilderService.BuildAsync(
                                        _tenantDbContext,
                                        entity.TenantId,
                                        dto.UpdatedBy,
                                        entity.AssignedToUserId,
                                        entity.CreatedBy,
                                        assignedToUserIdAsset
                                    );

                var targetEmails = notificationData.Emails?
                                    .Where(x => !string.IsNullOrWhiteSpace(x))
                                    .Append(TenantEmail ?? string.Empty)
                                    .Where(x => !string.IsNullOrWhiteSpace(x))
                                    .Distinct()
                                    .ToList();

                var eventDto = new WorkOrderEventDto
                {
                    WorkOrderId = entity.WorkOrderId,
                    TenantId = entity.TenantId,
                    TenantEmail = TenantEmail,
                    WorkOrderType = entity.WorkOrderType,
                    WorkOrderTypeName = entity.WorkOrderType?.ToString(),
                    WorkOrderNumber = entity.WorkOrderNumber,
                    Title = entity.Title,
                    EventType = oldAssignedUserId != entity.AssignedToUserId
                                ? "Assigned"
                                : "Updated",
                    EventTime = DateTime.UtcNow,
                    Priority = entity.Priority.ToString(),
                    Status = entity.Status.ToString(),

                    outgoingNotifications = notificationData.Outgoing,
                    incomingNotifications = notificationData.Incoming,
                    TargetUsersIds = notificationData.AllUsers,
                    TargetEmailAddresses = targetEmails!,

                    TargetUserId = entity.AssignedToUserId,
                    AssignedToUserId = entity.AssignedToUserId,
                    AssignedToTeamId = entity.AssignedToTeamId,

                    UpdatedBy = entity.UpdatedBy,
                    CreatedBy = entity.CreatedBy

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
       
            await transaction.CommitAsync();

            response.StatusCode = StatusCode.Success;
            response.StatusMessage = WorkOrderStatusMessage.WorkOrderUpdated;

            return response;
        }
        public async Task<CommonResponseModel> UpdateWorkOrderCalendarAsync(WorkOrderCalendarUpdateDto dto)
        {
            var response = new CommonResponseModel();

            using var tenantDb = _tenantDbContext.GetTenantDbContext(dto.TenantId);

            var entity = await tenantDb.WorkOrders
                .FirstOrDefaultAsync(x => x.WorkOrderId == dto.WorkOrderId && !x.IsDeleted);

            if (entity == null)
            {
                response.StatusCode = StatusCode.NotFound;
                response.StatusMessage = "Work Order not found";
                return response;
            }

           
            entity.ScheduleDate = dto.ScheduleDate.HasValue
                ? dto.ScheduleDate.Value.ToUniversalTime()
                : null;

            entity.AssignedToUserId = dto.AssignedToUserId;
            entity.AssignedToTeamId = dto.AssignedToTeamId;

            entity.UpdatedBy = dto.UpdatedBy;
            entity.UpdatedAt = DateTime.UtcNow;

            await tenantDb.SaveChangesAsync();

            response.StatusCode = StatusCode.Success;
            response.StatusMessage = "Work Order Calendar Updated Successfully";

            return response;
        }
        public async Task<CommonResponseModel> DeleteWorkOrderAsync(WorkOrderDeleteDto dto)
        {
            var response = new CommonResponseModel();

            using var tenantDb = _tenantDbContext.GetTenantDbContext(dto.TenantId);
            using var transaction = await tenantDb.Database.BeginTransactionAsync();

            var TenantEmail = await _masterDbcontext.FactoryTenants
                                .Where(x => x.TenantId == dto.TenantId)
                                .Select(x => x.AdminEmail)
                                .FirstOrDefaultAsync();

            var entity = await tenantDb.WorkOrders
                .FirstOrDefaultAsync(x => x.WorkOrderId == dto.WorkOrderId && !x.IsDeleted);

            if (entity == null)
            {
                response.StatusCode = StatusCode.NotFound;
                response.StatusMessage = WorkOrderStatusMessage.WorkOrderNotFound;
                return response;
            }

            if (!dto.DeletedBy.HasValue)
                return new CommonResponseModel
                {
                    StatusCode = StatusCode.BadRequest,
                    StatusMessage = "DeletedBy is required"
                };

            entity.IsDeleted = true;
            entity.IsActive = false;
            entity.DeletedBy = dto.DeletedBy;
            entity.DeletedAt = DateTime.UtcNow;

            var relatedTools = await tenantDb.WorkOrderRequiredTools
                .Where(rt => rt.WorkOrderId == dto.WorkOrderId && !rt.IsDeleted)
                .ToListAsync();

            foreach (var tool in relatedTools)
            {
                var inventoryItem = await tenantDb.Inventory
                    .FirstOrDefaultAsync(i => i.ItemId == tool.ToolId && !i.IsDeleted);

                if (inventoryItem != null)
                {
                    var restoreQty = tool.QuantityRequired ?? 1;

                    inventoryItem.QuantityAvailable += restoreQty;
                    inventoryItem.UpdatedAt = DateTime.UtcNow;
                    inventoryItem.UpdatedBy = entity.DeletedBy ?? entity.UpdatedBy ?? 0;
                }

                tool.IsDeleted = true;
                tool.IsActive = false;
                tool.DeletedBy = entity.DeletedBy;
                tool.DeletedAt = DateTime.UtcNow;
            }

            await tenantDb.SaveChangesAsync();

            var notificationData = await _notificationBuilderService.BuildAsync(
                                    _tenantDbContext,
                                    entity.TenantId,
                                    null,
                                    dto.DeletedBy,
                                    entity.AssignedToUserId,
                                    entity.CreatedBy
                                );

            var targetEmails = notificationData.Emails?
                                    .Where(x => !string.IsNullOrWhiteSpace(x))
                                    .Append(TenantEmail ?? string.Empty)
                                    .Where(x => !string.IsNullOrWhiteSpace(x))
                                    .Distinct()
                                    .ToList();

            // Kafka Event DTO
            var eventDto = new WorkOrderEventDto
            {
                WorkOrderId = entity.WorkOrderId,
                TenantId = entity.TenantId,
                TenantEmail = TenantEmail,

                WorkOrderType = entity.WorkOrderType,
                WorkOrderTypeName = entity.WorkOrderType?.ToString(),
                WorkOrderNumber = entity.WorkOrderNumber,
                Title = entity.Title,

                EventType = "Deleted",
                EventTime = DateTime.UtcNow,
                Priority = entity.Priority.ToString(),
                Status = entity.Status.ToString(),

                AssignedToUserId = entity.AssignedToUserId,
                AssignedToTeamId = entity.AssignedToTeamId,

                outgoingNotifications = notificationData.Outgoing,
                incomingNotifications = notificationData.Incoming,
                TargetUsersIds = notificationData.AllUsers,
                TargetEmailAddresses = targetEmails!,

                DeletedBy = dto.DeletedBy,
                CreatedBy = entity.CreatedBy
            };

            
            var correlationId = Guid.NewGuid().ToString();
            var topicName = KafkaCommonTopics.BuildTopicName(entity.TenantId, eventDto.EventType);

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

            using (var httpClient = new HttpClient())
            {
                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(kafkaRequest),
                    Encoding.UTF8,
                    "application/json");

                var kafkaResponse = await httpClient.PostAsync(ConstantUrls.kafkaPublish, jsonContent);
                kafkaResponse.EnsureSuccessStatusCode();
            }

            // Audit Log (Optional but Recommended)
            // await _auditLogger.LogAuditAsync(
            //     "WorkOrder",
            //     "Delete",
            //     entity.DeletedBy,
            //     entity.TenantId.ToString(),
            //     entity.WorkOrderId.ToString());

            await transaction.CommitAsync();

            response.StatusCode = StatusCode.Success;
            response.StatusMessage = WorkOrderStatusMessage.WorkOrderDeletion;

            return response;
        }
 
        public async Task<CommonResponseModel> BulkWorkOrderDelete(WorkOrderBulkDeleteDto dto)
        {
            var response = new CommonResponseModel();
            using var tenantDb = _tenantDbContext.GetTenantDbContext(dto.TenantId);

            var workOrders = await tenantDb.WorkOrders
                .Where(x => dto.WorkOrderIds.Contains(x.WorkOrderId) && !x.IsDeleted)
                .ToListAsync();

            foreach (var entity in workOrders)
            {
                entity.IsDeleted = true;
                entity.IsActive = false;
                entity.DeletedBy = entity.UpdatedBy;
                entity.DeletedAt = DateTime.UtcNow;

                var relatedTools = await tenantDb.WorkOrderRequiredTools
                    .Where(rt => rt.WorkOrderId == entity.WorkOrderId && !rt.IsDeleted)
                    .ToListAsync();

                foreach (var tool in relatedTools)
                {
                    tool.IsDeleted = true;
                    tool.IsActive = false;
                    tool.DeletedBy = entity.UpdatedBy;
                    tool.DeletedAt = DateTime.UtcNow;
                }

                await _auditLogger.LogAuditAsync("WorkOrder", "Delete", entity.UpdatedBy, entity.TenantId.ToString(), entity.WorkOrderId.ToString());
            }

            await tenantDb.SaveChangesAsync();

            response.StatusCode = StatusCode.Success;
            response.StatusMessage = WorkOrderStatusMessage.BulkWorkOrderDeletion;
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
                .Where(wo => technicianIds.Contains(wo.AssignedToUserId!.Value))
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
                .Where(wo => technicianIds.Contains(wo.AssignedToUserId!.Value))
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
                    .Average(wo => (wo.ScheduleDate!.Value - wo.CreatedAt).TotalHours);
            }

            var teamEfficiency = workOrders.Any()
                ? (int)((double)workOrders.Count(wo => wo.Status == WorkOrderStatus.Completed) / workOrders.Count * 100)
                : 0;

            var performanceMetrics = new PerformanceMetricsDto
            {
                AvgResponseTime = Math.Round(avgResponseTime, 2),
                FirstTimeFixRate = 0,
                TeamEfficiency = teamEfficiency
            };


            decimal regularHours = workOrders.Sum(wo => (decimal)(wo.EstimatedDurationMinutes ?? 0));
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
        //public async Task<CommonResponseModel> UpdateWorkOrderProgressAsync(WorkOrderProgresssUpdateDto dto)
        //{
        //    var response = new CommonResponseModel();

        //    using var tenantDb = _tenantDbContext.GetTenantDbContext(dto.TenantId);
        //    await using var masterTransaction = await _masterDbcontext.Database.BeginTransactionAsync();
        //    await using var tenantTransaction = await tenantDb.Database.BeginTransactionAsync();

        //    var TenantEmail = await _masterDbcontext.FactoryTenants
        //                   .Where(x => x.TenantId == dto.TenantId)
        //                   .Select(x => x.AdminEmail)
        //                   .FirstOrDefaultAsync();

        //    try
        //    {
        //        var workOrder = await tenantDb.WorkOrders
        //            .FirstOrDefaultAsync(w => w.WorkOrderId == dto.WorkOrderId && !w.IsDeleted);

        //        if (workOrder == null)
        //        {
        //            return new CommonResponseModel
        //            {
        //                StatusCode = "404",
        //                StatusMessage = "Work order not found."
        //            };
        //        }

        //        if (!dto.UpdatedBy.HasValue)
        //            return new CommonResponseModel
        //            {
        //                StatusCode = StatusCode.BadRequest,
        //                StatusMessage = "UpdatedBy is required"
        //            };

        //        string? fileName = null;
        //        string? filePath = null;
        //        string? fileType = null;

        //        var file = dto.WorkOrderProgressMedia;

        //        if (file != null && file.Length > 0)
        //        {
        //            if (file.Length > 100 * 1024 * 1024)
        //            {
        //                return new CommonResponseModel
        //                {
        //                    StatusCode = StatusCode.BadRequest,
        //                    StatusMessage = "File size exceeds 100 MB limit"
        //                };
        //            }

        //            var allowedExtensions = new[]
        //            {
        //                ".jpg", ".jpeg", ".png", ".gif", ".webp",
        //                ".mp4", ".mov", ".avi", ".mkv", ".webm",
        //                ".pdf", ".doc", ".docx", ".xls", ".xlsx"
        //            };

        //            var extension = Path.GetExtension(file.FileName).ToLower();

        //            if (!allowedExtensions.Contains(extension))
        //            {
        //                return new CommonResponseModel
        //                {
        //                    StatusCode = StatusCode.BadRequest,
        //                    StatusMessage = "Unsupported file format"
        //                };
        //            }

        //            fileType = GetFileType(file);

        //            string folder = fileType switch
        //            {
        //                "image" => "WorkOrderImages",
        //                "video" => "WorkOrderVideos",
        //                "document" => "WorkOrderDocuments",
        //                _ => "Others"
        //            };

        //            filePath = await _fileStorageService.SaveFileAsync(file, folder);
        //            fileName = file.FileName;

        //            workOrder.WorkOrderProgressMedia = fileName;
        //            workOrder.WorkOrderProgressMediaPath = filePath;
        //            workOrder.FileType = fileType;
        //        }

        //        if (dto.UpdateType != null)
        //            workOrder.UpdateType = (UpdateTypeEnum)dto.UpdateType.Value;

        //        if (!string.IsNullOrWhiteSpace(dto.LastUpdateMessage))
        //            workOrder.LastUpdateMessage = dto.LastUpdateMessage;

        //        if (!string.IsNullOrWhiteSpace(dto.Comments))
        //            workOrder.Comments = dto.Comments;

        //        if (dto.UpdatedBy != null)
        //            workOrder.UpdatedBy = dto.UpdatedBy;

        //        workOrder.UpdatedAt = DateTime.UtcNow;

        //        if (dto.ProgressPercentage != null)
        //            workOrder.ProgressPercentage = dto.ProgressPercentage.Value;

        //        if (dto.Status != null)
        //            workOrder.Status = dto.Status;

        //        decimal GetExistingTotalTime()
        //        {
        //            if (string.IsNullOrWhiteSpace(workOrder.TotalTime))
        //                return 0;

        //            return decimal.TryParse(workOrder.TotalTime, out var val) ? val : 0;
        //        }

        //        if (dto.Action == "Start")
        //        {
        //            workOrder.IsStarted = true;
        //            workOrder.IsPaused = false;
        //            workOrder.StartTime = DateTime.UtcNow;
        //            workOrder.Status = WorkOrderStatus.InProgress;
        //            workOrder.UpdatedBy = dto.UpdatedBy;
        //        }
        //        else if (dto.Action == "Pause")
        //        {
        //            workOrder.IsPaused = true;
        //            workOrder.IsStarted = false;
        //            workOrder.Status = WorkOrderStatus.OnHold;
        //            workOrder.PauseTime = DateTime.UtcNow;

        //            if (workOrder.StartTime.HasValue)
        //            {
        //                decimal previous = GetExistingTotalTime();
        //                var timeWorked = (decimal)(workOrder.PauseTime.Value - workOrder.StartTime.Value).TotalMinutes;
        //                workOrder.TotalTime = (previous + timeWorked).ToString();
        //            }
        //        }
        //        else if (dto.Action == "Resume")
        //        {
        //            workOrder.IsPaused = false;
        //            workOrder.IsStarted = true;
        //            workOrder.Status = WorkOrderStatus.InProgress;
        //            workOrder.StartTime = DateTime.UtcNow;
        //            workOrder.UpdatedBy = dto.UpdatedBy;
        //        }
        //        else if (dto.Action == "Complete")
        //        {
        //            if (workOrder.StartTime.HasValue)
        //            {
        //                decimal previous = GetExistingTotalTime();
        //                var timeWorked = (decimal)(DateTime.UtcNow - workOrder.StartTime.Value).TotalMinutes;
        //                workOrder.TotalTime = (previous + timeWorked).ToString();
        //            }

        //            workOrder.ProgressPercentage = 100;
        //            workOrder.Status = WorkOrderStatus.Completed;
        //            workOrder.IsStarted = false;
        //            workOrder.IsPaused = false;
        //            workOrder.UpdatedBy = dto.UpdatedBy;
        //        }

        //        var progressUpdate = new WorkOrderProgressUpdates
        //        {
        //            TenantId = dto.TenantId,
        //            WorkOrderId = workOrder.WorkOrderId,
        //            UpdateType = workOrder.UpdateType ?? UpdateTypeEnum.ProgressUpdate,
        //            Status = workOrder.Status,
        //            FileType = fileType,
        //            ProgressPercentage = workOrder.ProgressPercentage,
        //            AssignedToUserId = workOrder.AssignedToUserId,
        //            Message = dto.LastUpdateMessage ?? dto.Comments,
        //            Action = dto.Action,
        //            UpdatedBy = dto.UpdatedBy,
        //            CreatedAt = DateTime.UtcNow,
        //            AttachmentName = fileName,
        //            AttachmentPath = filePath
        //        };

        //        await tenantDb.WorkOrderProgressUpdates.AddAsync(progressUpdate);

        //        await tenantDb.SaveChangesAsync();
        //        await tenantTransaction.CommitAsync();
        //        await masterTransaction.CommitAsync();

        //        if (workOrder.Status == WorkOrderStatus.Completed ||
        //            workOrder.Status == WorkOrderStatus.InProgress ||
        //            workOrder.Status == WorkOrderStatus.OnHold)
        //        {

        //            var assignedToUserIdAsset = await tenantDb.AssetRegistry
        //           .Where(x => x.AssetId == workOrder.AssetId && x.IsActive && !x.IsDeleted)
        //           .Join(
        //               tenantDb.AssetTracking,
        //               assetRegistry => assetRegistry.AssetId,
        //               assetTracking => assetTracking.AssetId,
        //               (assetRegistry, assetTracking) => assetTracking
        //           )
        //           .Where(x => !x.IsDeleted)
        //           .Select(x => x.AssignedTo)
        //           .FirstOrDefaultAsync();


        //            var notificationData = await _notificationBuilderService.BuildAsync(
        //                                    _tenantDbContext,
        //                                    workOrder.TenantId,
        //                                    dto.UpdatedBy,               
        //                                    workOrder.AssignedToUserId,  
        //                                    workOrder.CreatedBy,
        //                                    assignedToUserIdAsset
        //                                );
        //            var technicianName = await GetUserNameAsync(workOrder.TenantId, dto.UpdatedBy);

        //            var progressEvent = new WorkOrderProgressUpdatedEventDto
        //            {
        //                TenantId = workOrder.TenantId,
        //                TenantEmail = TenantEmail,  
        //                Title = workOrder.Title,
        //                WorkOrderId = workOrder.WorkOrderId,
        //                WorkOrderNumber = workOrder.WorkOrderNumber,
        //                WorkOrderTypeName = workOrder.WorkOrderType?.ToString(),

        //                NewStatus = workOrder.Status.ToString()!,
        //                EventType = "WorkOrderProgressUpdated",

        //                ProgressPercentage = workOrder.ProgressPercentage,
        //                Action = dto.Action,
        //                Status = dto.Status,

        //                outgoingNotifications = notificationData.Outgoing,
        //                incomingNotifications = notificationData.Incoming,
        //                TargetUsersIds = notificationData.AllUsers,
        //                TargetEmailAddresses = notificationData.Emails,

        //                TechnicianName = technicianName,
        //                UpdatedAt = DateTime.UtcNow,
        //                UpdatedBy = dto.UpdatedBy
        //            };

        //            var correlationId = Guid.NewGuid().ToString();
        //            string topic = KafkaCommonTopics.BuildTopicName(workOrder.TenantId, progressEvent.EventType);

        //            var kafkaRequest = new
        //            {
        //                Topic = topic,
        //                Key = $"workorder-progress-{workOrder.WorkOrderId}",
        //                Payload = progressEvent,
        //                Source = "WorkOrderService",
        //                Headers = new Dictionary<string, string>
        //                {
        //                    ["tenant-id"] = workOrder.TenantId.ToString(),
        //                    ["correlation-id"] = correlationId
        //                }
        //            };

        //            using var httpClient = new HttpClient();
        //            var jsonBody = new StringContent(JsonSerializer.Serialize(kafkaRequest), Encoding.UTF8, "application/json");
        //            var kafkaResponse = await httpClient.PostAsync(ConstantUrls.kafkaPublish, jsonBody);
        //            kafkaResponse.EnsureSuccessStatusCode();
        //        }

        //        return new CommonResponseModel
        //        {
        //            StatusCode = "200",
        //            StatusMessage = "Work order progress updated successfully."
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        await tenantTransaction.RollbackAsync();
        //        await masterTransaction.RollbackAsync();

        //        return new CommonResponseModel
        //        {
        //            StatusCode = StatusCode.Error,
        //            StatusMessage = ex.Message
        //        };
        //    }
        //}

        public async Task<CommonResponseModel> UpdateWorkOrderProgressAsync(WorkOrderProgresssUpdateDto dto)
        {
            var response = new CommonResponseModel();

            using var tenantDb = _tenantDbContext.GetTenantDbContext(dto.TenantId);
            await using var masterTransaction = await _masterDbcontext.Database.BeginTransactionAsync();
            await using var tenantTransaction = await tenantDb.Database.BeginTransactionAsync();

            var TenantEmail = await _masterDbcontext.FactoryTenants
                .Where(x => x.TenantId == dto.TenantId)
                .Select(x => x.AdminEmail)
                .FirstOrDefaultAsync();

            try
            {
                var workOrder = await tenantDb.WorkOrders
                    .FirstOrDefaultAsync(w => w.WorkOrderId == dto.WorkOrderId && !w.IsDeleted);

                if (workOrder == null)
                {
                    return new CommonResponseModel
                    {
                        StatusCode = "404",
                        StatusMessage = "Work order not found."
                    };
                }

                var previousStatus = workOrder.Status;

                if (!dto.UpdatedBy.HasValue)
                    return new CommonResponseModel
                    {
                        StatusCode = StatusCode.BadRequest,
                        StatusMessage = "UpdatedBy is required"
                    };

                string? fileName = null;
                string? filePath = null;
                string? fileType = null;

                var file = dto.WorkOrderProgressMedia;

                if (file != null && file.Length > 0)
                {
                    if (file.Length > 100 * 1024 * 1024)
                    {
                        return new CommonResponseModel
                        {
                            StatusCode = StatusCode.BadRequest,
                            StatusMessage = "File size exceeds 100 MB limit"
                        };
                    }

                    var allowedExtensions = new[]
                    {
                        ".jpg", ".jpeg", ".png", ".gif", ".webp",
                        ".mp4", ".mov", ".avi", ".mkv", ".webm",
                        ".pdf", ".doc", ".docx", ".xls", ".xlsx"
                    };

                    var extension = Path.GetExtension(file.FileName).ToLower();

                    if (!allowedExtensions.Contains(extension))
                    {
                        return new CommonResponseModel
                        {
                            StatusCode = StatusCode.BadRequest,
                            StatusMessage = "Unsupported file format"
                        };
                    }

                    fileType = GetFileType(file);

                    string folder = fileType switch
                    {
                        "image" => "WorkOrderImages",
                        "video" => "WorkOrderVideos",
                        "document" => "WorkOrderDocuments",
                        _ => "Others"
                    };

                    filePath = await _fileStorageService.SaveFileAsync(file, folder);
                    fileName = file.FileName;

                    workOrder.WorkOrderProgressMedia = fileName;
                    workOrder.WorkOrderProgressMediaPath = filePath;
                    workOrder.FileType = fileType;
                }

                if (dto.UpdateType != null)
                    workOrder.UpdateType = (UpdateTypeEnum)dto.UpdateType.Value;

                if (!string.IsNullOrWhiteSpace(dto.LastUpdateMessage))
                    workOrder.LastUpdateMessage = dto.LastUpdateMessage;

                if (!string.IsNullOrWhiteSpace(dto.Comments))
                    workOrder.Comments = dto.Comments;

                workOrder.UpdatedBy = dto.UpdatedBy;
                workOrder.UpdatedAt = DateTime.UtcNow;

                if (dto.ProgressPercentage != null)
                    workOrder.ProgressPercentage = dto.ProgressPercentage.Value;

                decimal GetExistingTotalTime()
                {
                    if (string.IsNullOrWhiteSpace(workOrder.TotalTime))
                        return 0;

                    return decimal.TryParse(workOrder.TotalTime, out var val) ? val : 0;
                }

                // =========================
                // ACTION BASED STATUS
                // =========================
                if (dto.Action?.Equals("Start", StringComparison.OrdinalIgnoreCase) == true)
                {
                    workOrder.IsStarted = true;
                    workOrder.IsPaused = false;
                    workOrder.StartTime = DateTime.UtcNow;
                    workOrder.Status = WorkOrderStatus.InProgress;
                }
                else if (dto.Action?.Equals("Pause", StringComparison.OrdinalIgnoreCase) == true)
                {
                    workOrder.IsPaused = true;
                    workOrder.IsStarted = false;
                    workOrder.Status = WorkOrderStatus.OnHold;
                    workOrder.PauseTime = DateTime.UtcNow;

                    if (workOrder.StartTime.HasValue)
                    {
                        decimal previous = GetExistingTotalTime();
                        var timeWorked = (decimal)(workOrder.PauseTime.Value - workOrder.StartTime.Value).TotalMinutes;
                        workOrder.TotalTime = (previous + timeWorked).ToString();
                    }
                }
                else if (dto.Action?.Equals("Resume", StringComparison.OrdinalIgnoreCase) == true)
                {
                    workOrder.IsPaused = false;
                    workOrder.IsStarted = true;
                    workOrder.Status = WorkOrderStatus.InProgress;
                    workOrder.StartTime = DateTime.UtcNow;
                }
                else if (dto.Action?.Equals("Complete", StringComparison.OrdinalIgnoreCase) == true)
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

                    workOrder.CompletedAt = DateTime.UtcNow;
                    workOrder.IsApprovalReminderSent = false;
                }

                bool isStatusChanged = previousStatus != workOrder.Status;

                // =========================
                // SAVE PROGRESS
                // =========================

                var progressUpdate = new WorkOrderProgressUpdates
                {
                    TenantId = dto.TenantId,
                    WorkOrderId = workOrder.WorkOrderId,
                    UpdateType = workOrder.UpdateType ?? UpdateTypeEnum.ProgressUpdate,
                    Status = workOrder.Status,
                    FileType = fileType,
                    ProgressPercentage = workOrder.ProgressPercentage,
                    AssignedToUserId = workOrder.AssignedToUserId,
                    Message = dto.LastUpdateMessage ?? dto.Comments,
                    Action = dto.Action,
                    UpdatedBy = dto.UpdatedBy,
                    CreatedAt = DateTime.UtcNow,
                    AttachmentName = fileName,
                    AttachmentPath = filePath
                };

                await tenantDb.WorkOrderProgressUpdates.AddAsync(progressUpdate);

                await tenantDb.SaveChangesAsync();
                await tenantTransaction.CommitAsync();
                await masterTransaction.CommitAsync();

                // ONLY showing updated part (rest stays same)

                // =========================
                // ALWAYS PUBLISH EVENT
                // =========================

                if (isStatusChanged)

                { 
                    var assignedToUserIdAsset = await tenantDb.AssetRegistry
                    .Where(x => x.AssetId == workOrder.AssetId && x.IsActive && !x.IsDeleted)
                    .Join(tenantDb.AssetTracking,
                        a => a.AssetId,
                        t => t.AssetId,
                        (a, t) => t)
                    .Where(x => !x.IsDeleted)
                    .Select(x => x.AssignedTo)
                    .FirstOrDefaultAsync();

                var notificationData = await _notificationBuilderService.BuildAsync(
                    _tenantDbContext,
                    workOrder.TenantId,
                   // workOrder.AssignedToTeamId,
                    dto.UpdatedBy,
                    workOrder.AssignedToUserId,
                    workOrder.CreatedBy,
                    assignedToUserIdAsset
                );


                var technicianName = await GetUserNameAsync(workOrder.TenantId, dto.UpdatedBy);

                    var targetEmails = notificationData.Emails?
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Append(TenantEmail ?? string.Empty)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct()
                    .ToList();

                    var progressEvent = new WorkOrderProgressUpdatedEventDto
                {
                    TenantId = workOrder.TenantId,
                    TenantEmail = TenantEmail,
                    Title = workOrder.Title,
                    WorkOrderId = workOrder.WorkOrderId,
                    WorkOrderNumber = workOrder.WorkOrderNumber,
                    WorkOrderTypeName = workOrder.WorkOrderType?.ToString(),
                    Status = workOrder.Status.ToString(),
                    EventType = "WorkOrderProgressUpdated",
                    ProgressPercentage = workOrder.ProgressPercentage,
                    Action = dto.Action,

                    outgoingNotifications = notificationData.Outgoing,
                    incomingNotifications = notificationData.Incoming,
                    TargetUsersIds = notificationData.AllUsers,
                    TargetEmailAddresses = targetEmails!,

                    TechnicianName = technicianName,
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedBy = dto.UpdatedBy
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
            catch (Exception ex)
            {
                await tenantTransaction.RollbackAsync();
                await masterTransaction.RollbackAsync();

                return new CommonResponseModel
                {
                    StatusCode = StatusCode.Error,
                    StatusMessage = ex.Message
                };
            }
        }

        public async Task<CommonResponseModel> ApproveRejectWorkOrderAsync(WorkOrderApprovalDto dto)
        {
            var response = new CommonResponseModel();

            using var tenantDb = _tenantDbContext.GetTenantDbContext(dto.TenantId);
            using var transaction = await tenantDb.Database.BeginTransactionAsync();

            var TenantEmail = await _masterDbcontext.FactoryTenants
                    .Where(x => x.TenantId == dto.TenantId)
                    .Select(x => x.AdminEmail)
                    .FirstOrDefaultAsync();
            try
            {
                var workOrder = await tenantDb.WorkOrders
                    .FirstOrDefaultAsync(x => x.WorkOrderId == dto.WorkOrderId && !x.IsDeleted);

                if (workOrder == null)
                {
                    return new CommonResponseModel
                    {
                        StatusCode = StatusCode.NotFound,
                        StatusMessage = "Work order not found"
                    };
                }

                if (workOrder.Status != WorkOrderStatus.Completed)
                {
                    return new CommonResponseModel
                    {
                        StatusCode = StatusCode.BadRequest,
                        StatusMessage = "Only completed work orders can be approved/rejected"
                    };
                }

                var assignedToUserIdAsset = await tenantDb.AssetRegistry
                    .Where(x => x.AssetId == workOrder.AssetId && x.IsActive && !x.IsDeleted)
                    .Join(tenantDb.AssetTracking,
                        a => a.AssetId,
                        t => t.AssetId,
                        (a, t) => t)
                    .Where(x => !x.IsDeleted)
                    .Select(x => x.AssignedTo)
                    .FirstOrDefaultAsync();

                var notificationData = await _notificationBuilderService.BuildAsync(
                                        _tenantDbContext,
                                        workOrder.TenantId,
                                        dto.UpdatedBy,                
                                        workOrder.AssignedToUserId, 
                                        workOrder.CreatedBy,
                                        assignedToUserIdAsset
                                    );

                var targetEmails = notificationData.Emails?
                                    .Where(x => !string.IsNullOrWhiteSpace(x))
                                    .Append(TenantEmail ?? string.Empty)
                                    .Where(x => !string.IsNullOrWhiteSpace(x))
                                    .Distinct()
                                    .ToList();

                if (dto.Action == "Approve")
                {
                    workOrder.Status = WorkOrderStatus.Closed;
                    workOrder.UpdatedAt = DateTime.UtcNow;
                    workOrder.UpdatedBy = dto.UpdatedBy;
                    workOrder.IsApprovalReminderSent = true;

                    if (workOrder.ServiceRequestId.HasValue)
                    {
                        var sr = await tenantDb.ServiceRequests
                            .FirstOrDefaultAsync(x => x.ServiceRequestId == workOrder.ServiceRequestId && !x.IsDeleted);

                        if (sr != null)
                        {
                            sr.Status = ServiceRequestStatus.Closed;
                            sr.UpdatedAt = DateTime.UtcNow;
                            sr.UpdatedBy = dto.UpdatedBy;
                        }
                    }
                    

                    if (workOrder.AssignedToUserId.HasValue)

                    {
                        var eventDto = new WorkOrderEventDto
                        {
                            WorkOrderId = workOrder.WorkOrderId,
                            TenantId = workOrder.TenantId,
                            TenantEmail = TenantEmail,
                            WorkOrderNumber = workOrder.WorkOrderNumber,
                            Title = workOrder.Title,

                            EventType = "WorkOrderApproved",
                            EventTime = DateTime.UtcNow,

                            Status = workOrder.Status.ToString(),

                            AssignedToUserId = workOrder.AssignedToUserId,
                            AssignedToTeamId = workOrder.AssignedToTeamId,

                            WorkOrderType = workOrder.WorkOrderType,
                            WorkOrderTypeName = workOrder.WorkOrderType?.ToString(),
                            Priority = workOrder.Priority.ToString(),

                            outgoingNotifications = notificationData.Outgoing,
                            incomingNotifications = notificationData.Incoming,
                            TargetUsersIds = notificationData.AllUsers,
                            TargetEmailAddresses = targetEmails!,

                            CreatedBy = workOrder.CreatedBy,
                            UpdatedBy = dto.UpdatedBy
                        };

                        var kafkaRequest = new
                        {
                            Topic = KafkaCommonTopics.BuildTopicName(workOrder.TenantId, eventDto.EventType),
                            Key = $"workorder-{workOrder.WorkOrderId}",
                            Payload = eventDto,
                            Source = "WorkOrderService",
                            Headers = new Dictionary<string, string>
                            {
                                ["tenant-id"] = workOrder.TenantId.ToString(),
                                ["correlation-id"] = Guid.NewGuid().ToString()
                            }
                        };

                        using var httpClient = new HttpClient();
                        var jsonContent = new StringContent(JsonSerializer.Serialize(kafkaRequest),
                            Encoding.UTF8, "application/json");

                        var kafkaResponse = await httpClient.PostAsync(ConstantUrls.kafkaPublish, jsonContent);
                        kafkaResponse.EnsureSuccessStatusCode();
                    }
                }
                else if (dto.Action == "Reject")
                {
                    workOrder.Status = WorkOrderStatus.ReOpened;
                    workOrder.ProgressPercentage = 0;
                    workOrder.IsStarted = false;
                    workOrder.IsPaused = false;
                    workOrder.UpdatedAt = DateTime.UtcNow;
                    workOrder.UpdatedBy = dto.UpdatedBy;

                    workOrder.IsApprovalReminderSent = true;

                    if (workOrder.AssignedToUserId.HasValue)
                    {

                        var eventDto = new WorkOrderEventDto
                        {
                            WorkOrderId = workOrder.WorkOrderId,
                            TenantId = workOrder.TenantId,
                            TenantEmail = TenantEmail,

                            WorkOrderNumber = workOrder.WorkOrderNumber,
                            Title = workOrder.Title,

                            EventType = "WorkOrderRejected",
                            EventTime = DateTime.UtcNow,

                            Status = workOrder.Status.ToString(),
                            AssignedToUserId = workOrder.AssignedToUserId,

                            WorkOrderType = workOrder.WorkOrderType,
                            WorkOrderTypeName = workOrder.WorkOrderType?.ToString(),
                            Priority = workOrder.Priority.ToString(),

                            outgoingNotifications = notificationData.Outgoing,
                            incomingNotifications = notificationData.Incoming,
                            TargetUsersIds = notificationData.AllUsers,
                            TargetEmailAddresses = targetEmails!,

                            CreatedBy = workOrder.CreatedBy,
                            UpdatedBy = dto.UpdatedBy
                        };


                        var kafkaRequest = new
                        {
                            Topic = KafkaCommonTopics.BuildTopicName(workOrder.TenantId, eventDto.EventType),
                            Key = $"workorder-{workOrder.WorkOrderId}",
                            Payload = eventDto,
                            Source = "WorkOrderService",
                            Headers = new Dictionary<string, string>
                            {
                                ["tenant-id"] = workOrder.TenantId.ToString(),
                                ["correlation-id"] = Guid.NewGuid().ToString()
                            }
                        };

                        using var httpClient = new HttpClient();
                        var jsonContent = new StringContent(JsonSerializer.Serialize(kafkaRequest),
                            Encoding.UTF8, "application/json");

                        var kafkaResponse = await httpClient.PostAsync(ConstantUrls.kafkaPublish, jsonContent);
                        kafkaResponse.EnsureSuccessStatusCode();
                    }
                }
                else
                {
                    return new CommonResponseModel
                    {
                        StatusCode = StatusCode.BadRequest,
                        StatusMessage = "Invalid action. Use Approve or Reject"
                    };
                }

                await tenantDb.SaveChangesAsync();
                await transaction.CommitAsync();

                return new CommonResponseModel
                {
                    StatusCode = StatusCode.Success,
                    StatusMessage = $"Work order {dto.Action}d successfully"
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                return new CommonResponseModel
                {
                    StatusCode = StatusCode.Error,
                    StatusMessage = ex.Message
                };
            }
        }
        public async Task<GetAllRecord<WorkOrderTimelineDto>> GetWorkOrderTimelineAsync(int tenantId, int? userId = null)
        {
            string baseUrl = _configuration["BaseUrl:Staging"] ?? "https://ms.stagingsdei.com:8125";
            var response = new GetAllRecord<WorkOrderTimelineDto>();

            using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

            var query = tenantDb.WorkOrderProgressUpdates
               
                .Where(x => x.TenantId == tenantId && !x.IsDeleted);

            /* if (userId.HasValue)
             {
                 query = query.Where(x => x.AssignedToUserId == userId);
             }*/
            var supervisorRoleId = await tenantDb.FactoryRoles
             .Where(x => x.RoleName == "Maintenance Supervisor" && !x.IsDeleted)
             .Select(x => x.RoleId)
             .FirstOrDefaultAsync();
                    var supervisorUserIds = await tenantDb.FactoryUserRoles
                        .Where(x => x.TenantId == tenantId && x.RoleId == supervisorRoleId && !x.IsDeleted)
                        .Select(x => x.UserId)
                        .ToListAsync();

            if (userId.HasValue)
            {
                bool isSupervisor = supervisorUserIds.Contains(userId.Value);

                if (!isSupervisor)
                {
                    query = query.Where(x => x.AssignedToUserId == userId.Value);
                }
            }

            var updates = await query
                .Join(
                    tenantDb.WorkOrders,
                    update => update.WorkOrderId,
                    wo => wo.WorkOrderId,
                    (update, wo) => new
                    {
                        update.WorkOrderId,
                        wo.WorkOrderNumber,
                        Update = new WorkOrderTimelineItemDto
                        {
                            UpdateId = update.WorkOrderProgressUpdateId,
                            UpdateType = update.UpdateType!.Value,
                            Status = update.Status,
                            ProgressPercentage = update.ProgressPercentage,
                            Message = update.Message,
                            AttachmentPath = !string.IsNullOrEmpty(update.AttachmentPath)
                        ? $"{baseUrl.TrimEnd('/')}/{update.AttachmentPath}"
:                        null,
                            FileType = update.FileType,
                            Action = update.Action,
                            AssignedToUserId = update.AssignedToUserId,
                            CreatedAt = update.CreatedAt
                        }
                    })
                .OrderByDescending(x => x.Update.CreatedAt)
                .ToListAsync();

            var grouped = updates
                .GroupBy(x => new { x.WorkOrderId, x.WorkOrderNumber })
                .Select(g => new WorkOrderTimelineDto
                {
                    WorkOrderId = g.Key.WorkOrderId,
                    WorkOrderNumber = g.Key.WorkOrderNumber,
                    Timeline = g.Select(x => x.Update)
                                .OrderByDescending(x => x.CreatedAt)
                                .ToList()
                })
                .ToList();

            response.StatusCode = StatusCode.Success;
            response.StatusMessage = "Timeline fetched successfully";
            response.GetAllData = grouped;

            return response;
        }
        public async Task<GetAllRecord<GetWorkOrderProgresssUpdateDto>> GetWorkOrderProgressAsync(int tenantId, int? userId = null)
        {
            var response = new GetAllRecord<GetWorkOrderProgresssUpdateDto>();

            using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);
            string baseUrl = _configuration["BaseUrl:Staging"] ?? "https://ms.stagingsdei.com:8125";
            var excludedStatuses = new WorkOrderStatus?[]
            {
            WorkOrderStatus.Cancelled,
           // WorkOrderStatus.Overdue,
            WorkOrderStatus.Inactive,
            };

            var query = tenantDb.WorkOrders
                .Where(x =>
                    x.TenantId == tenantId &&
                    !x.IsDeleted &&
                    x.IsActive &&
                    x.AssignedToUserId != null &&
                    !excludedStatuses.Contains(x.Status)
                );

            /* var supervisorRoleId = await tenantDb.FactoryRoles
              .Where(x => x.RoleName == "Maintenance Supervisor" && !x.IsDeleted)
              .Select(x => x.RoleId)
              .FirstOrDefaultAsync();

             var supervisorUserId = await tenantDb.FactoryUserRoles
                 .Where(x => x.TenantId == x.TenantId && x.RoleId == supervisorRoleId && !x.IsDeleted)
                 .Select(x => x.UserId)
                 .ToListAsync();

             if (userId.HasValue)
             {
                 query = query.Where(x => x.AssignedToUserId == userId.Value );
             }*/

 
            var supervisorRoleId = await tenantDb.FactoryRoles
                .Where(x => x.RoleName == "Maintenance Supervisor" && !x.IsDeleted)
                .Select(x => x.RoleId)
                .FirstOrDefaultAsync();
            var supervisorUserIds = await tenantDb.FactoryUserRoles
                .Where(x => x.TenantId == tenantId && x.RoleId == supervisorRoleId && !x.IsDeleted)
                .Select(x => x.UserId)
                .ToListAsync();

            if (userId.HasValue)
            {
                bool isSupervisor = supervisorUserIds.Contains(userId.Value);

                if (!isSupervisor)
                {
                    query = query.Where(x => x.AssignedToUserId == userId.Value);
                }
            }
            var data = await query
                .Select(x => new GetWorkOrderProgresssUpdateDto
                {
                    WorkOrderId = x.WorkOrderId,
                    WorkOrderNumber = x.WorkOrderNumber,
                    TenantId = x.TenantId,
                    Status = x.Status,
                    ProgressPercentage = x.ProgressPercentage,
                    WorkOrderProgressMedia = x.WorkOrderProgressMedia,
                    WorkOrderProgressMediaPath = !string.IsNullOrEmpty(x.WorkOrderProgressMediaPath)
                        ? $"{baseUrl.TrimEnd('/')}/{x.WorkOrderProgressMediaPath}"
                            : null,
                    StartTime = x.StartTime,
                    PauseTime = x.PauseTime,
                    IsStarted = x.IsStarted,
                    IsPaused = x.IsPaused,
                    TotalTime = x.TotalTime,
                    AssignedToUser = x.AssignedToUser != null
                        ? x.AssignedToUser.FirstName + " " + x.AssignedToUser.LastName
                        : null,
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
        public async Task<GetAllRecord<RecentWorkOrderUpdateDto>> GetRecentWorkOrderUpdatesAsync(
    int tenantId,
    int? userId,
    int? workorderId)
        {
            string baseUrl = _configuration["BaseUrl:Staging"] ?? "https://ms.stagingsdei.com:8125";

            var response = new GetAllRecord<RecentWorkOrderUpdateDto>();

            using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

            var baseQuery = tenantDb.WorkOrderProgressUpdates
                .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.IsActive);

            var supervisorRoleId = await tenantDb.FactoryRoles
                .Where(x => x.RoleName == "Maintenance Supervisor" && !x.IsDeleted)
                .Select(x => x.RoleId)
                .FirstOrDefaultAsync();

            var supervisorUserIds = await tenantDb.FactoryUserRoles
                .Where(x => x.TenantId == tenantId &&
                            x.RoleId == supervisorRoleId &&
                            !x.IsDeleted)
                .Select(x => x.UserId)
                .ToListAsync();

            // User filter
            if (userId.HasValue)
            {
                bool isSupervisor = supervisorUserIds.Contains(userId.Value);

                if (!isSupervisor)
                {
                    baseQuery = baseQuery
                        .Where(x => x.AssignedToUserId == userId.Value);
                }
            }

            // WorkOrder filter
            if (workorderId.HasValue)
            {
                baseQuery = baseQuery
                    .Where(x => x.WorkOrderId == workorderId.Value);
            }

            var latestIds = await baseQuery
                .GroupBy(x => x.WorkOrderId)
                .Select(g => g.OrderByDescending(x => x.CreatedAt)
                    .Select(x => x.WorkOrderProgressUpdateId)
                    .First())
                .ToListAsync();

            var activities = await tenantDb.WorkOrderProgressUpdates
                .Where(x => latestIds.Contains(x.WorkOrderProgressUpdateId))
                .Join(
                    tenantDb.WorkOrders,
                    update => update.WorkOrderId,
                    wo => wo.WorkOrderId,
                    (update, wo) => new RecentWorkOrderUpdateDto
                    {
                        WorkOrderId = wo.WorkOrderId,
                        WorkOrderNumber = wo.WorkOrderNumber,
                        UpdateType = update.UpdateType!.Value,
                        Status = update.Status,
                        ProgressPercentage = update.ProgressPercentage,
                        Message = update.Message,
                        Action = update.Action,
                        AttachmentPath = !string.IsNullOrEmpty(update.AttachmentPath)
                            ? $"{baseUrl.TrimEnd('/')}/{update.AttachmentPath}"
                            : null,
                        FileType = update.FileType,
                        AssignedToUserId = update.AssignedToUserId,
                        CreatedAt = update.CreatedAt
                    })
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            response.StatusCode = StatusCode.Success;
            response.StatusMessage = "Recent workorder activity fetched successfully";
            response.GetAllData = activities;

            return response;
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
                        Status = x.Status.ToString()!,
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
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"{InventoryCostStatusMessage.Error}: {ex.Message}";
            }

            return response;
        }
        public async Task<CostReportDto> GetCostReportAsync(int tenantId)
        {
            using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

            var todayUtc = DateTime.UtcNow;
            var startOfLastMonth = DateTime.SpecifyKind(
                new DateTime(todayUtc.Year, todayUtc.Month, 1),
                DateTimeKind.Utc
            ).AddMonths(-1);

            var endOfLastMonth = startOfLastMonth
                .AddMonths(1)
                .AddTicks(-1);

            var workOrders = tenantDb.WorkOrders
                .Where(w =>
                    w.TenantId == tenantId &&
                    w.IsActive &&
                    !w.IsDeleted &&
                    w.CreatedAt >= startOfLastMonth &&
                    w.CreatedAt <= endOfLastMonth
                );

            var costAnalysis = new CostDetailsDto
            {
                TotalLaborCosts = await workOrders.SumAsync(w => w.LaborCost ?? 0),
                TotalPartsCosts = await workOrders.SumAsync(w => w.PartCost ?? 0),
                TotalCosts = await workOrders.SumAsync(w => w.TotalCost ?? 0)
            };

            var costPerCategory = new CostPerCategoryDto
            {
                PreventiveMaintenance = await workOrders
                    .Where(w => w.WorkOrderType == WorkOrderTypeEnum.Preventive)
                    .SumAsync(w => w.TotalCost ?? 0),

                EmergencyRepairs = await workOrders
                    .Where(w => w.WorkOrderType == WorkOrderTypeEnum.Corrective)
                    .SumAsync(w => w.TotalCost ?? 0),

                RoutineMaintenance = 0
            };

            return new CostReportDto
            {
                CostAnalysis = costAnalysis,
                CostPerCategory = costPerCategory
            };
        }
        public async Task<GetAllRecord<WorkOrderPartUsageDto>> GetWorkOrderPartUsageAsync(int tenantId)
        {
            var response = new GetAllRecord<WorkOrderPartUsageDto>();

            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                var data = await tenantDb.WorkOrders
                    .Where(wo =>
                        wo.TenantId == tenantId &&
                        wo.IsActive &&
                      
                        !wo.IsDeleted)
                    .SelectMany(
                        wo => wo.RequiredTools!
                            .Where(rt => !rt.IsDeleted),
                        (wo, rt) => new WorkOrderPartUsageDto
                        {
                            WorkOrderId = wo.WorkOrderId,
                            WorkOrderNumber = wo.WorkOrderNumber,
                            WorkOrderTitle = wo.Title,

                            ToolId = rt.ToolId,
                            PartNumber = rt.Tool != null ? rt.Tool.ItemCode : null,
                            PartName = rt.Tool != null ? rt.Tool.ItemName : null,

                            QuantityUsed = rt.QuantityRequired ?? 0,
                            UnitCost = rt.Tool != null ? rt.Tool.UnitPrice : 0,

                            PartCost =
                                (rt.QuantityRequired ?? 0) *
                                (rt.Tool != null ? rt.Tool.UnitPrice : 0),

                            LaborCost = wo.LaborCost ?? 0,

                            TotalCost =
                                (wo.LaborCost ?? 0) +
                                ((rt.QuantityRequired ?? 0) *
                                 (rt.Tool != null ? rt.Tool.UnitPrice : 0))
                        })
                    .ToListAsync();

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = InventoryCostStatusMessage.CostRetrieved;
                response.GetAllData = data;
            }
            catch (Exception ex)
            {
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"{InventoryCostStatusMessage.Error}: {ex.Message}";
            }

            return response;
        }
        public async Task<GetSpecificRecord<WorkOrderDashboardDto>> GetWorkOrderDashboardAsync(int tenantId)
        {
            var response = new GetSpecificRecord<WorkOrderDashboardDto>();

            using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

            var total = await tenantDb.WorkOrders
                .Where(x => x.TenantId == tenantId && !x.IsDeleted)
                .CountAsync();

            var result = new WorkOrderDashboardDto();

            // Work Orders by Type
            var typeData = await tenantDb.WorkOrders
                .Where(x => x.TenantId == tenantId && !x.IsDeleted)
                .GroupBy(x => x.WorkOrderType)
                .Select(g => new
                {
                    Type = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            result.WorkOrdersByType = typeData.Select(x => new WorkOrderTypeSummaryDto
            {
                Type = x.Type.ToString()!,
                Count = x.Count,
                Percentage = total == 0 ? 0 : Math.Round((decimal)x.Count * 100 / total, 1)
            }).ToList();


            // Work Orders by Priority
            var priorityData = await tenantDb.WorkOrders
                .Where(x => x.TenantId == tenantId && !x.IsDeleted)
                .GroupBy(x => x.Priority)
                .Select(g => new
                {
                    Priority = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            result.WorkOrdersByPriority = priorityData.Select(x => new WorkOrderPrioritySummaryDto
            {
                Priority = x.Priority.ToString()!,
                Count = x.Count,
                Percentage = total == 0 ? 0 : Math.Round((decimal)x.Count * 100 / total, 1)
            }).ToList();


            response.StatusCode = StatusCode.Success;
            response.StatusMessage = "Work order dashboard fetched successfully";
            response.Data = result;

            return response;
        }

        //Private Helper Methods...


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
        private static DateTime ToUtc(DateTime dateTime)
        {
            return dateTime.Kind switch
            {
                DateTimeKind.Utc => dateTime,
                DateTimeKind.Local => dateTime.ToUniversalTime(),
                _ => DateTime.SpecifyKind(dateTime, DateTimeKind.Utc)
            };
        }
        private List<T> ReadCsvFile<T>(IFormFile file) where T : new()
        {
            using var reader = new StreamReader(file.OpenReadStream());
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            return csv.GetRecords<T>().ToList();
        }
        private string GetFileType(IFormFile file)
        {
            var extension = Path.GetExtension(file.FileName).ToLower();

            if (new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" }.Contains(extension))
                return "image";

            if (new[] { ".mp4", ".mov", ".avi", ".mkv", ".webm" }.Contains(extension))
                return "video";

            if (new[] { ".pdf", ".doc", ".docx", ".xls", ".xlsx" }.Contains(extension))
                return "document";

            return "unknown";
        }
        private async Task<string> GetUserNameAsync(int tenantId, int? userId)
        {
            using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

            var user = await tenantDb.FactoryUsers
                .Where(x => x.UserId == userId
                         && x.TenantId == tenantId
                         && !x.IsDeleted)
                .Select(x => new
                {
                    x.FirstName,
                    x.LastName,
                    x.Username
                })
                .FirstOrDefaultAsync();

            if (user == null)
                return "Technician";

            // Priority: Full Name > Username
            var fullName = $"{user.FirstName} {user.LastName}".Trim();

            return !string.IsNullOrWhiteSpace(fullName)
                ? fullName
                : user.Username;
        }
    }
}

