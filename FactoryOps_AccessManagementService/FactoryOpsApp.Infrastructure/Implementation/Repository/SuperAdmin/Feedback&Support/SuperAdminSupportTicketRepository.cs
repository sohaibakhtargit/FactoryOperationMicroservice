using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.SuperAdmin.Feedback_Support;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.SuperAdmin.AuditLogs;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.ExceptionLogger;
using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Infrastructure.DBContext;
using Microsoft.EntityFrameworkCore;
using static FactoryOps_AccessManagementService.FactoryOpsApp.Common.CommonConstant;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Repository.SuperAdmin.Feedback_Support
{
    public class SuperAdminSupportTicketRepository : ISuperAdminSupportTicketRepository
    {
        private readonly MasterFactoryOpsDbContext _masterDbContext;
        private readonly TenantDbContextFactory _tenantDbContext;
        private readonly IExceptionLoggerService _exceptionLogger;
        private readonly IAuditLogService _auditLogger;
        public SuperAdminSupportTicketRepository(
            MasterFactoryOpsDbContext masterDbContext,
            TenantDbContextFactory tenantDbContext,
            IExceptionLoggerService exceptionLogger,
            IAuditLogService auditLogger)
        {
            _masterDbContext = masterDbContext;
            _tenantDbContext = tenantDbContext;
            _exceptionLogger = exceptionLogger;
            _auditLogger = auditLogger;
        }

        public async Task<GetAllRecord<SuperAdminSupportTicketDto>> GetAllSupportTicketsAsync()
        {
            GetAllRecord<SuperAdminSupportTicketDto> response = new();
            try
            {
                var tenants = await _masterDbContext.FactoryTenants
                    .Where(t => !t.IsDeleted && t.IsActive)
                    .ToListAsync();

                var tasks = tenants.Select(async tenant =>
                {
                    try
                    {
                        using var tenantDb = _tenantDbContext.GetTenantDbContext(tenant.TenantId);

                        var tickets = await tenantDb.FactorySupportTickets
                            .AsNoTracking()
                            .Where(t => !t.IsDeleted)
                            .Include(t => t.AssignedTeam)
                            .Select(t => new SuperAdminSupportTicketDto
                            {
                                TicketId = t.TicketId,
                                Subject = t.Subject,
                                Description = t.Description,
                                Priority = t.Priority,
                                AssignedTo = t.AssignedTo,
                                Status = t.Status,
                                AssignedName = t.AssignedTeam != null
                                                ? t.AssignedTeam.Name : null,
                                Module = t.Module,
                                CreatedBy = t.CreatedBy ?? null,
                                CreatedAt = t.CreatedAt,
                                TenantId = t.TenantId,
                                TenantName = tenant.TenantName
                            })
                            .ToListAsync();

                        return tickets;
                    }
                    catch (Exception ex)
                    {
                        await _exceptionLogger.LogExceptionAsync(
                            ex,
                            sourceModule: "SupportModule",
                            apiName: "GetAllSupportTicketsAsync",
                            tenantId: null,
                            userId: null
                        );
                        return new List<SuperAdminSupportTicketDto>();
                    }
                });

                var allTickets = (await Task.WhenAll(tasks)).SelectMany(t => t).ToList();
                response.StatusMessage = SuperAdminSupportTicketStatusMessage.DataFetched;
                response.StatusCode = StatusCode.Success;
                response.GetAllData = allTickets;
                return response;
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(
                            ex,
                            sourceModule: "SupportModule",
                            apiName: "GetAllSupportTicketsAsync",
                            tenantId: null,
                            userId: null
                        );
                return response;
            }
        }

        public async Task<CommonResponseModel> UpdateSupportTicketAsync(UpdateSupportTicketDto dto)
        {
            CommonResponseModel response = new();

            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(dto.TenantId);

                var ticket = await tenantDb.FactorySupportTickets
                    .FirstOrDefaultAsync(t => t.TicketId == dto.TicketId && !t.IsDeleted);

                if (ticket == null)
                {
                    response.StatusCode = StatusCode.NotFound;
                    response.StatusMessage = SuperAdminSupportTicketStatusMessage.NoRecordsFound;
                    return response;
                }

                if (!string.IsNullOrEmpty(dto.Status))
                    ticket.Status = dto.Status;

                if (!string.IsNullOrEmpty(dto.Priority))
                    ticket.Priority = dto.Priority;

                if (dto.AssignedTo.HasValue)
                    ticket.AssignedTo = dto.AssignedTo.Value;

                if (!string.IsNullOrEmpty(dto.ResolutionNotes))
                    ticket.ResolutionNotes = dto.ResolutionNotes;

                tenantDb.FactorySupportTickets.Update(ticket);

                await _auditLogger.LogAuditAsync(
                    action: "Update",
                    details: $"Support ticket ID {dto.TicketId} updated",
                    eventType: "SuperAdminSupportTickets",
                    email: null,
                    tenantId: dto.TenantId
                );

                await tenantDb.SaveChangesAsync();

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = SuperAdminSupportTicketStatusMessage.DataUpdated;
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(
                    ex,
                    sourceModule: "SupportModule",
                    apiName: "UpdateSupportTicketAsync",
                    tenantId: dto.TenantId,
                    userId: null
                );
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = SuperAdminSupportTicketStatusMessage.UpdateFailed + ex.Message;
            }

            return response;
        }
    }
}
