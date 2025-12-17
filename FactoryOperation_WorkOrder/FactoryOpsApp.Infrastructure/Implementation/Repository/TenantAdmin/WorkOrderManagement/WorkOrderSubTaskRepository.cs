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
    public class WorkOrderSubTaskRepository : IWorkOrderSubTaskRepository
    {
        private readonly TenantDbContextFactory _tenantDbContext;
        private readonly IAuditLogService _auditLogger;

        public WorkOrderSubTaskRepository(
            TenantDbContextFactory tenantDbContext,
            IExceptionLoggerService exceptionLogger,
            IAuditLogService auditLogger)
        {
            _tenantDbContext = tenantDbContext;
            _auditLogger = auditLogger;
        }


        public async Task<GetAllRecord<WorkOrderWithSubTasksDto>> GetAllWorkOrderSubTaskAsync(int tenantId)
        {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                var data = await (from w in tenantDb.WorkOrders
                                  where w.TenantId == tenantId && w.IsActive && !w.IsDeleted
                                  select new WorkOrderWithSubTasksDto
                                  {
                                      AssignedToUserId = w.AssignedToUserId,
                                      WorkOrderId = w.WorkOrderId,
                                      WorkOrderNumber = w.WorkOrderNumber,
                                      WorkOrderTitle = w.Title,
                                      WorkOrderStatus = w.Status.ToString(),
                                      WorkOrderPriority = w.Priority.ToString(),

                                      EstimatedDurationInMinutes = w.EstimatedDurationMinutes,

                                      SubTasks = w.WorkOrderSubTasks
                            .Where(s => s.IsActive)
                            .Select(s => new WorkOrderSubTaskDto
                            {
                                SubTaskId = s.SubTaskId,
                                WorkOrderId = s.WorkOrderId,
                                ParentTaskId = s.ParentTaskId,
                                Title = s.Title,
                                Description = s.Description,
                                Priority = s.Priority,
                                Status = s.Status,
                                EstimatedMinutes = s.EstimatedMinutes,
                                ActualMinutes = s.ActualMinutes,
                                AssignedToUserId = s.AssignedToUserId,
                                AssignedToUserName = s.AssignedToUser != null
                                    ? s.AssignedToUser.FirstName + " " + s.AssignedToUser.LastName
                                    : null,
                                AssignedToTeamId = s.AssignedToTeamId,
                                AssignedToTeamName = s.AssignedToTeam != null
                                    ? s.AssignedToTeam.Name
                                    : null,
                                Sequence = s.Sequence,
                                StartDate = s.StartDate,
                                EndDate = s.EndDate,
                                CompletedDate = s.CompletedDate,
                                TenantId = s.TenantId,
                                IsActive = s.IsActive,
                                CreatedAt = s.CreatedAt,
                                UpdatedAt = s.UpdatedAt
                            })
                            .ToList()
                                  })
                    .ToListAsync();

                return new GetAllRecord<WorkOrderWithSubTasksDto>
                {
                    StatusCode = StatusCode.Success,
                    StatusMessage = WorkOrderSubTaskStatusMessage.WorkOrderSubTasksFetched,
                    GetAllData = data
                };
            }
            


        public async Task<GetSpecificRecord<WorkOrderSubTaskDto>> GetWorkOrderSubTaskByIdAsync(int tenantId, int subTaskId)
        {
            
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);
                var subTask = await tenantDb.WorkOrderSubTasks
                    .Where(s => s.TenantId == tenantId && s.SubTaskId == subTaskId && s.IsActive)
                    .Include(s => s.AssignedToUser)
                    .Include(s => s.AssignedToTeam)
                    .Select(s => new WorkOrderSubTaskDto
                    {
                        SubTaskId = s.SubTaskId,
                        WorkOrderId = s.WorkOrderId,
                        ParentTaskId = s.ParentTaskId,
                        Title = s.Title,
                        Description = s.Description,
                        Priority = s.Priority,
                        Status = s.Status,
                        EstimatedMinutes = s.EstimatedMinutes,
                        ActualMinutes = s.ActualMinutes,
                        AssignedToUserId = s.AssignedToUserId,
                        AssignedToUserName = s.AssignedToUser != null ? s.AssignedToUser.FirstName + " " + s.AssignedToUser.LastName : null,
                        AssignedToTeamId = s.AssignedToTeamId,
                        AssignedToTeamName = s.AssignedToTeam != null ? s.AssignedToTeam.Name : null,
                        Sequence = s.Sequence,
                        StartDate = s.StartDate,
                        EndDate = s.EndDate,
                        CompletedDate = s.CompletedDate,
                        TenantId = s.TenantId,
                        IsActive = s.IsActive,
                        CreatedAt = s.CreatedAt,
                        UpdatedAt = s.UpdatedAt
                    })
                    .FirstOrDefaultAsync();

                if (subTask == null)
                {
                    return new GetSpecificRecord<WorkOrderSubTaskDto>
                    {
                        StatusCode = StatusCode.NotFound,
                        StatusMessage = WorkOrderSubTaskStatusMessage.WorkOrderNotFound
                    };
                }

                return new GetSpecificRecord<WorkOrderSubTaskDto>
                {
                    StatusCode = StatusCode.Success,
                    StatusMessage = WorkOrderSubTaskStatusMessage.WorkOrderSubTasksFetched,
                    Data = subTask
                };
            }
          
        public async Task<CommonResponseModel> AddWorkOrderSubTaskAsync(CreateWorkOrderSubTaskDto dto)
        {
            var response = new CommonResponseModel();

            
                using var tenantDb = _tenantDbContext.GetTenantDbContext(dto.TenantId);

                // ✅ Ensure WorkOrder exists
                var workOrderExists = await tenantDb.WorkOrders
                    .AnyAsync(w => w.WorkOrderId == dto.WorkOrderId && w.TenantId == dto.TenantId && !w.IsDeleted);

                if (!workOrderExists)
                {
                    response.StatusCode = StatusCode.NotFound;
                    response.StatusMessage = WorkOrderSubTaskStatusMessage.WorkOrderNotFound;
                    return response;
                }

                var entity = new WorkOrderSubTask
                {
                    WorkOrderId = dto.WorkOrderId,
                    //ParentTaskId = dto.ParentTaskId,
                    Title = dto.Title,
                    Description = dto.Description,
                    Priority = dto.Priority,
                    Status = dto.Status,
                    EstimatedMinutes = dto.EstimatedMinutes,
                    ActualMinutes = dto.ActualMinutes,
                    AssignedToUserId = dto.AssignedToUserId,
                    AssignedToTeamId = dto.AssignedToTeamId,
                    Sequence = dto.Sequence,
                    StartDate = NormalizeToUnspecified(dto.StartDate),
                    EndDate = NormalizeToUnspecified(dto.EndDate),
                    CompletedDate = NormalizeToUnspecified(dto.CompletedDate),
                    TenantId = dto.TenantId,
                    IsActive = true,
                    CreatedBy = dto.CreatedBy,
                    UpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                    CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),

                    //UpdatedAt = DateTime.UtcNow
                };

                tenantDb.WorkOrderSubTasks.Add(entity);
                await tenantDb.SaveChangesAsync();

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = WorkOrderSubTaskStatusMessage.WorkOrderSubTaskCreated;
           

            return response;
        }


        public async Task<CommonResponseModel> UpdateWorkOrderSubTaskAsync(UpdateWorkOrderSubTaskDto dto)
        {
            var response = new CommonResponseModel();
           
                using var tenantDb = _tenantDbContext.GetTenantDbContext(dto.TenantId);

                var entity = await tenantDb.WorkOrderSubTasks.FirstOrDefaultAsync(s => s.SubTaskId == dto.SubTaskId && s.TenantId == dto.TenantId && s.IsActive);
                if (entity == null)
                {
                    response.StatusCode = StatusCode.NotFound;
                    response.StatusMessage = WorkOrderSubTaskStatusMessage.SubTaskNotFound;
                    return response;
                }

                entity.Title = dto.Title;
                entity.Description = dto.Description;
                entity.Priority = dto.Priority;
                entity.Status = dto.Status;
                entity.EstimatedMinutes = dto.EstimatedMinutes;
                entity.ActualMinutes = dto.ActualMinutes;
                entity.AssignedToUserId = dto.AssignedToUserId;
                entity.AssignedToTeamId = dto.AssignedToTeamId;
                entity.Sequence = dto.Sequence;
                entity.StartDate = NormalizeToUnspecified(dto.StartDate);
                entity.EndDate = NormalizeToUnspecified(dto.EndDate);
                entity.CompletedDate = NormalizeToUnspecified(dto.CompletedDate);
                entity.UpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

                tenantDb.WorkOrderSubTasks.Update(entity);
                await tenantDb.SaveChangesAsync();

                await _auditLogger.LogAuditAsync("WorkOrderSubTask", "Update", dto.UpdatedBy, dto.TenantId.ToString(), entity.SubTaskId.ToString());

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = WorkOrderSubTaskStatusMessage.SubTaskCreated;
            
            return response;
        }
        private DateTime? NormalizeToUnspecified(DateTime? value)
        {
            if (!value.HasValue)
                return null;

            return DateTime.SpecifyKind(value.Value, DateTimeKind.Unspecified);
        }
        public async Task<CommonResponseModel> DeleteWorkOrderSubTaskAsync(int tenantId, int subTaskId)
        {
            var response = new CommonResponseModel();
          
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                var entity = await tenantDb.WorkOrderSubTasks.FirstOrDefaultAsync(s => s.SubTaskId == subTaskId && s.TenantId == tenantId && s.IsActive);
                if (entity == null)
                {
                    response.StatusCode = StatusCode.NotFound;
                    response.StatusMessage = WorkOrderSubTaskStatusMessage.SubTaskNotFound;
                    return response;
                }

                entity.IsActive = false;
                entity.UpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

                await tenantDb.SaveChangesAsync();

                await _auditLogger.LogAuditAsync("WorkOrderSubTask", "Delete", null, tenantId.ToString(), entity.SubTaskId.ToString());

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = WorkOrderSubTaskStatusMessage.WorkOrderSubTaskDeleted;
           
            return response;
        }
    }
}
