using FactoryOperation_PreventiveMaintenance.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.ExceptionLogger;
using FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.PreventiveMaintenance;
using FactoryOpsApp.Application.Interfaces.Services.SuperAdmin.AuditLogs;
using FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.Common;
using FactoryOpsApp.Domain.Entities.FactoryOpsTenants;
using FactoryOpsApp.Infrastructure.DBContext;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using static FactoryOpsApp.Common.CommonConstant;

namespace FactoryOpsApp.Infrastructure.Repository.TenantAdmin.PreventiveMaintenance
{
    public class MaintenanceScheduleRepository : IMaintenanceScheduleRepository
    {
        private readonly TenantDbContextFactory _tenantDbContext;
        private readonly IAuditLogService _auditLogger;
        private readonly IExceptionLoggerService _exceptionLogger;
        private readonly IFileStorageService _fileStorageService;
        private readonly IConfiguration _configuration;

        public MaintenanceScheduleRepository(
            TenantDbContextFactory tenantDbContext,
            IAuditLogService auditLogger,
            IExceptionLoggerService exceptionLogger,
            IFileStorageService fileStorageService,
            IConfiguration configuration)
        {
            _tenantDbContext = tenantDbContext;
            _auditLogger = auditLogger;
            _exceptionLogger = exceptionLogger;
            _fileStorageService = fileStorageService;
            _configuration = configuration;
        }

        public async Task<CommonResponseModel> AddMaintenanceScheduleAsync(MaintenanceScheduleDto dto)
        {
            using var tenantDb = _tenantDbContext.GetTenantDbContext(dto.TenantId);
            using var transaction = await tenantDb.Database.BeginTransactionAsync();

            try
            {

                string? relativePath = null;
                byte[]? imageBytes = null;

                if (dto.Image != null)
                {
                    relativePath = await _fileStorageService.SaveFileAsync(dto.Image, "MaintenanceScheduleImages");
                    imageBytes = await File.ReadAllBytesAsync(Path.Combine("wwwroot", relativePath));
                }

                if (dto.SubTaskIds != null && dto.SubTaskIds.Any())
                {
                    var validSubTasks = await tenantDb.WorkOrderSubTasks
                        .Where(st => st.WorkOrderId == dto.WorkOrderId)
                        .Select(st => st.SubTaskId)
                        .ToListAsync();

                    if (!dto.SubTaskIds.All(id => validSubTasks.Contains(id)))
                    {
                        return new CommonResponseModel
                        {
                            StatusCode = StatusCode.Error,
                            StatusMessage = "One or more selected SubTasks do not belong to the specified WorkOrder."
                        };
                    }
                }

                if (dto.PrimarySubTaskId.HasValue)
                {
                    var validPrimary = await tenantDb.WorkOrderSubTasks
                        .AnyAsync(st => st.SubTaskId == dto.PrimarySubTaskId && st.WorkOrderId == dto.WorkOrderId);

                    if (!validPrimary)
                    {
                        return new CommonResponseModel
                        {
                            StatusCode = StatusCode.Error,
                            StatusMessage = "The selected PrimarySubTask does not belong to the specified WorkOrder."
                        };
                    }
                }

                var entity = new MaintenanceSchedule
                {
                    WorkOrderNumber = dto.WorkOrderNumber,
                    WorkOrderId = dto.WorkOrderId,
                    Category = dto.Category,
                    Image = dto.Image.FileName ?? null,
                    ImageUrl = relativePath,
                    TenantId = dto.TenantId,
                    ScheduleName = dto.ScheduleName,
                    ScheduleType = dto.ScheduleType,
                    AssetId = dto.AssetId,
                    Description = dto.Description,
                    Frequency = dto.Frequency,
                    FrequencyValue = dto.FrequencyValue,
                    StartDate = dto.StartDate,
                    EndDate = dto.EndDate,
                    LocationFilter = dto.LocationFilter,
                    Status = dto.Status,
                    NextDueDate = CalculateNextDueDate(dto.StartDate, dto.Frequency, dto.FrequencyValue),
                    IsActive = true,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = dto.CreatedBy,
                    PrimarySubTaskId = dto.PrimarySubTaskId,
                    SelectedSubTaskIds = dto.SubTaskIds != null
                    ? JsonSerializer.Serialize(dto.SubTaskIds)
                    : null
                };

                await tenantDb.MaintenanceSchedules.AddAsync(entity);
                await tenantDb.SaveChangesAsync();

                var entityWorkorder = new WorkOrder
                {
                    MaintenanceScheduleId = entity.ScheduleId,
                    TenantId = dto.TenantId,
                    Title = dto.ScheduleName,
                    Description = dto.Description,
                    LocationId = dto.LocationId,
                    Status = dto.Status,
                    Priority = PriorityLevel.Medium,
                    WorkOrderType = WorkOrderTypeEnum.Preventive,
                    AssignedToUserId = null,
                    AssignedToTeamId = null,
                    DueDate = dto.EndDate,
                    ScheduleDate = dto.StartDate,
                    AssetId = dto.AssetId,
                    WorkOrderNumber = $"WO-{DateTime.UtcNow.Year}-{Guid.NewGuid().ToString().Substring(0, 4)}",
                    CreatedBy = dto.CreatedBy,
                    EstimatedDurationMinutes = null,
                    RequiredTools = null,
                    Instructions = null,
                    CompletionNotes = null,
                    LaborCost = null,
                    PartCost = null,
                    TotalCost = null,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsActive = true,
                    IsDeleted = false
                };

                tenantDb.WorkOrders.Add(entityWorkorder);

                var occurrences = GenerateOccurrences(entity);
                await tenantDb.MaintenanceScheduleOccurrences.AddRangeAsync(occurrences);

                await tenantDb.SaveChangesAsync();

                await transaction.CommitAsync();

                await _auditLogger.LogAuditAsync("Create",
                    $"Created Maintenance Schedule '{entity.ScheduleName}' with {occurrences.Count} occurrences",
                    dto.TenantId, "", "AddMaintenanceScheduleAsync");

                return new CommonResponseModel
                {
                    StatusCode = StatusCode.Success,
                    StatusMessage = MaintenanceScheduleStatusMessage.CreateSuccess
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _exceptionLogger.LogExceptionAsync(ex, "MaintenanceSchedule-Module",
                    "AddMaintenanceScheduleAsync", dto.TenantId, null);

                return new CommonResponseModel
                {
                    StatusCode = StatusCode.Error,
                    StatusMessage = $"{MaintenanceScheduleStatusMessage.CreateFailed}: {ex.Message}"
                };
            }
        }
        public async Task<CommonResponseModel> UpdateMaintenanceScheduleAsync(MaintenanceScheduleDto dto)
        {
            using var tenantDb = _tenantDbContext.GetTenantDbContext(dto.TenantId);
            using var transaction = await tenantDb.Database.BeginTransactionAsync();

            try
            {
                var entity = await tenantDb.MaintenanceSchedules
                    .FirstOrDefaultAsync(s => s.ScheduleId == dto.ScheduleId && !s.IsDeleted);

                if (entity == null)
                    return new CommonResponseModel
                    {
                        StatusCode = StatusCode.NotFound,
                        StatusMessage = MaintenanceScheduleStatusMessage.NotFound
                    };
                string? relativePath = null;
                byte[]? imageBytes = null;
                if (dto.Image != null)
                {
                    relativePath = await _fileStorageService.SaveFileAsync(dto.Image, "MaintenanceScheduleImages");
                    imageBytes = await File.ReadAllBytesAsync(Path.Combine("wwwroot", relativePath));
                    entity.Image = dto.Image.FileName ?? null;
                    entity.ImageUrl = relativePath;

                }
                bool freqChanged = entity.Frequency != dto.Frequency
                                   || entity.FrequencyValue != dto.FrequencyValue
                                   || entity.StartDate != dto.StartDate
                                   || entity.EndDate != dto.EndDate;

                entity.WorkOrderNumber = dto.WorkOrderNumber;
                entity.WorkOrderId = dto.WorkOrderId;
                entity.Category = dto.Category;
                entity.ScheduleName = dto.ScheduleName;
                entity.ScheduleType = dto.ScheduleType;
                entity.AssetId = dto.AssetId;
                entity.Description = dto.Description;
                entity.Frequency = dto.Frequency;
                entity.FrequencyValue = dto.FrequencyValue;
                entity.StartDate = dto.StartDate;
                entity.EndDate = dto.EndDate;
                entity.LocationFilter = dto.LocationFilter;
                entity.Status = dto.Status;
                entity.UpdatedAt = DateTime.UtcNow;
                entity.UpdatedBy = dto.UpdatedBy;
                if (freqChanged)
                    entity.NextDueDate = CalculateNextDueDate(dto.StartDate, dto.Frequency, dto.FrequencyValue);

                await tenantDb.SaveChangesAsync();

                var workOrder = await tenantDb.WorkOrders
                    .FirstOrDefaultAsync(w => w.MaintenanceScheduleId == entity.ScheduleId && !w.IsDeleted);

                if (workOrder != null)
                {
                    workOrder.Title = dto.ScheduleName;
                    workOrder.Description = dto.Description;
                    workOrder.LocationId = dto.LocationId;
                    workOrder.Status = dto.Status; 
                    workOrder.AssetId = dto.AssetId;
                    workOrder.ScheduleDate = dto.StartDate;
                    workOrder.DueDate = dto.EndDate;
                    workOrder.UpdatedAt = DateTime.UtcNow;
                    workOrder.UpdatedBy = dto.UpdatedBy;

                    //tenantDb.WorkOrders.Update(workOrder);
                    await tenantDb.SaveChangesAsync();
                }

                if (freqChanged)
                {
                    var oldOccurrences = tenantDb.MaintenanceScheduleOccurrences
                        .Where(o => o.ScheduleId == entity.ScheduleId && o.TenantId == dto.TenantId);

                    tenantDb.MaintenanceScheduleOccurrences.RemoveRange(oldOccurrences);

                    var newOccurrences = GenerateOccurrences(entity);
                    await tenantDb.MaintenanceScheduleOccurrences.AddRangeAsync(newOccurrences);

                    await tenantDb.SaveChangesAsync();
                }

                await transaction.CommitAsync();

                await _auditLogger.LogAuditAsync("Update",
                    $"Updated Maintenance Schedule '{entity.ScheduleName}' (and linked WorkOrder)",
                    dto.TenantId, "", "UpdateMaintenanceScheduleAsync");

                return new CommonResponseModel
                {
                    StatusCode = StatusCode.Success,
                    StatusMessage = MaintenanceScheduleStatusMessage.UpdateSuccess
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _exceptionLogger.LogExceptionAsync(ex, "MaintenanceSchedule-Module",
                    "UpdateMaintenanceScheduleAsync", dto.TenantId, null);

                return new CommonResponseModel
                {
                    StatusCode = StatusCode.Error,
                    StatusMessage = $"{MaintenanceScheduleStatusMessage.UpdateFailed}: {ex.Message}"
                };
            }
        }
        public async Task<CommonResponseModel> DeleteMaintenanceScheduleAsync(int scheduleId, int tenantId)
        {
            using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);
            using var transaction = await tenantDb.Database.BeginTransactionAsync();

            try
            {
                var entity = await tenantDb.MaintenanceSchedules
                    .FirstOrDefaultAsync(s => s.ScheduleId == scheduleId && !s.IsDeleted);

                if (entity == null)
                    return new CommonResponseModel
                    {
                        StatusCode = StatusCode.NotFound,
                        StatusMessage = MaintenanceScheduleStatusMessage.NotFound
                    };

                entity.IsDeleted = true;
                entity.IsActive = false;
                entity.DeletedAt = DateTime.UtcNow;
                entity.DeletedBy = tenantId;
                entity.Status = WorkOrderStatus.Inactive;

                var occurrences = await tenantDb.MaintenanceScheduleOccurrences
                    .Where(o => o.ScheduleId == scheduleId && o.TenantId == tenantId && !o.IsDeleted)
                    .ToListAsync();

                foreach (var occ in occurrences)
                {
                    occ.IsDeleted = true;
                    occ.IsActive = false;
                    occ.DeletedAt = DateTime.UtcNow;
                    occ.DeletedBy = tenantId;
                }

                var workOrders = await tenantDb.WorkOrders
                    .Where(w => w.MaintenanceScheduleId == scheduleId && !w.IsDeleted)
                    .ToListAsync();

                foreach (var wo in workOrders)
                {
                    wo.IsDeleted = true;
                    wo.IsActive = false;
                    wo.DeletedAt = DateTime.UtcNow;
                    wo.DeletedBy = tenantId;
                    wo.Status = WorkOrderStatus.Inactive;
                }

                await tenantDb.SaveChangesAsync();

                await transaction.CommitAsync();

                await _auditLogger.LogAuditAsync("Delete",
                    $"Soft deleted Maintenance Schedule '{entity.ScheduleName}', {occurrences.Count} occurrences, and {workOrders.Count} linked WorkOrders",
                    tenantId, "", "DeleteMaintenanceScheduleAsync");

                return new CommonResponseModel
                {
                    StatusCode = StatusCode.Success,
                    StatusMessage = MaintenanceScheduleStatusMessage.DeleteSuccess
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _exceptionLogger.LogExceptionAsync(ex,
                    "MaintenanceSchedule-Module", "DeleteMaintenanceScheduleAsync", tenantId, null);

                return new CommonResponseModel
                {
                    StatusCode = StatusCode.Error,
                    StatusMessage = $"{MaintenanceScheduleStatusMessage.DeleteFailed}: {ex.Message}"
                };
            }
        }
        public async Task<CommonResponseModel> ApproveScheduleAsync(ScheduleApprovalDto dto)
        {
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(dto.TenantId);

                var entity = await tenantDb.MaintenanceSchedules
                    .FirstOrDefaultAsync(s => s.ScheduleId == dto.ScheduleId && !s.IsDeleted);

                if (entity == null)
                    return new CommonResponseModel { StatusCode = StatusCode.NotFound, StatusMessage = MaintenanceScheduleStatusMessage.NotFound };

                if (dto.IsApproved)
                {
                    entity.Status = WorkOrderStatus.Active;
                    await _auditLogger.LogAuditAsync("Approve",
                        $"Approved Maintenance Schedule '{entity.ScheduleName}'",
                        dto.TenantId, "", "ApproveScheduleAsync");
                }
                else
                {
                    entity.Status = WorkOrderStatus.Inactive;
                    await _auditLogger.LogAuditAsync("Reject",
                        $"Rejected Maintenance Schedule '{entity.ScheduleName}'",
                        dto.TenantId, dto.RejectionReason, "ApproveScheduleAsync");
                }

                entity.UpdatedAt = DateTime.UtcNow;
                entity.UpdatedBy = dto.ApprovedBy;

                await tenantDb.SaveChangesAsync();

                return new CommonResponseModel
                {
                    StatusCode = StatusCode.Success,
                    StatusMessage = $"Schedule {(dto.IsApproved ? MaintenanceScheduleStatusMessage.ApproveSuccess : MaintenanceScheduleStatusMessage.RejectSuccess)} successfully"
                };
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "MaintenanceSchedule-Module",
                    "ApproveScheduleAsync", dto.TenantId, null);
                return new CommonResponseModel { StatusCode = StatusCode.Error, StatusMessage = $"{MaintenanceScheduleStatusMessage.ApproveFailed}: {ex.Message}" };
            }
        }   
        public async Task<GetAllRecord<GetMaintenanceScheduleDto>> GetAllMaintenanceSchedulesAsync(int tenantId, WorkOrderStatus? statusFilter = null)
        {
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);
                string baseUrl = _configuration["BaseUrl:Staging"] ?? "https://ms.stagingsdei.com:8107";
                var query = tenantDb.MaintenanceSchedules
                    .Where(s => s.TenantId == tenantId && !s.IsDeleted)
                    .Include(s => s.WorkOrders)
                    .Include(s => s.Asset)
                    .Include(s => s.Occurrences)
                    .AsQueryable();

                if (statusFilter.HasValue)
                    query = query.Where(s => s.Status == statusFilter);

                var entities = await query.ToListAsync();

                var dtoList = entities.Select(s => new GetMaintenanceScheduleDto
                {
                    ImageUrl = s.ImageUrl != null ? $"/api/companybranding/image/{s.ScheduleId}" : null,
                    Image = s.Image != null
                    ? $"{baseUrl}/{s.Image.Replace("\\", "/")}"
                        : null,
                    Category = s.Category,
                    WorkOrderId = s.WorkOrderId,
                    WorkOrderNumber = s.WorkOrderNumber,
                    ScheduleId = s.ScheduleId,
                    TenantId = s.TenantId,
                    ScheduleName = s.ScheduleName,
                    ScheduleType = s.ScheduleType,
                    AssetId = s.AssetId,
                    Asset = s.Asset?.AssetName ?? "Unassigned",
                    Description = s.Description,
                    Frequency = s.Frequency,
                    FrequencyValue = s.FrequencyValue,
                    StartDate = s.StartDate,
                    EndDate = s.EndDate,
                    LocationFilter = s.LocationFilter,
                    Status = s.Status,
                    NextDueDate = s.NextDueDate,
                    IsActive = s.IsActive,
                    NextDueFormatted = s.NextDueDate?.ToString("yyyy-MM-dd") ?? "Not set",
                    FrequencyDisplay = GetFrequencyDisplay(s.Frequency, s.FrequencyValue),
                    ActiveWorkOrders = s.WorkOrders?.Count(w => !w.IsDeleted) ?? 0,
                    SubTaskIds = !string.IsNullOrEmpty(s.SelectedSubTaskIds)
                    ? JsonSerializer.Deserialize<List<int>>(s.SelectedSubTaskIds)
                    : new List<int>(),

                    Occurrences = s.Occurrences
                        .Where(o => !o.IsDeleted)
                        .OrderBy(o => o.OccurrenceDate)
                        .Select(o => new MaintenanceScheduleOccurrenceDto
                        {
                            OccurrenceId = o.OccurrenceId,
                            ScheduleId = o.ScheduleId,
                            OccurrenceDate = o.OccurrenceDate,
                            FrequencyType = o.FrequencyType,
                            FrequencyValue = o.FrequencyValue,
                            IsDeleted = o.IsDeleted,
                            IsActive = o.IsActive
                        }).ToList()
                }).ToList();

                return new GetAllRecord<GetMaintenanceScheduleDto>
                {
                    StatusCode = StatusCode.Success,
                    StatusMessage = MaintenanceScheduleStatusMessage.FetchAllSuccess,
                    GetAllData = dtoList
                };
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "MaintenanceSchedule-Module",
                    "GetAllMaintenanceSchedulesAsync", tenantId, null);
                return new GetAllRecord<GetMaintenanceScheduleDto>
                {
                    StatusCode = StatusCode.Error,
                    StatusMessage = $"{MaintenanceScheduleStatusMessage.FetchAllFailed}: {ex.Message}"
                };
            }
        }     
        public async Task<GetSpecificRecord<GetMaintenanceScheduleDto>> GetMaintenanceScheduleByIdAsync(int scheduleId, int tenantId)
        {
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                var entity = await tenantDb.MaintenanceSchedules
                    .Include(s => s.WorkOrders).Include(s => s.Asset)   
                    .FirstOrDefaultAsync(s => s.ScheduleId == scheduleId && !s.IsDeleted);

                if (entity == null)
                    return new GetSpecificRecord<GetMaintenanceScheduleDto>
                    {
                        StatusCode = StatusCode.NotFound,
                        StatusMessage = MaintenanceScheduleStatusMessage.NotFound
                    };

                var dto = new GetMaintenanceScheduleDto
                {
                    ScheduleId = entity.ScheduleId,
                    TenantId = entity.TenantId,
                    ScheduleName = entity.ScheduleName,
                    ScheduleType = entity.ScheduleType,
                    AssetId = entity.AssetId,
                    Asset = entity.Asset?.AssetName ?? "Unassigned",
                    Description = entity.Description,
                    Frequency = entity.Frequency,
                    FrequencyValue = entity.FrequencyValue,
                    StartDate = entity.StartDate,
                    EndDate = entity.EndDate,
                    LocationFilter = entity.LocationFilter,
                    Status = entity.Status,
                    NextDueDate = entity.NextDueDate,
                    IsActive = entity.IsActive,
                    NextDueFormatted = entity.NextDueDate?.ToString("yyyy-MM-dd") ?? "Not set",
                    FrequencyDisplay = GetFrequencyDisplay(entity.Frequency, entity.FrequencyValue),
                    ActiveWorkOrders = entity.WorkOrders?.Count(w => !w.IsDeleted) ?? 0
                };

                return new GetSpecificRecord<GetMaintenanceScheduleDto>
                {
                    StatusCode = StatusCode.Success,
                    StatusMessage = MaintenanceScheduleStatusMessage.FetchByIdSuccess,
                    Data = dto
                };
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "MaintenanceSchedule-Module",
                    "GetMaintenanceScheduleByIdAsync", tenantId, null);
                return new GetSpecificRecord<GetMaintenanceScheduleDto>
                {
                    StatusCode = StatusCode.Error,
                    StatusMessage = $"{MaintenanceScheduleStatusMessage.FetchByIdFailed}: {ex.Message}"
                };
            }
        }
        public async Task<GetAllRecord<MaintenanceScheduleOccurrence>> GetOccurrencesByScheduleIdAsync(int scheduleId, int tenantId)
        {
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                var occurrences = await tenantDb.MaintenanceScheduleOccurrences
                    .Where(o => o.ScheduleId == scheduleId && o.TenantId == tenantId && !o.IsDeleted)
                    .OrderBy(o => o.OccurrenceDate)
                    .ToListAsync();

                return new GetAllRecord<MaintenanceScheduleOccurrence>
                {
                    StatusCode = StatusCode.Success,
                    StatusMessage = MaintenanceScheduleStatusMessage.FetchOccurrencesSuccess,
                    GetAllData = occurrences
                };
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "MaintenanceSchedule-Module",
                    "GetOccurrencesByScheduleIdAsync", tenantId, null);
                return new GetAllRecord<MaintenanceScheduleOccurrence>
                {
                    StatusCode = StatusCode.Error,
                    StatusMessage = $"{MaintenanceScheduleStatusMessage.FetchOccurrencesFailed}: {ex.Message}"
                };
            }
        }
        public async Task<GetAllRecord<MaintenanceScheduleOccurrence>> GetUpcomingOccurrencesAsync(int tenantId, int daysAhead)
        {
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);
                var cutoff = DateTime.UtcNow.AddDays(daysAhead);

                var occurrences = await tenantDb.MaintenanceScheduleOccurrences
                    .Where(o => o.TenantId == tenantId && o.OccurrenceDate >= DateTime.UtcNow && o.OccurrenceDate <= cutoff)
                    .OrderBy(o => o.OccurrenceDate)
                    .ToListAsync();

                return new GetAllRecord<MaintenanceScheduleOccurrence>
                {
                    StatusCode = StatusCode.Success,
                    StatusMessage = MaintenanceScheduleStatusMessage.UpcomingOccurrencesSuccess,
                    GetAllData = occurrences
                };
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "MaintenanceSchedule-Module",
                    "GetUpcomingOccurrencesAsync", tenantId, null);
                return new GetAllRecord<MaintenanceScheduleOccurrence>
                {
                    StatusCode = StatusCode.Error,
                    StatusMessage = $"{MaintenanceScheduleStatusMessage.UpcomingOccurrencesFailed}: {ex.Message}"
                };
            }
        }
        public async Task<CommonResponseModel> RegenerateOccurrencesAsync(int scheduleId, int tenantId)
        {
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                var schedule = await tenantDb.MaintenanceSchedules
                    .FirstOrDefaultAsync(s => s.ScheduleId == scheduleId && s.TenantId == tenantId);

                if (schedule == null)
                    return new CommonResponseModel { StatusCode = StatusCode.NotFound, StatusMessage = MaintenanceScheduleStatusMessage.NotFound };

                var oldOccurrences = tenantDb.MaintenanceScheduleOccurrences
                    .Where(o => o.ScheduleId == scheduleId && o.TenantId == tenantId);
                tenantDb.MaintenanceScheduleOccurrences.RemoveRange(oldOccurrences);
                await tenantDb.SaveChangesAsync();

                var newOccurrences = GenerateOccurrences(schedule);
                await tenantDb.MaintenanceScheduleOccurrences.AddRangeAsync(newOccurrences);
                await tenantDb.SaveChangesAsync();

                return new CommonResponseModel
                {
                    StatusCode = StatusCode.Success,
                    StatusMessage = $"{MaintenanceScheduleStatusMessage.RegenerateSuccess}: {newOccurrences.Count}"
                };
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "MaintenanceSchedule-Module",
                    "RegenerateOccurrencesAsync", tenantId, null);
                return new CommonResponseModel { StatusCode = StatusCode.Error, StatusMessage = $"{MaintenanceScheduleStatusMessage.RegenerateFailed}: {ex.Message}" };
            }
        }
        public async Task<CommonResponseModel> CalculateNextDueDateAsync(int scheduleId, int tenantId)
        {
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                var entity = await tenantDb.MaintenanceSchedules
                    .FirstOrDefaultAsync(s => s.ScheduleId == scheduleId && !s.IsDeleted);

                if (entity == null)
                    return new CommonResponseModel { StatusCode = StatusCode.NotFound, StatusMessage = MaintenanceScheduleStatusMessage.NotFound };

                entity.NextDueDate = CalculateNextDueDate(entity.StartDate, entity.Frequency, entity.FrequencyValue);
                entity.UpdatedAt = DateTime.UtcNow;

                await tenantDb.SaveChangesAsync();

                return new CommonResponseModel { StatusCode = StatusCode.Success, StatusMessage = MaintenanceScheduleStatusMessage.NextDueRecalculatedSuccess };
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "MaintenanceSchedule-Module",
                    "CalculateNextDueDateAsync", tenantId, null);
                return new CommonResponseModel { StatusCode = StatusCode.Error, StatusMessage = $"{MaintenanceScheduleStatusMessage.NextDueRecalculatedFailed}: {ex.Message}" };
            }
        }
        private DateTime CalculateNextDueDate(DateTime startDate, FrequencyType frequency, int frequencyValue)
        {
            return frequency switch
            {
                FrequencyType.Daily => startDate.AddDays(frequencyValue),
                FrequencyType.Weekly => startDate.AddDays(7 * frequencyValue),
                FrequencyType.Monthly => startDate.AddMonths(frequencyValue),
                FrequencyType.Quarterly => startDate.AddMonths(3 * frequencyValue),
                FrequencyType.Yearly => startDate.AddYears(frequencyValue),
                _ => startDate.AddDays(1)
            };
        }
        private string GetFrequencyDisplay(FrequencyType frequency, int frequencyValue)
        {
            return frequency switch
            {
                FrequencyType.Daily => frequencyValue == 1 ? "Daily" : $"Every {frequencyValue} days",
                FrequencyType.Weekly => frequencyValue == 1 ? "Weekly" : $"Every {frequencyValue} weeks",
                FrequencyType.Monthly => frequencyValue == 1 ? "Monthly" : $"Every {frequencyValue} months",
                FrequencyType.Quarterly => frequencyValue == 1 ? "Quarterly" : $"Every {frequencyValue} quarters",
                FrequencyType.Yearly => frequencyValue == 1 ? "Yearly" : $"Every {frequencyValue} years",
                _ => "Custom"
            };
        }
        private List<MaintenanceScheduleOccurrence> GenerateOccurrences(MaintenanceSchedule schedule)
        {
            var occurrences = new List<MaintenanceScheduleOccurrence>();
            var current = schedule.StartDate;
            var endDate = schedule.EndDate ?? current.AddYears(5);

            while (current <= endDate)
            {
                occurrences.Add(new MaintenanceScheduleOccurrence
                {
                    TenantId = schedule.TenantId,
                    ScheduleId = schedule.ScheduleId,
                    OccurrenceDate = current,
                    FrequencyType = schedule.Frequency,
                    FrequencyValue = schedule.FrequencyValue,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedBy = schedule.CreatedBy,
                    CreatedAt = DateTime.UtcNow
                });

                current = schedule.Frequency switch
                {
                    FrequencyType.Daily => current.AddDays(schedule.FrequencyValue),
                    FrequencyType.Weekly => current.AddDays(7 * schedule.FrequencyValue),
                    FrequencyType.Monthly => current.AddMonths(schedule.FrequencyValue),
                    FrequencyType.Quarterly => current.AddMonths(3 * schedule.FrequencyValue),
                    FrequencyType.Yearly => current.AddYears(schedule.FrequencyValue),
                    _ => current.AddDays(1)
                };
            }

            return occurrences;
        }
    }
}