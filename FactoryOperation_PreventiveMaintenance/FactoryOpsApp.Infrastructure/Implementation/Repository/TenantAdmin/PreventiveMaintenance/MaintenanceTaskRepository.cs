using FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.PreventiveMaintenance;
using FactoryOpsApp.Application.Interfaces.Services.SuperAdmin.AuditLogs;
using FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.Common;
using Microsoft.EntityFrameworkCore;
using FactoryOpsApp.Domain.Entities.FactoryOpsTenants;
using FactoryOpsApp.Infrastructure.DBContext;
using static FactoryOpsApp.Common.CommonConstant;
using FactoryOperation_PreventiveMaintenance.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.ExceptionLogger;

namespace FactoryOpsApp.Infrastructure.Repository.TenantAdmin.PreventiveMaintenance
{
    public class MaintenanceTaskRepository : IMaintenanceTaskRepository
    {
        private readonly TenantDbContextFactory _tenantDbContext;
        private readonly IAuditLogService _auditLogger;
        private readonly IExceptionLoggerService _exceptionLogger;
        private readonly IFileStorageService _fileStorageService;

        public MaintenanceTaskRepository(TenantDbContextFactory tenantDbContext,
                                       IAuditLogService auditLogger,
                                       IExceptionLoggerService exceptionLogger,
                                       IFileStorageService fileStorageService)
        {
            _tenantDbContext = tenantDbContext;
            _auditLogger = auditLogger;
            _exceptionLogger = exceptionLogger;
            _fileStorageService= fileStorageService;
        }

        public async Task<CommonResponseModel> AddMaintenanceTaskAsync(MaintenanceTaskDto dto)
        {
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(dto.TenantId);

                var entity = new WorkOrderSubTask
                {
                    TenantId = dto.TenantId,
                    Title = dto.TaskName,
                    WorkOrderId = dto.WorkOrderId,
                    Description = dto.Description,
                    Instructions = dto.Instructions,
                    Sequence = dto.SequenceOrder,
                    EstimatedMinutes = dto.EstimatedTimeMinutes,
                    Status = dto.Status,
                    IsMandatory = dto.IsMandatory,
                    VerificationRequired = dto.VerificationRequired,
                    Notes = dto.Notes,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = dto.CreatedBy
                };

                await tenantDb.WorkOrderSubTasks.AddAsync(entity);
                await tenantDb.SaveChangesAsync();

                await _auditLogger.LogAuditAsync("Create", $"Created Maintenance Task '{entity.Title}'", dto.TenantId, "", "AddMaintenanceTaskAsync");

                return new CommonResponseModel { StatusCode = StatusCode.Success, StatusMessage = MaintenanceTaskStatusMessage.CreateSuccess };
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "MaintenanceTask-Module", "AddMaintenanceTaskAsync", dto.TenantId, null);
                return new CommonResponseModel { StatusCode = StatusCode.Error, StatusMessage = $"{MaintenanceTaskStatusMessage.CreateFailed}: {ex.Message}" };
            }
        }
        public async Task<CommonResponseModel> UpdateMaintenanceTaskAsync(MaintenanceTaskDto dto)
        {
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(dto.TenantId);

                var entity = await tenantDb.WorkOrderSubTasks
                    .FirstOrDefaultAsync(t => t.SubTaskId == dto.TaskId && !t.IsDeleted);

                if (entity == null)
                    return new CommonResponseModel { StatusCode = StatusCode.NotFound, StatusMessage = MaintenanceTaskStatusMessage.NotFound };

                entity.Title = dto.TaskName;
                entity.Description = dto.Description;
                entity.Instructions = dto.Instructions;
                entity.Sequence = dto.SequenceOrder;
                entity.EstimatedMinutes = dto.EstimatedTimeMinutes;
                entity.IsMandatory = dto.IsMandatory;
                entity.VerificationRequired = dto.VerificationRequired;
                entity.Notes = dto.Notes;
                entity.UpdatedAt = DateTime.UtcNow;
                entity.UpdatedBy = dto.UpdatedBy;

                await tenantDb.SaveChangesAsync();
                await _auditLogger.LogAuditAsync("Update", $"Updated Maintenance Task '{entity.Title}'", dto.TenantId, "", "UpdateMaintenanceTaskAsync");

                return new CommonResponseModel { StatusCode = StatusCode.Success, StatusMessage = MaintenanceTaskStatusMessage.UpdateSuccess };
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "MaintenanceTask-Module", "UpdateMaintenanceTaskAsync", dto.TenantId, null);
                return new CommonResponseModel { StatusCode = StatusCode.Error, StatusMessage = $"{MaintenanceTaskStatusMessage.UpdateFailed}: {ex.Message}" };
            }
        }
        public async Task<CommonResponseModel> DeleteMaintenanceTaskAsync(int taskId, int tenantId)
        {
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                var entity = await tenantDb.WorkOrderSubTasks
                    .FirstOrDefaultAsync(t => t.SubTaskId == taskId && !t.IsDeleted);

                if (entity == null)
                    return new CommonResponseModel { StatusCode = StatusCode.NotFound, StatusMessage = MaintenanceTaskStatusMessage.NotFound };

                entity.IsDeleted = true;
                entity.IsActive = false;
                entity.DeletedAt = DateTime.UtcNow;
                entity.DeletedBy = tenantId;

                await tenantDb.SaveChangesAsync();

                await _auditLogger.LogAuditAsync("Delete", $"Deleted Maintenance Task '{entity.Title}'", tenantId, "", "DeleteMaintenanceTaskAsync");

                return new CommonResponseModel { StatusCode = StatusCode.Success, StatusMessage = MaintenanceTaskStatusMessage.DeleteSuccess };
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "MaintenanceTask-Module", "DeleteMaintenanceTaskAsync", tenantId, null);
                return new CommonResponseModel { StatusCode = StatusCode.Error, StatusMessage = $"{MaintenanceTaskStatusMessage.DeleteFailed}: {ex.Message}" };
            }
        }
        public async Task<CommonResponseModel> UpdateTaskStatusAsync(TaskStatusUpdateDto dto)
        {
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(dto.TenantId);

                var entity = await tenantDb.WorkOrderSubTasks
                    .FirstOrDefaultAsync(t => t.SubTaskId == dto.TaskId && !t.IsDeleted);

                if (entity == null)
                    return new CommonResponseModel { StatusCode = StatusCode.NotFound, StatusMessage = MaintenanceTaskStatusMessage.NotFound };

                entity.Status = dto.Status;
                entity.Notes = dto.Notes;
                entity.UpdatedAt = DateTime.UtcNow;
                entity.UpdatedBy = dto.UpdatedBy;

                if (dto.Status == SubTaskStatus.InProgress && entity.StartDate == null)
                {
                    entity.StartDate = DateTime.UtcNow;
                }
                else if ((dto.Status == SubTaskStatus.Completed || dto.Status == SubTaskStatus.Skipped || dto.Status == SubTaskStatus.Failed) && entity.CompletedDate == null)
                {
                    entity.CompletedDate = DateTime.UtcNow;
                    entity.ActualMinutes = dto.ActualTimeMinutes;
                }

                await tenantDb.SaveChangesAsync();

                var action = dto.Status switch
                {
                    SubTaskStatus.InProgress => "Started",
                    SubTaskStatus.Completed => "Completed",
                    SubTaskStatus.Skipped => "Skipped",
                    SubTaskStatus.Failed => "Failed",
                    _ => "Updated"
                };

                await _auditLogger.LogAuditAsync("Update", $"{action} Maintenance Task '{entity.Title}'", dto.TenantId, dto.Notes, "UpdateTaskStatusAsync");

                return new CommonResponseModel { StatusCode = StatusCode.Success, StatusMessage = $"{MaintenanceTaskStatusMessage.UpdateTaskStatusSuccess} - {dto.Status}" };
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "MaintenanceTask-Module", "UpdateTaskStatusAsync", dto.TenantId, null);
                return new CommonResponseModel { StatusCode = StatusCode.Error, StatusMessage = $"{MaintenanceTaskStatusMessage.UpdateTaskStatusFailed}: {ex.Message}" };
            }
        }
        public async Task<CommonResponseModel> VerifyTaskAsync(TaskVerificationDto dto)
        {
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(dto.TenantId);

                var entity = await tenantDb.MaintenanceTasks
                    .FirstOrDefaultAsync(t => t.TaskId == dto.TaskId && !t.IsDeleted && t.VerificationRequired);

                if (entity == null)
                    return new CommonResponseModel { StatusCode = StatusCode.NotFound, StatusMessage = MaintenanceTaskStatusMessage.NotFound };

                if (entity.Status != MaintenanceTaskStatus.Completed)
                    return new CommonResponseModel { StatusCode = StatusCode.BadRequest, StatusMessage = MaintenanceTaskStatusMessage.MustBeCompletedBeforeVerification };

                entity.VerifiedByUserId = dto.VerifiedByUserId;
                entity.VerifiedAt = DateTime.UtcNow;
                entity.Notes = dto.VerificationNotes;
                entity.UpdatedAt = DateTime.UtcNow;

                await tenantDb.SaveChangesAsync();

                await _auditLogger.LogAuditAsync("Verify", $"Verified Maintenance Task '{entity.TaskName}'", dto.TenantId, dto.VerificationNotes, "VerifyTaskAsync");

                return new CommonResponseModel { StatusCode = StatusCode.Success, StatusMessage = MaintenanceTaskStatusMessage.VerifySuccess };
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "MaintenanceTask-Module", "VerifyTaskAsync", dto.TenantId, null);
                return new CommonResponseModel { StatusCode = StatusCode.Error, StatusMessage = $"{MaintenanceTaskStatusMessage.VerifyFailed}: {ex.Message}" };
            }
        }
        public async Task<GetAllRecord<GetMaintenanceTaskDto>> GetTasksByWorkOrderAsync(int workOrderId, int tenantId)
        {
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                var entities = await tenantDb.WorkOrderSubTasks
                    .Where(t => t.WorkOrderId == workOrderId && !t.IsDeleted)
                    .Include(t => t.WorkOrder)
                    .Include(t => t.VerifiedByUser)
                    .OrderBy(t => t.Sequence)
                    .ToListAsync();

                var dtoList = entities.Select(t => new GetMaintenanceTaskDto
                {
                    TaskId = t.SubTaskId,
                    TenantId = t.TenantId,
                    TaskName = t.Title,
                    WorkOrderId = t.WorkOrderId,
                    WorkOrderNumber = t.WorkOrder?.WorkOrderNumber ?? "Unknown",
                    Description = t.Description,
                    Instructions = t.Instructions,
                    SequenceOrder = t.Sequence,
                    EstimatedTimeMinutes = t.EstimatedMinutes,
                    Status = t.Status,
                    IsMandatory = t.IsMandatory,
                    VerificationRequired = t.VerificationRequired,
                    Notes = t.Notes,
                    StartedAt = t.StartDate,
                    CompletedAt = t.CompletedDate,
                    ActualTimeMinutes = t.ActualMinutes,
                    VerifiedByUserId = t.VerifiedByUserId,
                    VerifiedByUserName = t.VerifiedByUser != null ? $"{t.VerifiedByUser.FirstName} {t.VerifiedByUser.LastName}" : null,
                    VerifiedAt = t.VerifiedAt,
                    IsActive = t.IsActive,
                    StatusDisplay = t.Status.ToString(),
                    TimeSpent = t.ActualMinutes.HasValue ? $"{t.ActualMinutes}m" : "Not recorded",
                    VerificationStatus = (t.VerificationRequired ?? false)
                        ? (t.VerifiedAt.HasValue ? "Verified" : "Pending Verification")
                        : "Not Required"
                }).ToList();

                return new GetAllRecord<GetMaintenanceTaskDto>
                {
                    StatusCode = StatusCode.Success,
                    StatusMessage = MaintenanceTaskStatusMessage.FetchByWorkOrderSuccess,
                    GetAllData = dtoList
                };
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "MaintenanceTask-Module", "GetTasksByWorkOrderAsync", tenantId, null);
                return new GetAllRecord<GetMaintenanceTaskDto>
                {
                    StatusCode = StatusCode.Error,
                    StatusMessage = $"{MaintenanceTaskStatusMessage.FetchByWorkOrderFailed}: {ex.Message}"
                };
            }
        }
        public async Task<GetAllRecord<GetMaintenanceTaskDto>> GetTasksByStatusAsync(int tenantId)
        {
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                var entities = await tenantDb.WorkOrderSubTasks
                    .Where(t => t.TenantId == tenantId && !t.IsDeleted)
                    .Include(t => t.WorkOrder)
                    .Include(t => t.VerifiedByUser)
                    .OrderBy(t => t.Sequence)
                    .ToListAsync();

                var dtoList = entities.Select(t => new GetMaintenanceTaskDto
                {
                    TaskId = t.SubTaskId,
                    TenantId = t.TenantId,
                    TaskName = t.Title,
                    WorkOrderId = t.WorkOrderId,
                    WorkOrderNumber = t.WorkOrder?.WorkOrderNumber ?? "Unknown",
                    Description = t.Description,
                    Instructions = t.Instructions,
                    SequenceOrder = t.Sequence,
                    EstimatedTimeMinutes = t.EstimatedMinutes,
                    Status = t.Status,
                    IsMandatory = t.IsMandatory,
                    VerificationRequired = t.VerificationRequired,
                    Notes = t.Notes,
                    StartedAt = t.StartDate,
                    CompletedAt = t.CompletedDate,
                    ActualTimeMinutes = t.ActualMinutes,
                    VerifiedByUserId = t.VerifiedByUserId,
                    VerifiedByUserName = t.VerifiedByUser != null ? $"{t.VerifiedByUser.FirstName} {t.VerifiedByUser.LastName}" : null,
                    VerifiedAt = t.VerifiedAt,
                    IsActive = t.IsActive,
                    StatusDisplay = t.Status.ToString(),
                    TimeSpent = t.ActualMinutes.HasValue ? $"{t.ActualMinutes}m" : "Not recorded",
                    // Fix for CS0266: Add explicit cast to bool for nullable bool properties in ternary expressions

                    VerificationStatus = (t.VerificationRequired ?? false)
                        ? (t.VerifiedAt.HasValue ? "Verified" : "Pending Verification")
                        : "Not Required"
                }).ToList();

                return new GetAllRecord<GetMaintenanceTaskDto>
                {
                    StatusCode = StatusCode.Success,
                    StatusMessage = MaintenanceTaskStatusMessage.FetchByStatusSuccess,
                    GetAllData = dtoList
                };
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "MaintenanceTask-Module", "GetTasksByStatusAsync", tenantId, null);
                return new GetAllRecord<GetMaintenanceTaskDto>
                {
                    StatusCode = StatusCode.Error,
                    StatusMessage = $"{MaintenanceTaskStatusMessage.FetchByStatusFailed}: {ex.Message}"
                };
            }
        }
        public async Task<GetSpecificRecord<GetMaintenanceTaskDto>> GetMaintenanceTaskByIdAsync(int taskId, int tenantId)
        {
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                var entity = await tenantDb.WorkOrderSubTasks
                    .Include(t => t.WorkOrder)
                    .Include(t => t.VerifiedByUser)
                    .FirstOrDefaultAsync(t => t.SubTaskId == taskId && !t.IsDeleted);

                if (entity == null)
                    return new GetSpecificRecord<GetMaintenanceTaskDto>
                    {
                        StatusCode = StatusCode.NotFound,
                        StatusMessage = MaintenanceTaskStatusMessage.NotFound
                    };

                var dto = new GetMaintenanceTaskDto
                {
                    TaskId = entity.SubTaskId,
                    TenantId = entity.TenantId,
                    TaskName = entity.Title,
                    WorkOrderId = entity.WorkOrderId,
                    WorkOrderNumber = entity.WorkOrder?.WorkOrderNumber ?? "Unknown",
                    Description = entity.Description,
                    Instructions = entity.Instructions,
                    SequenceOrder = entity.Sequence,
                    EstimatedTimeMinutes = entity.EstimatedMinutes,
                    Status = entity.Status,
                    IsMandatory = entity.IsMandatory,
                    VerificationRequired = entity.VerificationRequired,
                    Notes = entity.Notes,
                    StartedAt = entity.StartDate,
                    CompletedAt = entity.CompletedDate,
                    ActualTimeMinutes = entity.ActualMinutes,
                    VerifiedByUserId = entity.VerifiedByUserId,
                    VerifiedByUserName = entity.VerifiedByUser != null ? $"{entity.VerifiedByUser.FirstName} {entity.VerifiedByUser.LastName}" : null,
                    VerifiedAt = entity.VerifiedAt,
                    IsActive = entity.IsActive,
                    StatusDisplay = entity.Status.ToString(),
                    TimeSpent = entity.ActualMinutes.HasValue ? $"{entity.ActualMinutes}m" : "Not recorded",
                    VerificationStatus = (entity.VerificationRequired ?? false)
                        ? (entity.VerifiedAt.HasValue ? "Verified" : "Pending Verification")
                        : "Not Required"
                };

                return new GetSpecificRecord<GetMaintenanceTaskDto>
                {
                    StatusCode = StatusCode.Success,
                    StatusMessage = MaintenanceTaskStatusMessage.FetchByIdSuccess,
                    Data = dto
                };
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "MaintenanceTask-Module", "GetMaintenanceTaskByIdAsync", tenantId, null);
                return new GetSpecificRecord<GetMaintenanceTaskDto>
                {
                    StatusCode = StatusCode.Error,
                    StatusMessage = $"{MaintenanceTaskStatusMessage.FetchByIdFailed}: {ex.Message}"
                };
            }
        }
        public async Task<GetAllRecord<GetMaintenanceTaskDto>> GetTasksRequiringVerificationAsync(int tenantId)
        {
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                var entities = await tenantDb.WorkOrderSubTasks
                    .Where(t => t.TenantId == tenantId && t.Status == SubTaskStatus.Completed && !t.IsDeleted)
                    .Include(t => t.WorkOrder)
                    .Include(t => t.VerifiedByUser)
                    .OrderBy(t => t.CompletedDate)
                    .ToListAsync();

                var dtoList = entities.Select(t => new GetMaintenanceTaskDto
                {
                    TaskId = t.SubTaskId,
                    TenantId = t.TenantId,
                    TaskName = t.Title,
                    WorkOrderId = t.WorkOrderId,
                    WorkOrderNumber = t.WorkOrder?.WorkOrderNumber ?? "Unknown",
                    Description = t.Description,
                    Instructions = t.Instructions,
                    SequenceOrder = t.Sequence,
                    EstimatedTimeMinutes = t.EstimatedMinutes,
                    Status = t.Status,
                    IsMandatory = t.IsMandatory,
                    VerificationRequired = t.VerificationRequired,
                    Notes = t.Notes,
                    StartedAt = t.StartDate,
                    CompletedAt = t.CompletedDate,
                    ActualTimeMinutes = t.ActualMinutes,
                    VerifiedByUserId = t.VerifiedByUserId,
                    VerifiedByUserName = t.VerifiedByUser != null ? $"{t.VerifiedByUser.FirstName} {t.VerifiedByUser.LastName}" : null,
                    VerifiedAt = t.VerifiedAt,
                    IsActive = t.IsActive,
                    StatusDisplay = t.Status.ToString(),
                    TimeSpent = t.ActualMinutes.HasValue ? $"{t.ActualMinutes}m" : "Not recorded",
                    VerificationStatus = "Pending Verification"
                }).ToList();

                return new GetAllRecord<GetMaintenanceTaskDto>
                {
                    StatusCode = StatusCode.Success,
                    StatusMessage = MaintenanceTaskStatusMessage.TasksRequiringVerificationSuccess,
                    GetAllData = dtoList
                };
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "MaintenanceTask-Module", "GetTasksRequiringVerificationAsync", tenantId, null);
                return new GetAllRecord<GetMaintenanceTaskDto>
                {
                    StatusCode = StatusCode.Error,
                    StatusMessage = $"{MaintenanceTaskStatusMessage.TasksRequiringVerificationFailed}: {ex.Message}"
                };
            }
        }
    }
}