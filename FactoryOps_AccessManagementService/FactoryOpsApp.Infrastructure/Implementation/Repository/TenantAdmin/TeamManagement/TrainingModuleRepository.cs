using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.TeamManagement;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.SuperAdmin.AuditLogs;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.ExceptionLogger;
using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Domain.Entities.FactoryOpsTenants;
using FactoryOpsApp.Infrastructure.DBContext;
using Microsoft.EntityFrameworkCore;
using static FactoryOps_AccessManagementService.FactoryOpsApp.Common.CommonConstant;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Repository.TenantAdmin.TeamManagement
{
    public class TrainingModuleRepository : ITrainingModuleRepository
    {
        private readonly TenantDbContextFactory _tenantDbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IExceptionLoggerService _exceptionLogger;
        private readonly IAuditLogService _auditLogger;

        public TrainingModuleRepository
            (TenantDbContextFactory tenantDbContext,
            IHttpContextAccessor httpContextAccessor,
            IExceptionLoggerService exceptionLogger,
            IAuditLogService auditLogger)
        {
            _tenantDbContext = tenantDbContext;
            _httpContextAccessor = httpContextAccessor;
            _exceptionLogger = exceptionLogger;
            _auditLogger = auditLogger;
        }
        public async Task<CommonResponseModel> AddTrainingModuleAsync(AddTrainingModuleDto dto)
        {
            CommonResponseModel response = new();
            using var tenantDb = _tenantDbContext.GetTenantDbContext(dto.TenantId);
            try
            {
                var exists = await tenantDb.TrainingModules
                    .AnyAsync(t => t.Title.ToLower() == dto.Title.ToLower() && !t.IsActive);
                if (exists)
                {
                    response.StatusCode = StatusCode.BadRequest;
                    response.StatusMessage = TrainingModuleStatusMessage.TrainingModuleAlreadyExists;
                    return response;
                }
                var addtrainingModule = new TrainingModule
                {
                    TenantId = dto.TenantId,
                    Title = dto.Title,
                    Description = dto.Description,
                    ModuleType = dto.ModuleType,
                    Difficulty = dto.Difficulty,
                    DurationMinutes = dto.DurationMinutes,
                    Category = dto.Category,
                    Prerequisites = dto.Prerequisites,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = dto.TenantId,

                };
                await tenantDb.TrainingModules.AddAsync(addtrainingModule);
                await tenantDb.SaveChangesAsync();

                await _auditLogger.LogAuditAsync("TrainingModule", "Create", dto.CreatedBy, dto.TenantId.ToString(), addtrainingModule.TrainingModuleId.ToString());

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = TrainingModuleStatusMessage.TrainingModuleAdded;
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "Training-Module", "Create-TrainingModule", dto.TenantId, dto.CreatedBy);
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"{TrainingModuleStatusMessage.TrainingModuleAddFailed}: {ex.Message}";
            }

            return response;
        }
        public async Task<CommonResponseModel> UpdateTrainingModuleAsync(AddTrainingModuleDto dto)
        {
            CommonResponseModel response = new();
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(dto.TenantId);
                var updateTrainingModule = await tenantDb.TrainingModules.FirstOrDefaultAsync(
                    t => t.TrainingModuleId == dto.TrainingModuleId && t.IsActive);
                if (updateTrainingModule == null)
                {
                    response.StatusCode = StatusCode.NotFound;
                    response.StatusMessage = TrainingModuleStatusMessage.TrainingModuleNotFound;
                    return response;
                }
                updateTrainingModule.TenantId = dto.TenantId;
                updateTrainingModule.Title = dto.Title;
                updateTrainingModule.Description = dto.Description;
                updateTrainingModule.ModuleType = dto.ModuleType;
                updateTrainingModule.Difficulty = dto.Difficulty;
                updateTrainingModule.DurationMinutes = dto.DurationMinutes;
                updateTrainingModule.Category = dto.Category;
                updateTrainingModule.Prerequisites = dto.Prerequisites;
                updateTrainingModule.IsActive = true;
                updateTrainingModule.CreatedAt = DateTime.UtcNow;
                updateTrainingModule.CreatedBy = dto.TenantId;
                updateTrainingModule.UpdatedAt = dto.UpdatedAt;
                await tenantDb.SaveChangesAsync();

                await _auditLogger.LogAuditAsync("TrainingModule", "Updated", null, dto.TenantId.ToString(), updateTrainingModule.TrainingModuleId.ToString());

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = TrainingModuleStatusMessage.TrainingModuleUpdated;
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(
                    ex,
                    sourceModule: "TrainingModule",
                    apiName: "UpdateTrainingModule",
                    tenantId: dto?.TenantId,
                    userId: null
                );
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"{TrainingModuleStatusMessage.TrainingModuleUpdateFailed} : : {ex.Message}";
            }
            return response;
        }
        public async Task<CommonResponseModel> DeleteTrainingModuleAsync(int id, int tenantId)
        {
            CommonResponseModel response = new();
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);
                var deleteTrainingModule = await tenantDb.TrainingModules.FirstOrDefaultAsync(
                    t => t.TrainingModuleId == id && t.IsActive);
                if (deleteTrainingModule == null)
                {
                    response.StatusCode = StatusCode.NotFound;
                    response.StatusMessage = TrainingModuleStatusMessage.TrainingModuleNotFound;
                    return response;
                }
                deleteTrainingModule.IsActive = false;

                await tenantDb.SaveChangesAsync();
                await _auditLogger.LogAuditAsync("TrainingModule", "Deleted", null, tenantId.ToString(), deleteTrainingModule.TrainingModuleId.ToString());
                response.StatusCode = StatusCode.Success;
                response.StatusMessage = TrainingModuleStatusMessage.TrainingModuleDeleted;
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(
                        ex,
                        sourceModule: "TrainingModule",
                        apiName: "DeleteTrainingModule",
                        tenantId: tenantId,
                        userId: null
                    );
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"{TrainingModuleStatusMessage.TrainingModuleDeleteFailed} : : {ex.Message}";
            }
            return response;
        }

        public async Task<GetAllRecord<GetTrainingModuleDto>> GetAllTrainingModuleAsync(int tenantId)
        {
            GetAllRecord<GetTrainingModuleDto> response = new();
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);
                var trainingModule = tenantDb.TrainingModules
                    .Where(t => t.IsActive && t.TenantId == tenantId)
                    .AsEnumerable()
                    .Select(t => new GetTrainingModuleDto
                    {
                        TrainingModuleId = t.TrainingModuleId,
                        Title = t.Title,
                        Description = t.Description,
                        ModuleType = t.ModuleType,
                        Difficulty = t.Difficulty,
                        DurationMinutes = t.DurationMinutes,
                        Category = t.Category,
                        Prerequisites = t.Prerequisites,
                        TenantId = t.TenantId,
                        IsActive = t.IsActive,
                        CreatedAt = t.CreatedAt,
                        CreatedBy = t.CreatedBy,
                    })
                    .ToList();
                response.StatusCode = StatusCode.Success;
                response.StatusMessage = TrainingModuleStatusMessage.TrainingModulesFetched;
                response.GetAllData = trainingModule;
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(
                        ex,
                        sourceModule: "TrainingModule",
                        apiName: "GetAllTrainingModule",
                        tenantId: tenantId,
                        userId: null
                    );
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"{TrainingModuleStatusMessage.TrainingModulesFetchFailed}: {ex.Message}";
            }

            return response;
        }
    }
}

