using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.SuperAdmin.IsolationControl;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.ExceptionLogger;
using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Domain.Entities.MasterTenantsAdmin;
using FactoryOpsApp.Infrastructure.DBContext;
using Microsoft.EntityFrameworkCore;
using static FactoryOps_AccessManagementService.FactoryOpsApp.Common.CommonConstant;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Repository.SuperAdmin.IsolationControl
{
    public class TenantIsolationRepository : ITenantIsolationRepository
    {
        private readonly MasterFactoryOpsDbContext _dbContext;
        private readonly MasterFactoryOpsDbContext _masterDbcontext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IExceptionLoggerService _exceptionLogger;

        public TenantIsolationRepository(MasterFactoryOpsDbContext dbContext,
            MasterFactoryOpsDbContext masterDbcontext,
            IHttpContextAccessor httpContextAccessor, IExceptionLoggerService exceptionLogger)
        {
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
            _masterDbcontext = masterDbcontext;
            _exceptionLogger = exceptionLogger;
        }

        public async Task<TenantIsolation?> GetByTenantIdAsync(int tenantId)
        {
            return await _dbContext.TenantIsolations.FirstOrDefaultAsync(t => t.TenantId == tenantId);
        }

        public async Task<bool> AddOrUpdateAsync(TenantIsolation isolation)
        {
            var existing = await _dbContext.TenantIsolations.FirstOrDefaultAsync(t => t.TenantId == isolation.TenantId);

            var ctx = _httpContextAccessor.HttpContext;

            if (existing == null)
            {
                await _dbContext.TenantIsolations.AddAsync(isolation);

                var auditData = new Audit_Log_MasterDb()
                {
                    Action = "Create",
                    Details = "Add-Tenant-Isolation",
                    EventType = "",
                    TenantId = isolation.TenantId,
                    Email = "",
                    Timestamp = DateTime.UtcNow,
                    IsActive = true,
                    IsDeleted = false,
                    UserName = Environment.UserName,
                    Ipaddress = ctx?.Connection.RemoteIpAddress?.ToString(),
                };

                await _masterDbcontext.Audit_Log_MasterDb.AddAsync(auditData);
            }
            else
            {
                existing.DataEncryption = isolation.DataEncryption;
                existing.TenantName = isolation.TenantName;
                existing.EncryptionKeyId = isolation.EncryptionKeyId;
                existing.CustomBranding = isolation.CustomBranding;
                if (!string.IsNullOrWhiteSpace(isolation.LogoUrl))
                {
                    existing.LogoUrl = isolation.LogoUrl;
                }

                if (!string.IsNullOrWhiteSpace(isolation.LogoText))
                {
                    existing.LogoText = isolation.LogoText;
                }

                existing.ColorScheme = isolation.ColorScheme;
                existing.DataPartitionId = isolation.DataPartitionId;
                _dbContext.TenantIsolations.Update(existing);

                var TenantDetails = await _dbContext.FactoryTenants.FirstOrDefaultAsync(t => t.TenantId == isolation.TenantId);
                if (TenantDetails != null)
                {
                    TenantDetails.TenantName = isolation.TenantName ?? string.Empty;
                    TenantDetails.BrandingLogoUrl = isolation.LogoUrl;
                }

                var auditData = new Audit_Log_MasterDb()
                {
                    Action = "Update",
                    Details = "Update-Existing_Isolation",
                    EventType = "",
                    TenantId = isolation.TenantId,
                    Email = "",
                    Timestamp = DateTime.UtcNow,
                    IsActive = true,
                    IsDeleted = false,
                    UserName = Environment.UserName,
                    Ipaddress = ctx?.Connection.RemoteIpAddress?.ToString(),
                };

            }
            await _masterDbcontext.SaveChangesAsync();

            return await _dbContext.SaveChangesAsync() > 0;
        }


        public async Task<List<FactoryComplianceAndAudit>> GetAllComplianceAuditsAsync(int tenantId)
        {
            return await _dbContext.FactoryComplianceAndAudits
                .Where(a => a.TenantId == tenantId && !a.IsDeleted)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<FactoryComplianceAndAudit?> GetComplianceAuditByIdAsync(int id)
        {
            return await _dbContext.FactoryComplianceAndAudits
                .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);
        }

        public async Task<bool> AddComplianceAuditAsync(FactoryComplianceAndAudit audit)
        {
            await _dbContext.FactoryComplianceAndAudits.AddAsync(audit);
            return await _dbContext.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateComplianceAuditAsync(FactoryComplianceAndAudit audit)
        {
            var existing = await _dbContext.FactoryComplianceAndAudits
                .FirstOrDefaultAsync(a => a.Id == audit.Id && !a.IsDeleted);

            if (existing == null) return false;

            existing.ComplianceType = audit.ComplianceType;
            existing.Description = audit.Description;
            existing.LastReviewedOn = audit.LastReviewedOn;
            existing.Status = audit.Status;
            existing.CreatedBy = audit.CreatedBy;
            existing.CreatedAt = audit.CreatedAt;
            existing.IsActive = audit.IsActive;
            existing.IsDeleted = audit.IsDeleted;

            _dbContext.FactoryComplianceAndAudits.Update(existing);
            return await _dbContext.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteComplianceAuditAsync(int id)
        {
            var existing = await _dbContext.FactoryComplianceAndAudits
                .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);

            if (existing == null) return false;

            existing.IsDeleted = true;
            existing.UpdatedAt = DateTime.UtcNow;

            _dbContext.FactoryComplianceAndAudits.Update(existing);
            return await _dbContext.SaveChangesAsync() > 0;
        }

        public async Task<CommonResponseModel> UpsertAuditComplianceMetricAsync(UpdateAuditComplianceMetricsDto dto)
        {
            var response = new CommonResponseModel();

            try
            {
                var ctx = _httpContextAccessor.HttpContext;
                var ipAddress = ctx?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown";

                var entity = await _dbContext.AuditComplianceMetrics
                    .FirstOrDefaultAsync(x => x.Id == dto.Id && !x.IsDeleted);

                string actionType;
                string eventType;
                string details;

                if (entity == null)
                {
                    entity = new AuditComplianceMetric
                    {
                        TenantId = dto.TenantId,
                        IsolationLevel = dto.IsolationLevel!,
                        DataRegion = dto.DataRegion!,
                        ComplianceLevel = dto.ComplianceLevel!,
                        LastAuditDate = dto.LastAuditDate,
                        MonthlyMaintenanceCost = dto.MonthlyMaintenanceCost,

                        ValidationIsolationLevel = dto.ValidationIsolationLevel!,
                        ValidationSchemaIsolation = dto.ValidationSchemaIsolation!,
                        ValidationFileStorage = dto.ValidationFileStorage!,
                        ValidationApiAccessControl = dto.ValidationApiAccessControl!,
                        ValidationTimestamp = dto.ValidationTimestamp,

                        MetricDataIntegrityScore = dto.MetricDataIntegrityScore,
                        MetricAccessControl = dto.MetricAccessControl,
                        MetricSchemaIsolation = dto.MetricSchemaIsolation,
                        MetricNetworkSegmentation = dto.MetricNetworkSegmentation,
                        MetricsTimestamp = dto.MetricsTimestamp,

                        AccessAttempts24h = dto.AccessAttempts24H,
                        BlockedRequests24h = dto.BlockedRequests24H,
                        PolicyViolations24h = dto.PolicyViolations24H,
                        DataQueries24h = dto.DataQueries24H,
                        SecurityStatsTimestamp = dto.SecurityStatsTimestamp,

                        CreatedAt = DateTime.UtcNow,
                        IsActive = true,
                        IsDeleted = false
                    };

                    _dbContext.AuditComplianceMetrics.Add(entity);

                    actionType = "Create";
                    eventType = "Audit-compliance-created";
                    details = "Add-AuditComplianceMetrics";

                    response.StatusMessage = TenantIsolationStatusMessage.AddAudit;
                }
                else
                {
                    entity.IsolationLevel = dto.IsolationLevel!;
                    entity.DataRegion = dto.DataRegion!;
                    entity.ComplianceLevel = dto.ComplianceLevel!;
                    entity.LastAuditDate = dto.LastAuditDate;
                    entity.MonthlyMaintenanceCost = dto.MonthlyMaintenanceCost;

                    entity.ValidationIsolationLevel = dto.ValidationIsolationLevel!;
                    entity.ValidationSchemaIsolation = dto.ValidationSchemaIsolation!;
                    entity.ValidationFileStorage = dto.ValidationFileStorage!;
                    entity.ValidationApiAccessControl = dto.ValidationApiAccessControl!;
                    entity.ValidationTimestamp = dto.ValidationTimestamp;

                    entity.MetricDataIntegrityScore = dto.MetricDataIntegrityScore;
                    entity.MetricAccessControl = dto.MetricAccessControl;
                    entity.MetricSchemaIsolation = dto.MetricSchemaIsolation;
                    entity.MetricNetworkSegmentation = dto.MetricNetworkSegmentation;
                    entity.MetricsTimestamp = dto.MetricsTimestamp;

                    entity.AccessAttempts24h = dto.AccessAttempts24H;
                    entity.BlockedRequests24h = dto.BlockedRequests24H;
                    entity.PolicyViolations24h = dto.PolicyViolations24H;
                    entity.DataQueries24h = dto.DataQueries24H;
                    entity.SecurityStatsTimestamp = dto.SecurityStatsTimestamp;

                    actionType = "Update";
                    eventType = "Audit-compliance-updated";
                    details = "Update-AuditComplianceMetrics";

                    response.StatusMessage = TenantIsolationStatusMessage.AuditUpdated;
                }

                var auditLog = new Audit_Log_MasterDb
                {
                    Action = actionType,
                    Details = details,
                    EventType = eventType,
                    TenantId = dto.TenantId,
                    Email = "",
                    Timestamp = DateTime.UtcNow,
                    IsActive = true,
                    IsDeleted = false,
                    UserName = Environment.UserName,
                    Ipaddress = ipAddress
                };

                await _masterDbcontext.Audit_Log_MasterDb.AddAsync(auditLog);
                await _dbContext.SaveChangesAsync();

                response.StatusCode = "200";
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(
                    ex,
                    sourceModule: "AuditComplianceMetrics",
                    apiName: "FactoryAuditComplianceMetrics",
                    tenantId: dto.TenantId,
                    userId: null
                );

                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"Error: {ex.Message}";
            }

            return response;
        }

        public GetAllRecord<GetAuditComplianceMetricsDto?> GetAuditComplianceMetricsAsync()
        {
            var response = new GetAllRecord<GetAuditComplianceMetricsDto?>
            {
                StatusCode = StatusCode.Success,
                StatusMessage = TenantIsolationStatusMessage.AuditFetched
            };

            try
            {
                var dataList = _dbContext.AuditComplianceMetrics
                                .Where(x => !x.IsDeleted)
                                .OrderByDescending(x => x.Id)
                                .ToList();

                if (dataList.Any())
                {
                    foreach (var data in dataList)
                    {
                        response.GetAllData.Add(new GetAuditComplianceMetricsDto
                        {
                            Id = data.Id,
                            IsolationLevel = data.IsolationLevel,
                            DataRegion = data.DataRegion,
                            ComplianceLevel = data.ComplianceLevel,
                            LastAuditDate = data.LastAuditDate,
                            MonthlyMaintenanceCost = data.MonthlyMaintenanceCost,

                            ValidationIsolationLevel = data.ValidationIsolationLevel,
                            ValidationSchemaIsolation = data.ValidationSchemaIsolation,
                            ValidationFileStorage = data.ValidationFileStorage,
                            ValidationApiAccessControl = data.ValidationApiAccessControl,
                            ValidationTimestamp = data.ValidationTimestamp,

                            MetricDataIntegrityScore = data.MetricDataIntegrityScore,
                            MetricAccessControl = data.MetricAccessControl,
                            MetricSchemaIsolation = data.MetricSchemaIsolation,
                            MetricNetworkSegmentation = data.MetricNetworkSegmentation,
                            MetricsTimestamp = data.MetricsTimestamp,

                            AccessAttempts24h = data.AccessAttempts24h,
                            BlockedRequests24h = data.BlockedRequests24h,
                            PolicyViolations24h = data.PolicyViolations24h,
                            DataQueries24h = data.DataQueries24h,
                            SecurityStatsTimestamp = data.SecurityStatsTimestamp
                        });
                    }
                }
                else
                {
                    response.StatusCode = StatusCode.NotFound;
                    response.StatusMessage = TenantIsolationStatusMessage.NoRecordsFound;
                }

            }
            catch (Exception ex)
            {
                _exceptionLogger.LogExceptionAsync(
                    ex,
                    sourceModule: "AuditComplianceMetrics",
                    apiName: "GetAuditComplianceMetrics",
                    tenantId: null,
                    userId: null
                );

                response.StatusCode = StatusCode.Error;
                response.StatusMessage = TenantIsolationStatusMessage.InternalServerError;
            }

            return response;
        }


    }
}
