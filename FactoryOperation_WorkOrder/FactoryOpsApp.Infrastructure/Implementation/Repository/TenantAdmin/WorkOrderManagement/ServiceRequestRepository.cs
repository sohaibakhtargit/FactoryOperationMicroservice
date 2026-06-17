using FactoryOperation_WorkOrder.FactoryOpsApp.Application.DTOs;
using FactoryOperation_WorkOrder.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.WorkOrderManagement;
using FactoryOperation_WorkOrder.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.AuditLogs;
using FactoryOperation_WorkOrder.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.Common;
using FactoryOperation_WorkOrder.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.EventTrace;
using FactoryOperation_WorkOrder.FactoryOpsApp.Application.Models;
using FactoryOperation_WorkOrder.FactoryOpsApp.Infrastructure.Implementation.Services.TenantAdmin.WorkOrderManagement;
using FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Domain.Entities;
using FactoryOpsApp.Domain.Entities.FactoryOpsTenants;
using FactoryOpsApp.Infrastructure.DBContext;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using System.Linq;
using System.Text;
using System.Text.Json;
using static FactoryOperation_WorkOrder.FactoryOpsApp.Common.CommonConstantURLs;
using static FactoryOpsApp.Common.CommonConstant;

namespace FactoryOperation_WorkOrder.FactoryOpsApp.Infrastructure.Implementation.Repository.TenantAdmin.WorkOrderManagement
{
    public class ServiceRequestRepository : IServiceRequestRepository
    {
        private readonly MasterFactoryOpsDbContext _masterDbcontext;
        private readonly TenantDbContextFactory _tenantDbContext;
        private readonly IAuditLogService _auditLogger;
        private readonly IEventTraceLogger _trace;
        private readonly IFileStorageService _fileStorageService;
        private readonly IConfiguration _configuration;

        public ServiceRequestRepository
            (MasterFactoryOpsDbContext masterDbcontext, 
            TenantDbContextFactory tenantDbContext,
            IAuditLogService auditLogger, IEventTraceLogger trace,
            IFileStorageService fileStorageService, IConfiguration configuration)
        {
            _masterDbcontext = masterDbcontext;
            _tenantDbContext = tenantDbContext;
            _auditLogger = auditLogger;
            _trace = trace;
            _fileStorageService = fileStorageService;
            _configuration = configuration;
        }

        //public async Task<CommonServiceRequestResponseModel> CreateServiceRequestAsync(ServiceRequestDto dto)
        //{
        //    var correlationId = Guid.NewGuid().ToString();
        //    var response = new CommonServiceRequestResponseModel();

        //    using var tenantDb = _tenantDbContext.GetTenantDbContext(dto.TenantId);
        //    using var transaction = await tenantDb.Database.BeginTransactionAsync();

        //    try
        //    {

        //        var requestNumber = GenerateRequestNumber();

        //        var entity = new ServiceRequest
        //        {
        //            TenantId = dto.TenantId,
        //            RequestNumber = requestNumber,
        //            Title = dto.Title,
        //            Description = dto.Description,
        //            LocationId = dto.LocationId,
        //            Status = ServiceRequestStatus.Pending,
        //            AssetId = dto.AssetId,
        //            Priority = dto.Priority!.Value,
        //            AssignedToUserId = null,
        //            AssignedToTeamId = null,                   
        //            DueDate = dto.DueDate,
        //            ScheduleDate = dto.ScheduleDate,
        //            EstimatedDurationMinutes = dto.EstimatedDurationMinutes,
        //            RequestType = dto.RequestType!.Value,
        //            Instructions = dto.Instructions,
        //            LaborCost = dto.LaborCost!.Value,
        //            PartCost = dto.PartCost!.Value,
        //            TotalCost = dto.LaborCost.Value + dto.PartCost.Value,
        //            IsActive = true,
        //            IsDeleted = false,
        //            CreatedAt = DateTime.UtcNow,
        //            UpdatedAt = DateTime.UtcNow,
        //            CreatedBy = dto.CreatedBy
        //        };

        //        await tenantDb.ServiceRequests.AddAsync(entity);
        //        await tenantDb.SaveChangesAsync();
        //        await _trace.TrackAsync(new EventTraceEntry
        //        {
        //            CorrelationId = correlationId,
        //            TenantId = dto.TenantId,
        //            Service = "ServiceRequestService",
        //            Stage = "CREATED",
        //            Status = "SUCCESS",
        //            Message = $"SR {requestNumber} created"
        //        });


        //        var productionRoleId = await tenantDb.FactoryRoles
        //            .Where(x => x.RoleName == "Production Supervisor" && !x.IsDeleted)
        //            .Select(x => x.RoleId)
        //            .FirstOrDefaultAsync();

        //        var productionUsers = await tenantDb.FactoryUserRoles
        //            .Where(x => x.TenantId == dto.TenantId && x.RoleId == productionRoleId && !x.IsDeleted)
        //            .Select(x => x.UserId)
        //            .ToListAsync();

        //        var MaintenanceSupervisorRoleId = await tenantDb.FactoryRoles
        //            .Where(x => x.RoleName == "Maintenance Supervisor" && !x.IsDeleted)
        //            .Select(x => x.RoleId)
        //            .FirstOrDefaultAsync();

        //        var MaintenanceSupervisorUserId = await tenantDb.FactoryUserRoles
        //            .Where(x => x.TenantId == dto.TenantId &&
        //                        x.RoleId == MaintenanceSupervisorRoleId &&
        //                        !x.IsDeleted)
        //            .Select(x => x.UserId)
        //            .ToListAsync();

        //        var MaintenanceSupervisorEmail = await tenantDb.FactoryUserRoles
        //            .Where(x => x.TenantId == dto.TenantId &&
        //                        x.RoleId == MaintenanceSupervisorRoleId &&
        //                        !x.IsDeleted)
        //            .Select(x => x.FactoryUsers.Email)
        //            .ToListAsync();
        //        var TenantEmail = await _masterDbcontext.FactoryTenants
        //            .Where(x => x.TenantId == dto.TenantId)
        //            .Select(x => x.AdminEmail)
        //            .FirstOrDefaultAsync();

        //        var senderId = dto.CreatedBy;

        //        var incomingNotification = MaintenanceSupervisorUserId
        //            .Concat(productionUsers)
        //            .Where(x => x != 0)
        //            .Distinct()
        //            .Select(x => (int?)x)
        //            .Where(x => x != senderId!.Value)
        //            .ToList();

        //        var TargetUserIds = incomingNotification
        //            .Append(dto.CreatedBy ?? 0)
        //            .Where(x => x != 0)
        //            .Distinct()
        //            .Select(x => (int?)x)
        //            .ToList();

        //        var eventDto = new ServiceRequestEventDto
        //        {
        //            ServiceRequestId = entity.ServiceRequestId,
        //            TenantId = entity.TenantId,
        //            TenantEmail = TenantEmail,
        //            ServiceRequestType = entity.RequestType,
        //            ServiceRequestNumber = entity.RequestNumber,
        //            Title = entity.Title,
        //            Description = entity.Description,
        //            AssetId = entity.AssetId,
        //            EventType = "ServiceRequestCreated",
        //            NotificationType = "ServiceRequestCreated",
        //            PartCost = entity.PartCost,
        //            LaborCost = entity.LaborCost,
        //            incomingNotifications = incomingNotification,
        //            outgoingNotifications = dto.CreatedBy,
        //            TotalCost = entity.TotalCost,
        //            EventTime = DateTime.UtcNow,
        //            Priority = entity.Priority.ToString(),
        //            Status = entity.Status.ToString(),
        //            LocationId = entity.LocationId,
        //            CreatedBy = entity.CreatedBy,
        //            TargetUserIds = TargetUserIds,
        //            TargetEmailAddresses = MaintenanceSupervisorEmail
        //        };

        //        string topicName = KafkaCommonTopics.BuildTopicName(entity.TenantId, "ServiceRequestCreated");

        //        await _trace.TrackAsync(new EventTraceEntry
        //        {
        //            CorrelationId = correlationId,
        //            TenantId = dto.TenantId,
        //            Service = "ServiceRequestService",
        //            Stage = "PUBLISH_REQUESTED",
        //            Topic = topicName,
        //            Status = "SUCCESS"
        //        });

        //        var kafkaRequest = new
        //        {
        //            Topic = topicName,
        //            Key = $"servicerequest-{entity.ServiceRequestId}",
        //            Payload = eventDto,
        //            Source = "ServiceRequestService",
        //            Headers = new Dictionary<string, string>
        //            {
        //                ["tenant-id"] = entity.TenantId.ToString(),
        //                ["correlation-id"] = correlationId
        //            }
        //        };

        //        using var httpClient = new HttpClient();
        //        var jsonContent = new StringContent(
        //            JsonSerializer.Serialize(kafkaRequest),
        //            Encoding.UTF8,
        //            "application/json");

        //        var kafkaResponse = await httpClient.PostAsync(ConstantUrls.kafkaPublish, jsonContent);
        //        kafkaResponse.EnsureSuccessStatusCode();

        //        await transaction.CommitAsync();
        //        response.ServiceRequestId = entity.ServiceRequestId;
        //        response.StatusCode = StatusCode.Success;
        //        response.StatusMessage = ServiceRequestStatusMessage.ServiceRequestCreated;

        //        return response;
        //    }
        //    catch (Exception ex)
        //    {
        //        await transaction.RollbackAsync();

        //        await _trace.TrackAsync(new EventTraceEntry
        //        {
        //            CorrelationId = correlationId,
        //            TenantId = dto.TenantId,
        //            Service = "ServiceRequestService",
        //            Stage = "FAILED",
        //            Status = "ERROR",
        //            Error = ex.Message
        //        });

        //        throw;
        //    }
        //}

        public async Task<CommonServiceRequestResponseModel> CreateServiceRequestAsync(ServiceRequestDto dto)
        {
            var correlationId = Guid.NewGuid().ToString();
            var response = new CommonServiceRequestResponseModel();

            using var tenantDb = _tenantDbContext.GetTenantDbContext(dto.TenantId);
            using var transaction = await tenantDb.Database.BeginTransactionAsync();

            try
            {

                var requestNumber = GenerateRequestNumber();

                if (dto.ScheduleDate.HasValue && dto.DueDate.HasValue)
                {
                    var scheduleDate = dto.ScheduleDate.Value.ToUniversalTime();
                    var dueDate = dto.DueDate.Value.ToUniversalTime();

                    if (scheduleDate > dueDate)
                    {
                        response.StatusCode = StatusCode.BadRequest;
                        response.StatusMessage = "Schedule date cannot be later than due date.";

                        return response;
                    }
                }

                var entity = new ServiceRequest
                {
                    TenantId = dto.TenantId,
                    RequestNumber = requestNumber,
                    Title = dto.Title,
                    Description = dto.Description,
                    LocationId = dto.LocationId,
                    Status = ServiceRequestStatus.Pending,
                    AssetId = dto.AssetId,
                    Priority = dto.Priority!.Value,
                    AssignedToUserId = null,
                    AssignedToTeamId = null,
                    DueDate = dto.DueDate,
                    ScheduleDate = dto.ScheduleDate,
                    EstimatedDurationMinutes = dto.EstimatedDurationMinutes,
                    RequestType = dto.RequestType!.Value,
                    Instructions = dto.Instructions,
                    LaborCost = dto.LaborCost!.Value,
                    PartCost = dto.PartCost!.Value,
                    TotalCost = dto.LaborCost.Value + dto.PartCost.Value,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedBy = dto.CreatedBy
                };

                await tenantDb.ServiceRequests.AddAsync(entity);
                await tenantDb.SaveChangesAsync();

                if (dto.RequiredTools != null && dto.RequiredTools.Any())
                {
                    foreach (var toolDto in dto.RequiredTools)
                    {
                        var requiredQty = (toolDto.QuantityRequired == null || toolDto.QuantityRequired <= 0)
                            ? 1
                            : toolDto.QuantityRequired.Value;

                        var inventoryItem = await tenantDb.Inventory
                            .FirstOrDefaultAsync(i => i.ItemId == toolDto.ToolId);

                        if (inventoryItem == null)
                            throw new Exception($"Tool with ID {toolDto.ToolId} not found.");

                        var requiredTool = new WorkOrderRequiredTool
                        {
                            ServiceRequestId = entity.ServiceRequestId,
                            ToolId = toolDto.ToolId,
                            QuantityRequired = requiredQty,
                            CreatedBy = dto.CreatedBy ?? 0,
                            CreatedAt = DateTime.UtcNow
                        };

                        tenantDb.WorkOrderRequiredTools.Add(requiredTool);
                    }

                    await tenantDb.SaveChangesAsync();
                }

                await _trace.TrackAsync(new EventTraceEntry
                {
                    CorrelationId = correlationId,
                    TenantId = dto.TenantId,
                    Service = "ServiceRequestService",
                    Stage = "CREATED",
                    Status = "SUCCESS",
                    Message = $"SR {requestNumber} created"
                });

                var senderId = dto.CreatedBy ?? 0;

                // COMMON: Role Ids
                var productionRoleId = await tenantDb.FactoryRoles
                    .Where(x => x.RoleName == "Production Supervisor" && !x.IsDeleted)
                    .Select(x => x.RoleId)
                    .FirstOrDefaultAsync();

                var maintenanceRoleId = await tenantDb.FactoryRoles
                    .Where(x => x.RoleName == "Maintenance Supervisor" && !x.IsDeleted)
                    .Select(x => x.RoleId)
                    .FirstOrDefaultAsync();

                // TEAM FILTER (only if CreatedBy > 0)

                List<int> teamIds = new();

                if (dto.CreatedBy.GetValueOrDefault() > 0)
                {
                    teamIds = await tenantDb.FactoryTeamMembers
                        .Where(x => x.UserId == dto.CreatedBy && x.IsActive && !x.IsDeleted)
                        .Select(x => x.TeamId)
                        .ToListAsync();
                }

                // CHECK IF TEAM EXISTS
                bool hasTeamMapping = teamIds.Any();

                // USERS QUERY (single query)

                var userQuery = from fur in tenantDb.FactoryUserRoles
                                join tm in tenantDb.FactoryTeamMembers
                                    on fur.UserId equals tm.UserId into tmJoin
                                from tm in tmJoin.DefaultIfEmpty()
                                where fur.TenantId == dto.TenantId
                                      && !fur.IsDeleted
                                      && (
                                          fur.RoleId == productionRoleId ||
                                          fur.RoleId == maintenanceRoleId
                                      )
                                      && (
                                          dto.CreatedBy <= 0
                                          || !hasTeamMapping
                                          || (tm != null
                                              && tm.IsActive
                                              && !tm.IsDeleted
                                              && teamIds.Contains(tm.TeamId))
                                      )
                                select new
                                {
                                    fur.UserId,
                                    fur.RoleId,
                                    Email = fur.FactoryUsers.Email
                                };

                var users = await userQuery.Distinct().ToListAsync();

                // SPLIT USERS

                var productionUsers = users
                    .Where(x => x.RoleId == productionRoleId)
                    .Select(x => x.UserId)
                    .ToList();

                var maintenanceUsers = users
                    .Where(x => x.RoleId == maintenanceRoleId)
                    .Select(x => x.UserId)
                    .ToList();

                var productionEmails = users
                    .Where(x => x.RoleId == productionRoleId)
                    .Select(x => x.Email)
                    .Where(e => !string.IsNullOrEmpty(e))
                    .Distinct()
                    .ToList();

                var maintenanceEmails = users
                    .Where(x => x.RoleId == maintenanceRoleId)
                    .Select(x => x.Email)
                    .Where(e => !string.IsNullOrEmpty(e))
                    .Distinct()
                    .ToList();

                // TENANT EMAIL
            

                var TenantEmail = await _masterDbcontext.FactoryTenants
                    .Where(x => x.TenantId == dto.TenantId)
                    .Select(x => x.AdminEmail)
                    .FirstOrDefaultAsync();

          
                // CREATOR EMAIL
              
                var createdByEmail = await tenantDb.FactoryUsers
                    .Where(x => x.UserId == dto.CreatedBy)
                    .Select(x => x.Email)
                    .FirstOrDefaultAsync();

               
                // INCOMING USERS
                
                var incomingNotification = maintenanceUsers
                    .Concat(productionUsers)
                    .Where(x => x != 0 && x != senderId)
                    .Distinct()
                    .Select(x => (int?)x)
                    .ToList();

                // TARGET USERS

                var TargetUserIds = incomingNotification
                    .Append(dto.CreatedBy ?? 0)
                    .Where(x => x != 0)
                    .Distinct()
                    .Select(x => (int?)x)
                    .ToList();

                // EMAILS

                var TargetEmailAddress = productionEmails
                    .Concat(maintenanceEmails)
                    .Append(createdByEmail)
                    .Append(TenantEmail)
                    .Where(e => !string.IsNullOrEmpty(e))
                    .Distinct()
                    .ToList();

                var eventDto = new ServiceRequestEventDto
                {
                    ServiceRequestId = entity.ServiceRequestId,
                    TenantId = entity.TenantId,
                    TenantEmail = TenantEmail,
                    ServiceRequestType = entity.RequestType,
                    ServiceRequestNumber = entity.RequestNumber,
                    Title = entity.Title,
                    Description = entity.Description,
                    AssetId = entity.AssetId,
                    EventType = "ServiceRequestCreated",
                    NotificationType = "ServiceRequestCreated",
                    PartCost = entity.PartCost,
                    LaborCost = entity.LaborCost,
                    incomingNotifications = incomingNotification,
                    outgoingNotifications = dto.CreatedBy,
                    TotalCost = entity.TotalCost,
                    EventTime = DateTime.UtcNow,
                    Priority = entity.Priority.ToString(),
                    Status = entity.Status.ToString(),
                    LocationId = entity.LocationId,
                    CreatedBy = entity.CreatedBy,
                    TargetUserIds = TargetUserIds,
                    TargetEmailAddresses = TargetEmailAddress!
                };

                string topicName = KafkaCommonTopics.BuildTopicName(entity.TenantId, "ServiceRequestCreated");

                await _trace.TrackAsync(new EventTraceEntry
                {
                    CorrelationId = correlationId,
                    TenantId = dto.TenantId,
                    Service = "ServiceRequestService",
                    Stage = "PUBLISH_REQUESTED",
                    Topic = topicName,
                    Status = "SUCCESS"
                });

                var kafkaRequest = new
                {
                    Topic = topicName,
                    Key = $"servicerequest-{entity.ServiceRequestId}",
                    Payload = eventDto,
                    Source = "ServiceRequestService",
                    Headers = new Dictionary<string, string>
                    {
                        ["tenant-id"] = entity.TenantId.ToString(),
                        ["correlation-id"] = correlationId
                    }
                };

                using var httpClient = new HttpClient();
                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(kafkaRequest),
                    Encoding.UTF8,
                    "application/json");

                var kafkaResponse = await httpClient.PostAsync(ConstantUrls.kafkaPublish, jsonContent);
                kafkaResponse.EnsureSuccessStatusCode();

                await transaction.CommitAsync();
                response.ServiceRequestId = entity.ServiceRequestId;
                response.StatusCode = StatusCode.Success;
                response.StatusMessage = ServiceRequestStatusMessage.ServiceRequestCreated;

                return response;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                await _trace.TrackAsync(new EventTraceEntry
                {
                    CorrelationId = correlationId,
                    TenantId = dto.TenantId,
                    Service = "ServiceRequestService",
                    Stage = "FAILED",
                    Status = "ERROR",
                    Error = ex.Message
                });

                throw;
            }
        }

        public async Task<CommonResponseModel> ApproveServiceRequestAsync(ApproveServiceRequestDto dto)
        {
            var correlationId = Guid.NewGuid().ToString();

            using var db = _tenantDbContext.GetTenantDbContext(dto.TenantId);
            using var transaction = await db.Database.BeginTransactionAsync();

            try
            {
                var sr = await db.ServiceRequests
                    .FirstOrDefaultAsync(x => x.ServiceRequestId == dto.ServiceRequestId);

                if (sr == null)
                    throw new Exception("Service Request not found");

                if (sr.Status != ServiceRequestStatus.Pending && sr.Status != ServiceRequestStatus.ReOpened)
                    throw new Exception("Only pending and ReOpened requests can be approved");

                var requiredTools = await db.WorkOrderRequiredTools
                    .Where(x =>
                        x.ServiceRequestId == sr.ServiceRequestId &&
                        !x.IsDeleted)
                    .ToListAsync();
                foreach (var tool in requiredTools)
                {
                    var requiredQty =
                        tool.QuantityRequired == null ||
                        tool.QuantityRequired <= 0
                            ? 1
                            : tool.QuantityRequired.Value;

                    var inventoryItem = await db.Inventory
                        .FirstOrDefaultAsync(x =>
                            x.ItemId == tool.ToolId &&
                            !x.IsDeleted);

                    if (inventoryItem == null)
                    {
                        throw new Exception(
                            $"Tool with ID {tool.ToolId} not found in inventory.");
                    }

                    if (inventoryItem.QuantityAvailable < requiredQty)
                    {
                        throw new Exception(
                            $"Insufficient quantity for tool '{inventoryItem.ItemName}'. " +
                            $"Available: {inventoryItem.QuantityAvailable}, " +
                            $"Required: {requiredQty}");
                    }
                }
                sr.Status = ServiceRequestStatus.Approved;
                sr.ApprovedBy = dto.ApprovedBy;
                sr.ApprovedAt = DateTime.UtcNow;

                var workOrder = await CreateWorkOrderFromServiceRequest(db, sr, dto.ApprovedBy);
                sr.WorkOrderId = workOrder.WorkOrderId;

                List<LowStockEventDto> lowStockWarnings = new();

                foreach (var tool in requiredTools)
                {
                    var requiredQty =
                        tool.QuantityRequired == null ||
                        tool.QuantityRequired <= 0
                            ? 1
                            : tool.QuantityRequired.Value;

                    // Get inventory item
                    var inventoryItem = await db.Inventory
                        .FirstOrDefaultAsync(x =>
                            x.ItemId == tool.ToolId &&
                            !x.IsDeleted);

                    if (inventoryItem != null)
                    {
                        // Deduct inventory
                        inventoryItem.QuantityAvailable -= requiredQty;
                        inventoryItem.UpdatedAt = DateTime.UtcNow;
                        inventoryItem.UpdatedBy = dto.ApprovedBy;

                        // Low stock validation
                        if (inventoryItem.QuantityAvailable <= inventoryItem.ReorderLevel)
                        {
                            var StoreKeeperRoleId = await db.FactoryRoles
                                .Where(x =>
                                    x.RoleName == "StoreKeeper" &&
                                    !x.IsDeleted)
                                .Select(x => x.RoleId)
                                .FirstOrDefaultAsync();

                            var StoreKeeperUserIds = await db.FactoryUserRoles
                                .Where(x =>
                                    x.TenantId == dto.TenantId &&
                                    x.RoleId == StoreKeeperRoleId &&
                                    !x.IsDeleted)
                                .Select(x => x.UserId)
                                .ToListAsync();

                            var nullableUserIds = StoreKeeperUserIds
                                .Select(x => (int?)x)
                                .ToList();

                            var userEmails = await db.FactoryUsers
                                .Where(x =>
                                    StoreKeeperUserIds.Contains(x.UserId) &&
                                    !x.IsDeleted)
                                .Select(x => x.Email)
                                .Where(x => !string.IsNullOrEmpty(x))
                                .ToListAsync();

                            var tenantEmail = await _masterDbcontext.FactoryTenants
                                .Where(x => x.TenantId == dto.TenantId)
                                .Select(x => x.AdminEmail)
                                .FirstOrDefaultAsync();

                            lowStockWarnings.Add(new LowStockEventDto
                            {
                                TenantId = dto.TenantId,
                                TenantEmail = tenantEmail,
                                ItemId = inventoryItem.ItemId,
                                ItemCode = inventoryItem.ItemCode,
                                ItemName = inventoryItem.ItemName,
                                QuantityAvailable = inventoryItem.QuantityAvailable,
                                ReorderLevel = inventoryItem.ReorderLevel,
                                EventType = "LowStockWarning",
                                TargetUsersIds = nullableUserIds,
                                incomingNotifications = nullableUserIds,
                                TargetEmailAddresses = userEmails
                            });
                        }
                    }

                    tool.WorkOrderId = workOrder.WorkOrderId;
                    tool.UpdatedAt = DateTime.UtcNow;
                    tool.UpdatedBy = dto.ApprovedBy;
                }
                await db.SaveChangesAsync();

                var ProductionSupervisorRoleId = await db.FactoryRoles
                    .Where(x => x.RoleName == "Production Supervisor" && !x.IsDeleted)
                    .Select(x => x.RoleId)
                    .FirstOrDefaultAsync();
                var ProductionSupervisorUserId = await db.FactoryUserRoles
                    .Where(x => x.TenantId == dto.TenantId &&
                                x.RoleId == ProductionSupervisorRoleId &&
                                !x.IsDeleted)
                    .Select(x => x.UserId)
                    .ToListAsync();

                var MaintenaceSupervisorRoleId = await db.FactoryRoles
                    .Where(x => x.RoleName == "Maintenance Supervisor" && !x.IsDeleted)
                    .Select(x => x.RoleId)
                    .FirstOrDefaultAsync();
                var MaintenaceSupervisorUserId = await db.FactoryUserRoles
                    .Where(x => x.TenantId == dto.TenantId &&
                                x.RoleId == MaintenaceSupervisorRoleId &&
                                !x.IsDeleted)
                    .Select(x => x.UserId)
                    .ToListAsync();

                var MaintenaceSupervisorEmail = await db.FactoryUserRoles
                    .Where(x => x.TenantId == dto.TenantId &&
                                x.RoleId == MaintenaceSupervisorRoleId &&
                                !x.IsDeleted)
                    .Select(x => x.FactoryUsers.Email)
                    .ToListAsync();

                var TenantEmail = await _masterDbcontext.FactoryTenants
                    .Where(x => x.TenantId == dto.TenantId)
                    .Select(x => x.AdminEmail)
                    .FirstOrDefaultAsync();

                var ProductionSupervisorEmail = await db.FactoryUserRoles
                    .Where(x => x.TenantId == dto.TenantId &&
                                x.RoleId == ProductionSupervisorRoleId &&
                                !x.IsDeleted)
                    .Select(x => x.FactoryUsers.Email)
                    .ToListAsync();

                var TargetEmailAddress  = ProductionSupervisorEmail
                    .Concat(MaintenaceSupervisorEmail)
                    .Where(e => !string.IsNullOrEmpty(e))
                    .Append(TenantEmail)
                    .Distinct()
                    .ToList();


                var incomingNotification = new List<int?>();

                if (sr.AssignedToUserId.HasValue)
                {
                     incomingNotification = ProductionSupervisorUserId
                    .Concat(MaintenaceSupervisorUserId)
                    .Append(sr.AssignedToUserId.Value)
                    .Where(x => x != 0 && x != dto.ApprovedBy)
                    .Select(x => (int?)x)
                    .ToList();
                }
                else
                {
                  incomingNotification = ProductionSupervisorUserId
                 .Concat(MaintenaceSupervisorUserId)
                 .Where(x => x != 0 && x != dto.ApprovedBy)
                 .Select(x => (int?)x)
                 .ToList();
                }

               
                var outgoingNotification = dto.ApprovedBy;

                // Bell users = incoming + creator
                /*  var TargetUserIds = ProductionSupervisorUserId
                      .Concat( MaintenaceSupervisorUserId ) 
                      .Append(dto.ApprovedBy )
                      .Where(x => x != 0)
                      .Distinct()
                      .ToList();*/

                var TargetUserIds = incomingNotification
                    .Append(outgoingNotification)
                    .Where(x => x != 0)
                    .Distinct()
                    .Select(x => (int?)x)   
                    .ToList();

                var eventDto = new ServiceRequestEventDto
                {
                    EventType = "ServiceRequestApproved",
                    NotificationType = "ServiceRequestApproved",
                    EventTime = DateTime.UtcNow,
                    TenantEmail = TenantEmail,
                    TenantId = sr.TenantId,
                    ServiceRequestId = sr.ServiceRequestId,
                    ServiceRequestNumber = sr.RequestNumber,
                    Status = sr.Status.ToString(),

                    CreatedBy = sr.CreatedBy,
                    ApprovedBy = sr.ApprovedBy,

                    Title = sr.Title,
                    Description = sr.Description,

                    WorkOrderId = workOrder.WorkOrderId,
                    WorkOrderNumber = workOrder.WorkOrderNumber,
                    incomingNotifications = incomingNotification,
                    outgoingNotifications = outgoingNotification,
                    TargetUserIds = TargetUserIds,
                    TargetEmailAddresses = TargetEmailAddress!
                };

                await PublishEvent(dto.TenantId, correlationId, "ServiceRequestApproved", eventDto);

                /* await PublishEvent(dto.TenantId, correlationId, "ServiceRequestApproved", new

                     {
                     EventType = "ServiceRequestApproved",
                     NotificationType = "ServiceRequestApproved",
                     EventTime = DateTime.UtcNow,

                     TenantId = sr.TenantId,
                     WorkOrderType = sr.RequestType,
                     ServiceRequestType = sr.RequestType.ToString(),
                     ServiceRequestId = sr.ServiceRequestId,
                     RequestNumber = sr.RequestNumber,
                     Status = sr.Status.ToString(),
                     ApprovedBy = sr.ApprovedBy,
                     CreatedBy = sr.CreatedBy,
                     Title = sr.Title,
                     Description = sr.Description,
                     WorkOrderId = workOrder.WorkOrderId,
                     WorkOrderNumber = workOrder.WorkOrderNumber
                 });*/
                //await PublishWorkOrderEvent(workOrder, correlationId);

                foreach (var lowStockWarning in lowStockWarnings)
                {
                    await PublishLowStockKafkaAsync(dto.TenantId, lowStockWarning);
                }

                await transaction.CommitAsync();

                return new CommonResponseModel
                {
                    StatusCode = StatusCode.Success,
                    StatusMessage = "Service Request Approved & WorkOrder Created"
                };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<CommonResponseModel> RejectServiceRequestAsync(RejectServiceRequestDto dto)
        {
            var correlationId = Guid.NewGuid().ToString();

            using var db = _tenantDbContext.GetTenantDbContext(dto.TenantId);
            using var transaction = await db.Database.BeginTransactionAsync();

            try
            {

                var sr = await db.ServiceRequests
                    .FirstOrDefaultAsync(x => x.ServiceRequestId == dto.ServiceRequestId);

                if (sr == null)
                    throw new Exception("Service Request not found");

                sr.Status = ServiceRequestStatus.Rejected;
                sr.RejectedBy = dto.RejectedBy;
                sr.RejectedAt = DateTime.UtcNow;
                sr.RejectionReason = dto.Reason;
                sr.UpdatedAt = DateTime.UtcNow;

                await db.SaveChangesAsync();

                string? workOrderNumber = null;

                if (sr.WorkOrderId.HasValue)
                {
                    var wo = await db.WorkOrders
                        .FirstOrDefaultAsync(x => x.WorkOrderId == sr.WorkOrderId);

                    if (wo != null)
                    {
                       
                        workOrderNumber = wo.WorkOrderNumber;
                    }
                }

                var MaintenanceSupervisorRoleId = await db.FactoryRoles
                    .Where(x => x.RoleName == "Maintenance Supervisor" && !x.IsDeleted)
                    .Select(x => x.RoleId)
                    .FirstOrDefaultAsync();
                var MaintenanceSupervisorUserId = await db.FactoryUserRoles
                    .Where(x => x.TenantId == dto.TenantId &&
                                x.RoleId == MaintenanceSupervisorRoleId &&
                                !x.IsDeleted)
                    .Select(x => x.UserId)
                    .ToListAsync();

                var MaintenanceSupervisorEmail = await db.FactoryUserRoles
                    .Where(x => x.TenantId == dto.TenantId &&
                                x.RoleId == MaintenanceSupervisorRoleId &&
                                !x.IsDeleted)
                    .Select(x => x.FactoryUsers.Email)
                    .ToListAsync();
                var TenantEmail = await _masterDbcontext.FactoryTenants
                    .Where(x => x.TenantId == dto.TenantId)
                    .Select(x => x.AdminEmail)
                    .FirstOrDefaultAsync();
                var ProductionSupervisorRoleId = await db.FactoryRoles
                    .Where(x => x.RoleName == "Production Supervisor" && !x.IsDeleted)
                    .Select(x => x.RoleId)
                    .FirstOrDefaultAsync();
                var ProductionSupervisorUserId = await db.FactoryUserRoles
                    .Where(x => x.TenantId == dto.TenantId &&
                                x.RoleId == ProductionSupervisorRoleId &&
                                !x.IsDeleted)
                    .Select(x => x.UserId)
                    .ToListAsync();
                var ProductionSupervisorEmail = await db.FactoryUserRoles.Where(x => x.TenantId == dto.TenantId &&
                                x.RoleId == ProductionSupervisorRoleId &&
                                !x.IsDeleted)
                    .Select(x => x.FactoryUsers.Email)
                    .ToListAsync();

                var CreatedByEmail = await db.FactoryUsers
                    .Where(x => x.UserId == sr.CreatedBy)
                    .Select(x => x.Email)
                    .FirstOrDefaultAsync();

                var TargetEmailAddress = ProductionSupervisorEmail
                    .Concat(MaintenanceSupervisorEmail)
                    .Append(CreatedByEmail)
                    .Append(TenantEmail)
                    .Where(e => !string.IsNullOrEmpty(e))
                    .Distinct()
                    .ToList();

                var incomingNotification = new List<int?>();

                if (sr.AssignedToUserId.HasValue)
                {
                    incomingNotification = ProductionSupervisorUserId
                        .Append(sr.AssignedToUserId.Value)
                        .Append(sr.CreatedBy ??0)
                        .Concat(MaintenanceSupervisorUserId)
                        .Where(x => x != 0 && x!= dto.RejectedBy)
                        .Distinct()
                        .Select(x => (int?)x)
                        .ToList();
                }
                else
                {
                    incomingNotification = ProductionSupervisorUserId
                        .Append(sr.CreatedBy ?? 0)
                        .Concat(MaintenanceSupervisorUserId)
                        .Where(x => x != 0 && x != dto.RejectedBy)
                        .Distinct()
                        .Select(x => (int?)x)
                        .ToList();
                }

                var OutgoingNotifications = dto.RejectedBy;

                var TargetUserIds = incomingNotification
                        .Append(OutgoingNotifications)
                        .Where(x => x != 0)
                        .Distinct()
                        .Select(x => (int?)x)
                        .ToList();

                var eventDto = new ServiceRequestEventDto
                {
                    EventType = "ServiceRequestRejected",
                    NotificationType = "ServiceRequestRejected",
                    EventTime = DateTime.UtcNow,
                    TenantEmail = TenantEmail,
                    TenantId = sr.TenantId,
                    ServiceRequestId = sr.ServiceRequestId,
                    ServiceRequestNumber = sr.RequestNumber,
                    ServiceRequestType = sr.RequestType,
                  //  WorkOrderType = sr.RequestType,
                    WorkOrderId = sr.WorkOrderId,
                    WorkOrderTypeName = sr.RequestType.ToString(),
                    Title = sr.Title,
                    Description = sr.Description,
                    Status = sr.Status.ToString(),
                    RejectedBy = sr.RejectedBy,
                    CreatedBy = sr.CreatedBy,
                    Reason = dto.Reason,
                    WorkOrderNumber = workOrderNumber!,
                    incomingNotifications = incomingNotification,
                    outgoingNotifications = OutgoingNotifications,
                    TargetUserIds = TargetUserIds,
                    TargetEmailAddresses = TargetEmailAddress!
                };

                /*  var eventDto = new 
                  {
                      EventType = "ServiceRequestRejected",
                      NotificationType = "ServiceRequestRejected",
                      EventTime = DateTime.UtcNow,
                      ServiceRequestType = sr.RequestType.ToString(),
                      WorkOrderType = sr.RequestType,
                      TenantId = sr.TenantId,
                      ServiceRequestId = sr.ServiceRequestId,
                      WorkOrderNumber = workOrderNumber,
                      RequestNumber = sr.RequestNumber,
                      Title = sr.Title,
                      Description = sr.Description,
                      Status = sr.Status.ToString(),
                      RejectedBy = sr.RejectedBy,
                      CreatedBy = sr.CreatedBy,
                      Reason = dto.Reason,
                      TargetUserIds = new List<int?> { sr.CreatedBy }
                  };*/

                string topicName = KafkaCommonTopics.BuildTopicName(sr.TenantId, "ServiceRequestRejected");

                var kafkaRequest = new
                {
                    Topic = topicName,
                    Key = $"servicerequest-{sr.ServiceRequestId}",
                    Payload = eventDto,
                    Source = "ServiceRequestService",
                    Headers = new Dictionary<string, string>
                    {
                        ["tenant-id"] = sr.TenantId.ToString(),
                        ["correlation-id"] = correlationId
                    }
                };

                using var httpClient = new HttpClient();

                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(kafkaRequest),
                    Encoding.UTF8,
                    "application/json");

                var response = await httpClient.PostAsync(ConstantUrls.kafkaPublish, jsonContent);
                response.EnsureSuccessStatusCode();

                await transaction.CommitAsync();

                return new CommonResponseModel
                {
                    StatusCode = StatusCode.Success,
                    StatusMessage = "Service Request Rejected"
                };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<CommonResponseModel> AssignServiceRequestAsync(AssignServiceRequestDto dto)
        {
            var correlationId = Guid.NewGuid().ToString();
            using var db = _tenantDbContext.GetTenantDbContext(dto.TenantId);
            using var transaction = await db.Database.BeginTransactionAsync();

            try
            {
                var sr = await db.ServiceRequests
                    .FirstOrDefaultAsync(x => x.ServiceRequestId == dto.ServiceRequestId);

                if (sr == null)
                    throw new Exception("Service Request not found");

                sr.Status = ServiceRequestStatus.Assigned;
                sr.AssignedToUserId = dto.AssignedToUserId;
                sr.AssignedToTeamId = dto.AssignedToTeamId;
                sr.UpdatedAt = DateTime.UtcNow;
                sr.UpdatedBy = dto.AssignedBy;

                string? workOrderNumber = null;

                if (sr.WorkOrderId.HasValue)
                {
                    var wo = await db.WorkOrders
                        .FirstOrDefaultAsync(x => x.WorkOrderId == sr.WorkOrderId);

                    if (wo != null)
                    {
                        wo.AssignedToTeamId = dto.AssignedToTeamId;
                        wo.AssignedToUserId = dto.AssignedToUserId;
                        wo.UpdatedAt = DateTime.UtcNow;
                        workOrderNumber = wo.WorkOrderNumber;
                    }
                }

                var assignedUser = await db.FactoryUsers
                    .Where(u => u.UserId == dto.AssignedToUserId)
                    .Select(u => new { Name = u.FirstName + " " + u.LastName, u.Email })
                    .FirstOrDefaultAsync();

                var assignedUserTeamIds = await db.FactoryTeamMembers
                      .Where(x => x.UserId == dto.AssignedToUserId &&
                                  x.IsActive && !x.IsDeleted)
                      .Select(x => x.TeamId)
                      .ToListAsync();

                var CreatedByUserId = await db.ServiceRequests
                    .Where(x => x.ServiceRequestId == dto.ServiceRequestId)
                    .Select(x => x.CreatedBy)
                    .FirstOrDefaultAsync();

                var ProductionSupervisorRoleId = await db.FactoryRoles
                    .Where(x => x.RoleName == "Production Supervisor" && !x.IsDeleted)
                    .Select(x => x.RoleId)
                    .FirstOrDefaultAsync();

                var ProductionSupervisorUserId = await (
                    from fur in db.FactoryUserRoles
                    join tm in db.FactoryTeamMembers on fur.UserId equals tm.UserId
                    where fur.TenantId == dto.TenantId
                          && fur.RoleId == ProductionSupervisorRoleId
                          && !fur.IsDeleted
                          && tm.IsActive && !tm.IsDeleted
                          && assignedUserTeamIds.Contains(tm.TeamId)
                    select fur.UserId
                ).Distinct().ToListAsync();

                var MaintenaceSupervisorRoleId = await db.FactoryRoles
                    .Where(x => x.RoleName == "Maintenance Supervisor" && !x.IsDeleted)
                    .Select(x => x.RoleId)
                    .FirstOrDefaultAsync();
         
                var MaintenaceSupervisorUserId = await (
                        from fur in db.FactoryUserRoles
                        join tm in db.FactoryTeamMembers on fur.UserId equals tm.UserId
                        where fur.TenantId == dto.TenantId
                                && fur.RoleId == MaintenaceSupervisorRoleId
                                && !fur.IsDeleted
                                && tm.IsActive && !tm.IsDeleted
                                && assignedUserTeamIds.Contains(tm.TeamId)
                        select fur.UserId
                    ).Distinct().ToListAsync();

                //   var incomingNotification = new List<int?>();


                var incomingNotification = ProductionSupervisorUserId
                    .Concat(new List<int> { dto.AssignedToUserId })
                    .Concat(MaintenaceSupervisorUserId)
                    .Append(CreatedByUserId ?? 0)
                    .Where(x => x != 0 && x != dto.AssignedBy)
                    .Distinct()
                    .Select(x => (int?)x)
                    .ToList();

                var OutgoingNotifications = dto.AssignedBy;
                  /*  .Append(MaintenaceSupervisorUserId)
                    .Where(x => x != 0)
                    .Distinct()
                    .Select(x => (int?)x)
                    .ToList();*/

                var TragetUserIds = incomingNotification
                    .Append(OutgoingNotifications)
                    .Where(x => x != 0)
                    .Distinct()
                    .Select(x => (int?)x)
                    .ToList();


                var MaintenaceSupervisorEmail = await (
                            from fur in db.FactoryUserRoles
                            join tm in db.FactoryTeamMembers on fur.UserId equals tm.UserId
                            where fur.TenantId == dto.TenantId
                                    && fur.RoleId == MaintenaceSupervisorRoleId
                                    && !fur.IsDeleted
                                    && tm.IsActive && !tm.IsDeleted
                                    && assignedUserTeamIds.Contains(tm.TeamId)
                            select fur.FactoryUsers.Email
                        ).Distinct().ToListAsync();

                var ProductionSupervisorEmail = await (
                                from fur in db.FactoryUserRoles
                                join tm in db.FactoryTeamMembers on fur.UserId equals tm.UserId
                                where fur.TenantId == dto.TenantId
                                      && fur.RoleId == ProductionSupervisorRoleId
                                      && !fur.IsDeleted
                                      && tm.IsActive && !tm.IsDeleted
                                      && assignedUserTeamIds.Contains(tm.TeamId)
                                select fur.FactoryUsers.Email
                            ).Distinct().ToListAsync();

                var AssignedToEmail = await db.FactoryUsers
                    .Where(x => x.UserId == dto.AssignedToUserId)
                    .Select(x => x.Email)
                    .FirstOrDefaultAsync();

                var CreateByEmail = await db.FactoryUsers
                    .Where(x => x.UserId == sr.CreatedBy)
                    .Select(x => x.Email)
                    .FirstOrDefaultAsync();

                var TenantEmail = await _masterDbcontext.FactoryTenants
                    .Where(x => x.TenantId == dto.TenantId)
                    .Select(x => x.AdminEmail)
                    .FirstOrDefaultAsync();

                var TargetEmailAddress = ProductionSupervisorEmail
                    .Concat(MaintenaceSupervisorEmail)
                    .Append(CreateByEmail)
                    .Append(assignedUser!.Email)
                    .Append(AssignedToEmail)
                    .Append(TenantEmail)
                    .Where(e => !string.IsNullOrEmpty(e))
                    .Distinct()
                    .ToList();

                await db.SaveChangesAsync();

                var eventDto = new ServiceRequestEventDto
                {
                    EventType = "ServiceRequestWorkOrderAssigned",
                    NotificationType = "ServiceRequestWorkOrderAssigned",
                    EventTime = DateTime.UtcNow,
                    TenantId = sr.TenantId,
                    TenantEmail = TenantEmail,
                    ServiceRequestType = sr.RequestType,
                    ServiceRequestId = sr.ServiceRequestId,
                    WorkOrderId = sr.WorkOrderId,
                    WorkOrderNumber = workOrderNumber!,
                    ServiceRequestNumber = sr.RequestNumber,
                    Priority = sr.Priority.ToString(),
                    Title = sr.Title,
                    Description = sr.Description,
                    Status = sr.Status.ToString(),
                    UpdatedBy = sr.UpdatedBy,
                    CreatedBy = sr.CreatedBy,
                    AssignedToUserId = sr.AssignedToUserId,
                    AssignedTo = assignedUser?.Name,
                    AssignedToTeamId = sr.AssignedToTeamId,
                    WorkOrderTypeName = sr.RequestType.ToString(),
                    /*     TargetUserIds = new List<int?> { sr.AssignedToUserId },
                         TargetEmailAddresses = new List<string> { assignedUser!.Email }*/
                    incomingNotifications = incomingNotification,
                    outgoingNotifications = OutgoingNotifications,
                    TargetUserIds = TragetUserIds,
                    TargetEmailAddresses = TargetEmailAddress!
                };

                string topicName = KafkaCommonTopics.BuildTopicName(sr.TenantId,"ServiceRequestWorkOrderAssigned");

                var kafkaRequest = new
                {
                    Topic = topicName,
                    Key = $"servicerequest-{sr.ServiceRequestId}",
                    Payload = eventDto,
                    Source = "ServiceRequestService",
                    Headers = new Dictionary<string, string>
                    {
                        ["tenant-id"] = sr.TenantId.ToString(),
                        ["correlation-id"] = correlationId
                    }
                };

                using var httpClient = new HttpClient();

                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(kafkaRequest),
                    Encoding.UTF8,
                    "application/json");

                var response = await httpClient.PostAsync(ConstantUrls.kafkaPublish, jsonContent);
                response.EnsureSuccessStatusCode();
                await transaction.CommitAsync();

                return new CommonResponseModel
                {
                    StatusCode = StatusCode.Success,
                    StatusMessage = "Service Request Assigned"
                };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        public async Task<CommonResponseModel> ReopenServiceRequestAsync(ServiceRequestDto dto)
        {
            var correlationId = Guid.NewGuid().ToString();

            using var db = _tenantDbContext.GetTenantDbContext(dto.TenantId);
            using var transaction = await db.Database.BeginTransactionAsync();

            try
            {
                var sr = await db.ServiceRequests
                    .FirstOrDefaultAsync(x => x.ServiceRequestId == dto.ServiceRequestId);

                if (sr == null)
                    throw new Exception("Service Request not found");

                if (sr.Status != ServiceRequestStatus.Rejected)
                    throw new Exception("Only rejected requests can be reopened");

                sr.Title = dto.Title ?? sr.Title;
                sr.Description = dto.Description ?? sr.Description;
             
                if (dto.Priority.HasValue)
                    sr.Priority = dto.Priority.Value;

                sr.AssetId = dto.AssetId ?? sr.AssetId;

                sr.Status = ServiceRequestStatus.ReOpened;
                sr.UpdatedAt = DateTime.UtcNow;
                sr.UpdatedBy = dto.UpdatedBy;

                sr.Title = dto.Title ?? sr.Title;
                sr.Description = dto.Description ?? sr.Description;
                sr.LocationId = dto.LocationId ?? sr.LocationId;

                sr.AssetId = dto.AssetId ?? sr.AssetId;
                
                sr.AssignedToUserId = null;
                sr.AssignedToTeamId = null;                   
                sr.DueDate = dto.DueDate ?? sr.DueDate;
                sr.ScheduleDate = dto.ScheduleDate ??  sr.ScheduleDate;
                sr.EstimatedDurationMinutes = dto.EstimatedDurationMinutes ?? sr.EstimatedDurationMinutes;

                if (dto.RequestType.HasValue)
                    sr.RequestType = dto.RequestType.Value;
                sr.Instructions = dto.Instructions ?? sr.Instructions;

                sr.LaborCost = dto.LaborCost ?? sr.LaborCost;
                sr.PartCost = dto.PartCost ?? sr.PartCost;
                sr.TotalCost = dto.LaborCost + dto.PartCost ?? sr.TotalCost;
                /*         ServiceRequestImageName = fileName, 
                         ServiceRequestImageUrl = filePath,*/
                sr.IsActive = true;
                sr.IsDeleted = false;
                sr.UpdatedAt = DateTime.UtcNow;

                if (dto.RequiredTools != null)
                {
                    var existingTools = await db.WorkOrderRequiredTools
                        .Where(x =>
                            x.ServiceRequestId == sr.ServiceRequestId &&
                            !x.IsDeleted)
                        .ToListAsync();

                    // Soft delete removed tools
                    foreach (var existingTool in existingTools)
                    {
                        bool stillExists = dto.RequiredTools
                            .Any(x => x.ToolId == existingTool.ToolId);

                        if (!stillExists)
                        {
                            existingTool.IsDeleted = true;
                            existingTool.DeletedAt = DateTime.UtcNow;
                            existingTool.DeletedBy = dto.UpdatedBy;
                        }
                    }

                    // Add or update tools
                    foreach (var toolDto in dto.RequiredTools)
                    {
                        var requiredQty =
                            toolDto.QuantityRequired == null ||
                            toolDto.QuantityRequired <= 0
                                ? 1
                                : toolDto.QuantityRequired.Value;

                        var existingTool = existingTools
                            .FirstOrDefault(x =>
                                x.ToolId == toolDto.ToolId &&
                                !x.IsDeleted);

                        if (existingTool != null)
                        {
                            // Update existing tool
                            existingTool.QuantityRequired = requiredQty;
                            existingTool.UpdatedAt = DateTime.UtcNow;
                            existingTool.UpdatedBy = dto.UpdatedBy;

                            // If previously linked to WO, remove mapping
                            existingTool.WorkOrderId = null;
                        }
                        else
                        {
                            // Add new tool mapping
                            var newTool = new WorkOrderRequiredTool
                            {
                                ServiceRequestId = sr.ServiceRequestId,
                                ToolId = toolDto.ToolId,
                                QuantityRequired = requiredQty,
                                CreatedBy = dto.UpdatedBy ?? 0,
                                CreatedAt = DateTime.UtcNow,
                                IsActive = true,
                                IsDeleted = false
                            };

                            await db.WorkOrderRequiredTools.AddAsync(newTool);
                        }
                    }
                }

                await db.SaveChangesAsync();

                var MaintenanceSupervisorRoleId = await db.FactoryRoles
                    .Where(x => x.RoleName == "Maintenance Supervisor" && !x.IsDeleted)
                    .Select(x => x.RoleId)
                    .FirstOrDefaultAsync();

                var MaintenanceSupervisorUserIds = await db.FactoryUserRoles
                    .Where(x => x.TenantId == dto.TenantId &&
                                x.RoleId == MaintenanceSupervisorRoleId &&
                                !x.IsDeleted)
                    .Select(x => x.UserId)
                    .ToListAsync();

                var ProductionSupervisorRoleId = await db.FactoryRoles
                    .Where(x => x.RoleName == "Production Supervisor" && !x.IsDeleted)
                    .Select(x => x.RoleId)
                    .FirstOrDefaultAsync();
                var ProductionSupervisorUserIds = await db.FactoryUserRoles
                    .Where(x => x.TenantId == dto.TenantId &&
                                x.RoleId == ProductionSupervisorRoleId &&
                                !x.IsDeleted)
                    .Select(x => x.UserId)
                    .ToListAsync();
                var CreateByUserId = await db.ServiceRequests
                    .Where(x => x.ServiceRequestId == dto.ServiceRequestId)
                    .Select(x => x.CreatedBy)
                    .FirstOrDefaultAsync();

               

                var incomingNotification = new List<int?>();
                 
                if (sr.AssignedToUserId.HasValue)
                {
                    incomingNotification = ProductionSupervisorUserIds
                   .Concat(MaintenanceSupervisorUserIds)
                   .Append(CreateByUserId ?? 0)
                   .Append(sr.AssignedToUserId.Value)
                   .Where(x => x != 0 && x != dto.UpdatedBy)
                   .Distinct()
                   .Select(x => (int?)x)
                   .ToList();
                }
                else
                {
                    incomingNotification = ProductionSupervisorUserIds
                    .Concat(MaintenanceSupervisorUserIds)
                   .Append(CreateByUserId ?? 0)
                  .Append(CreateByUserId ?? 0)
                  .Where(x => x != 0 && x != dto.UpdatedBy)
                  .Distinct()
                  .Select(x => (int?)x)
                  .ToList();
                }
                var OutgoingNotifications = dto.UpdatedBy;

                var TargetUserIds = incomingNotification
                    .Append(OutgoingNotifications)
                    .Where(x => x != 0)
                    .Distinct()
                    .Select(x => (int?)x)
                    .ToList();


                var MaintenanceSupervisorEmails = await db.FactoryUserRoles
                    .Where(x => x.TenantId == dto.TenantId &&
                                x.RoleId == MaintenanceSupervisorRoleId &&
                                !x.IsDeleted)
                    .Select(x => x.FactoryUsers.Email)
                    .ToListAsync();

                var TenantEmail = await _masterDbcontext.FactoryTenants
                    .Where(x => x.TenantId == dto.TenantId)
                    .Select(x => x.AdminEmail)
                    .FirstOrDefaultAsync();

                var ProductionSupervisorEmails = await db.FactoryUserRoles
                    .Where(x => x.TenantId == dto.TenantId &&
                                x.RoleId == ProductionSupervisorRoleId &&
                                !x.IsDeleted)
                    .Select(x => x.FactoryUsers.Email)
                    .ToListAsync();

               var CreatedByEmail = await db.FactoryUsers
                    .Where(x => x.UserId == CreateByUserId)
                    .Select(x => x.Email)
                    .FirstOrDefaultAsync();

                var AssignedToEmailId = await db.FactoryUsers
                    .Where(x => x.UserId == sr.AssignedToUserId)
                    .Select(x => x.Email)
                    .FirstOrDefaultAsync();

                var TargetEmailAddress = ProductionSupervisorEmails
                    .Concat(MaintenanceSupervisorEmails)
                    .Append(AssignedToEmailId)
                    .Append(CreatedByEmail)
                    .Append(TenantEmail)
                    .Where(e => !string.IsNullOrEmpty(e))
                    .Distinct()
                    .ToList();

                string? workOrderNumber = null;

                if (sr.WorkOrderId.HasValue)
                {
                    var wo = await db.WorkOrders
                        .FirstOrDefaultAsync(x => x.WorkOrderId == sr.WorkOrderId);

                    if (wo != null)
                    {
                        workOrderNumber = wo.WorkOrderNumber;
                    }
                }
                var eventDto = new ServiceRequestEventDto
                {
                    EventType = "ServiceRequestReopened",
                    NotificationType = "ServiceRequestReopened",
                    EventTime = DateTime.UtcNow,
                    ServiceRequestType = sr.RequestType,
                    TenantId = sr.TenantId,
                    TenantEmail = TenantEmail,
                    ServiceRequestId = sr.ServiceRequestId,
                    WorkOrderNumber = workOrderNumber!,
                    ServiceRequestNumber = sr.RequestNumber,
                    Title = sr.Title,
                    Description = sr.Description,
                    Status = sr.Status.ToString(),
                    UpdatedBy = dto.UpdatedBy,
                    outgoingNotifications = OutgoingNotifications,
                    incomingNotifications = incomingNotification,
                    TargetUserIds = TargetUserIds,
                    TargetEmailAddresses = TargetEmailAddress!

                };

                string topicName = KafkaCommonTopics.BuildTopicName(
                    sr.TenantId,
                    "ServiceRequestReopened"
                );

                var kafkaRequest = new
                {
                    Topic = topicName,
                    Key = $"servicerequest-{sr.ServiceRequestId}",
                    Payload = eventDto,
                    Source = "ServiceRequestService",
                    Headers = new Dictionary<string, string>
                    {
                        ["tenant-id"] = sr.TenantId.ToString(),
                        ["correlation-id"] = correlationId
                    }
                };

                using var httpClient = new HttpClient();

                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(kafkaRequest),
                    Encoding.UTF8,
                    "application/json");

                var response = await httpClient.PostAsync(ConstantUrls.kafkaPublish, jsonContent);
                response.EnsureSuccessStatusCode();

                await transaction.CommitAsync();

                return new CommonResponseModel
                {
                    StatusCode = StatusCode.Success,
                    StatusMessage = "Service Request Reopened Successfully"
                };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<CommonResponseModel> UploadServiceRequestMediaAsync(ServiceRequestMediaDto dto)
        {
            var response = new CommonResponseModel();

            await using var tenantDb = _tenantDbContext.GetTenantDbContext(dto.TenantId);
            await using var transaction = await tenantDb.Database.BeginTransactionAsync();

            try
            {
                var existingServiceRequest = await tenantDb.ServiceRequests
                    .FirstOrDefaultAsync(l => l.ServiceRequestId == dto.ServiceRequestId && !l.IsDeleted);

                if (existingServiceRequest == null)
                {
                    response.StatusCode = StatusCode.NotFound;
                    response.StatusMessage = ServiceRequestStatusMessage.ServiceRequestNotFound;
                    return response;
                }

                var file = dto.ServiceRequestMedia;

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
                    "image" => "ServiceRequestImages",
                    "video" => "ServiceRequestVideos",
                    "document" => "ServiceRequestDocuments",
                    _ => "Others"
                };

                var relativePath = await _fileStorageService.SaveFileAsync(file, folder);

                existingServiceRequest.ServiceRequestMedia = file.FileName ?? string.Empty;
                existingServiceRequest.ServiceRequestMediaPath = relativePath ?? string.Empty;
                existingServiceRequest.FileType = fileType;
                existingServiceRequest.UpdatedAt = DateTime.UtcNow;
                existingServiceRequest.UpdatedBy = dto.UpdatedBy ?? 0;

                await tenantDb.SaveChangesAsync();
                await transaction.CommitAsync();

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = "Service request media uploaded successfully";

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

        public async Task<CommonResponseModel> UpdateServiceRequestAsync(ServiceRequestDto dto)
        {
           
                using var tenantDb = _tenantDbContext.GetTenantDbContext(dto.TenantId);

                var entity = await tenantDb.ServiceRequests
                    .FirstOrDefaultAsync(sr => sr.ServiceRequestId == dto.ServiceRequestId && !sr.IsDeleted);

                if (entity == null)
                    return new CommonResponseModel { StatusCode = StatusCode.NotFound, StatusMessage = ServiceRequestStatusMessage.ServiceRequestNotFound };

                entity.Title = dto.Title;
                entity.Description = dto.Description;
                entity.LocationId = dto.LocationId;
                entity.Status = dto.Status!.Value;
                entity.Priority = dto.Priority!.Value;
                entity.AssignedToUserId = dto.AssignedToUserId;
                entity.AssignedToTeamId = dto.AssignedToTeamId;
                entity.DueDate = dto.DueDate;
                entity.ScheduleDate = dto.ScheduleDate;
                entity.EstimatedDurationMinutes = dto.EstimatedDurationMinutes;
                entity.RequestType = dto.RequestType!.Value;
                entity.Instructions = dto.Instructions;
                entity.CompletionNotes = dto.CompletionNotes;
                entity.LaborCost = dto.LaborCost!.Value;
                entity.PartCost = dto.PartCost!.Value;
                entity.TotalCost = dto.LaborCost!.Value + dto.PartCost!.Value;
                entity.UpdatedAt = DateTime.UtcNow;
                entity.UpdatedBy = dto.UpdatedBy;

            if (dto.RequiredTools != null)
            {
                var existingTools = await tenantDb.WorkOrderRequiredTools
                    .Where(x =>
                        x.ServiceRequestId == entity.ServiceRequestId &&
                        !x.IsDeleted)
                    .ToListAsync();

                foreach (var existing in existingTools)
                {
                    bool stillExists = dto.RequiredTools
                        .Any(x => x.ToolId == existing.ToolId);

                    if (!stillExists)
                    {
                        existing.IsDeleted = true;
                        existing.DeletedAt = DateTime.UtcNow;
                        existing.DeletedBy = dto.UpdatedBy;
                    }
                }

                foreach (var toolDto in dto.RequiredTools)
                {
                    var requiredQty =
                        toolDto.QuantityRequired == null ||
                        toolDto.QuantityRequired <= 0
                            ? 1
                            : toolDto.QuantityRequired.Value;

                    var existingTool = existingTools
                        .FirstOrDefault(x =>
                            x.ToolId == toolDto.ToolId &&
                            !x.IsDeleted);

                    if (existingTool != null)
                    {
                        existingTool.QuantityRequired = requiredQty;
                        existingTool.UpdatedAt = DateTime.UtcNow;
                        existingTool.UpdatedBy = dto.UpdatedBy;
                    }
                    else
                    {
                        var newTool = new WorkOrderRequiredTool
                        {
                            ServiceRequestId = entity.ServiceRequestId,
                            ToolId = toolDto.ToolId,
                            QuantityRequired = requiredQty,
                            CreatedAt = DateTime.UtcNow,
                            CreatedBy = dto.UpdatedBy ?? 0
                        };

                        tenantDb.WorkOrderRequiredTools.Add(newTool);
                    }
                }
            }

            await tenantDb.SaveChangesAsync();

                await _auditLogger.LogAuditAsync("Update", $"Updated Service Request: {entity.RequestNumber}", dto.TenantId, "", "UpdateServiceRequestAsync");

                return new CommonResponseModel { StatusCode = StatusCode.Success, StatusMessage = ServiceRequestStatusMessage.ServiceRequestStatusUpdated };
            }
            
        public async Task<CommonResponseModel> DeleteServiceRequestAsync(int serviceRequestId, int tenantId)
        {
           
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                var entity = await tenantDb.ServiceRequests
                    .FirstOrDefaultAsync(sr => sr.ServiceRequestId == serviceRequestId && !sr.IsDeleted);

                if (entity == null)
                    return new CommonResponseModel { StatusCode = StatusCode.NotFound, StatusMessage = ServiceRequestStatusMessage.ServiceRequestNotFound };

                entity.IsDeleted = true;
                entity.IsActive = false;
                entity.DeletedAt = DateTime.UtcNow;
                entity.DeletedBy = tenantId;

                await tenantDb.SaveChangesAsync();

                await _auditLogger.LogAuditAsync("Delete", $"Deleted Service Request: {entity.RequestNumber}", tenantId, "", "DeleteServiceRequestAsync");

                return new CommonResponseModel { StatusCode = StatusCode.Success, StatusMessage = ServiceRequestStatusMessage.ServiceRequestDeleted };
            }
           
        public async Task<CommonResponseModel> UpdateServiceRequestStatusAsync(ServiceRequestStatusUpdateDto dto)
        {
              using var tenantDb = _tenantDbContext.GetTenantDbContext(dto.TenantId);

                var entity = await tenantDb.ServiceRequests
                    .FirstOrDefaultAsync(sr => sr.ServiceRequestId == dto.ServiceRequestId && !sr.IsDeleted);

                if (entity == null)
                    return new CommonResponseModel { StatusCode = StatusCode.NotFound, StatusMessage = ServiceRequestStatusMessage.ServiceRequestNotFound };

                entity.Status = dto.Status;
                entity.CompletionNotes = dto.CompletionNotes;
                entity.UpdatedAt = DateTime.UtcNow;
                entity.UpdatedBy = dto.UpdatedBy;

                await tenantDb.SaveChangesAsync();

                await _auditLogger.LogAuditAsync("Update", $"Updated Service Request Status: {entity.RequestNumber} to {dto.Status}", dto.TenantId, dto.CompletionNotes, "UpdateServiceRequestStatusAsync");

                return new CommonResponseModel { StatusCode = StatusCode.Success, StatusMessage = $"{ServiceRequestStatusMessage.ServiceRequestStatusUpdated} {dto.Status}" };
            }
        public async Task<GetAllRecord<GetServiceRequestDto>> GetAllServiceRequestsAsync(int tenantId)
        {
            string baseUrl = _configuration["BaseUrl:Staging"] ?? "https://ms.stagingsdei.com:8125";
            using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);
                var AssetName = await tenantDb.AssetRegistry
                    .Where(a => a.TenantId == tenantId)
                    .Select(a => new { a.AssetId, a.AssetName })
                    .ToListAsync();
                
            var serviceRequests = await tenantDb.ServiceRequests
                    .Where(sr => sr.TenantId == tenantId && !sr.IsDeleted)
                    .Include(sr => sr.AssignedUser)
                    .Include(sr => sr.AssignedTeam)
                    .Include(sr => sr.Location)
                    .Include(sr => sr.Asset) 
                    .OrderByDescending(sr => sr.CreatedAt)
                    .ToListAsync();

            var requiredTools = await tenantDb.WorkOrderRequiredTools
                    .Where(x =>
                        !x.IsDeleted &&
                        x.ServiceRequestId != null)
                    .Include(x => x.Tool)
                    .ToListAsync();

            var dtoList = serviceRequests.Select(sr => new GetServiceRequestDto
                {
                    ServiceRequestId = sr.ServiceRequestId,
                    WorkOrderId = sr.WorkOrderId,
                    TenantId = sr.TenantId,
                    AssetId = sr.AssetId,
                    AssetName = sr.Asset != null ? sr.Asset.AssetName : "N/A",
                    RequestNumber = sr.RequestNumber,
                    Title = sr.Title,
                    Description = sr.Description,
                    LocationId = sr.LocationId,
                    LocationName = sr.Location?.LocationName ?? "N/A",
                    Status = sr.Status,
                    StatusDisplay = sr.Status.ToString(),
                    Priority = sr.Priority,
                    PriorityDisplay = sr.Priority.ToString(),
                    AssignedToUserId = sr.AssignedToUserId,
                    ServiceRequestMedia = sr.ServiceRequestMedia,
                    ServiceRequestMediaPath = !string.IsNullOrEmpty(sr.ServiceRequestMediaPath)
                                 ? $"{baseUrl}/{sr.ServiceRequestMediaPath}"
                                 : null,
                    FileType = sr.FileType,
                    AssignedUserName = sr.AssignedUser != null ? $"{sr.AssignedUser.FirstName} {sr.AssignedUser.LastName}" : "Not Assigned",
                    AssignedToTeamId = sr.AssignedToTeamId,
                    AssignedTeamName = sr.AssignedTeam?.Name ?? "Not Assigned",
                    DueDate = ToUtc(sr.DueDate),
                    ScheduleDate = ToUtc(sr.ScheduleDate),
                    EstimatedDurationMinutes = sr.EstimatedDurationMinutes,
                    RequestType = sr.RequestType,
                    RequestTypeDisplay = sr.RequestType.ToString(),
                    Instructions = sr.Instructions!,
                    CompletionNotes = sr.CompletionNotes!,
                    LaborCost = sr.LaborCost,
                    PartCost = sr.PartCost,
                    TotalCost = sr.TotalCost,
                    IsActive = sr.IsActive,
                    DueDateFormatted = ToUtc(sr.DueDate)?.ToString("yyyy-MM-dd") ?? "Not set",
                    ScheduleDateFormatted = ToUtc(sr.ScheduleDate)?.ToString("yyyy-MM-dd") ?? "Not scheduled",
                    /* DueDateFormatted = sr.DueDate?.ToString("yyyy-MM-dd") ?? "Not set",
                     ScheduleDateFormatted = sr.ScheduleDate?.ToString("yyyy-MM-dd") ?? "Not scheduled",*/
                    DaysUntilDue = GetDaysUntilDue(sr.DueDate),
                        CreatedBy = sr.CreatedBy,
                        CreatedAt = sr.CreatedAt,
                RequiredTools = requiredTools
                    .Where(x => x.ServiceRequestId == sr.ServiceRequestId)
                    .Select(x => new RequiredToolDto
                    {
                        ToolId = x.Id,
                        ToolName = x.Tool != null
                            ? x.Tool.ItemName
                            : "N/A",
                        QuantityRequired = x.QuantityRequired
                    })
                    .ToList(),
            }).ToList();

                return new GetAllRecord<GetServiceRequestDto>
                {
                    StatusCode = StatusCode.Success,
                    StatusMessage = ServiceRequestStatusMessage.ServiceRequestFetched,
                    GetAllData = dtoList
                };
            }

        public async Task<GetSpecificRecord<GetServiceRequestDto>> GetServiceRequestByIdAsync(int serviceRequestId, int tenantId)
        {
            string baseUrl = _configuration["BaseUrl:Staging"] ?? "https://ms.stagingsdei.com:8125";
            using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                var entity = await tenantDb.ServiceRequests
                    .Include(sr => sr.AssignedUser)
                    .Include(sr => sr.AssignedTeam)
                    .Include(sr => sr.Location)
                    .Include(sr => sr.Asset) 
                    .FirstOrDefaultAsync(sr => sr.ServiceRequestId == serviceRequestId && !sr.IsDeleted);

                if (entity == null)
                    return new GetSpecificRecord<GetServiceRequestDto>
                    {
                        StatusCode = StatusCode.NotFound,
                        StatusMessage = ServiceRequestStatusMessage.ServiceRequestNotFound
                    };
                var requiredTools = await tenantDb.WorkOrderRequiredTools
                 .Where(x =>
                     !x.IsDeleted &&
                     x.ServiceRequestId != null)
                 .Include(x => x.Tool)
                 .ToListAsync();
            var dto = new GetServiceRequestDto
                {
                    ServiceRequestId = entity.ServiceRequestId,
                    TenantId = entity.TenantId,
                    RequestNumber = entity.RequestNumber,
                    AssetId = entity.AssetId,
                    AssetName = entity.Asset != null ? entity.Asset.AssetName : "N/A",
                    Title = entity.Title,
                    Description = entity.Description,
                    LocationId = entity.LocationId,
                    LocationName = entity.Location?.LocationName ?? "N/A",
                    Status = entity.Status,
                    StatusDisplay = entity.Status.ToString(),
                    ServiceRequestMedia = entity.ServiceRequestMedia,
                    ServiceRequestMediaPath = !string.IsNullOrEmpty(entity.ServiceRequestMediaPath)
                                 ? $"{baseUrl}/{entity.ServiceRequestMediaPath}"
                                 : null,
                    FileType = entity.FileType,
                    Priority = entity.Priority,
                    PriorityDisplay = entity.Priority.ToString(),
                    AssignedToUserId = entity.AssignedToUserId,
                    AssignedUserName = entity.AssignedUser != null ? $"{entity.AssignedUser.FirstName} {entity.AssignedUser.LastName}" : "Not Assigned",
                    AssignedToTeamId = entity.AssignedToTeamId,
                    AssignedTeamName = entity.AssignedTeam?.Name ?? "Not Assigned",
                    DueDate = ToUtc(entity.DueDate),
                    ScheduleDate = ToUtc(entity.ScheduleDate),
                    EstimatedDurationMinutes = entity.EstimatedDurationMinutes,
                    RequestType = entity.RequestType,
                    RequestTypeDisplay = entity.RequestType.ToString(),
                    Instructions = entity.Instructions!,
                    CompletionNotes = entity.CompletionNotes!,
                    LaborCost = entity.LaborCost,
                    PartCost = entity.PartCost,
                    TotalCost = entity.TotalCost,
                    IsActive = entity.IsActive,
                    DueDateFormatted = ToUtc(entity.DueDate)?.ToString("yyyy-MM-dd") ?? "Not set",
                    ScheduleDateFormatted = ToUtc(entity.ScheduleDate)?.ToString("yyyy-MM-dd") ?? "Not scheduled",
                    DaysUntilDue = GetDaysUntilDue(entity.DueDate),
                    CreatedBy = entity.CreatedBy,
                    CreatedAt = entity.CreatedAt,
                    RequiredTools = await tenantDb.WorkOrderRequiredTools
                            .Where(x =>
                                x.ServiceRequestId == entity.ServiceRequestId &&
                                !x.IsDeleted)
                            .Include(x => x.Tool)
                            .Select(x => new RequiredToolDto
                            {
                                ToolId = x.Id,
                                ToolName = x.Tool != null
                                    ? x.Tool.ItemName
                                    : "N/A",
                                QuantityRequired = x.QuantityRequired
                            })
                            .ToListAsync(),
                };

                return new GetSpecificRecord<GetServiceRequestDto>
                {
                    StatusCode = StatusCode.Success,
                    StatusMessage = ServiceRequestStatusMessage.ServiceRequestFetched,
                    Data = dto
                };
            }    

        public async Task<GetAllRecord<GetServiceRequestDto>> GetServiceRequestsByStatusAsync(int tenantId, ServiceRequestStatus status)
        {
            string baseUrl = _configuration["BaseUrl:Staging"] ?? "https://ms.stagingsdei.com:8125";
            using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                var entities = await tenantDb.ServiceRequests
                    .Where(sr => sr.TenantId == tenantId && sr.Status == status && !sr.IsDeleted)
                    .Include(sr => sr.AssignedUser)
                    .Include(sr => sr.AssignedTeam)
                    .Include(sr => sr.Location)
                    .OrderByDescending(sr => sr.CreatedAt)
                    .ToListAsync();

               var requiredTools = await tenantDb.WorkOrderRequiredTools
                    .Where(x =>
                        !x.IsDeleted &&
                        x.ServiceRequestId != null)
                    .Include(x => x.Tool)
                    .ToListAsync();

            var dtoList = entities.Select(sr => new GetServiceRequestDto
                {
                    ServiceRequestId = sr.ServiceRequestId,
                    TenantId = sr.TenantId,
                    RequestNumber = sr.RequestNumber,
                    AssetId =   sr.AssetId,
                    ServiceRequestMedia = sr.ServiceRequestMedia,
                    ServiceRequestMediaPath = !string.IsNullOrEmpty(sr.ServiceRequestMediaPath)
                                 ? $"{baseUrl}/{sr.ServiceRequestMediaPath}"
                                 : null,
                    FileType = sr.FileType,
                    Title = sr.Title,
                    Description = sr.Description,
                    LocationId = sr.LocationId,
                    LocationName = sr.Location?.LocationName ?? "N/A",
                    Status = sr.Status,
                    StatusDisplay = sr.Status.ToString(),
                    Priority = sr.Priority,
                    PriorityDisplay = sr.Priority.ToString(),
                    AssignedToUserId = sr.AssignedToUserId,
                    AssignedUserName = sr.AssignedUser != null ? $"{sr.AssignedUser.FirstName} {sr.AssignedUser.LastName}" : "Not Assigned",
                    AssignedToTeamId = sr.AssignedToTeamId,
                    AssignedTeamName = sr.AssignedTeam?.Name ?? "Not Assigned",
                    DueDate = ToUtc(sr.DueDate),
                    ScheduleDate = ToUtc(sr.ScheduleDate),
                    EstimatedDurationMinutes = sr.EstimatedDurationMinutes,
                    RequestType = sr.RequestType,
                    RequestTypeDisplay = sr.RequestType.ToString(),
                    Instructions = sr.Instructions!,
                    CompletionNotes = sr.CompletionNotes!,
                    LaborCost = sr.LaborCost,
                    PartCost = sr.PartCost,
                    TotalCost = sr.TotalCost,
                    IsActive = sr.IsActive,
                    DueDateFormatted = ToUtc(sr.DueDate)?.ToString("yyyy-MM-dd") ?? "Not set",
                    ScheduleDateFormatted = ToUtc(sr.ScheduleDate)?.ToString("yyyy-MM-dd") ?? "Not scheduled",
                    DaysUntilDue = GetDaysUntilDue(sr.DueDate),
                    RequiredTools = requiredTools
                    .Where(x => x.ServiceRequestId == sr.ServiceRequestId)
                    .Select(x => new RequiredToolDto
                    {
                        ToolId = x.ToolId,
                        ToolName = x.Tool != null
                            ? x.Tool.ItemName
                            : "N/A",
                        QuantityRequired = x.QuantityRequired
                    })
                    .ToList(),

            }).ToList();

                return new GetAllRecord<GetServiceRequestDto>
                {
                    StatusCode = StatusCode.Success,
                    StatusMessage = $"{ServiceRequestStatusMessage.ServiceRequestsByStatusFetched} {status} {ServiceRequestStatusMessage.ServiceRequestsByStatusFetched2}",
                    GetAllData = dtoList
                };
            }

        public async Task<GetAllRecord<GetServiceRequestDto>> GetOverdueServiceRequestsAsync(int tenantId)
        {
            using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);
            string baseUrl = _configuration["BaseUrl:Staging"] ?? "https://ms.stagingsdei.com:8125";
            var entities = await tenantDb.ServiceRequests
                    .Where(sr => sr.TenantId == tenantId &&
                                !sr.IsDeleted &&
                                sr.DueDate.HasValue &&
                                sr.DueDate < DateTime.UtcNow &&
                                sr.Status != ServiceRequestStatus.Completed )//&&
                              //  sr.Status != ServiceRequestStatus.Cancelled)
                    .Include(sr => sr.AssignedUser)
                    .Include(sr => sr.AssignedTeam)
                    .Include(sr => sr.Location)
                    .OrderBy(sr => sr.DueDate)
                    .ToListAsync();

            var requiredTools = await tenantDb.WorkOrderRequiredTools
                    .Where(x =>
                        !x.IsDeleted &&
                        x.ServiceRequestId != null)
                    .Include(x => x.Tool)
                    .ToListAsync();

            var dtoList = entities.Select(sr => new GetServiceRequestDto
                {
                    ServiceRequestId = sr.ServiceRequestId,
                    TenantId = sr.TenantId,
                    AssetId = sr.AssetId,
                    RequestNumber = sr.RequestNumber,
                    Title = sr.Title,
                    Description = sr.Description,
                    LocationId = sr.LocationId,
                    LocationName = sr.Location?.LocationName ?? "N/A",
                    Status = sr.Status,
                    ServiceRequestMedia = sr.ServiceRequestMedia,
                    ServiceRequestMediaPath = !string.IsNullOrEmpty(sr.ServiceRequestMediaPath)
                                 ? $"{baseUrl}/{sr.ServiceRequestMediaPath}"
                                 : null,
                    FileType = sr.FileType,
                    StatusDisplay = sr.Status.ToString(),
                    Priority = sr.Priority,
                    PriorityDisplay = sr.Priority.ToString(),
                    AssignedToUserId = sr.AssignedToUserId,
                    AssignedUserName = sr.AssignedUser != null ? $"{sr.AssignedUser.FirstName} {sr.AssignedUser.LastName}" : "Not Assigned",
                    AssignedToTeamId = sr.AssignedToTeamId,
                    AssignedTeamName = sr.AssignedTeam?.Name ?? "Not Assigned",
                    DueDate = ToUtc(sr.DueDate),
                    ScheduleDate = ToUtc(sr.ScheduleDate),
                    EstimatedDurationMinutes = sr.EstimatedDurationMinutes,
                    RequestType = sr.RequestType,
                    RequestTypeDisplay = sr.RequestType.ToString(),
                    Instructions = sr.Instructions!,
                    CompletionNotes = sr.CompletionNotes!,
                    LaborCost = sr.LaborCost,
                    PartCost = sr.PartCost,
                    TotalCost = sr.TotalCost,
                    IsActive = sr.IsActive,
                    DueDateFormatted = ToUtc(sr.DueDate)?.ToString("yyyy-MM-dd") ?? "Not set",
                    ScheduleDateFormatted = ToUtc(sr.ScheduleDate)?.ToString("yyyy-MM-dd") ?? "Not scheduled",
                    DaysUntilDue = GetDaysUntilDue(sr.DueDate),
                    RequiredTools = requiredTools
                        .Where(x => x.ServiceRequestId == sr.ServiceRequestId)
                        .Select(x => new RequiredToolDto
                        {
                            ToolId = x.ToolId,
                            ToolName = x.Tool != null
                                ? x.Tool.ItemName
                                : "N/A",
                            QuantityRequired = x.QuantityRequired
                        })
                        .ToList(),
            }).ToList();

                return new GetAllRecord<GetServiceRequestDto>
                {
                    StatusCode = StatusCode.Success,
                    StatusMessage = ServiceRequestStatusMessage.OverdueServiceRequestsFetched,
                    GetAllData = dtoList
                };
            }



        //Private helper method
        private DateTime? ToUtc(DateTime? date)
        {
            if (!date.HasValue) return null;

            return date.Value.Kind == DateTimeKind.Utc
                ? date
                : DateTime.SpecifyKind(date.Value, DateTimeKind.Utc).ToUniversalTime();
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
        private string GetDaysUntilDue(DateTime? dueDate)
        {
            if (!dueDate.HasValue) return "No due date";

            var timeSpan = dueDate.Value - DateTime.UtcNow;
            if (timeSpan.TotalDays < 0) return $"Overdue by {(int)-timeSpan.TotalDays} days";
            if (timeSpan.TotalDays < 1) return "Due today";

            return $"{(int)timeSpan.TotalDays} days remaining";
        }
        private string GenerateWorkOrderNumber()
        {
            var datePart = DateTime.UtcNow.ToString("yyMMdd");
            var randomSuffix = Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();
            return $"WO{datePart}{randomSuffix}";
        }
        private async Task PublishEvent(int tenantId, string correlationId, string eventType, object payload)
        {
            string topicName = KafkaCommonTopics.BuildTopicName(tenantId, eventType);

            var kafkaRequest = new
            {
                Topic = topicName,
                Key = $"servicerequest-{Guid.NewGuid()}",
                Payload = payload,
                Source = "ServiceRequestService",
                Headers = new Dictionary<string, string>
                {
                    ["tenant-id"] = tenantId.ToString(),
                    ["correlation-id"] = correlationId
                }
            };

            using var httpClient = new HttpClient();
            var content = new StringContent(
                JsonSerializer.Serialize(kafkaRequest),
                Encoding.UTF8,
                "application/json");

            var response = await httpClient.PostAsync(ConstantUrls.kafkaPublish, content);
            response.EnsureSuccessStatusCode();
        }
        private async Task<WorkOrder> CreateWorkOrderFromServiceRequest(FactoryOpsDBContext db, ServiceRequest sr, int approvedBy)
        {
            var workOrderNumber = GenerateWorkOrderNumber();

            var wo = new WorkOrder
            {
                TenantId = sr.TenantId,
                WorkOrderNumber = workOrderNumber,
                Title = sr.Title,
                Description = sr.Description,
                LocationId = sr.LocationId,
                Priority = (PriorityLevel?)sr.Priority,
                WorkOrderType = (WorkOrderTypeEnum?)sr.RequestType,
                AssetId = sr.AssetId,
                ServiceRequestId = sr.ServiceRequestId,
                LaborCost = sr.LaborCost,
                PartCost = sr.PartCost,
                TotalCost = sr.TotalCost,
                EstimatedDurationMinutes = sr.EstimatedDurationMinutes,
                DueDate = sr.DueDate?.ToUniversalTime(),
                ScheduleDate = sr.ScheduleDate?.ToUniversalTime(),
                WorkOrderMedia = sr.ServiceRequestMedia,
                WorkOrderMediaPath = sr.ServiceRequestMediaPath,
                FileType = sr.FileType,
                // AssignedToUserId = sr.AssignedToUserId,
                Status = WorkOrderStatus.Open,
                CreatedBy = approvedBy,
                CreatedAt = DateTime.UtcNow
            };

            await db.WorkOrders.AddAsync(wo);
            await db.SaveChangesAsync();

            return wo;
        }
        private async Task PublishWorkOrderEvent(WorkOrder wo, string correlationId)
        {
            var eventDto = new
            {
                EventType = "ServiceRequestApproved",
                NotificationType = "ServiceRequestApproved",
                EventTime = DateTime.UtcNow,
                WorkOrderId = wo.WorkOrderId,
                workorderType = wo.WorkOrderType,
                ServiceRequestType = wo.WorkOrderType.ToString(),
                TenantId = wo.TenantId,
                WorkOrderNumber = wo.WorkOrderNumber,
                Title = wo.Title,
                Description = wo.Description,
                ServiceRequestId = wo.ServiceRequestId,
                Priority = wo.Priority?.ToString(),
                Status = wo.Status.ToString(),
                LocationId = wo.LocationId,
                CreatedBy = wo.CreatedBy,
                AssignedToUserId = wo.AssignedToUserId,
            };

            string topicName = KafkaCommonTopics.BuildTopicName(wo.TenantId, "ServiceRequestApproved");

            var kafkaRequest = new
            {
                Topic = topicName,
                Key = $"workorder-{wo.WorkOrderId}",
                Payload = eventDto,
                Source = "ServiceRequestService",
                Headers = new Dictionary<string, string>
                {
                    ["tenant-id"] = wo.TenantId.ToString(),
                    ["correlation-id"] = correlationId
                }
            };

            using var httpClient = new HttpClient();

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(kafkaRequest),
                Encoding.UTF8,
                "application/json");

            var response = await httpClient.PostAsync(ConstantUrls.kafkaPublish, jsonContent);
            response.EnsureSuccessStatusCode();
        }
        private string GenerateRequestNumber()
        {
            var datePart = DateTime.UtcNow.ToString("yyMMdd");
            var randomSuffix = Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();
            return $"SR{datePart}{randomSuffix}";
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

    }
}
