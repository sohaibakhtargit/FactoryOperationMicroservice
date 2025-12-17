using FactoryOperation_WorkOrder.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.WorkOrderManagement;
using FactoryOperation_WorkOrder.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.AuditLogs;
using FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Domain.Entities.FactoryOpsTenants;
using FactoryOpsApp.Infrastructure.DBContext;
using Microsoft.EntityFrameworkCore;
using static FactoryOpsApp.Common.CommonConstant;

namespace FactoryOperation_WorkOrder.FactoryOpsApp.Infrastructure.Implementation.Repository.TenantAdmin.WorkOrderManagement
{
    public class ServiceRequestRepository : IServiceRequestRepository
    {
        private readonly TenantDbContextFactory _tenantDbContext;
        private readonly IAuditLogService _auditLogger;

        public ServiceRequestRepository(TenantDbContextFactory tenantDbContext,
                                      IAuditLogService auditLogger
                                     )
        {
            _tenantDbContext = tenantDbContext;
            _auditLogger = auditLogger;
          
        }

        public async Task<CommonResponseModel> CreateServiceRequestAsync(ServiceRequestDto dto)
        {
            
                using var tenantDb = _tenantDbContext.GetTenantDbContext(dto.TenantId);

                var requestNumber = GenerateRequestNumber();

                var entity = new ServiceRequest
                {
                    TenantId = dto.TenantId,
                    RequestNumber = requestNumber,
                    Title = dto.Title,
                    Description = dto.Description,
                    LocationId = dto.LocationId,
                    Status = dto.Status,
                    Priority = dto.Priority,
                    AssignedToUserId = dto.AssignedToUserId,
                    AssignedToTeamId = dto.AssignedToTeamId,
                    DueDate = dto.DueDate,
                    ScheduleDate = dto.ScheduleDate,
                    EstimatedDurationMinutes = dto.EstimatedDurationMinutes,
                    RequestType = dto.RequestType,
                    Instructions = dto.Instructions,
                    LaborCost = dto.LaborCost,
                    PartCost = dto.PartCost,
                    TotalCost = dto.LaborCost + dto.PartCost,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = dto.CreatedBy
                };

                await tenantDb.ServiceRequests.AddAsync(entity);
                await tenantDb.SaveChangesAsync();

                await _auditLogger.LogAuditAsync("Create", $"Created Service Request: {requestNumber} - {dto.Title}", dto.TenantId, "", "CreateServiceRequestAsync");

                return new CommonResponseModel { StatusCode = StatusCode.Success, StatusMessage = ServiceRequestStatusMessage.ServiceRequestCreated };
            }
          

        private string GenerateRequestNumber()
        {
            var datePart = DateTime.UtcNow.ToString("yyMMdd");
            var randomSuffix = Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();
            return $"SR{datePart}{randomSuffix}";
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
                entity.Status = dto.Status;
                entity.Priority = dto.Priority;
                entity.AssignedToUserId = dto.AssignedToUserId;
                entity.AssignedToTeamId = dto.AssignedToTeamId;
                entity.DueDate = dto.DueDate;
                entity.ScheduleDate = dto.ScheduleDate;
                entity.EstimatedDurationMinutes = dto.EstimatedDurationMinutes;
                entity.RequestType = dto.RequestType;
                entity.Instructions = dto.Instructions;
                entity.CompletionNotes = dto.CompletionNotes;
                entity.LaborCost = dto.LaborCost;
                entity.PartCost = dto.PartCost;
                entity.TotalCost = dto.LaborCost + dto.PartCost;
                entity.UpdatedAt = DateTime.UtcNow;
                entity.UpdatedBy = dto.UpdatedBy;

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
           
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                var serviceRequests = await tenantDb.ServiceRequests
                    .Where(sr => sr.TenantId == tenantId && !sr.IsDeleted)
                    .Include(sr => sr.AssignedUser)
                    .Include(sr => sr.AssignedTeam)
                    .Include(sr => sr.Location)
                    .OrderByDescending(sr => sr.CreatedAt)
                    .ToListAsync();

                var dtoList = serviceRequests.Select(sr => new GetServiceRequestDto
                {
                    ServiceRequestId = sr.ServiceRequestId,
                    TenantId = sr.TenantId,
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
                    AssignedUserName = sr.AssignedUser != null ? $"{sr.AssignedUser.FirstName} {sr.AssignedUser.LastName}" : "Not Assigned",
                    AssignedToTeamId = sr.AssignedToTeamId,
                    AssignedTeamName = sr.AssignedTeam?.Name ?? "Not Assigned",
                    DueDate = sr.DueDate,
                    ScheduleDate = sr.ScheduleDate,
                    EstimatedDurationMinutes = sr.EstimatedDurationMinutes,
                    RequestType = sr.RequestType,
                    RequestTypeDisplay = sr.RequestType.ToString(),
                    Instructions = sr.Instructions,
                    CompletionNotes = sr.CompletionNotes,
                    LaborCost = sr.LaborCost,
                    PartCost = sr.PartCost,
                    TotalCost = sr.TotalCost,
                    IsActive = sr.IsActive,
                    DueDateFormatted = sr.DueDate?.ToString("yyyy-MM-dd") ?? "Not set",
                    ScheduleDateFormatted = sr.ScheduleDate?.ToString("yyyy-MM-dd") ?? "Not scheduled",
                    DaysUntilDue = GetDaysUntilDue(sr.DueDate)
                }).ToList();

                return new GetAllRecord<GetServiceRequestDto>
                {
                    StatusCode = StatusCode.Success,
                    StatusMessage = ServiceRequestStatusMessage.ServiceRequestFetched,
                    GetAllData = dtoList
                };
            }

        private string GetDaysUntilDue(DateTime? dueDate)
        {
            if (!dueDate.HasValue) return "No due date";

            var timeSpan = dueDate.Value - DateTime.UtcNow;
            if (timeSpan.TotalDays < 0) return $"Overdue by {(int)-timeSpan.TotalDays} days";
            if (timeSpan.TotalDays < 1) return "Due today";

            return $"{(int)timeSpan.TotalDays} days remaining";
        }


        public async Task<GetSpecificRecord<GetServiceRequestDto>> GetServiceRequestByIdAsync(int serviceRequestId, int tenantId)
        {
            
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                var entity = await tenantDb.ServiceRequests
                    .Include(sr => sr.AssignedUser)
                    .Include(sr => sr.AssignedTeam)
                    .Include(sr => sr.Location)
                    .FirstOrDefaultAsync(sr => sr.ServiceRequestId == serviceRequestId && !sr.IsDeleted);

                if (entity == null)
                    return new GetSpecificRecord<GetServiceRequestDto>
                    {
                        StatusCode = StatusCode.NotFound,
                        StatusMessage = ServiceRequestStatusMessage.ServiceRequestNotFound
                    };

                var dto = new GetServiceRequestDto
                {
                    ServiceRequestId = entity.ServiceRequestId,
                    TenantId = entity.TenantId,
                    RequestNumber = entity.RequestNumber,
                    Title = entity.Title,
                    Description = entity.Description,
                    LocationId = entity.LocationId,
                    LocationName = entity.Location?.LocationName ?? "N/A",
                    Status = entity.Status,
                    StatusDisplay = entity.Status.ToString(),
                    Priority = entity.Priority,
                    PriorityDisplay = entity.Priority.ToString(),
                    AssignedToUserId = entity.AssignedToUserId,
                    AssignedUserName = entity.AssignedUser != null ? $"{entity.AssignedUser.FirstName} {entity.AssignedUser.LastName}" : "Not Assigned",
                    AssignedToTeamId = entity.AssignedToTeamId,
                    AssignedTeamName = entity.AssignedTeam?.Name ?? "Not Assigned",
                    DueDate = entity.DueDate,
                    ScheduleDate = entity.ScheduleDate,
                    EstimatedDurationMinutes = entity.EstimatedDurationMinutes,
                    RequestType = entity.RequestType,
                    RequestTypeDisplay = entity.RequestType.ToString(),
                    Instructions = entity.Instructions,
                    CompletionNotes = entity.CompletionNotes,
                    LaborCost = entity.LaborCost,
                    PartCost = entity.PartCost,
                    TotalCost = entity.TotalCost,
                    IsActive = entity.IsActive,
                    DueDateFormatted = entity.DueDate?.ToString("yyyy-MM-dd") ?? "Not set",
                    ScheduleDateFormatted = entity.ScheduleDate?.ToString("yyyy-MM-dd") ?? "Not scheduled",
                    DaysUntilDue = GetDaysUntilDue(entity.DueDate)
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
           
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                var entities = await tenantDb.ServiceRequests
                    .Where(sr => sr.TenantId == tenantId && sr.Status == status && !sr.IsDeleted)
                    .Include(sr => sr.AssignedUser)
                    .Include(sr => sr.AssignedTeam)
                    .Include(sr => sr.Location)
                    .OrderByDescending(sr => sr.CreatedAt)
                    .ToListAsync();

                var dtoList = entities.Select(sr => new GetServiceRequestDto
                {
                    ServiceRequestId = sr.ServiceRequestId,
                    TenantId = sr.TenantId,
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
                    AssignedUserName = sr.AssignedUser != null ? $"{sr.AssignedUser.FirstName} {sr.AssignedUser.LastName}" : "Not Assigned",
                    AssignedToTeamId = sr.AssignedToTeamId,
                    AssignedTeamName = sr.AssignedTeam?.Name ?? "Not Assigned",
                    DueDate = sr.DueDate,
                    ScheduleDate = sr.ScheduleDate,
                    EstimatedDurationMinutes = sr.EstimatedDurationMinutes,
                    RequestType = sr.RequestType,
                    RequestTypeDisplay = sr.RequestType.ToString(),
                    Instructions = sr.Instructions,
                    CompletionNotes = sr.CompletionNotes,
                    LaborCost = sr.LaborCost,
                    PartCost = sr.PartCost,
                    TotalCost = sr.TotalCost,
                    IsActive = sr.IsActive,
                    DueDateFormatted = sr.DueDate?.ToString("yyyy-MM-dd") ?? "Not set",
                    ScheduleDateFormatted = sr.ScheduleDate?.ToString("yyyy-MM-dd") ?? "Not scheduled",
                    DaysUntilDue = GetDaysUntilDue(sr.DueDate)
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

                var entities = await tenantDb.ServiceRequests
                    .Where(sr => sr.TenantId == tenantId &&
                                !sr.IsDeleted &&
                                sr.DueDate.HasValue &&
                                sr.DueDate < DateTime.UtcNow &&
                                sr.Status != ServiceRequestStatus.Completed &&
                                sr.Status != ServiceRequestStatus.Cancelled)
                    .Include(sr => sr.AssignedUser)
                    .Include(sr => sr.AssignedTeam)
                    .Include(sr => sr.Location)
                    .OrderBy(sr => sr.DueDate)
                    .ToListAsync();

                var dtoList = entities.Select(sr => new GetServiceRequestDto
                {
                    ServiceRequestId = sr.ServiceRequestId,
                    TenantId = sr.TenantId,
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
                    AssignedUserName = sr.AssignedUser != null ? $"{sr.AssignedUser.FirstName} {sr.AssignedUser.LastName}" : "Not Assigned",
                    AssignedToTeamId = sr.AssignedToTeamId,
                    AssignedTeamName = sr.AssignedTeam?.Name ?? "Not Assigned",
                    DueDate = sr.DueDate,
                    ScheduleDate = sr.ScheduleDate,
                    EstimatedDurationMinutes = sr.EstimatedDurationMinutes,
                    RequestType = sr.RequestType,
                    RequestTypeDisplay = sr.RequestType.ToString(),
                    Instructions = sr.Instructions,
                    CompletionNotes = sr.CompletionNotes,
                    LaborCost = sr.LaborCost,
                    PartCost = sr.PartCost,
                    TotalCost = sr.TotalCost,
                    IsActive = sr.IsActive,
                    DueDateFormatted = sr.DueDate?.ToString("yyyy-MM-dd") ?? "Not set",
                    ScheduleDateFormatted = sr.ScheduleDate?.ToString("yyyy-MM-dd") ?? "Not scheduled",
                    DaysUntilDue = GetDaysUntilDue(sr.DueDate)
                }).ToList();

                return new GetAllRecord<GetServiceRequestDto>
                {
                    StatusCode = StatusCode.Success,
                    StatusMessage = ServiceRequestStatusMessage.OverdueServiceRequestsFetched,
                    GetAllData = dtoList
                };
            }
          
    }
}
